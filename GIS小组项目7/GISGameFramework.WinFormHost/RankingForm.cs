using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GISGameFramework.Core;
using GISGameFramework.Game.Modules;

namespace GISGameFramework.WinFormHost
{
    public partial class RankingForm : Form
    {
        // 从外部拉数据的委托，避免依赖 ScoreManager 单例
        private readonly Func<IList<LeaderboardEntry>> _getRankings;
        private readonly Func<string, PlayerProfile> _getPlayer;
        private readonly string _playerId;

        private readonly List<ScoreRecord> _history = new List<ScoreRecord>();
        private Timer _updateTimer;

        public RankingForm(Func<IList<LeaderboardEntry>> getRankings,
                           Func<string, PlayerProfile> getPlayer,
                           string playerId)
        {
            InitializeComponent();
            _getRankings = getRankings;
            _getPlayer   = getPlayer;
            _playerId    = playerId;

            InitializeData();
            BindEvents();
            StartTimer();
        }

        private void InitializeData()
        {
            listViewRanking.Columns.Add("排名", 60);
            listViewRanking.Columns.Add("玩家", 120);
            listViewRanking.Columns.Add("积分", 80);

            listViewHistory.Columns.Add("时间", 140);
            listViewHistory.Columns.Add("变动", 60);
            listViewHistory.Columns.Add("积分", 80);
            listViewHistory.Columns.Add("原因", 200);

            RefreshRanking();
            RefreshPersonalInfo();
            RefreshHistory();
        }

        private void BindEvents()
        {
            btnRefresh.Click += BtnRefresh_Click;
            btnClose.Click   += BtnClose_Click;
        }

        private void StartTimer()
        {
            _updateTimer = new Timer();
            _updateTimer.Interval = 30000;
            _updateTimer.Tick += (s, e) => RefreshAll();
            _updateTimer.Start();
        }

        // 外部调用：分数变化时通知排行榜刷新并追加历史记录
        public void NotifyScoreChanged(int delta, int newTotal, string reason, string detail = "")
        {
            _history.Add(new ScoreRecord
            {
                ChangeAmount = delta,
                NewTotal     = newTotal,
                Reason       = reason,
                ReasonDetail = detail
            });

            if (_history.Count > 100)
                _history.RemoveAt(0);

            if (this.InvokeRequired)
                this.Invoke(new Action(RefreshAll));
            else
                RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshRanking();
            RefreshPersonalInfo();
            RefreshHistory();
            lblUpdateTime.Text = "最后更新: " + DateTime.Now.ToString("HH:mm:ss");
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RefreshRanking()
        {
            listViewRanking.Items.Clear();

            var rankings = _getRankings();
            if (rankings == null) return;

            foreach (var rank in rankings)
            {
                ListViewItem item = new ListViewItem(rank.Rank.ToString());
                item.SubItems.Add(rank.PlayerName);
                item.SubItems.Add(rank.Score.ToString());

                if (rank.PlayerId == _playerId)
                {
                    item.BackColor = Color.LightYellow;
                    item.Font = new Font(listViewRanking.Font, FontStyle.Bold);
                }

                if (rank.Rank == 1)      item.ForeColor = Color.Gold;
                else if (rank.Rank == 2) item.ForeColor = Color.Silver;
                else if (rank.Rank == 3) item.ForeColor = Color.Brown;

                listViewRanking.Items.Add(item);
            }
        }

        private void RefreshPersonalInfo()
        {
            var player = _getPlayer(_playerId);
            if (player == null) return;

            lblPlayerName.Text  = "玩家: " + player.PlayerName;
            lblPlayerScore.Text = "当前积分: " + player.TotalScore;
            lblPlayerRank.Text  = "当前排名: 第" + player.CurrentRank + "名";

            int earned = 0, spent = 0;
            var today = DateTime.Today;
            foreach (var r in _history)
            {
                if (r.ChangeTime.Date != today) continue;
                if (r.ChangeAmount > 0) earned += r.ChangeAmount;
                else spent += -r.ChangeAmount;
            }
            lblTodayEarned.Text = "今日获得: " + earned + "分";
            lblTodaySpent.Text  = "今日消耗: " + spent + "分";
        }

        private void RefreshHistory()
        {
            listViewHistory.Items.Clear();

            for (int i = _history.Count - 1; i >= 0 && listViewHistory.Items.Count < 30; i--)
            {
                var record = _history[i];
                string sign = record.ChangeAmount > 0 ? "+" : "";
                ListViewItem item = new ListViewItem(record.ChangeTime.ToString("HH:mm:ss"));
                item.SubItems.Add(sign + record.ChangeAmount);
                item.SubItems.Add(record.NewTotal.ToString());
                item.SubItems.Add(string.IsNullOrEmpty(record.ReasonDetail)
                    ? record.Reason
                    : record.Reason + " - " + record.ReasonDetail);

                item.ForeColor = record.ChangeAmount > 0 ? Color.Green : Color.Red;
                listViewHistory.Items.Add(item);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _updateTimer.Stop();
            _updateTimer.Dispose();
            base.OnFormClosing(e);
        }
    }
}
