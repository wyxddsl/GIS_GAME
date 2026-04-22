using System;
using System.Text;
using System.Windows.Forms;
using GISGameFramework.Core;
using GISGameFramework.Game;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Collections.Generic;
using GISGameFramework.Game.ArcEngine;
using ESRI.ArcGIS.Controls;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace GISGameFramework.WinFormHost
{
    public partial class FormMain : Form
    {
        private const string DemoPlayer1Id = "user_1";
        private const string DemoPlayer2Id = "user_2";
        private const string DemoQuizPointId = "quiz_point_1";
        private const string DemoSearchPointId = "search_point_1";
        private const string DemoOpenTreasureId = "tr_open_1";
        private IMapControlInteract mapControlInteract;
        private TcpListener _tcpListener;
        private Thread _tcpThread;
        private bool _tcpRunning = false;

        private readonly GameCoreManager _gameCoreManager;
        private MultiPlayerWalker _walker;
        private QuizPosition _quizPosition;
        private bool _frameworkInitialized;
        private bool _seeded;
        private string _soloSessionId;
        private string _lastInputJson;
        private string _lastOutputJson;
        private bool _treasuresVisible = true;  // 宝藏是否可见
        private double PkBufferRadius = 10.0;  // PK缓冲半径（米），与FindNearbyOpponent距离保持一致
        private RankingForm _rankingForm;

        #region 消息提示辅助方法

        /// <summary>
        /// 显示信息提示对话框
        /// </summary>
        private void ShowInfo(string message, string title = "提示")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 显示警告对话框
        /// </summary>
        private void ShowWarning(string message, string title = "警告")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// 显示错误对话框
        /// </summary>
        private void ShowError(string message, string title = "错误")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 显示成功对话框（带图标）
        /// </summary>
        private void ShowSuccess(string message, string title = "成功")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        public FormMain()
        {
            InitializeComponent();

            _gameCoreManager = new GameCoreManager();
            FormClosed += FormMainClosed;
            var mapCtrl = this.axMapControl1.Object as IMapControl3;
            mapControlInteract = new AEMapControlInteract(mapCtrl);
            _walker = new MultiPlayerWalker(mapCtrl);
            _walker.OnPlayerPositionChanged += OnPlayerWalked;
            _walker.RefreshPlayersOnMap = () => mapControlInteract.PlacePlayersOnMap();
            _quizPosition = new QuizPosition(mapCtrl);
            btGpsListenOpen.Click += OnGpsListenToggleClicked;
        }

        private void FormMainClosed(object sender, FormClosedEventArgs e)
        {
            if (_walker != null) _walker.StopAll();
            if (_tcpRunning) StopTcpListener();
            if (_frameworkInitialized)
                _gameCoreManager.Shutdown();
        }

        private void OnInitializeClicked(object sender, EventArgs e)
        {
            var result = _gameCoreManager.Initialize();
            _frameworkInitialized = result.Success;
            WriteResult(result);
            RefreshSnapshot();
        }

        private void OnSeedClicked(object sender, EventArgs e)
        {
            if (!EnsureInitialized())
            {
                return;
            }

            if (_seeded)
            {
                AppendLog("演示数据已加载，无需重复初始化。");
                RefreshSnapshot();
                return;
            }


            mapControlInteract.loadMap();

            var map = (this.axMapControl1.Object as IMapControl3).Map;
            string demPath = AEUtil.GetDEMPath();
            string hillshadePath = AEUtil.GetHillshadeShpPath();
            _gameCoreManager.ReplaceEnvironmentAdapter(new ArcEngineAdapter(map, demPath, hillshadePath));

            var r = _gameCoreManager.UpdateHillshadeFromLightLevel(ConUtil.DefaultLightLevel);
            AppendLog(r.Success ? r.Message : "首次 Hillshade 重算失败：" + r.Message);
            SyncLightSlider(50);

            SeedSampleData();
            mapControlInteract.GameCoreManager = _gameCoreManager;
            mapControlInteract.AllPlayers = _gameCoreManager.PlayerService.GetAllPlayers();

            mapControlInteract.ShowTreasureMarkerAndText = true;
            mapControlInteract.ShowTreasureImage = false;

            mapControlInteract.PlacePlayersOnMap();
            mapControlInteract.PlaceTreasuresOnMap();

            _quizPosition.OnQuizPointTriggered += OnQuizPointTriggered;
            _quizPosition.Load(AppendLog);

            _seeded = true;

            AppendLog("演示数据加载完成。");
            RefreshSnapshot();
        }


        private void OnPositionClicked(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            _walker.StartWalk(DemoPlayer1Id, 0, ConUtil.StepIntervalMs);
            btnStopPosition.Text = "玩家停止移动";
            AppendLog("开始沿手绘路径移动玩家。");
        }
        //标记玩家行走的过程中，是否搜索宝藏
        private bool isFindTreasure = true;
        private void OnPlayerWalked(string playerId, GeoPosition pos)
        {
            _gameCoreManager.HandlePlayerPositionUpdate(playerId, pos);

            // 答题点位缓冲区检测（仅对玩家一）
            if (playerId == DemoPlayer1Id && _quizPosition != null)
                _quizPosition.CheckProximity(playerId, pos);
            if (isFindTreasure)
            {
                // 查询玩家当前位置附近的点位
                var nearby = _gameCoreManager.InteractionPointService.GetNearbyPoints(pos, ConUtil.NearbyPointScanRadius);
                if (nearby.Success && nearby.Data != null)
                {
                    foreach (var point in nearby.Data)
                    {
                        double dist = AEUtil.CalcDistanceMeters(pos, point.Position);
                        // 搜索型宝藏
                        if (point.PointType == InteractionPointType.Search && dist <= point.TriggerRadius)
                        {
                            var result = _gameCoreManager.ProcessTreasureSearchFlow(playerId, point.PointId);
                            if (result.Success)
                            {
                                lock (objThread)
                                {
                                    var treasureResult = _gameCoreManager.TreasureService.GetTreasureByPointId(point.PointId);
                                    int jiFen = treasureResult.Success ? treasureResult.Data.ScoreValue : 0;
                                    bool isCollect = treasureResult.Success ? treasureResult.Data.IsCollected : false;
                                    string treasureName = treasureResult.Success ? treasureResult.Data.TreasureName : "宝藏";
                                    AppendLog(string.Format("自动触发搜索点，找到宝藏：{0}", treasureName));
                                    // 显示图片
                                    mapControlInteract.RemoveTreasureFromMap(point.PointId);
                                    mapControlInteract.ShowTreasureImageOnMap(point.PointId);
                                    WriteDispatch(result);
                                    ShowSuccess(string.Format("自动发现宝藏：{0}", treasureName), "找到宝藏");
                                }
                            }
                        }
                        else if (point.PointType == InteractionPointType.OpenTreasure && dist <= ConUtil.OpenTreaDistance)
                        {
                            Console.WriteLine("附近存在露天宝藏");
                            var treasureResult = _gameCoreManager.TreasureService.GetTreasureByPointId(point.PointId);
                            lock (objThread)
                            {
                                if (treasureResult.Success)
                                {
                                    var treasure = treasureResult.Data;
                                    string treasureName = treasureResult.Success ? treasureResult.Data.TreasureName : "宝藏";
                                    int jiFen = treasureResult.Success ? treasureResult.Data.ScoreValue : 0;
                                    bool isCollect = treasureResult.Success ? treasureResult.Data.IsCollected : true;
                                    var visibilityResult = _gameCoreManager.EnvironmentService.EvaluateVisibility(
                                        pos, point.Position, treasure.RequiredLightLevel);
                                    if (visibilityResult.Success && visibilityResult.Data.IsVisible && !isCollect)
                                    {
                                        treasureResult.Data.IsCollected = true;
                                        string info = string.Format("发现可见的露天宝藏：{0}（距离 {1:F1}m，光照 {2}）", treasure.TreasureName, dist, visibilityResult.Data.ActualLightLevel);
                                        AppendLog(info);
                                        // 显示图片
                                        mapControlInteract.RemoveTreasureFromMap(point.PointId);
                                        mapControlInteract.ShowTreasureImageOnMap(point.PointId);
                                        ShowSuccess(string.Format("自动发现宝藏：{0},宝藏积分：{1}", treasureName, jiFen), "找到宝藏");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            WriteDispatch(_gameCoreManager.RunPositionDemoStep(playerId));
            if (_tysonElements.Count > 0)
            {
                UpdateAreaNameVisibility();
            }
        }

        private object objThread = new object();

        private void OnQuizPointTriggered(string pointId)
        {
            // 找到对应点位名称
            string pointName = pointId;
            foreach (var pt in _quizPosition.Points)
                if (pt.PointId == pointId) { pointName = pt.DisplayName; break; }

            AppendLog(string.Format("进入答题点缓冲区：{0}，自动开启单人答题。", pointName));

            // 创建答题会话
            var result = _gameCoreManager.StartSoloQuizFlow(DemoPlayer1Id, pointId);
            if (!result.Success)
            {
                AppendLog("创建答题会话失败：" + result.Message);
                return;
            }

            var sessionResult = result.Data.BusinessResult as ResponseResult<QuizSession>;
            if (sessionResult == null || !sessionResult.Success)
            {
                AppendLog("无法获取答题会话信息。");
                return;
            }

            _soloSessionId = sessionResult.Data.SessionId;

            // 在 UI 线程弹出答题窗口
            Action openForm = () =>
            {
                var form = new QA.Form2(_gameCoreManager, _soloSessionId, (msg) =>
                {
                    AppendLog(msg);
                    if (_rankingForm != null && !_rankingForm.IsDisposed)
                    {
                        var p = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
                        int total = p.Success ? p.Data.TotalScore : 0;
                        _rankingForm.NotifyScoreChanged(0, total, "答题得分", msg);
                    }
                });
                // 答题完成后重新激活点位，允许再次触发
                form.FormClosed += (s, e2) => _quizPosition.ReactivatePoint(pointId);
                form.Show();
            };

            if (this.InvokeRequired)
                this.Invoke(openForm);
            else
                openForm();
        }

        private void OnStartSoloQuizClicked(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            if (_quizPosition == null)
            {
                ShowError("答题点位未初始化。");
                return;
            }

            // 获取玩家当前位置，检测是否在某个答题点缓冲区内
            var playerResult = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
            if (!playerResult.Success)
            {
                ShowError("无法获取玩家信息。");
                return;
            }

            GeoPosition playerPos = playerResult.Data.CurrentPosition;
            InteractionPoint triggeredPoint = null;
            foreach (var pt in _quizPosition.Points)
            {
                if (!pt.IsActive) continue;
                if (AEUtil.CalcDistanceMeters(playerPos, pt.Position) <= QuizPosition.BufferMeters)
                {
                    triggeredPoint = pt;
                    break;
                }
            }

            if (triggeredPoint == null)
            {
                ShowInfo("你不在任何答题点缓冲区内。\n请移动到答题点附近（20米范围内）再开始答题。");
                AppendLog("开始答题失败：玩家不在任何答题点缓冲区内。");
                return;
            }

            // 与 CheckProximity 保持一致：触发后将点位设为非激活
            triggeredPoint.IsActive = false;

            AppendLog(string.Format("进入答题点缓冲区：{0}，手动开启单人答题。", triggeredPoint.DisplayName));

            var result = _gameCoreManager.StartSoloQuizFlow(DemoPlayer1Id, triggeredPoint.PointId);
            if (!result.Success)
            {
                triggeredPoint.IsActive = true; // 失败时恢复点位
                ShowError("创建答题会话失败：" + result.Message);
                return;
            }

            var sessionResult = result.Data.BusinessResult as ResponseResult<QuizSession>;
            if (sessionResult == null || !sessionResult.Success)
            {
                triggeredPoint.IsActive = true;
                ShowError("无法获取答题会话信息。");
                return;
            }

            _soloSessionId = sessionResult.Data.SessionId;
            result.Data.Detail = "演示步骤：进入答题点缓冲区，创建单人答题会话并下发题目数据。";
            WriteDispatch(result);

            string pointId = triggeredPoint.PointId;
            var form = new QA.Form2(_gameCoreManager, _soloSessionId, (msg) =>
            {
                AppendLog(msg);
                if (_rankingForm != null && !_rankingForm.IsDisposed)
                {
                    var p = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
                    int total = p.Success ? p.Data.TotalScore : 0;
                    _rankingForm.NotifyScoreChanged(0, total, "答题得分", msg);
                }
            });
            form.FormClosed += (s, e2) => _quizPosition.ReactivatePoint(pointId);
            form.Show();
        }

        private void OnSubmitSoloAnswerClicked(object sender, EventArgs e)
        {
            AppendLog("答题请通过答题窗口进行。");
        }

        private void OnSearchTreasureClicked(object sender, EventArgs e)
        {
            if (!EnsureReady())
            {
                return;
            }

            var player = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
            if (!player.Success)
            {
                ShowError("无法获取玩家信息。");
                return;
            }
            var nearby = _gameCoreManager.InteractionPointService.GetNearbyPoints(player.Data.CurrentPosition, ConUtil.ManualSearchRadius);
            if (!nearby.Success || nearby.Data == null || nearby.Data.Count == 0)
            {
                ShowInfo("附近没有可搜索的宝藏点。\n请移动到宝藏点附近（20米范围内）再尝试搜索。");
                AppendLog("搜索失败：附近 20 米范围内没有宝藏点。");
                return;
            }

            // 找到第一个搜索类型的点位
            InteractionPoint searchPoint = null;
            foreach (var point in nearby.Data)
            {
                if (point.PointType == InteractionPointType.Search)
                {
                    searchPoint = point;
                    break;
                }
            }

            if (searchPoint == null)
            {
                ShowInfo("附近没有可搜索的宝藏点。\n附近只有露天宝藏，需要满足光照条件才能发现。");
                AppendLog("搜索失败：附近没有搜索类型的宝藏点。");
                return;
            }

            double distance = AEUtil.CalcDistanceMeters(player.Data.CurrentPosition, searchPoint.Position);
            var result = _gameCoreManager.ProcessTreasureSearchFlow(DemoPlayer1Id, searchPoint.PointId);

            if (result.Success)
            {
                mapControlInteract.RemoveTreasureFromMap(searchPoint.PointId);
                mapControlInteract.ShowTreasureImageOnMap(searchPoint.PointId);

                var treasureResult = _gameCoreManager.TreasureService.GetTreasureByPointId(searchPoint.PointId);
                string treasureName = treasureResult.Success ? treasureResult.Data.TreasureName : "宝藏";
                int score = treasureResult.Success ? treasureResult.Data.ScoreValue : 0;
                string rarity = treasureResult.Success ? GetRarityText(treasureResult.Data.Rarity) : "普通";
                ShowSuccess(
                    string.Format("找到宝藏\n\n" +
                                  "名称：{0}\n" +
                                  "稀有度：{1}\n" +
                                  "距离：{2:F1} 米\n" +
                                  "积分：+{3}",
                                  treasureName, rarity, distance, score),
                    "搜索宝藏");

                AppendLog(string.Format("成功搜索到宝藏：{0}（{1}），距离 {2:F1} 米，获得 {3} 积分。",
                    treasureName, rarity, distance, score));
                WriteDispatch(result);
                RefreshSnapshot();
            }
            else
            {
                ShowWarning(
                    string.Format("搜索失败：{0}", result.Message),
                    "搜索失败");
                AppendLog(string.Format("搜索宝藏失败：{0}", result.Message));
            }
        }

        // 获取稀有度文本
        private string GetRarityText(int rarity)
        {
            switch (rarity)
            {
                case 0: return "普通";
                case 1: return "稀有";
                case 2: return "史诗";
                case 3: return "传说";
                case 4: return "神话";
                default: return "未知";
            }
        }

        private void OnOpenTreasureClicked(object sender, EventArgs e)
        {
            if (!EnsureReady())
            {
                return;
            }
            var player = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
            if (!player.Success)
            {
                ShowError("无法获取玩家信息。");
                return;
            }
            var nearby = _gameCoreManager.InteractionPointService.GetNearbyPoints(player.Data.CurrentPosition, ConUtil.OpenTreaDistance);
            if (!nearby.Success || nearby.Data == null || nearby.Data.Count == 0)
            {
                ShowInfo("附近没有可发现的露天宝藏。\n请移动到露天宝藏点附近（50米范围内）再尝试。");
                AppendLog("发现失败：附近 50 米范围内没有宝藏点。");
                return;
            }
            InteractionPoint openPoint = null;
            foreach (var point in nearby.Data)
            {
                if (point.PointType == InteractionPointType.OpenTreasure)
                {
                    openPoint = point;
                    break;
                }
            }
            if (openPoint == null)
            {
                ShowInfo("附近没有露天宝藏。\n附近只有搜索型宝藏，请使用\"搜索宝藏\"功能。");
                AppendLog("发现失败：附近没有露天宝藏类型的点位。");
                return;
            }
            // 获取该点位绑定的宝藏
            var treasureResult = _gameCoreManager.TreasureService.GetTreasureByPointId(openPoint.PointId);
            if (!treasureResult.Success)
            {
                ShowError("无法获取宝藏信息。");
                AppendLog("发现失败：无法获取宝藏信息。");
                return;
            }

            string treasureId = treasureResult.Data.TreasureId;
            string treasureName = treasureResult.Data.TreasureName;
            int requiredLight = treasureResult.Data.RequiredLightLevel;
            double distance = AEUtil.CalcDistanceMeters(player.Data.CurrentPosition, openPoint.Position);
            // 检查可见性条件（光照、距离、视域）
            var visibilityResult = _gameCoreManager.EnvironmentService.EvaluateVisibility(
                player.Data.CurrentPosition,
                openPoint.Position,
                requiredLight);

            if (!visibilityResult.Success)
            {
                ShowError(string.Format("无法评估宝藏可见性：{0}", visibilityResult.Message));
                AppendLog(string.Format("可见性评估失败：{0}", visibilityResult.Message));
                return;
            }
            var visibility = visibilityResult.Data;
            if (!visibility.IsVisible)
            {
                var reasons = new System.Collections.Generic.List<string>();
                if (!visibility.MeetsDistanceRequirement)
                    reasons.Add(string.Format("距离过远（当前 {0:F1} 米，需要 ≤50 米）", visibility.Distance));
                if (!visibility.MeetsLightRequirement)
                    reasons.Add(string.Format("光照不足（当前 {0}，需要 ≥{1}）",
                        visibility.ActualLightLevel, requiredLight));
                if (!visibility.HasLineOfSight)
                    reasons.Add("视线被遮挡，无法通视");
                string reasonText = string.Join("\n", reasons.ToArray());

                ShowWarning(
                    string.Format("无法发现露天宝藏：{0}\n\n不满足的条件：\n{1}\n\n提示：\n" +
                                  "靠近宝藏点（50米内）\n" +
                                  "等待光照充足时\n" +
                                  "确保视线无遮挡",
                                  treasureName, reasonText),
                    "条件不满足");
                AppendLog(string.Format("发现失败：{0} - {1}", treasureName, reasonText.Replace("\n", ", ")));
                return;
            }
            // 执行露天宝藏发现流程
            var result = _gameCoreManager.ProcessOpenTreasureDiscoveryFlow(DemoPlayer1Id, treasureId);
            if (result.Success)
            {
                mapControlInteract.RemoveTreasureFromMap(openPoint.PointId);
                mapControlInteract.ShowTreasureImageOnMap(openPoint.PointId);
                int score = treasureResult.Data.ScoreValue;
                string rarity = GetRarityText(treasureResult.Data.Rarity);
                ShowSuccess(
                    string.Format("发现露天宝藏\n\n" +
                                  "名称：{0}\n" +
                                  "稀有度：{1}\n" +
                                  "距离：{2:F1} 米\n" +
                                  "光照：{3}\n" +
                                  "积分：+{4}\n\n" +
                                  "满足条件：\n" +
                                  "-距离 ≤50米\n" +
                                  "光照 ≥{5}\n" +
                                  "视线通畅",
                                  treasureName, rarity, distance,
                                  visibility.ActualLightLevel, score, requiredLight),
                    "发现宝藏");

                AppendLog(string.Format("成功发现露天宝藏：{0}（{1}），距离 {2:F1} 米，光照 {3}，获得 {4} 积分。",
                    treasureName, rarity, distance, visibility.ActualLightLevel, score));
                WriteDispatch(result);
                RefreshSnapshot();
            }
            else
            {
                ShowWarning(string.Format("发现失败：{0}", result.Message), "发现失败");
                AppendLog(string.Format("发现露天宝藏失败：{0}", result.Message));
            }
        }

        private void OnStartPkClicked(object sender, EventArgs e)
        {
            if (!EnsureReady())
            {
                return;
            }

            var result = _gameCoreManager.TryStartNearbyPkFlow(DemoPlayer1Id);
            if (result.Success)
            {
                result.Data.Detail = "演示步骤：附近两名玩家进入 PK 会话，并下发相同题目。";
            }
            WriteDispatch(result);
        }

        private void OnFinishPkClicked(object sender, EventArgs e)
        {
            if (!EnsureReady())
            {
                return;
            }

            WriteDispatch(_gameCoreManager.RunPkDemoStep(DemoPlayer1Id, DemoPlayer2Id));
        }

        private void OnDashboardClicked(object sender, EventArgs e)
        {
            if (!EnsureReady())
            {
                return;
            }

            var snapshot = _gameCoreManager.GetDemoSnapshot(DemoPlayer1Id);
            WriteResult(snapshot);
            RefreshSnapshot();
        }

        private void OnShowRankingClicked(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            if (_rankingForm == null || _rankingForm.IsDisposed)
            {
                _rankingForm = new RankingForm(
                    () => _gameCoreManager.LeaderboardService.GetTop(15).Data,
                    id => _gameCoreManager.PlayerService.GetPlayer(id).Data,
                    DemoPlayer1Id);
            }
            _rankingForm.Show();
            _rankingForm.BringToFront();
        }

        // 最邻近分析技能按钮：找到距玩家最近的藏宝点，显示方向和距离
        private void OnNearestTreasureHintClicked(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;
            var result = _gameCoreManager.UseNearestTreasureHintFlow(DemoPlayer1Id);
            if (result.Success)
            {
                result.Data.Detail = "演示步骤：最邻近分析技能，提示距玩家最近的藏宝点方向和距离。";
                ShowInfo(result.Message, "最邻近分析");
            }
            WriteDispatch(result);
        }

        // 切换宝藏显示/隐藏
        private void OnToggleTreasureVisibilityClicked(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            _treasuresVisible = !_treasuresVisible;

            // 获取按钮引用
            Button btn = sender as Button;

            if (_treasuresVisible)
            {
                mapControlInteract.PlaceTreasuresOnMap();
                if (btn != null) btn.Text = "隐藏宝藏";
                AppendLog("宝藏已显示在地图上。");
            }
            else
            {
                IGraphicsContainer gc = (this.axMapControl1.Object as IMapControl3).Map as IGraphicsContainer;
                if (gc != null)
                {
                    gc.Reset();
                    var toDelete = new List<IElement>();
                    IElement elem;
                    while ((elem = gc.Next()) != null)
                    {
                        IElementProperties props = elem as IElementProperties;
                        if (props != null && props.Name == "treasure")
                            toDelete.Add(elem);
                    }
                    foreach (var e2 in toDelete)
                        gc.DeleteElement(e2);

                    IActiveView activeView = (this.axMapControl1.Object as IMapControl3).Map as IActiveView;
                    if (activeView != null)
                        activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                }
                if (btn != null) btn.Text = "显示宝藏";
                AppendLog("宝藏已从地图上隐藏。");
            }
        }

        // 重新布设宝物：从 geojson 重新随机抽点，重置宝藏状态，刷新地图标记
        private void btResetTreasures_Click(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            List<InteractionPoint> searchPoints, openPoints;
            if (!AEUtil.SamplePointsFromGeoJson(out searchPoints, out openPoints, new Action<string>((s) =>
            {
                AppendLog(s);
            })))
            {
                return;
            }

            for (int i = 0; i < AEUtil.SearchPointIds.Length; i++)
            {
                _gameCoreManager.InteractionPointService.AddPoint(new InteractionPoint
                {
                    PointId = AEUtil.SearchPointIds[i],
                    PointType = InteractionPointType.Search,
                    DisplayName = "搜索点" + (i + 1),
                    TriggerRadius = ConUtil.SearchTreaRadius,
                    Position = searchPoints[i].Position
                });
            }
            for (int i = 0; i < AEUtil.OpenPointIds.Length; i++)
            {
                _gameCoreManager.InteractionPointService.AddPoint(new InteractionPoint
                {
                    PointId = AEUtil.OpenPointIds[i],
                    PointType = InteractionPointType.OpenTreasure,
                    DisplayName = "露天点" + (i + 1),
                    TriggerRadius = 20,
                    Position = openPoints[i].Position
                });
            }

            var result = _gameCoreManager.RedeployTreasures(searchPoints, openPoints);
            if (!result.Success)
            {
                AppendLog("重新布设失败：" + result.Message);
                return;
            }
            mapControlInteract.PlaceTreasuresOnMap();
            AppendLog("宝物已重新布设，点位已随机更新。");
            RefreshSnapshot();
        }

        //***********************************************TCP通讯***********************************************//
        private void OnGpsListenToggleClicked(object sender, EventArgs e)
        {
            if (_tcpRunning)
            {
                StopTcpListener();
                btGpsListenOpen.Text = "打开GPS监听";
                AppendLog("GPS 监听已关闭。");
            }
            else
            {
                StartTcpListener();
                btGpsListenOpen.Text = "关闭GPS监听";
                AppendLog("GPS 监听已启动，端口 9000。");
            }
        }

        private void StartTcpListener()
        {
            _tcpListener = new TcpListener(IPAddress.Any, 9000); // 9000为监听端口，可自定义
            _tcpListener.Start();
            _tcpRunning = true;
            _tcpThread = new Thread(TcpListenLoop);
            _tcpThread.IsBackground = true;
            _tcpThread.Start();
        }

        private void StopTcpListener()
        {
            _tcpRunning = false;
            if (_tcpListener != null)
            {
                _tcpListener.Stop();
                _tcpListener = null;
            }
            if (_tcpThread != null && _tcpThread.IsAlive)
            {
                _tcpThread.Join();
                _tcpThread = null;
            }
        }

        private void OnTcpMessageReceived(string json, string playerId)
        {
            try
            {
                // 调试日志
                AppendLog(string.Format("TCP 原始消息：{0}", json));

                if (json.Contains("\"msg_type\":\"C2S_POS\""))
                {
                    // 提取 data 对象内的 x, y, acc 字段
                    double x = ExtractJsonDoubleFromData(json, "\"x\":");
                    double y = ExtractJsonDoubleFromData(json, "\"y\":");
                    double acc = ExtractJsonDoubleFromData(json, "\"acc\":");

                    AppendLog(string.Format("解析位置 - x={0}, y={1}, acc={2}", x, y, acc));

                    // 约定：x=经度，y=纬度
                    var envelope = new GISGameFramework.Core.MessageEnvelope
                    {
                        Header = new GISGameFramework.Core.MessageHeader
                        {
                            MessageType = GISGameFramework.Core.MessageType.C2S_POS,
                            ClientId = playerId,
                            SentAt = DateTime.UtcNow
                        },
                        Payload = new GISGameFramework.Core.PositionPayload
                        {
                            X = x,  // Longitude
                            Y = y,  // Latitude
                            Accuracy = acc
                        }
                    };

                    AppendLog(string.Format("更新玩家位置 - Longitude={0}, Latitude={1}", x, y));
                    _gameCoreManager.ProcessIncomingMessage(envelope);
                    mapControlInteract.PlacePlayersOnMap();
                }
                else
                {
                    AppendLog("未知消息类型：" + json);
                }
            }
            catch (Exception ex)
            {
                ShowError("消息解析失败：" + ex.Message);
                AppendLog("异常详情：" + ex.ToString());
            }

            CheckAndTriggerPkIfNearby(playerId);
        }
        private int FindMatchingBrace(string str, int startIdx)
        {
            int depth = 0;
            for (int i = startIdx; i < str.Length; i++)
            {
                if (str[i] == '{') depth++;
                else if (str[i] == '}')
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        private double ExtractJsonDouble(string json, string key)
        {
            int idx = json.IndexOf(key);
            if (idx < 0) return 0;
            idx += key.Length;

            while (idx < json.Length && (json[idx] == ' ' || json[idx] == ':'))
                idx++;

            int end = idx;
            while (end < json.Length && !char.IsWhiteSpace(json[end]) && json[end] != ',' && json[end] != '}' && json[end] != ']')
                end++;

            if (end <= idx) return 0;

            string numStr = json.Substring(idx, end - idx).Trim();
            double val = 0;
            double.TryParse(numStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val);
            return val;
        }

        private double ExtractJsonDoubleFromData(string json, string key)
        {
            int dataIdx = json.IndexOf("\"data\"");
            if (dataIdx < 0) return ExtractJsonDouble(json, key); // 兼容旧格式
            return ExtractJsonDouble(json.Substring(dataIdx), key);
        }

        private void TcpListenLoop()
        {
            while (_tcpRunning)
            {
                try
                {
                    var client = _tcpListener.AcceptTcpClient();
                    var stream = client.GetStream();
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string json = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(json))
                        {

                            this.Invoke((MethodInvoker)delegate
                            {
                                OnTcpMessageReceived(json, DemoPlayer1Id);
                            });
                        }
                    }
                    client.Close();
                }
                catch { /* 可加日志 */ }
            }
        }


        private void CheckAndTriggerPkIfNearby(string playerId)
        {
            // 只对玩家一检测
            if (playerId != DemoPlayer1Id) return;

            // 获取玩家一当前位置
            var player1 = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
            if (!player1.Success) return;
            var pos1 = player1.Data.CurrentPosition;

            // 获取所有玩家
            var allPlayers = _gameCoreManager.PlayerService.GetAllPlayers();
            if (!allPlayers.Success) return;

            foreach (var p in allPlayers.Data)
            {
                if (p.PlayerId == DemoPlayer1Id) continue; // 跳过自己
                var pos2 = p.CurrentPosition;
                double dist = AEUtil.CalcDistanceMeters(pos1, pos2);
                if (dist <= PkBufferRadius)
                {
                    // 触发PK
                    var result = _gameCoreManager.TryStartNearbyPkFlow(DemoPlayer1Id);
                    if (result.Success)
                    {
                        ShowInfo("检测到玩家进入PK缓冲区，已自动进入PK对战！");
                        AppendLog(string.Format("玩家距离：{0:F2}米，触发自动PK", dist));
                        WriteDispatch(result);
                    }
                    else
                    {
                        // 处理PK失败情况
                        ShowWarning(string.Format("PK启动失败：{0}", result.Message));
                        AppendLog(string.Format("PK启动失败 - {0}", result.Message));
                    }
                    break;
                }
            }
        }
        //**********************************************************************************************************************//

        private void SetUiEnabled(bool enabled)
        {
            foreach (Control ctrl in this.Controls)
                SetControlEnabled(ctrl, enabled);
        }

        private static void SetControlEnabled(Control parent, bool enabled)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Button || ctrl is TabControl)
                    ctrl.Enabled = enabled;
                SetControlEnabled(ctrl, enabled);
            }
        }

        private bool EnsureInitialized()
        {
            if (_frameworkInitialized)
            {
                return true;
            }

            AppendLog("请先点击“初始化框架”。");
            return false;
        }

        private bool EnsureReady()
        {
            if (!EnsureInitialized())
            {
                return false;
            }

            if (_seeded)
            {
                return true;
            }

            AppendLog("请先点击“加载演示数据”。");
            return false;
        }

        private void SeedSampleData()
        {
            _gameCoreManager.PlayerService.RegisterPlayer(new PlayerProfile
            {
                PlayerId = DemoPlayer1Id,
                PlayerName = "玩家一号",
                TotalScore = 2500,
                State = PlayerState.Exploring,
                //121.501786  31.284559
                CurrentPosition = new GeoPosition { Latitude = 31.284659, Longitude = 121.501686, Accuracy = 10.5 }
            });

            _gameCoreManager.PlayerService.RegisterPlayer(new PlayerProfile
            {
                PlayerId = DemoPlayer2Id,
                PlayerName = "玩家二号",
                TotalScore = 2100,
                State = PlayerState.Exploring,
                //121.501786  31.284559
                //121.492787
                CurrentPosition = new GeoPosition { Latitude = 31.288072, Longitude = 121.493798, Accuracy = 10.5 }
            });

            _gameCoreManager.InteractionPointService.AddPoint(new InteractionPoint
            {
                PointId = DemoQuizPointId,
                PointType = InteractionPointType.Quiz,
                DisplayName = "示例答题点",
                TriggerRadius = 20,
                Position = new GeoPosition { Latitude = 39.908, Longitude = 116.397 }
            });

            _gameCoreManager.InteractionPointService.AddPoint(new InteractionPoint
            {
                PointId = DemoSearchPointId,
                PointType = InteractionPointType.Search,
                DisplayName = "示例搜索点",
                TriggerRadius = 20,
                Position = new GeoPosition { Latitude = 39.908, Longitude = 116.397 }
            });

            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "1", Name = "初出茅庐", Description = "答对第一道题目，踏上探索之路。", ImagePath = "first_answer.png", TriggerType = AchievementTriggerType.FirstQuiz });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "2", Name = "宝藏猎人", Description = "首次成功获取宝藏。", ImagePath = "first_treasure.png", TriggerType = AchievementTriggerType.FirstTreasure });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "3", Name = "知识先锋", Description = "累计答对题目5道。", ImagePath = "answer_5.png", TriggerType = AchievementTriggerType.QuizCorrectCount, Threshold = 5 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "4", Name = "学霸附体", Description = "累计答对题目10道。", ImagePath = "answer_10.png", TriggerType = AchievementTriggerType.QuizCorrectCount, Threshold = 10 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "5", Name = "答题达人", Description = "累计答对题目20道。", ImagePath = "answer_20.png", TriggerType = AchievementTriggerType.QuizCorrectCount, Threshold = 20 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "6", Name = "初露锋芒", Description = "首次获得积分。", ImagePath = "score_start.png", TriggerType = AchievementTriggerType.ScoreThreshold, Threshold = 1 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "7", Name = "渐入佳境", Description = "累计获得积分超过10分。", ImagePath = "score_10.png", TriggerType = AchievementTriggerType.ScoreThreshold, Threshold = 10 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "8", Name = "融会贯通", Description = "累计获得积分超过20分。", ImagePath = "score_20.png", TriggerType = AchievementTriggerType.ScoreThreshold, Threshold = 20 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "9", Name = "登峰造极", Description = "累计获得积分超过50分。", ImagePath = "score_50.png", TriggerType = AchievementTriggerType.ScoreThreshold, Threshold = 50 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "10", Name = "榜首荣耀", Description = "游戏进行时积分排名第一。", ImagePath = "rank_first.png", TriggerType = AchievementTriggerType.RankFirst });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "11", Name = "探索者", Description = "解锁地图上3个不同地点。", ImagePath = "explore_3.png", TriggerType = AchievementTriggerType.AreaExploration, Threshold = 3 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "12", Name = "地图通", Description = "解锁地图上全部地点。", ImagePath = "explore_all.png", TriggerType = AchievementTriggerType.AllAreasUnlocked });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "13", Name = "找得着北", Description = "首次使用方向导航技能指引宝藏。", ImagePath = "navigation.png", TriggerType = AchievementTriggerType.NavigationUsed });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "14", Name = "战火初燃", Description = "首次参与PK对战。", ImagePath = "pk_first.png", TriggerType = AchievementTriggerType.FirstPk });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "15", Name = "势如破竹", Description = "连续赢得3次PK对战。", ImagePath = "pk_win3.png", TriggerType = AchievementTriggerType.PkWinStreak, Threshold = 3 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "16", Name = "横扫千军", Description = "累计赢得5次PK对战。", ImagePath = "pk_win5.png", TriggerType = AchievementTriggerType.PkWinCount, Threshold = 5 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "17", Name = "绝地反击", Description = "PK失败后再次参战并获胜。", ImagePath = "pk_comeback.png", TriggerType = AchievementTriggerType.PkComeback });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "18", Name = "人多力量大", Description = "与超过2名玩家同时在线游戏。", ImagePath = "multi_player.png", TriggerType = AchievementTriggerType.MultiPlayerOnline });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "19", Name = "宝藏收藏家", Description = "累计获取5个宝藏。", ImagePath = "treasure_5.png", TriggerType = AchievementTriggerType.TreasureCollectionCount, Threshold = 5 });
            _gameCoreManager.AchievementService.AddAchievement(new AchievementInfo { AchievementId = "20", Name = "全能冠军", Description = "同时位于积分榜首且解锁全部地点。", ImagePath = "champion.png", TriggerType = AchievementTriggerType.AllAreasAndRankFirst });

            _gameCoreManager.LeaderboardService.Refresh(_gameCoreManager.PlayerService.GetAllPlayers().Data);

            // 
            List<InteractionPoint> searchPoints, openPoints;
            if (!AEUtil.SamplePointsFromGeoJson(out searchPoints, out openPoints, new Action<string>((s) => { AppendLog(s); })))
            {
                // 备用占位（不影响编译和演示框架运行）
                searchPoints = new List<InteractionPoint>();
                openPoints = new List<InteractionPoint>();
                for (int i = 0; i < AEUtil.SearchPointIds.Length; i++)
                    searchPoints.Add(new InteractionPoint { PointId = AEUtil.SearchPointIds[i], PointType = InteractionPointType.Search, DisplayName = "搜索点" + (i + 1), TriggerRadius = ConUtil.SearchTreaRadius, Position = new GeoPosition { Latitude = 31.284 + i * 0.0001, Longitude = 121.499 + i * 0.0001 } });
                for (int i = 0; i < AEUtil.OpenPointIds.Length; i++)
                    openPoints.Add(new InteractionPoint { PointId = AEUtil.OpenPointIds[i], PointType = InteractionPointType.OpenTreasure, DisplayName = "露天点" + (i + 1), TriggerRadius = ConUtil.OpenTreaDistance, Position = new GeoPosition { Latitude = 31.287 + i * 0.0001, Longitude = 121.493 + i * 0.0001 } });
            }

            // 注册点位到 InteractionPointService
            foreach (var pt in searchPoints)
                _gameCoreManager.InteractionPointService.AddPoint(pt);
            foreach (var pt in openPoints)
                _gameCoreManager.InteractionPointService.AddPoint(pt);

            // id, name, type, score, ability, rarity
            var treasureData = new object[,]
            {
                {  1, "学苑木楔",         TreasureType.Search, 1,  0, 0 },
                {  2, "一朵樱花",         TreasureType.Search, 1,  0, 0 },
                {  3, "测绘馆结构图纸碎片", TreasureType.Search, 1,  0, 0 },
                {  4, "卷尺",             TreasureType.Search, 1,  0, 0 },
                {  5, "老旧的高数下课本", TreasureType.Search, 1,  0, 0 },
                {  6, "废弃的校园卡",     TreasureType.Search, 1,  0, 0 },
                {  7, "损坏的鼠标",       TreasureType.Search, 1,  0, 0 },
                {  8, "一包餐巾纸",       TreasureType.Search, 1,  0, 0 },
                {  9, "5kg哑铃",          TreasureType.Search, 2,  0, 1 },
                { 10, "测绘迎新文件袋",   TreasureType.Search, 2,  0, 1 },
                { 11, "脉动",             TreasureType.Search, 2,  0, 1 },
                { 12, "一捆衣架",         TreasureType.Search, 2,  0, 1 },
                { 13, "校徽钥匙扣",       TreasureType.Search, 2,  0, 1 },
                { 14, "测绘院服",         TreasureType.Search, 2,  0, 1 },
                { 15, "猫学长纪念贴纸",   TreasureType.Search, 5,  0, 2 },
                { 16, "校内景观明信片",   TreasureType.Search, 5,  0, 2 },
                { 17, "图书馆模型",       TreasureType.Search, 5,  0, 2 },
                { 18, "校徽胸章",         TreasureType.Search, 5,  0, 2 },
                { 19, "同济纪念外套",     TreasureType.Search, 5,  0, 2 },
                { 20, "家居扫地机",       TreasureType.Open,   10, 0, 3 },
                { 21, "同济玩偶小熊",     TreasureType.Open,   10, 0, 3 },
                { 22, "大疆航拍无人机",   TreasureType.Open,   10, 0, 3 },
                { 23, "都彭打火机",       TreasureType.Open,   10, 0, 3 },
                { 24, "马年纪念币",       TreasureType.Open,   10, 0, 3 },
                { 25, "鎏金卡牌",         TreasureType.Open,   10, 0, 3 },
            };

            for (int i = 0; i < treasureData.GetLength(0); i++)
            {
                int id = (int)treasureData[i, 0];
                string name = (string)treasureData[i, 1];
                var type = (TreasureType)treasureData[i, 2];
                int score = (int)treasureData[i, 3];
                int ability = (int)treasureData[i, 4];
                int rarity = (int)treasureData[i, 5];

                _gameCoreManager.TreasureService.AddTreasure(new TreasureInfo
                {
                    TreasureId = id.ToString(),
                    TreasureName = name,
                    TreasureType = type,
                    DiscoveryMode = type == TreasureType.Search
                                    ? TreasureDiscoveryMode.SearchAtPoint
                                    : TreasureDiscoveryMode.VisibleWhenLitAndClear,
                    ScoreValue = score,
                    Ability = ability,
                    Rarity = rarity,
                    RequiredLightLevel = type == TreasureType.Open ? ConUtil.OpenTreaLightLevel : 0,
                    Description = type == TreasureType.Search ? "搜索型宝藏" : "露天宝藏，需光照充足且能通视",
                    IconPath = System.IO.Path.Combine(Application.StartupPath, "Treasure", id + ".png"),
                    IsCollected = false,
                    ImgName = id + ".png"
                });
            }

            // ── 特殊宝藏 26（开局自动获得）──
            _gameCoreManager.TreasureService.AddTreasure(new TreasureInfo
            {
                TreasureId = "26",
                TreasureName = "神奇望远镜",
                TreasureType = TreasureType.Open,
                DiscoveryMode = TreasureDiscoveryMode.SearchAtPoint,
                ScoreValue = 0,
                Ability = 1,
                Rarity = 4,
                Description = "开局奖励",
                IconPath = System.IO.Path.Combine(Application.StartupPath, "Treasure", "26.png"),
                ImgName = "26.png"
            });

            // ── 特殊宝藏 27（预留）──
            _gameCoreManager.TreasureService.AddTreasure(new TreasureInfo
            {
                TreasureId = "27",
                TreasureName = "作弊胶囊",
                TreasureType = TreasureType.Open,
                DiscoveryMode = TreasureDiscoveryMode.SearchAtPoint,
                ScoreValue = 0,
                Ability = 2,
                Rarity = 4,
                Description = "预留宝藏",
                IconPath = System.IO.Path.Combine(Application.StartupPath, "Treasure", "27.png"),
                ImgName = "27.png"
            });

            // ── 随机分配宝藏到点位，并发放初始宝藏 ──
            //var searchPoints = new System.Collections.Generic.List<InteractionPoint>();
            //var openPoints = new System.Collections.Generic.List<InteractionPoint>();
            //foreach (var id in SearchPointIds)
            //{
            //    var r = _gameCoreManager.InteractionPointService.TryTriggerPoint(DemoPlayer1Id, id);
            //    if (r.Success) searchPoints.Add(r.Data);
            //}
            //foreach (var id in OpenPointIds)
            //{
            //    var r = _gameCoreManager.InteractionPointService.TryTriggerPoint(DemoPlayer1Id, id);
            //    if (r.Success) openPoints.Add(r.Data);
            //}
            _gameCoreManager.InitializeTreasureSystem(searchPoints, openPoints);
            _gameCoreManager.InitializePlayerTreasures(DemoPlayer1Id);
            _gameCoreManager.InitializePlayerTreasures(DemoPlayer2Id);
        }

        private void WriteDispatch(ResponseResult<MessageDispatchResult> result)
        {
            WriteResult(result);
            UpdateMessagePanels(result);
            RefreshSnapshot();
        }

        private void UpdateMessagePanels(ResponseResult<MessageDispatchResult> result)
        {
            var summaryBuilder = new StringBuilder();
            var jsonBuilder = new StringBuilder();

            summaryBuilder.AppendLine("本次业务摘要");
            summaryBuilder.AppendLine("结果：" + result.Message);
            summaryBuilder.AppendLine("成功：" + result.Success);

            if (result.Success && result.Data != null)
            {
                if (result.Data.IncomingMessage != null)
                {
                    summaryBuilder.AppendLine();
                    summaryBuilder.AppendLine("输入消息类型：" + result.Data.IncomingMessage.Header.MessageType);
                    summaryBuilder.AppendLine("输入消息说明：" + DescribeMessageType(result.Data.IncomingMessage.Header.MessageType));

                    var inputJson = _gameCoreManager.DataTransferService.SerializeEnvelope(result.Data.IncomingMessage);
                    if (inputJson.Success)
                    {
                        _lastInputJson = inputJson.Data;
                    }
                }

                summaryBuilder.AppendLine();
                summaryBuilder.AppendLine("输出消息类型列表：");
                if (result.Data.OutgoingMessages.Count == 0)
                {
                    summaryBuilder.AppendLine("无");
                }

                var outputJsonBuilder = new StringBuilder();
                foreach (var envelope in result.Data.OutgoingMessages)
                {
                    summaryBuilder.AppendLine("- " + envelope.Header.MessageType + "：" + DescribeMessageType(envelope.Header.MessageType));
                    var json = _gameCoreManager.DataTransferService.SerializeEnvelope(envelope);
                    if (json.Success)
                    {
                        outputJsonBuilder.AppendLine(json.Data);
                    }

                    // 成就解锁弹窗提示
                    if (envelope.Header.MessageType == MessageType.S2C_ACH)
                    {
                        var ach = envelope.Payload as GISGameFramework.Core.AchievementNotificationPayload;
                        if (ach != null)
                        {
                            ShowSuccess(
                                string.Format("成就解锁：{0}\n\n{1}", ach.Name, ach.Description),
                                "成就解锁");
                        }
                    }
                }

                _lastOutputJson = outputJsonBuilder.ToString();
                summaryBuilder.AppendLine();
                summaryBuilder.AppendLine("核心业务结果：" + result.Data.Detail);
            }

            jsonBuilder.AppendLine("输入消息 JSON");
            jsonBuilder.AppendLine(string.IsNullOrWhiteSpace(_lastInputJson) ? "本步骤无输入 JSON。" : _lastInputJson);
            jsonBuilder.AppendLine();
            jsonBuilder.AppendLine("输出消息 JSON");
            jsonBuilder.AppendLine(string.IsNullOrWhiteSpace(_lastOutputJson) ? "本步骤无输出 JSON。" : _lastOutputJson);

            txtMessageSummary.Text = summaryBuilder.ToString();
            txtJson.Text = jsonBuilder.ToString();
        }

        private void RefreshSnapshot()
        {
            if (!_seeded)
            {
                txtStatus.Text = "尚未加载演示数据。";
                return;
            }

            var snapshotResult = _gameCoreManager.GetDemoSnapshot(DemoPlayer1Id);
            if (!snapshotResult.Success || snapshotResult.Data == null)
            {
                txtStatus.Text = snapshotResult.Message;
                return;
            }

            var snapshot = snapshotResult.Data;
            var builder = new StringBuilder();
            builder.AppendLine("当前玩家状态");
            builder.AppendLine("玩家：" + snapshot.Player.PlayerName + " (" + snapshot.Player.PlayerId + ")");
            builder.AppendLine("状态：" + snapshot.Player.State);
            builder.AppendLine("总分：" + snapshot.Player.TotalScore);
            builder.AppendLine("排名：" + snapshot.Player.CurrentRank);
            builder.AppendLine("当前位置：" + snapshot.Player.CurrentPosition.Latitude + ", " + snapshot.Player.CurrentPosition.Longitude);
            builder.AppendLine();

            builder.AppendLine("当前会话状态");
            if (snapshot.CurrentSession == null)
            {
                builder.AppendLine("暂无活动会话。");
            }
            else
            {
                builder.AppendLine("会话 ID：" + snapshot.CurrentSession.SessionId);
                builder.AppendLine("类型：" + snapshot.CurrentSession.SessionType);
                builder.AppendLine("状态：" + snapshot.CurrentSession.Status);
                builder.AppendLine("当前题号：" + snapshot.CurrentSession.CurrentQuestionIndex);
                builder.AppendLine("玩家 1 得分：" + snapshot.CurrentSession.InitiatorScore);
                builder.AppendLine("玩家 2 得分：" + snapshot.CurrentSession.OpponentScore);
                builder.AppendLine("赢家：" + (string.IsNullOrWhiteSpace(snapshot.CurrentSession.WinnerPlayerId) ? "未决 / 平局" : snapshot.CurrentSession.WinnerPlayerId));
            }
            builder.AppendLine();

            builder.AppendLine("图册与成就");
            builder.AppendLine("图册数量：" + snapshot.Album.Count);
            builder.AppendLine("已解锁成就数：" + snapshot.Achievements.Count);
            foreach (var achievement in snapshot.Achievements)
            {
                builder.AppendLine("- " + achievement.Name);
            }
            builder.AppendLine();

            builder.AppendLine("排行榜");
            foreach (var entry in snapshot.Leaderboard)
            {
                builder.AppendLine(entry.Rank + ". " + entry.PlayerName + " - " + entry.Score);
            }
            builder.AppendLine();

            builder.AppendLine("最近一次 PK 赢家");
            builder.AppendLine(string.IsNullOrWhiteSpace(snapshot.LastPkWinnerId) ? "暂无" : snapshot.LastPkWinnerId);
            builder.AppendLine();
            builder.AppendLine("快照摘要");
            builder.AppendLine(snapshot.SummaryText);

            txtStatus.Text = builder.ToString();

        }

        private void WriteResult<T>(ResponseResult<T> result)
        {
            var builder = new StringBuilder();
            builder.AppendLine("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + result.Message);
            builder.AppendLine("Success=" + result.Success + ", ErrorCode=" + result.ErrorCode);
            if (result.Data != null)
            {
                builder.AppendLine("DataType=" + result.Data.GetType().Name);
            }
            builder.AppendLine();
            txtLog.AppendText(builder.ToString());
        }

        private void AppendLog(string text)
        {
            string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text + Environment.NewLine + Environment.NewLine;
            if (txtLog.InvokeRequired)
                txtLog.Invoke(new Action(() => txtLog.AppendText(line)));
            else
                txtLog.AppendText(line);
        }

        private static string DescribeMessageType(MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.C2S_POS: return "客户端上传位置。";
                case MessageType.C2S_QUIZ_RESULT: return "客户端提交答题结果。";
                case MessageType.C2S_USE_TR: return "客户端请求使用宝藏能力，本版只保留协议位。";
                case MessageType.S2C_ENCOUNTER: return "服务端通知附近玩家相遇或进入 PK。";
                case MessageType.S2C_TREASURE: return "服务端通知宝藏获取结果。";
                case MessageType.S2C_ACH: return "服务端推送成就解锁通知。";
                case MessageType.RANKING_LIST: return "服务端返回最新排行榜。";
                case MessageType.QUIZ_DATA: return "服务端下发题目数据。";
                case MessageType.QUIZ_START: return "服务端通知单人答题开始。";
                case MessageType.PK_START: return "服务端通知 PK 开始。";
                case MessageType.SCORE_CHANGED: return "服务端通知分数变化。";
                case MessageType.NAV_HINT: return "服务端返回方向与距离指引。";
                case MessageType.PK_RESULT: return "服务端返回 PK 结算。";
                case MessageType.SERVER_ACK: return "服务端统一业务回执。";
                default: return "未定义说明。";
            }
        }

        private void OnDistanceTestClicked(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            if (_walker.IsAnyWalking)
            {
                _walker.PauseAll();
                btnStopPosition.Text = "继续移动";
                AppendLog("玩家已暂停移动。");
            }
            else
            {
                _walker.ResumeAll();
                btnStopPosition.Text = "玩家停止移动";
                AppendLog("玩家继续移动。");
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// 把 sunValue(0-100) 同步到界面光照滑块（如果存在）。
        /// </summary>
        private void SyncLightSlider(int sunValue)
        {
            var slider = FindControlByName(this.panelSteps, "trackBarLight") as TrackBar;
            if (slider == null) return;
            slider.Value = Math.Max(slider.Minimum, Math.Min(slider.Maximum, sunValue));
        }

        private static Control FindControlByName(Control parent, string name)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == name) return c;
                var found = FindControlByName(c, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// 光照滑块拖动事件：实时重算 DEM Hillshade。
        /// </summary>
        private void trackBarLight_ValueChanged(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;
            var slider = sender as TrackBar;
            if (slider == null) return;

            int lightLevel = slider.Value;
            var r = _gameCoreManager.UpdateHillshadeFromLightLevel(lightLevel);
            AppendLog(r.Success ? string.Format("Hillshade 已更新（光照 {0}，高度角 {1:F1}°）", lightLevel, 5.0 + lightLevel * 0.55)
                : "Hillshade 更新失败：" + r.Message);
        }

        /// <summary>
        /// 点对点通视分析按钮：以玩家当前位置为观察点，对所有露天宝藏点逐一分析。
        /// </summary>
        private void OnLineOfSightClicked(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            var player = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
            if (!player.Success) { ShowError("无法获取玩家位置。"); return; }

            var allPoints = _gameCoreManager.TreasureService.GetAllTreasurePoints();
            if (!allPoints.Success || allPoints.Data.Count == 0)
            {
                ShowInfo("暂无宝藏点位，请先加载演示数据。");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("观察点：({0:F6}, {1:F6})",
                player.Data.CurrentPosition.Latitude,
                player.Data.CurrentPosition.Longitude));
            sb.AppendLine();

            foreach (var pt in allPoints.Data)
            {
                var tr = _gameCoreManager.TreasureService.GetTreasureByPointId(pt.PointId);
                string name = tr.Success ? tr.Data.TreasureName : pt.PointId;

                var r = _gameCoreManager.CheckLineOfSight(player.Data.CurrentPosition, pt.Position);
                string status = r.Data ? "通视" : "遮挡";
                sb.AppendLine(string.Format("{0}  {1}  ({2})", status, name, r.Message));
                AppendLog(string.Format("通视分析 [{0}] {1}：{2}", pt.PointId, name, r.Message));
            }

            ShowInfo(sb.ToString(), "点对点通视分析结果");
        }
        private void OnViewshedClicked(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            var player = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
            if (!player.Success) { ShowError("无法获取玩家位置。"); return; }

            var allPoints = _gameCoreManager.TreasureService.GetAllTreasurePoints();
            if (!allPoints.Success || allPoints.Data.Count == 0)
            {
                ShowInfo("暂无宝藏点位，请先加载演示数据。");
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(string.Format("观察点：({0:F6}, {1:F6})",
                player.Data.CurrentPosition.Latitude,
                player.Data.CurrentPosition.Longitude));
            sb.AppendLine();

            foreach (var pt in allPoints.Data)
            {
                var tr = _gameCoreManager.TreasureService.GetTreasureByPointId(pt.PointId);
                if (!tr.Success || tr.Data.TreasureType != TreasureType.Open) continue;

                var r = _gameCoreManager.ComputeViewshed(player.Data.CurrentPosition, pt.Position);
                string status = r.Data ? "可视域内" : "不可见";
                sb.AppendLine(string.Format("{0}  {1}  ({2})", status, tr.Data.TreasureName, r.Message));
                AppendLog(string.Format("可视域分析 [{0}] {1}：{2}", pt.PointId, tr.Data.TreasureName, r.Message));
            }

            ShowInfo(sb.ToString(), "可视域分析结果");
        }




        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl1.SelectedIndex == 1)
            {
                this.axMapControl1.Extent = this.axMapControl1.FullExtent;
            }
        }

        private void btnStopPosition_Click(object sender, EventArgs e)
        {
            if (!EnsureReady()) return;

            if (_walker.IsAnyWalking)
            {
                _walker.PauseAll();
                btnStopPosition.Text = "继续移动";
                AppendLog("玩家已暂停移动。");
            }
            else
            {
                _walker.ResumeAll();
                btnStopPosition.Text = "玩家停止移动";
                AppendLog("玩家继续移动。");
            }
        }
        #region 显示区域
        //以下2352025 杨麟烨
        private List<IElement> _tysonElements = new List<IElement>();
        private TysonPolygonTool _tool = new TysonPolygonTool();

        private Dictionary<int, ITextElement> _areaTextElements = new Dictionary<int, ITextElement>();
        private List<IPolygon> _tysonPolygons = new List<IPolygon>();
        private void btnTyson_Click(object sender, EventArgs e)
        {
            if (!EnsureReady())
                return;

            Button btn = sender as Button;

            IMapControl3 mapCtrl = axMapControl1.Object as IMapControl3;
            IActiveView av = mapCtrl.Map as IActiveView;
            IGraphicsContainer gc = mapCtrl.Map as IGraphicsContainer;

            if (_tysonElements.Count > 0)
            {
                foreach (IElement elem in _tysonElements)
                    gc.DeleteElement(elem);
                _tysonElements.Clear();
                _tysonPolygons.Clear();
                _areaTextElements.Clear();
                av.Refresh();
                if (btn != null) btn.Text = "13 显示区域";
                return;
            }

            ISimpleLineSymbol lineSym = new SimpleLineSymbolClass();
            IRgbColor color = new RgbColorClass();
            color.Red = 0;
            color.Green = 0;
            color.Blue = 0;
            lineSym.Color = color;
            lineSym.Width = 1;

            var polygons = _tool.CreateTysonAreas(_tool.GetYour10CenterPoints());
            _tysonPolygons = polygons;

            foreach (IPolygon pg in polygons)
            {
                IPolyline pl = new PolylineClass();
                IPointCollection plPc = pl as IPointCollection;
                IPointCollection pgPc = pg as IPointCollection;
                plPc.AddPointCollection(pgPc);
                ILineElement lineElem = new LineElementClass();
                lineElem.Symbol = lineSym;
                IElement elem = lineElem as IElement;
                elem.Geometry = pl;
                gc.AddElement(elem, 0);
                _tysonElements.Add(elem);
            }

            IRgbColor textColor = new RgbColorClass();
            textColor.Red = 255;
            textColor.Green = 0;
            textColor.Blue = 0;
            ITextSymbol textSymbol = new TextSymbolClass();
            textSymbol.Color = textColor as IColor;
            textSymbol.Size = 16;

            for (int i = 0; i < AreaManager.AllAreas.Count; i++)
            {
                var area = AreaManager.AllAreas[i];
                IPoint centerPoint = new PointClass();
                centerPoint.X = area.CenterLon;
                centerPoint.Y = area.CenterLat;
                ITextElement textElement = new TextElementClass();
                textElement.Text = "???";
                textElement.Symbol = textSymbol;
                IElement element = textElement as IElement;
                element.Geometry = centerPoint;
                IElementProperties props = element as IElementProperties;
                if (props != null)
                    props.Name = "tyson_label";
                gc.AddElement(element, 0);
                _tysonElements.Add(element);
                _areaTextElements[area.AreaId] = textElement;
            }

            UpdateAreaNameVisibility();
            av.Refresh();

            if (btn != null) btn.Text = "13 隐藏区域";
        }
        private bool IsPointInPolygon(IPoint point, IPolygon polygon)
        {
            IRelationalOperator relOp = polygon as IRelationalOperator;
            return relOp.Contains(point);
        }
        private void UpdateAreaNameVisibility()
        {
            if (_areaTextElements.Count == 0)
                return;

            var playerResult = _gameCoreManager.PlayerService.GetPlayer(DemoPlayer1Id);
            if (!playerResult.Success)
                return;

            var player = playerResult.Data;
            GeoPosition playerPos = player.CurrentPosition;
            IPoint playerPoint = new PointClass();
            playerPoint.X = playerPos.Longitude;
            playerPoint.Y = playerPos.Latitude;

            bool playerStateChanged = false;

            for (int i = 0; i < AreaManager.AllAreas.Count && i < _tysonPolygons.Count; i++)
            {
                var area = AreaManager.AllAreas[i];
                var polygon = _tysonPolygons[i];
                ITextElement textElem;
                if (!_areaTextElements.TryGetValue(area.AreaId, out textElem))
                    continue;

                bool inside = IsPointInPolygon(playerPoint, polygon);

                if (inside && !player.ExploredAreaIds.Contains(area.AreaId))
                {
                    player.ExploredAreaIds.Add(area.AreaId);
                    playerStateChanged = true;
                }

                bool explored = player.ExploredAreaIds.Contains(area.AreaId);
                textElem.Text = explored ? area.AreaName : "???";
            }

            if (playerStateChanged)
            {
                _gameCoreManager.PlayerService.UpdatePlayer(player);
            }

            IActiveView av = (axMapControl1.Object as IMapControl3).Map as IActiveView;
            av.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        #endregion

    }
}
