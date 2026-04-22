using System.Drawing;
using System.Windows.Forms;

namespace GISGameFramework.WinFormHost
{
    partial class RankingForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.panelMain       = new Panel();
            this.lblTitle        = new Label();
            this.tabControl      = new TabControl();
            this.tabPageGlobal   = new TabPage();
            this.tabPagePersonal = new TabPage();
            this.tabPageHistory  = new TabPage();
            this.listViewRanking = new ListView();
            this.panelPersonalInfo = new Panel();
            this.btnRefresh      = new Button();
            this.btnClose        = new Button();
            this.lblUpdateTime   = new Label();
            this.pictureBoxTrophy = new PictureBox();
            this.lblPlayerName   = new Label();
            this.lblPlayerScore  = new Label();
            this.lblPlayerRank   = new Label();
            this.lblTodayEarned  = new Label();
            this.lblTodaySpent   = new Label();
            this.listViewHistory = new ListView();

            this.panelMain.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageGlobal.SuspendLayout();
            this.tabPagePersonal.SuspendLayout();
            this.tabPageHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTrophy)).BeginInit();
            this.SuspendLayout();

            // panelMain
            this.panelMain.BackColor = Color.FromArgb(245, 245, 245);
            this.panelMain.BorderStyle = BorderStyle.FixedSingle;
            this.panelMain.Controls.Add(this.lblTitle);
            this.panelMain.Controls.Add(this.tabControl);
            this.panelMain.Controls.Add(this.btnRefresh);
            this.panelMain.Controls.Add(this.btnClose);
            this.panelMain.Controls.Add(this.lblUpdateTime);
            this.panelMain.Dock = DockStyle.Fill;
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new Size(600, 500);
            this.panelMain.TabIndex = 0;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("微软雅黑", 16F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(33, 33, 33);
            this.lblTitle.Location = new Point(20, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Text = "积分排行榜";

            // pictureBoxTrophy
            this.pictureBoxTrophy.BackColor = Color.Gold;
            this.pictureBoxTrophy.Location = new Point(170, 15);
            this.pictureBoxTrophy.Name = "pictureBoxTrophy";
            this.pictureBoxTrophy.Size = new Size(30, 30);
            this.pictureBoxTrophy.TabStop = false;

            // tabControl
            this.tabControl.Controls.Add(this.tabPageGlobal);
            this.tabControl.Controls.Add(this.tabPagePersonal);
            this.tabControl.Controls.Add(this.tabPageHistory);
            this.tabControl.Font = new Font("微软雅黑", 9F);
            this.tabControl.Location = new Point(20, 60);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new Size(560, 380);
            this.tabControl.TabIndex = 2;

            // tabPageGlobal
            this.tabPageGlobal.Controls.Add(this.listViewRanking);
            this.tabPageGlobal.Name = "tabPageGlobal";
            this.tabPageGlobal.Padding = new Padding(3);
            this.tabPageGlobal.Size = new Size(552, 348);
            this.tabPageGlobal.Text = "全球排行";
            this.tabPageGlobal.UseVisualStyleBackColor = true;

            // tabPagePersonal
            this.tabPagePersonal.Controls.Add(this.panelPersonalInfo);
            this.tabPagePersonal.Name = "tabPagePersonal";
            this.tabPagePersonal.Padding = new Padding(3);
            this.tabPagePersonal.Size = new Size(552, 348);
            this.tabPagePersonal.Text = "个人统计";
            this.tabPagePersonal.UseVisualStyleBackColor = true;

            // tabPageHistory
            this.tabPageHistory.Controls.Add(this.listViewHistory);
            this.tabPageHistory.Name = "tabPageHistory";
            this.tabPageHistory.Padding = new Padding(3);
            this.tabPageHistory.Size = new Size(552, 348);
            this.tabPageHistory.Text = "积分历史";
            this.tabPageHistory.UseVisualStyleBackColor = true;

            // listViewRanking
            this.listViewRanking.Dock = DockStyle.Fill;
            this.listViewRanking.FullRowSelect = true;
            this.listViewRanking.GridLines = true;
            this.listViewRanking.Name = "listViewRanking";
            this.listViewRanking.UseCompatibleStateImageBehavior = false;
            this.listViewRanking.View = View.Details;

            // panelPersonalInfo
            this.panelPersonalInfo.BackColor = Color.White;
            this.panelPersonalInfo.BorderStyle = BorderStyle.FixedSingle;
            this.panelPersonalInfo.Controls.Add(this.lblPlayerName);
            this.panelPersonalInfo.Controls.Add(this.lblPlayerScore);
            this.panelPersonalInfo.Controls.Add(this.lblPlayerRank);
            this.panelPersonalInfo.Controls.Add(this.lblTodayEarned);
            this.panelPersonalInfo.Controls.Add(this.lblTodaySpent);
            this.panelPersonalInfo.Dock = DockStyle.Fill;
            this.panelPersonalInfo.Name = "panelPersonalInfo";

            // 个人标签
            this.lblPlayerName.Font = new Font("微软雅黑", 14F, FontStyle.Bold);
            this.lblPlayerName.Location = new Point(20, 20);
            this.lblPlayerName.Size = new Size(500, 30);
            this.lblPlayerName.Text = "玩家: -";

            this.lblPlayerScore.Font = new Font("微软雅黑", 12F);
            this.lblPlayerScore.Location = new Point(20, 60);
            this.lblPlayerScore.Size = new Size(500, 25);
            this.lblPlayerScore.Text = "当前积分: -";

            this.lblPlayerRank.Font = new Font("微软雅黑", 12F);
            this.lblPlayerRank.Location = new Point(20, 95);
            this.lblPlayerRank.Size = new Size(500, 25);
            this.lblPlayerRank.Text = "当前排名: -";

            this.lblTodayEarned.Font = new Font("微软雅黑", 11F);
            this.lblTodayEarned.ForeColor = Color.Green;
            this.lblTodayEarned.Location = new Point(20, 140);
            this.lblTodayEarned.Size = new Size(250, 25);
            this.lblTodayEarned.Text = "今日获得: 0分";

            this.lblTodaySpent.Font = new Font("微软雅黑", 11F);
            this.lblTodaySpent.ForeColor = Color.Red;
            this.lblTodaySpent.Location = new Point(20, 175);
            this.lblTodaySpent.Size = new Size(250, 25);
            this.lblTodaySpent.Text = "今日消耗: 0分";

            // listViewHistory
            this.listViewHistory.Dock = DockStyle.Fill;
            this.listViewHistory.FullRowSelect = true;
            this.listViewHistory.GridLines = true;
            this.listViewHistory.Name = "listViewHistory";
            this.listViewHistory.UseCompatibleStateImageBehavior = false;
            this.listViewHistory.View = View.Details;

            // btnRefresh
            this.btnRefresh.BackColor = Color.FromArgb(33, 150, 243);
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.ForeColor = Color.White;
            this.btnRefresh.Location = new Point(400, 455);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(80, 30);
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = false;

            // btnClose
            this.btnClose.BackColor = Color.FromArgb(244, 67, 54);
            this.btnClose.FlatStyle = FlatStyle.Flat;
            this.btnClose.ForeColor = Color.White;
            this.btnClose.Location = new Point(500, 455);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(80, 30);
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = false;

            // lblUpdateTime
            this.lblUpdateTime.AutoSize = true;
            this.lblUpdateTime.Font = new Font("微软雅黑", 8F);
            this.lblUpdateTime.ForeColor = Color.Gray;
            this.lblUpdateTime.Location = new Point(20, 462);
            this.lblUpdateTime.Name = "lblUpdateTime";
            this.lblUpdateTime.Text = "最后更新: 刚刚";

            // RankingForm
            this.AutoScaleDimensions = new SizeF(6F, 12F);
            this.ClientSize = new Size(600, 500);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.pictureBoxTrophy);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RankingForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "排行榜 - 同济寻宝";

            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageGlobal.ResumeLayout(false);
            this.tabPagePersonal.ResumeLayout(false);
            this.tabPageHistory.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTrophy)).EndInit();
            this.ResumeLayout(false);
        }

        private Panel panelMain;
        private Label lblTitle;
        private TabControl tabControl;
        private TabPage tabPageGlobal;
        private TabPage tabPagePersonal;
        private TabPage tabPageHistory;
        private ListView listViewRanking;
        private Panel panelPersonalInfo;
        private Label lblPlayerName;
        private Label lblPlayerScore;
        private Label lblPlayerRank;
        private Label lblTodayEarned;
        private Label lblTodaySpent;
        private ListView listViewHistory;
        private Button btnRefresh;
        private Button btnClose;
        private Label lblUpdateTime;
        private PictureBox pictureBoxTrophy;
    }
}
