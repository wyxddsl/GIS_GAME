using System.Collections.Generic;
using System.Text;
using GISGameFramework.Core;
using GISGameFramework.Core.Contracts;
using GISGameFramework.Game.ArcEngine;
using GISGameFramework.Game.Modules;

namespace GISGameFramework.Game
{
    public class GameCoreManager
    {
        // 总控类只负责串联模块，不把业务规则散落到 UI 和传输层。
        public IArcEngineAdapter ArcEngineAdapter { get; private set; }
        public IMapAreaService MapAreaService { get; private set; }
        public IInteractionPointService InteractionPointService { get; private set; }
        public IPlayerService PlayerService { get; private set; }
        public IGpsService GpsService { get; private set; }
        public IQuizService QuizService { get; private set; }
        public ITreasureService TreasureService { get; private set; }
        public IAchievementService AchievementService { get; private set; }
        public IScoreService ScoreService { get; private set; }
        public ILeaderboardService LeaderboardService { get; private set; }
        public IMultiplayerService MultiplayerService { get; private set; }
        public IEnvironmentService EnvironmentService { get; private set; }
        public IDataTransferService DataTransferService { get; private set; }
        public ISkillService SkillService { get; private set; }

        private readonly IList<IGameModule> _modules;
        private readonly IList<string> _recentMessageTypes;
        private readonly IDictionary<string, string> _playerSessionMap;
        private readonly IDictionary<string, IList<AchievementInfo>> _pendingAchievementMessages;
        private string _lastPkWinnerId;

        public GameCoreManager()
        {
            ArcEngineAdapter = new NullArcEngineAdapter();
            PlayerService = new PlayerModule();
            MapAreaService = new MapAreaModule();
            InteractionPointService = new InteractionPointModule();
            ((InteractionPointModule)InteractionPointService).SetCollectedChecker(pointId =>
            {
                var tr = TreasureService.GetTreasureByPointId(pointId);
                return tr.Success && tr.Data.IsCollected;
            });
            GpsService = new GpsModule();
            QuizService = new QuizModule();
            TreasureService = new TreasureModule();
            AchievementService = new AchievementModule();
            ScoreService = new ScoreModule();
            LeaderboardService = new LeaderboardModule();
            MultiplayerService = new MultiplayerModule(PlayerService);
            EnvironmentService = new EnvironmentModule(ArcEngineAdapter);
            DataTransferService = new DataTransferModule(this);
            SkillService = new SkillModule(PlayerService, TreasureService);

            _modules = new List<IGameModule>
            {
                PlayerService,
                MapAreaService,
                InteractionPointService,
                GpsService,
                QuizService,
                TreasureService,
                AchievementService,
                ScoreService,
                LeaderboardService,
                MultiplayerService,
                EnvironmentService,
                DataTransferService,
                SkillService
            };
            _recentMessageTypes = new List<string>();
            _playerSessionMap = new Dictionary<string, string>();
            _pendingAchievementMessages = new Dictionary<string, IList<AchievementInfo>>();
            _lastPkWinnerId = string.Empty;
        }

        public ResponseResult<bool> Initialize()
        {
            foreach (var module in _modules)
            {
                var result = module.Initialize();
                if (!result.Success)
                {
                    return result;
                }
            }

            return ResponseFactory.Ok(true, "Framework initialized.");
        }

        public ResponseResult<bool> Shutdown()
        {
            foreach (var module in _modules)
            {
                var result = module.Shutdown();
                if (!result.Success)
                {
                    return result;
                }
            }

            return ResponseFactory.Ok(true, "Framework closed.");
        }

        /// <summary>
        /// 地图加载完成后，替换为真实的 ArcEngine Adapter 并重建 EnvironmentModule。
        /// </summary>
        public void ReplaceEnvironmentAdapter(IArcEngineAdapter adapter)
        {
            ArcEngineAdapter = adapter;
            EnvironmentService = new EnvironmentModule(adapter);
            for (int i = 0; i < _modules.Count; i++)
            {
                if (_modules[i] is EnvironmentModule)
                {
                    _modules[i] = (IGameModule)EnvironmentService;
                    break;
                }
            }
        }

        /// <summary>
        /// 根据光照值（0-100）换算太阳高度角，调用 ArcEngineAdapter 重算 Hillshade。
        /// </summary>
        //public ResponseResult<bool> UpdateHillshadeFromLightLevel(int lightLevel)
        //{
        //    double altitude = 5.0 + lightLevel * 0.55; // 0->5°, 100->60°
        //    var adapter = ArcEngineAdapter as GISGameFramework.Game.ArcEngine.ArcEngineAdapter;
        //    if (adapter == null)
        //        return ResponseFactory.Fail<bool>(ErrorCodes.InvalidState, "ArcEngine 适配器未初始化，无法重算 Hillshade。");
        //    return adapter.UpdateHillshade(altitude);
        //}

        /// <summary>
        /// 点对点通视分析，委托给 ArcEngineAdapter。
        /// </summary>
        public ResponseResult<bool> CheckLineOfSight(GeoPosition observer, GeoPosition target)
        {
            return ArcEngineAdapter.CheckLineOfSight(observer, target);
        }

        /// <summary>
        /// 可视域分析，委托给 ArcEngineAdapter。
        /// </summary>
        //public ResponseResult<bool> ComputeViewshed(GeoPosition observer, GeoPosition target)
        //{
        //    return ArcEngineAdapter.ComputeViewshed(observer, target);
        //}

        public ResponseResult<PlayerDashboard> GetPlayerDashboard(string playerId)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (!player.Success)
            {
                return ResponseFactory.Fail<PlayerDashboard>(ErrorCodes.NotFound, player.Message);
            }

            return ResponseFactory.Ok(new PlayerDashboard
            {
                Player = player.Data,
                Achievements = GetList(AchievementService.GetUnlocked(playerId)),
                Album = GetList(TreasureService.GetAlbum(playerId)),
                Leaderboard = GetList(LeaderboardService.GetTop(10))
            }, "Player dashboard loaded.");
        }

        public ResponseResult<bool> HandlePlayerPositionUpdate(string playerId, GeoPosition position)
        {
            var normalized = GpsService.NormalizePosition(position);
            if (!normalized.Success)
            {
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidArgument, normalized.Message);
            }

            return PlayerService.UpdatePlayerPosition(playerId, normalized.Data);
        }

        public ResponseResult<QuizSubmissionResult> ProcessQuizSubmission(QuizSubmission submission)
        {
            // 判题留在 QuizModule，这里只处理主流程联动。
            var answer = QuizService.SubmitAnswer(submission);
            if (!answer.Success)
            {
                return answer;
            }

            if (answer.Data.ScoreAwarded > 0)
            {
                ApplyScore(submission.PlayerId, answer.Data.ScoreAwarded, "Quiz reward");
                TriggerAchievement(submission.PlayerId, AchievementTriggerType.FirstQuiz);

                // 积分阈值成就
                var playerAfter = PlayerService.GetPlayer(submission.PlayerId);
                if (playerAfter.Success)
                    TriggerAchievementWithValue(submission.PlayerId, AchievementTriggerType.ScoreThreshold, playerAfter.Data.TotalScore);

                // 答题正确数成就
                var session = QuizService.GetSession(submission.SessionId);
                if (session.Success && session.Data != null)
                {
                    int correctCount = 0;
                    foreach (var rec in session.Data.Answers)
                        if (rec.PlayerId == submission.PlayerId && rec.IsCorrect) correctCount++;
                    TriggerAchievementWithValue(submission.PlayerId, AchievementTriggerType.QuizCorrectCount, correctCount);
                }
            }

            if (answer.Data.IsSessionCompleted)
            {
                var ended = QuizService.EndSession(submission.SessionId);
                if (ended.Success)
                {
                    answer.Data.Session = ended.Data;
                    RestorePlayerState(ended.Data.InitiatorPlayerId);

                    if (!string.IsNullOrWhiteSpace(ended.Data.OpponentPlayerId))
                    {
                        RestorePlayerState(ended.Data.OpponentPlayerId);
                    }

                    if (ended.Data.SessionType == QuizSessionType.PlayerVsPlayer &&
                        !string.IsNullOrWhiteSpace(ended.Data.WinnerPlayerId))
                    {
                        TriggerAchievement(ended.Data.WinnerPlayerId, AchievementTriggerType.PkVictory);
                    }
                }
            }

            return answer;
        }

        public ResponseResult<MessageDispatchResult> ProcessQuizSubmissionFlow(QuizSubmission submission)
        {
            var result = ProcessQuizSubmission(submission);
            if (!result.Success)
            {
                return ResponseFactory.Fail<MessageDispatchResult>((ErrorCodes)result.ErrorCode, result.Message);
            }

            var dispatch = CreateDispatchResult("Quiz submission flow completed.", result);
            dispatch.OutgoingMessages.Add(BuildAck(true, result.Message, result.ErrorCode));

            if (result.Data.ScoreAwarded > 0)
            {
                AddMessages(dispatch, BuildScoreMessages(submission.PlayerId, result.Data.ScoreAwarded, "Quiz reward"));
            }

            AddAchievementMessages(dispatch, ConsumePendingAchievements(submission.PlayerId));

            if (result.Data.IsSessionCompleted &&
                result.Data.Session != null &&
                result.Data.Session.SessionType == QuizSessionType.PlayerVsPlayer)
            {
                AddMessages(dispatch, BuildPkResultMessages(result.Data.Session));

                if (!string.IsNullOrWhiteSpace(result.Data.Session.WinnerPlayerId) &&
                    result.Data.Session.WinnerPlayerId != submission.PlayerId)
                {
                    AddAchievementMessages(dispatch, ConsumePendingAchievements(result.Data.Session.WinnerPlayerId));
                }
            }

            RememberSession(submission.PlayerId, result.Data.Session);
            TrackMessages(dispatch.OutgoingMessages);
            return ResponseFactory.Ok(dispatch, "Quiz submission flow completed.");
        }

        public ResponseResult<MessageDispatchResult> StartSoloQuizFlow(string playerId, string pointId)
        {
            var session = QuizService.StartSoloQuiz(playerId, pointId);
            if (!session.Success)
            {
                return ResponseFactory.Fail<MessageDispatchResult>((ErrorCodes)session.ErrorCode, session.Message);
            }

            SetPlayerState(playerId, PlayerState.InQuiz);

            var dispatch = CreateDispatchResult("Solo quiz started.", session);
            AddMessages(dispatch, BuildOutgoingMessages(playerId, "quiz_start", new QuizStartPayload
            {
                SessionId = session.Data.SessionId,
                PlayerId = playerId,
                QuestionIds = session.Data.QuestionIds
            }));
            AddMessages(dispatch, BuildQuizDataMessages(session.Data.QuestionIds));

            RememberSession(playerId, session.Data);
            TrackMessages(dispatch.OutgoingMessages);
            return ResponseFactory.Ok(dispatch, "Solo quiz flow completed.");
        }

        public ResponseResult<TreasureDiscoveryResult> ProcessOpenTreasureDiscovery(string playerId, string treasureId)
        {
            // 露天宝藏链路：玩家位置 -> 环境可见性 -> 宝藏判定 -> 领取。
            var player = PlayerService.GetPlayer(playerId);
            var treasure = TreasureService.GetTreasure(treasureId);
            if (!player.Success || !treasure.Success)
            {
                return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.NotFound, "Player or treasure not found.");
            }

            var environment = EnvironmentService.EvaluateVisibility(
                player.Data.CurrentPosition,
                treasure.Data.Position,
                treasure.Data.RequiredLightLevel);
            if (!environment.Success)
            {
                return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.InvalidState, environment.Message);
            }

            var visibility = TreasureService.CheckOpenTreasureVisibility(playerId, treasureId, environment.Data);
            if (!visibility.Success)
            {
                return ResponseFactory.Fail<TreasureDiscoveryResult>((ErrorCodes)visibility.ErrorCode, visibility.Message);
            }

            if (!visibility.Data.IsVisible)
            {
                return ResponseFactory.Fail<TreasureDiscoveryResult>(ErrorCodes.InvalidState, visibility.Data.DetailMessage);
            }

            var collect = TreasureService.CollectOpenTreasure(playerId, treasureId);
            if (!collect.Success)
            {
                return collect;
            }

            AddCollectedTreasure(playerId, collect.Data.Treasure.TreasureId);
            ApplyScore(playerId, collect.Data.ScoreAwarded, "Open treasure reward");
            TriggerTreasureAchievements(playerId);
            return collect;
        }

        public ResponseResult<MessageDispatchResult> ProcessOpenTreasureDiscoveryFlow(string playerId, string treasureId)
        {
            var result = ProcessOpenTreasureDiscovery(playerId, treasureId);
            if (!result.Success)
            {
                return ResponseFactory.Fail<MessageDispatchResult>((ErrorCodes)result.ErrorCode, result.Message);
            }

            var dispatch = CreateDispatchResult("Open treasure flow completed.", result);
            AddMessages(dispatch, BuildOutgoingMessages(playerId, "treasure", new TreasureNotificationPayload
            {
                TreasureId = result.Data.Treasure.TreasureId,
                Content = result.Data.Detail,
                Score = result.Data.ScoreAwarded
            }));
            AddMessages(dispatch, BuildScoreMessages(playerId, result.Data.ScoreAwarded, "Open treasure reward"));
            AddAchievementMessages(dispatch, ConsumePendingAchievements(playerId));
            TrackMessages(dispatch.OutgoingMessages);
            return ResponseFactory.Ok(dispatch, "Open treasure flow completed.");
        }

        public ResponseResult<TreasureDiscoveryResult> ProcessTreasureSearch(string playerId, string pointId)
        {
            var result = TreasureService.SearchTreasure(playerId, pointId);
            if (!result.Success)
            {
                return result;
            }

            AddCollectedTreasure(playerId, result.Data.Treasure.TreasureId);
            ApplyScore(playerId, result.Data.ScoreAwarded, "Search treasure reward");
            TriggerTreasureAchievements(playerId);
            return result;
        }

        public ResponseResult<MessageDispatchResult> ProcessTreasureSearchFlow(string playerId, string pointId)
        {
            var result = ProcessTreasureSearch(playerId, pointId);
            if (!result.Success)
            {
                return ResponseFactory.Fail<MessageDispatchResult>((ErrorCodes)result.ErrorCode, result.Message);
            }

            var dispatch = CreateDispatchResult("Search treasure flow completed.", result);
            AddMessages(dispatch, BuildOutgoingMessages(playerId, "treasure", new TreasureNotificationPayload
            {
                TreasureId = result.Data.Treasure.TreasureId,
                Content = result.Data.Detail,
                Score = result.Data.ScoreAwarded
            }));
            AddMessages(dispatch, BuildScoreMessages(playerId, result.Data.ScoreAwarded, "Search treasure reward"));
            AddAchievementMessages(dispatch, ConsumePendingAchievements(playerId));
            TrackMessages(dispatch.OutgoingMessages);
            return ResponseFactory.Ok(dispatch, "Search treasure flow completed.");
        }

        public ResponseResult<QuizSession> TryStartNearbyPk(string playerId)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (!player.Success)
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.NotFound, player.Message);
            }

            if (player.Data.State == PlayerState.InPk)
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.InvalidState, "Player is already in PK.");
            }

            var opponent = MultiplayerService.FindNearbyOpponent(playerId, 10d);
            if (!opponent.Success)
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.NotFound, opponent.Message);
            }

            if (opponent.Data.State == PlayerState.InPk)
            {
                return ResponseFactory.Fail<QuizSession>(ErrorCodes.InvalidState, "Opponent is already in PK.");
            }

            var session = QuizService.StartPkQuiz(playerId, opponent.Data.PlayerId);
            if (!session.Success)
            {
                return session;
            }

            SetPlayerState(playerId, PlayerState.InPk);
            SetPlayerState(opponent.Data.PlayerId, PlayerState.InPk);
            return session;
        }

        public ResponseResult<MessageDispatchResult> TryStartNearbyPkFlow(string playerId)
        {
            var session = TryStartNearbyPk(playerId);
            if (!session.Success)
            {
                return ResponseFactory.Fail<MessageDispatchResult>((ErrorCodes)session.ErrorCode, session.Message);
            }

            var dispatch = CreateDispatchResult("PK flow started.", session);
            AddMessages(dispatch, BuildOutgoingMessages(playerId, "pk_start", new PkStartPayload
            {
                SessionId = session.Data.SessionId,
                Player1Id = session.Data.InitiatorPlayerId,
                Player2Id = session.Data.OpponentPlayerId,
                QuestionIds = session.Data.QuestionIds
            }));
            AddMessages(dispatch, BuildQuizDataMessages(session.Data.QuestionIds));
            AddMessages(dispatch, BuildOutgoingMessages(playerId, "encounter", new EncounterPayload
            {
                TargetPlayerId = session.Data.OpponentPlayerId
            }));

            RememberSession(playerId, session.Data);
            RememberSession(session.Data.OpponentPlayerId, session.Data);
            TrackMessages(dispatch.OutgoingMessages);
            return ResponseFactory.Ok(dispatch, "PK start flow completed.");
        }

        public ResponseResult<InteractionPoint> TryTriggerInteraction(string playerId, string pointId)
        {
            return InteractionPointService.TryTriggerPoint(playerId, pointId);
        }

        public ResponseResult<MessageDispatchResult> RunPositionDemoStep(string playerId)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (!player.Success)
            {
                return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.NotFound, "Demo player not found.");
            }

            var envelope = new MessageEnvelope
            {
                Header = new MessageHeader
                {
                    ClientId = playerId,
                    MessageType = MessageType.C2S_POS
                },
                Payload = new PositionPayload
                {
                    X = player.Data.CurrentPosition.Longitude,
                    Y = player.Data.CurrentPosition.Latitude,
                    Accuracy = player.Data.CurrentPosition.Accuracy
                }
            };

            var result = ProcessIncomingMessage(envelope);
            if (result.Success)
            {
                result.Data.Detail = "演示步骤：玩家进入区域并触发附近点位导航提示。";
            }
            return result;
        }

        public ResponseResult<MessageDispatchResult> RunSoloQuizDemoStep(string playerId, string pointId)
        {
            ResponseResult<MessageDispatchResult> start = null;

            var session = GetActiveSession(playerId, QuizSessionType.Solo);
            if (session == null)
            {
                start = StartSoloQuizFlow(playerId, pointId);
                if (!start.Success)
                {
                    return start;
                }

                session = ExtractSession(start.Data.BusinessResult);
                RememberSession(playerId, session);
            }

            if (session == null)
            {
                return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.InvalidState, "Solo quiz session was not created.");
            }

            var finalDispatch = start != null ? start.Data : CreateDispatchResult("Solo quiz submit demo.", null);
            var questions = QuizService.GetQuestionsForSession(session.SessionId);
            if (!questions.Success || questions.Data.Count == 0)
            {
                return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.NotFound, "No quiz questions available for demo.");
            }

            foreach (var question in questions.Data)
            {
                var submit = ProcessQuizSubmissionFlow(new QuizSubmission
                {
                    SessionId = session.SessionId,
                    PlayerId = playerId,
                    QuestionId = question.QuestionId,
                    Answer = question.AnswerKey
                });
                if (!submit.Success)
                {
                    return submit;
                }

                MergeDispatch(finalDispatch, submit.Data);
            }

            finalDispatch.Detail = "演示步骤：完成单人答题，并同步更新积分、成就和排行榜。";
            return ResponseFactory.Ok(finalDispatch, finalDispatch.Detail);
        }

        public ResponseResult<MessageDispatchResult> RunSearchTreasureDemoStep(string playerId, string pointId)
        {
            var result = ProcessTreasureSearchFlow(playerId, pointId);
            if (result.Success)
            {
                result.Data.Detail = "演示步骤：在搜索点获得搜索型宝藏，并写入收藏图册。";
            }
            return result;
        }

        public ResponseResult<MessageDispatchResult> RunOpenTreasureDemoStep(string playerId, string treasureId)
        {
            var result = ProcessOpenTreasureDiscoveryFlow(playerId, treasureId);
            if (result.Success)
            {
                result.Data.Detail = "演示步骤：满足亮度、距离和视域条件后发现露天宝藏。ArcEngine 目前仍是预留占位。";
            }
            return result;
        }

        public ResponseResult<MessageDispatchResult> RunPkDemoStep(string player1Id, string player2Id)
        {
            ResponseResult<MessageDispatchResult> start = null;

            var session = GetActiveSession(player1Id, QuizSessionType.PlayerVsPlayer);
            if (session == null)
            {
                start = TryStartNearbyPkFlow(player1Id);
                if (!start.Success)
                {
                    return start;
                }

                session = ExtractSession(start.Data.BusinessResult);
            }

            if (session == null)
            {
                return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.InvalidState, "PK session was not created.");
            }

            var finalDispatch = start != null ? start.Data : CreateDispatchResult("Pk submit demo.", null);
            var questions = QuizService.GetQuestionsForSession(session.SessionId);
            if (!questions.Success || questions.Data.Count == 0)
            {
                return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.NotFound, "No PK questions available for demo.");
            }

            // PK 演示固定为 player1 正确、player2 错误，保证答辩结果稳定。
            foreach (var question in questions.Data)
            {
                var submit1 = ProcessQuizSubmissionFlow(new QuizSubmission
                {
                    SessionId = session.SessionId,
                    PlayerId = player1Id,
                    QuestionId = question.QuestionId,
                    Answer = question.AnswerKey
                });
                if (!submit1.Success)
                {
                    return submit1;
                }

                MergeDispatch(finalDispatch, submit1.Data);

                var submit2 = ProcessQuizSubmissionFlow(new QuizSubmission
                {
                    SessionId = session.SessionId,
                    PlayerId = player2Id,
                    QuestionId = question.QuestionId,
                    Answer = "wrong"
                });
                if (!submit2.Success)
                {
                    return submit2;
                }

                MergeDispatch(finalDispatch, submit2.Data);
            }

            finalDispatch.Detail = "演示步骤：完成多人 PK，展示双方得分、赢家、状态恢复和 PK_RESULT 消息。";
            return ResponseFactory.Ok(finalDispatch, finalDispatch.Detail);
        }

        public ResponseResult<DemoSnapshot> GetDemoSnapshot(string playerId)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (!player.Success)
            {
                return ResponseFactory.Fail<DemoSnapshot>(ErrorCodes.NotFound, player.Message);
            }

            var snapshot = new DemoSnapshot
            {
                Player = player.Data,
                Leaderboard = GetList(LeaderboardService.GetTop(10)),
                Achievements = GetList(AchievementService.GetUnlocked(playerId)),
                Album = GetList(TreasureService.GetAlbum(playerId)),
                LastPkWinnerId = _lastPkWinnerId
            };

            if (_playerSessionMap.ContainsKey(playerId))
            {
                var session = QuizService.GetSession(_playerSessionMap[playerId]);
                if (session.Success)
                {
                    snapshot.CurrentSession = session.Data;
                }
            }

            foreach (var type in _recentMessageTypes)
            {
                snapshot.RecentMessageTypes.Add(type);
            }

            snapshot.SummaryText = BuildSnapshotSummary(snapshot);
            return ResponseFactory.Ok(snapshot, "Demo snapshot loaded.");
        }

        public ResponseResult<IList<MessageEnvelope>> BuildOutgoingMessages(string playerId, string eventType, object payload)
        {
            var messages = new List<MessageEnvelope>();

            if (eventType == "position")
            {
                var player = PlayerService.GetPlayer(playerId);
                if (player.Success)
                {
                    var nearby = InteractionPointService.GetNearbyPoints(player.Data.CurrentPosition, 100d);
                    if (nearby.Success)
                    {
                        foreach (var point in nearby.Data)
                        {
                            var hint = InteractionPointService.BuildNavigationHint(player.Data.CurrentPosition, point.PointId);
                            if (hint.Success)
                            {
                                messages.Add(DataTransferService.BuildPushMessage(MessageType.NAV_HINT, new NavigationHintPayload
                                {
                                    TargetId = hint.Data.TargetId,
                                    Distance = hint.Data.Distance,
                                    DirectionText = hint.Data.DirectionText
                                }).Data);
                            }
                        }
                    }
                }
            }
            else if (eventType == "score" && payload is ScoreChangedPayload)
            {
                messages.Add(DataTransferService.BuildPushMessage(MessageType.SCORE_CHANGED, (PayloadBase)payload).Data);
                messages.Add(BuildRankingListMessage());
            }
            else if (eventType == "treasure" && payload is TreasureNotificationPayload)
            {
                messages.Add(DataTransferService.BuildPushMessage(MessageType.S2C_TREASURE, (PayloadBase)payload).Data);
            }
            else if (eventType == "quiz_start" && payload is QuizStartPayload)
            {
                messages.Add(DataTransferService.BuildPushMessage(MessageType.QUIZ_START, (PayloadBase)payload).Data);
            }
            else if (eventType == "pk_start" && payload is PkStartPayload)
            {
                messages.Add(DataTransferService.BuildPushMessage(MessageType.PK_START, (PayloadBase)payload).Data);
            }
            else if (eventType == "pk_result" && payload is PkResultPayload)
            {
                messages.Add(DataTransferService.BuildPushMessage(MessageType.PK_RESULT, (PayloadBase)payload).Data);
            }
            else if (eventType == "encounter" && payload is EncounterPayload)
            {
                messages.Add(DataTransferService.BuildPushMessage(MessageType.S2C_ENCOUNTER, (PayloadBase)payload).Data);
            }
            else if (eventType == "nav_hint" && payload is NavigationHintPayload)
            {
                messages.Add(DataTransferService.BuildPushMessage(MessageType.NAV_HINT, (PayloadBase)payload).Data);
            }

            return ResponseFactory.Ok((IList<MessageEnvelope>)messages, "Outgoing messages built.");
        }

        public ResponseResult<MessageDispatchResult> ProcessIncomingMessage(MessageEnvelope envelope)
        {
            var dispatch = new MessageDispatchResult
            {
                IncomingMessage = envelope,
                Success = true,
                Detail = "Message processed."
            };

            switch (envelope.Header.MessageType)
            {
                case MessageType.C2S_POS:
                    var pos = envelope.Payload as PositionPayload;
                    if (pos == null)
                    {
                        return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.InvalidArgument, "C2S_POS payload mismatch.");
                    }

                    var updated = HandlePlayerPositionUpdate(
                        envelope.Header.ClientId,
                        new GeoPosition { Latitude = pos.Y, Longitude = pos.X, Accuracy = pos.Accuracy });
                    dispatch.BusinessResult = updated;
                    dispatch.OutgoingMessages = GetList(BuildOutgoingMessages(envelope.Header.ClientId, "position", null));
                    dispatch.OutgoingMessages.Add(BuildAck(updated.Success, updated.Message, updated.ErrorCode));
                    break;

                case MessageType.C2S_QUIZ_RESULT:
                    var quiz = envelope.Payload as QuizResultPayload;
                    if (quiz == null)
                    {
                        return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.InvalidArgument, "C2S_QUIZ_RESULT payload mismatch.");
                    }

                    var result = ProcessQuizSubmission(new QuizSubmission
                    {
                        SessionId = quiz.SessionId,
                        PlayerId = quiz.PlayerId,
                        QuestionId = quiz.QuestionId,
                        Answer = quiz.Answer
                    });
                    dispatch.BusinessResult = result;
                    dispatch.OutgoingMessages = new List<MessageEnvelope>
                    {
                        BuildAck(result.Success, result.Message, result.ErrorCode)
                    };

                    if (result.Success && result.Data.ScoreAwarded > 0)
                    {
                        AddMessages(dispatch, BuildScoreMessages(quiz.PlayerId, result.Data.ScoreAwarded, "Quiz reward"));
                    }

                    if (result.Success)
                    {
                        AddAchievementMessages(dispatch, ConsumePendingAchievements(quiz.PlayerId));
                    }

                    if (result.Success &&
                        result.Data.IsSessionCompleted &&
                        result.Data.Session != null &&
                        result.Data.Session.SessionType == QuizSessionType.PlayerVsPlayer)
                    {
                        AddMessages(dispatch, BuildPkResultMessages(result.Data.Session));
                    }
                    break;

                case MessageType.C2S_USE_TR:
                    dispatch.BusinessResult = ResponseFactory.Fail<bool>(ErrorCodes.NotImplemented, "Treasure skill usage is reserved and not implemented.");
                    dispatch.OutgoingMessages = new List<MessageEnvelope>
                    {
                        BuildAck(false, "Treasure skill usage is not supported in this version.", (int)ErrorCodes.NotImplemented)
                    };
                    break;

                default:
                    return ResponseFactory.Fail<MessageDispatchResult>(ErrorCodes.RouteNotFound, "Unsupported message type.");
            }

            TrackMessages(dispatch.OutgoingMessages);
            return ResponseFactory.Ok(dispatch, "Message dispatched.");
        }

        private void TriggerTreasureAchievements(string playerId)
        {
            TriggerAchievement(playerId, AchievementTriggerType.FirstTreasure);
            var album = TreasureService.GetAlbum(playerId);
            if (album.Success && album.Data != null)
                TriggerAchievementWithValue(playerId, AchievementTriggerType.TreasureCollectionCount, album.Data.Count);
        }

        private void TriggerAchievement(string playerId, AchievementTriggerType type)
        {
            var unlocked = AchievementService.Evaluate(playerId, type);
            if (unlocked.Success && unlocked.Data != null && unlocked.Data.Count > 0)
            {
                MergeAchievements(playerId, unlocked.Data);
                QueueAchievements(playerId, unlocked.Data);
            }
        }

        private void TriggerAchievementWithValue(string playerId, AchievementTriggerType type, int value)
        {
            var module = AchievementService as AchievementModule;
            if (module == null) return;
            var unlocked = module.EvaluateWithValue(playerId, type, value);
            if (unlocked.Success && unlocked.Data != null && unlocked.Data.Count > 0)
            {
                MergeAchievements(playerId, unlocked.Data);
                QueueAchievements(playerId, unlocked.Data);
            }
        }

        // 成就消息先放入队列，等本次业务流结束再统一下发。
        private void QueueAchievements(string playerId, IList<AchievementInfo> achievements)
        {
            if (!_pendingAchievementMessages.ContainsKey(playerId))
            {
                _pendingAchievementMessages[playerId] = new List<AchievementInfo>();
            }

            foreach (var achievement in achievements)
            {
                var exists = false;
                foreach (var queued in _pendingAchievementMessages[playerId])
                {
                    if (queued.AchievementId == achievement.AchievementId)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    _pendingAchievementMessages[playerId].Add(achievement);
                }
            }
        }

        private IList<AchievementInfo> ConsumePendingAchievements(string playerId)
        {
            if (!_pendingAchievementMessages.ContainsKey(playerId))
            {
                return new List<AchievementInfo>();
            }

            var list = new List<AchievementInfo>(_pendingAchievementMessages[playerId]);
            _pendingAchievementMessages[playerId].Clear();
            return list;
        }

        private void AddCollectedTreasure(string playerId, string treasureId)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (player.Success && !player.Data.CollectedTreasureIds.Contains(treasureId))
            {
                player.Data.CollectedTreasureIds.Add(treasureId);
                PlayerService.UpdatePlayer(player.Data);
            }
        }

        // ScoreModule 只负责算分，玩家总分和排名回写由总控统一处理。
        private void ApplyScore(string playerId, int delta, string reason)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (!player.Success)
            {
                return;
            }

            var score = ScoreService.ApplyScoreChange(new ScoreChange
            {
                PlayerId = playerId,
                BaseScore = player.Data.TotalScore,
                Delta = delta,
                Reason = reason
            });
            if (score.Success)
            {
                player.Data.TotalScore = score.Data;
                PlayerService.UpdatePlayer(player.Data);
                RefreshLeaderboard();
            }
        }

        private void RefreshLeaderboard()
        {
            var players = PlayerService.GetAllPlayers();
            if (!players.Success)
            {
                return;
            }

            var board = LeaderboardService.Refresh(players.Data);
            if (!board.Success)
            {
                return;
            }

            foreach (var entry in board.Data)
            {
                var player = PlayerService.GetPlayer(entry.PlayerId);
                if (player.Success)
                {
                    player.Data.CurrentRank = entry.Rank;
                    PlayerService.UpdatePlayer(player.Data);
                }
            }
        }

        private ResponseResult<IList<MessageEnvelope>> BuildScoreMessages(string playerId, int delta, string reason)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (!player.Success)
            {
                return ResponseFactory.Fail<IList<MessageEnvelope>>(ErrorCodes.NotFound, player.Message);
            }

            return BuildOutgoingMessages(playerId, "score", new ScoreChangedPayload
            {
                PlayerId = playerId,
                Delta = delta,
                TotalScore = player.Data.TotalScore,
                Reason = reason
            });
        }

        private MessageEnvelope BuildRankingListMessage()
        {
            var payload = new RankingListPayload();
            var top = LeaderboardService.GetTop(10);
            if (top.Success)
            {
                foreach (var entry in top.Data)
                {
                    payload.Items.Add(new RankingListItem
                    {
                        Rank = entry.Rank,
                        UserId = entry.PlayerId,
                        Score = entry.Score
                    });
                }
            }

            return DataTransferService.BuildPushMessage(MessageType.RANKING_LIST, payload).Data;
        }

        private ResponseResult<IList<MessageEnvelope>> BuildPkResultMessages(QuizSession session)
        {
            _lastPkWinnerId = session.WinnerPlayerId ?? string.Empty;
            return BuildOutgoingMessages(session.InitiatorPlayerId, "pk_result", new PkResultPayload
            {
                SessionId = session.SessionId,
                WinnerPlayerId = session.WinnerPlayerId,
                Player1Score = session.InitiatorScore,
                Player2Score = session.OpponentScore,
                IsDraw = string.IsNullOrWhiteSpace(session.WinnerPlayerId)
            });
        }

        private ResponseResult<IList<MessageEnvelope>> BuildQuizDataMessages(IList<string> questionIds)
        {
            var messages = new List<MessageEnvelope>();
            foreach (var id in questionIds)
            {
                var question = QuizService.GetQuestion(id);
                if (!question.Success)
                {
                    continue;
                }

                var payload = new QuizDataPayload
                {
                    QuestionId = question.Data.QuestionId,
                    Content = question.Data.Content,
                    Mode = question.Data.Mode.ToString(),
                    OptionCount = question.Data.Options.Count,
                    AnswerKey = question.Data.AnswerKey
                };
                foreach (var option in question.Data.Options)
                {
                    payload.Options.Add(option);
                }

                messages.Add(DataTransferService.BuildPushMessage(MessageType.QUIZ_DATA, payload).Data);
            }

            return ResponseFactory.Ok((IList<MessageEnvelope>)messages, "Quiz data messages built.");
        }

        private void AddAchievementMessages(MessageDispatchResult dispatch, IList<AchievementInfo> achievements)
        {
            foreach (var achievement in achievements)
            {
                dispatch.OutgoingMessages.Add(DataTransferService.BuildPushMessage(MessageType.S2C_ACH, new AchievementNotificationPayload
                {
                    Name = achievement.Name,
                    Description = achievement.Description,
                    Image = achievement.ImagePath
                }).Data);
            }
        }

        private void MergeAchievements(string playerId, IList<AchievementInfo> achievements)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (!player.Success)
            {
                return;
            }

            foreach (var achievement in achievements)
            {
                if (!player.Data.AchievementIds.Contains(achievement.AchievementId))
                {
                    player.Data.AchievementIds.Add(achievement.AchievementId);
                }
            }

            PlayerService.UpdatePlayer(player.Data);
        }

        private void RestorePlayerState(string playerId)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (player.Success)
            {
                player.Data.State = PlayerState.Exploring;
                PlayerService.UpdatePlayer(player.Data);
            }
        }

        private void SetPlayerState(string playerId, PlayerState state)
        {
            var player = PlayerService.GetPlayer(playerId);
            if (player.Success)
            {
                player.Data.State = state;
                PlayerService.UpdatePlayer(player.Data);
            }
        }

        private void RememberSession(string playerId, QuizSession session)
        {
            if (!string.IsNullOrWhiteSpace(playerId) &&
                session != null &&
                !string.IsNullOrWhiteSpace(session.SessionId))
            {
                _playerSessionMap[playerId] = session.SessionId;
            }
        }

        private QuizSession GetActiveSession(string playerId, QuizSessionType type)
        {
            if (!_playerSessionMap.ContainsKey(playerId))
            {
                return null;
            }

            var session = QuizService.GetSession(_playerSessionMap[playerId]);
            if (!session.Success ||
                session.Data == null ||
                session.Data.SessionType != type ||
                session.Data.IsCompleted)
            {
                return null;
            }

            return session.Data;
        }

        private void TrackMessages(IList<MessageEnvelope> messages)
        {
            foreach (var message in messages)
            {
                if (message == null || message.Header == null)
                {
                    continue;
                }

                var text = message.Header.MessageType.ToString();
                if (_recentMessageTypes.Contains(text))
                {
                    _recentMessageTypes.Remove(text);
                }

                _recentMessageTypes.Add(text);
                while (_recentMessageTypes.Count > 8)
                {
                    _recentMessageTypes.RemoveAt(0);
                }
            }
        }

        private static QuizSession ExtractSession(object businessResult)
        {
            var result = businessResult as ResponseResult<QuizSession>;
            return result != null && result.Success ? result.Data : null;
        }

        private static void MergeDispatch(MessageDispatchResult target, MessageDispatchResult source)
        {
            if (target == null || source == null)
            {
                return;
            }

            target.Success = target.Success && source.Success;
            if (!string.IsNullOrWhiteSpace(source.Detail))
            {
                target.Detail = source.Detail;
            }

            foreach (var message in source.OutgoingMessages)
            {
                target.OutgoingMessages.Add(message);
            }

            target.BusinessResult = source.BusinessResult;
        }

        private static MessageDispatchResult CreateDispatchResult(string detail, object businessResult)
        {
            return new MessageDispatchResult
            {
                Success = true,
                Detail = detail,
                BusinessResult = businessResult
            };
        }

        private static void AddMessages(MessageDispatchResult dispatch, ResponseResult<IList<MessageEnvelope>> result)
        {
            if (result.Success && result.Data != null)
            {
                foreach (var message in result.Data)
                {
                    dispatch.OutgoingMessages.Add(message);
                }
            }
        }

        private MessageEnvelope BuildAck(bool success, string message, int errorCode)
        {
            return DataTransferService.BuildPushMessage(MessageType.SERVER_ACK, new ServerAckPayload
            {
                Success = success,
                Message = message,
                ErrorCode = errorCode
            }).Data;
        }

        private static IList<T> GetList<T>(ResponseResult<IList<T>> result)
        {
            return result.Success && result.Data != null ? result.Data : new List<T>();
        }

        /// <summary>
        /// 初始化宝藏系统：随机分配宝藏到固定点位，并设置初始光照。
        /// 在 Initialize() 和 SeedSampleData() 之后调用。
        /// </summary>
        public ResponseResult<bool> InitializeTreasureSystem(IList<InteractionPoint> searchPoints, IList<InteractionPoint> openTreasurePoints)
        {
            var allocate = TreasureService.RandomAllocateTreasures(searchPoints, openTreasurePoints);
            if (!allocate.Success)
            {
                return allocate;
            }

            var setLight = EnvironmentService.SetGlobalLightLevel(ArcEngine.ConUtil.DefaultLightLevel);
            if (!setLight.Success)
            {
                return setLight;
            }

            return ResponseFactory.Ok(true, "宝藏系统初始化完成，宝藏已随机分配。");
        }

        /// <summary>
        /// 玩家开局时调用：自动奖励初始宝藏(ID:26)。
        /// </summary>
        public ResponseResult<MessageDispatchResult> InitializePlayerTreasures(string playerId)
        {
            var initialTreasure = TreasureService.AwardInitialTreasure(playerId);
            if (!initialTreasure.Success)
            {
                return ResponseFactory.Fail<MessageDispatchResult>((ErrorCodes)initialTreasure.ErrorCode, initialTreasure.Message);
            }

            AddCollectedTreasure(playerId, initialTreasure.Data.Treasure.TreasureId);
            ApplyScore(playerId, initialTreasure.Data.ScoreAwarded, "Initial treasure");
            TriggerTreasureAchievements(playerId);

            var dispatch = CreateDispatchResult("开局初始宝藏已发放。", initialTreasure);
            AddMessages(dispatch, BuildOutgoingMessages(playerId, "treasure", new TreasureNotificationPayload
            {
                TreasureId = initialTreasure.Data.Treasure.TreasureId,
                Content = initialTreasure.Data.Detail,
                Score = initialTreasure.Data.ScoreAwarded
            }));
            AddMessages(dispatch, BuildScoreMessages(playerId, initialTreasure.Data.ScoreAwarded, "Initial treasure"));
            TrackMessages(dispatch.OutgoingMessages);
            return ResponseFactory.Ok(dispatch, "Initial treasure flow completed.");
        }

        /// <summary>
        /// 最邻近分析技能流程：找到最近藏宝点，返回方向和距离提示。
        /// </summary>
        public ResponseResult<MessageDispatchResult> UseNearestTreasureHintFlow(string playerId)
        {
            var result = SkillService.UseNearestTreasureHint(playerId);
            if (!result.Success)
            {
                return ResponseFactory.Fail<MessageDispatchResult>((ErrorCodes)result.ErrorCode, result.Message);
            }

            var dispatch = CreateDispatchResult(result.Data.Message, result);
            AddMessages(dispatch, BuildOutgoingMessages(playerId, "nav_hint", new NavigationHintPayload
            {
                TargetId = result.Data.NearestTreasureHint.TargetId,
                Distance = result.Data.NearestTreasureHint.Distance,
                DirectionText = result.Data.NearestTreasureHint.DirectionText
            }));
            TrackMessages(dispatch.OutgoingMessages);
            return ResponseFactory.Ok(dispatch, result.Data.Message);
        }

        /// <summary>
        /// 推进全局光照周期（可由 UI 定时器调用）。
        /// </summary>
        public ResponseResult<bool> UpdateGlobalLighting()
        {
            return EnvironmentService.UpdateLightingCycle();
        }

        /// <summary>
        /// 层次2：把当前全局光照等级（0-100）映射到太阳高度角（5-60°），
        /// 调用 ArcEngineAdapter.UpdateHillshade 重算 DEM Hillshade，
        /// 并同步更新 EnvironmentModule 的光照采样缓存。
        /// 由 UI 光照滑块或定时器触发。
        /// </summary>
        public ResponseResult<bool> UpdateHillshadeFromLightLevel(int lightLevel)
        {
            // 先更新 EnvironmentModule 的全局光照值
            var setResult = EnvironmentService.SetGlobalLightLevel(lightLevel);
            if (!setResult.Success) return setResult;

            // 光照等级 0-100 → 太阳高度角 5-60°
            double altitude = 5.0 + lightLevel * 0.55;

            var adapter = ArcEngineAdapter as ArcEngineAdapter;
            if (adapter == null)
                return ResponseFactory.Fail<bool>(ErrorCodes.InvalidState, "当前 Adapter 不支持 UpdateHillshade（未接入 ArcEngine）。");

            return adapter.UpdateHillshade(altitude);
        }

        /// <summary>
        /// 点对点通视分析，直接暴露给 UI 调用。
        /// </summary>
        //public ResponseResult<bool> CheckLineOfSight(GeoPosition observer, GeoPosition target)
        //{
        //    return ArcEngineAdapter.CheckLineOfSight(observer, target);
        //}

        /// <summary>
        /// 可视域分析：判断 target 是否在 observer 的可视域内。
        /// </summary>
        public ResponseResult<bool> ComputeViewshed(GeoPosition observer, GeoPosition target)
        {
            var adapter = ArcEngineAdapter as ArcEngineAdapter;
            if (adapter == null)
                return ResponseFactory.Ok(true, "当前 Adapter 不支持可视域分析，默认可见。");
            return adapter.ComputeViewshed(observer, target);
        }

        /// <summary>
        /// 重新布设宝物：重置所有宝藏收集状态，并用新的随机点位重新分配。
        /// 由 UI "重新布设宝物" 按钮调用。
        /// </summary>
        public ResponseResult<bool> RedeployTreasures(IList<InteractionPoint> newSearchPoints, IList<InteractionPoint> newOpenPoints)
        {
            // 重置宝藏收集状态
            TreasureService.ResetAllTreasures();

            // 用新点位重新随机分配
            return InitializeTreasureSystem(newSearchPoints, newOpenPoints);
        }

        private static string BuildSnapshotSummary(DemoSnapshot snapshot)
        {
            var builder = new StringBuilder();
            builder.Append("玩家=");
            builder.Append(snapshot.Player != null ? snapshot.Player.PlayerName : "未知");
            builder.Append("，总分=");
            builder.Append(snapshot.Player != null ? snapshot.Player.TotalScore.ToString() : "0");
            builder.Append("，排名=");
            builder.Append(snapshot.Player != null ? snapshot.Player.CurrentRank.ToString() : "0");
            builder.Append("，成就=");
            builder.Append(snapshot.Achievements.Count);
            builder.Append("，图册=");
            builder.Append(snapshot.Album.Count);

            if (snapshot.CurrentSession != null)
            {
                builder.Append("，当前会话=");
                builder.Append(snapshot.CurrentSession.SessionType);
                builder.Append("/");
                builder.Append(snapshot.CurrentSession.Status);
            }

            if (!string.IsNullOrWhiteSpace(snapshot.LastPkWinnerId))
            {
                builder.Append("，最近 PK 赢家=");
                builder.Append(snapshot.LastPkWinnerId);
            }

            if (snapshot.RecentMessageTypes.Count > 0)
            {
                builder.Append("，最近消息=");
                for (int i = 0; i < snapshot.RecentMessageTypes.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(",");
                    }
                    builder.Append(snapshot.RecentMessageTypes[i]);
                }
            }

            return builder.ToString();
        }
    }
}
