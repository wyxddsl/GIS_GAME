namespace GISGameFramework.WinFormHost
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel layoutRoot;
        private System.Windows.Forms.TableLayoutPanel layoutTop;
        private System.Windows.Forms.GroupBox groupSteps;
        private System.Windows.Forms.FlowLayoutPanel panelSteps;
        private System.Windows.Forms.Button btnInitialize;
        private System.Windows.Forms.Button btnSeed;
        private System.Windows.Forms.Button btnToggleTreasureVisibility;
        private System.Windows.Forms.Button btnPosition;
        private System.Windows.Forms.Button btnStartSoloQuiz;
        private System.Windows.Forms.Button btnSubmitSoloAnswer;
        private System.Windows.Forms.Button btnSearchTreasure;
        private System.Windows.Forms.Button btnOpenTreasure;
        private System.Windows.Forms.Button btnStartPk;
        private System.Windows.Forms.Button btnFinishPk;
        
        private System.Windows.Forms.Button btnRanking;
        private System.Windows.Forms.Button btnNearestTreasureHint;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.GroupBox groupMessage;
        private System.Windows.Forms.TableLayoutPanel layoutMessage;
        private System.Windows.Forms.GroupBox groupMessageSummary;
        private System.Windows.Forms.TextBox txtMessageSummary;
        private System.Windows.Forms.GroupBox groupJson;
        private System.Windows.Forms.TextBox txtJson;
        private System.Windows.Forms.GroupBox groupLog;
        private System.Windows.Forms.TextBox txtLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.layoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.layoutTop = new System.Windows.Forms.TableLayoutPanel();
            this.groupSteps = new System.Windows.Forms.GroupBox();
            this.panelSteps = new System.Windows.Forms.FlowLayoutPanel();
            this.btnInitialize = new System.Windows.Forms.Button();
            this.btnSeed = new System.Windows.Forms.Button();
            this.btnToggleTreasureVisibility = new System.Windows.Forms.Button();
            this.btnPosition = new System.Windows.Forms.Button();
            this.btnStopPosition = new System.Windows.Forms.Button();
            this.btnStartSoloQuiz = new System.Windows.Forms.Button();
            this.btnSubmitSoloAnswer = new System.Windows.Forms.Button();
            this.btnSearchTreasure = new System.Windows.Forms.Button();
            this.btnOpenTreasure = new System.Windows.Forms.Button();
            this.btnStartPk = new System.Windows.Forms.Button();
            this.btnFinishPk = new System.Windows.Forms.Button();
            this.btnRanking = new System.Windows.Forms.Button();
            this.btnNearestTreasureHint = new System.Windows.Forms.Button();
            this.btResetTreasures = new System.Windows.Forms.Button();
            this.btViewKeShiYu = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.btGpsListenOpen = new System.Windows.Forms.Button();
            this.lblHint = new System.Windows.Forms.Label();
            this.groupMessage = new System.Windows.Forms.GroupBox();
            this.layoutMessage = new System.Windows.Forms.TableLayoutPanel();
            this.groupMessageSummary = new System.Windows.Forms.GroupBox();
            this.txtMessageSummary = new System.Windows.Forms.TextBox();
            this.groupJson = new System.Windows.Forms.GroupBox();
            this.txtJson = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.axMapControl1 = new ESRI.ArcGIS.Controls.AxMapControl();
            this.axTOCControl1 = new ESRI.ArcGIS.Controls.AxTOCControl();
            this.axToolbarControl1 = new ESRI.ArcGIS.Controls.AxToolbarControl();
            this.axLicenseControl1 = new ESRI.ArcGIS.Controls.AxLicenseControl();
            this.groupLog = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.btnTyson = new System.Windows.Forms.Button();
            this.layoutRoot.SuspendLayout();
            this.layoutTop.SuspendLayout();
            this.groupSteps.SuspendLayout();
            this.panelSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupMessage.SuspendLayout();
            this.layoutMessage.SuspendLayout();
            this.groupMessageSummary.SuspendLayout();
            this.groupJson.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axMapControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axTOCControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axToolbarControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).BeginInit();
            this.groupLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // layoutRoot
            // 
            this.layoutRoot.ColumnCount = 1;
            this.layoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutRoot.Controls.Add(this.layoutTop, 0, 0);
            this.layoutRoot.Controls.Add(this.groupLog, 0, 1);
            this.layoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutRoot.Location = new System.Drawing.Point(0, 0);
            this.layoutRoot.Margin = new System.Windows.Forms.Padding(6);
            this.layoutRoot.Name = "layoutRoot";
            this.layoutRoot.RowCount = 2;
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 72F));
            this.layoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.layoutRoot.Size = new System.Drawing.Size(2884, 1759);
            this.layoutRoot.TabIndex = 0;
            // 
            // layoutTop
            // 
            this.layoutTop.ColumnCount = 3;
            this.layoutTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 440F));
            this.layoutTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70.35361F));
            this.layoutTop.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.64638F));
            this.layoutTop.Controls.Add(this.groupSteps, 0, 0);
            this.layoutTop.Controls.Add(this.groupMessage, 2, 0);
            this.layoutTop.Controls.Add(this.tabControl1, 1, 0);
            this.layoutTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutTop.Location = new System.Drawing.Point(6, 6);
            this.layoutTop.Margin = new System.Windows.Forms.Padding(6);
            this.layoutTop.Name = "layoutTop";
            this.layoutTop.RowCount = 1;
            this.layoutTop.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutTop.Size = new System.Drawing.Size(2872, 1254);
            this.layoutTop.TabIndex = 0;
            // 
            // groupSteps
            // 
            this.groupSteps.Controls.Add(this.panelSteps);
            this.groupSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupSteps.Location = new System.Drawing.Point(6, 6);
            this.groupSteps.Margin = new System.Windows.Forms.Padding(6);
            this.groupSteps.Name = "groupSteps";
            this.groupSteps.Padding = new System.Windows.Forms.Padding(6);
            this.groupSteps.Size = new System.Drawing.Size(428, 1242);
            this.groupSteps.TabIndex = 0;
            this.groupSteps.TabStop = false;
            this.groupSteps.Text = "推荐演示步骤";
            // 
            // panelSteps
            // 
            this.panelSteps.AutoScroll = true;
            this.panelSteps.Controls.Add(this.btnInitialize);
            this.panelSteps.Controls.Add(this.btnSeed);
            this.panelSteps.Controls.Add(this.btnToggleTreasureVisibility);
            this.panelSteps.Controls.Add(this.btnPosition);
            this.panelSteps.Controls.Add(this.btnStopPosition);
            this.panelSteps.Controls.Add(this.btnStartSoloQuiz);
            this.panelSteps.Controls.Add(this.btnSubmitSoloAnswer);
            this.panelSteps.Controls.Add(this.btnSearchTreasure);
            this.panelSteps.Controls.Add(this.btnOpenTreasure);
            this.panelSteps.Controls.Add(this.btnStartPk);
            this.panelSteps.Controls.Add(this.btnFinishPk);
            this.panelSteps.Controls.Add(this.btnRanking);
            this.panelSteps.Controls.Add(this.btnNearestTreasureHint);
            this.panelSteps.Controls.Add(this.btResetTreasures);
            this.panelSteps.Controls.Add(this.btViewKeShiYu);
            this.panelSteps.Controls.Add(this.button1);
            this.panelSteps.Controls.Add(this.trackBar1);
            this.panelSteps.Controls.Add(this.btGpsListenOpen);
            this.panelSteps.Controls.Add(this.btnTyson);
            this.panelSteps.Controls.Add(this.lblHint);
            this.panelSteps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSteps.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panelSteps.Location = new System.Drawing.Point(6, 34);
            this.panelSteps.Margin = new System.Windows.Forms.Padding(6);
            this.panelSteps.Name = "panelSteps";
            this.panelSteps.Size = new System.Drawing.Size(416, 1202);
            this.panelSteps.TabIndex = 0;
            this.panelSteps.WrapContents = false;
            // 
            // btnInitialize
            // 
            this.btnInitialize.Location = new System.Drawing.Point(6, 6);
            this.btnInitialize.Margin = new System.Windows.Forms.Padding(6);
            this.btnInitialize.Name = "btnInitialize";
            this.btnInitialize.Size = new System.Drawing.Size(360, 72);
            this.btnInitialize.TabIndex = 0;
            this.btnInitialize.Text = "1 初始化框架";
            this.btnInitialize.UseVisualStyleBackColor = true;
            this.btnInitialize.Click += new System.EventHandler(this.OnInitializeClicked);
            // 
            // btnSeed
            // 
            this.btnSeed.Location = new System.Drawing.Point(6, 90);
            this.btnSeed.Margin = new System.Windows.Forms.Padding(6);
            this.btnSeed.Name = "btnSeed";
            this.btnSeed.Size = new System.Drawing.Size(360, 72);
            this.btnSeed.TabIndex = 1;
            this.btnSeed.Text = "2 加载演示数据";
            this.btnSeed.UseVisualStyleBackColor = true;
            this.btnSeed.Click += new System.EventHandler(this.OnSeedClicked);
            // 
            // btnToggleTreasureVisibility
            // 
            this.btnToggleTreasureVisibility.Location = new System.Drawing.Point(6, 174);
            this.btnToggleTreasureVisibility.Margin = new System.Windows.Forms.Padding(6);
            this.btnToggleTreasureVisibility.Name = "btnToggleTreasureVisibility";
            this.btnToggleTreasureVisibility.Size = new System.Drawing.Size(360, 72);
            this.btnToggleTreasureVisibility.TabIndex = 2;
            this.btnToggleTreasureVisibility.Text = "隐藏宝藏";
            this.btnToggleTreasureVisibility.UseVisualStyleBackColor = true;
            this.btnToggleTreasureVisibility.Click += new System.EventHandler(this.OnToggleTreasureVisibilityClicked);
            // 
            // btnPosition
            // 
            this.btnPosition.Location = new System.Drawing.Point(6, 258);
            this.btnPosition.Margin = new System.Windows.Forms.Padding(6);
            this.btnPosition.Name = "btnPosition";
            this.btnPosition.Size = new System.Drawing.Size(360, 72);
            this.btnPosition.TabIndex = 2;
            this.btnPosition.Text = "3 玩家移动";
            this.btnPosition.UseVisualStyleBackColor = true;
            this.btnPosition.Click += new System.EventHandler(this.OnPositionClicked);
            // 
            // btnStopPosition
            // 
            this.btnStopPosition.Location = new System.Drawing.Point(6, 342);
            this.btnStopPosition.Margin = new System.Windows.Forms.Padding(6);
            this.btnStopPosition.Name = "btnStopPosition";
            this.btnStopPosition.Size = new System.Drawing.Size(360, 72);
            this.btnStopPosition.TabIndex = 12;
            this.btnStopPosition.Text = "玩家停止移动";
            this.btnStopPosition.UseVisualStyleBackColor = true;
            this.btnStopPosition.Click += new System.EventHandler(this.btnStopPosition_Click);
            // 
            // btnStartSoloQuiz
            // 
            this.btnStartSoloQuiz.Location = new System.Drawing.Point(6, 426);
            this.btnStartSoloQuiz.Margin = new System.Windows.Forms.Padding(6);
            this.btnStartSoloQuiz.Name = "btnStartSoloQuiz";
            this.btnStartSoloQuiz.Size = new System.Drawing.Size(360, 72);
            this.btnStartSoloQuiz.TabIndex = 3;
            this.btnStartSoloQuiz.Text = "4 开始单人答题";
            this.btnStartSoloQuiz.UseVisualStyleBackColor = true;
            this.btnStartSoloQuiz.Click += new System.EventHandler(this.OnStartSoloQuizClicked);
            // 
            // btnSubmitSoloAnswer
            // 
            this.btnSubmitSoloAnswer.Location = new System.Drawing.Point(6, 510);
            this.btnSubmitSoloAnswer.Margin = new System.Windows.Forms.Padding(6);
            this.btnSubmitSoloAnswer.Name = "btnSubmitSoloAnswer";
            this.btnSubmitSoloAnswer.Size = new System.Drawing.Size(360, 72);
            this.btnSubmitSoloAnswer.TabIndex = 4;
            this.btnSubmitSoloAnswer.Text = "5 提交单人答案";
            this.btnSubmitSoloAnswer.UseVisualStyleBackColor = true;
            this.btnSubmitSoloAnswer.Click += new System.EventHandler(this.OnSubmitSoloAnswerClicked);
            // 
            // btnSearchTreasure
            // 
            this.btnSearchTreasure.Location = new System.Drawing.Point(6, 594);
            this.btnSearchTreasure.Margin = new System.Windows.Forms.Padding(6);
            this.btnSearchTreasure.Name = "btnSearchTreasure";
            this.btnSearchTreasure.Size = new System.Drawing.Size(360, 72);
            this.btnSearchTreasure.TabIndex = 5;
            this.btnSearchTreasure.Text = "6 搜索宝藏";
            this.btnSearchTreasure.UseVisualStyleBackColor = true;
            this.btnSearchTreasure.Click += new System.EventHandler(this.OnSearchTreasureClicked);
            // 
            // btnOpenTreasure
            // 
            this.btnOpenTreasure.Location = new System.Drawing.Point(6, 678);
            this.btnOpenTreasure.Margin = new System.Windows.Forms.Padding(6);
            this.btnOpenTreasure.Name = "btnOpenTreasure";
            this.btnOpenTreasure.Size = new System.Drawing.Size(360, 72);
            this.btnOpenTreasure.TabIndex = 6;
            this.btnOpenTreasure.Text = "7 发现露天宝藏";
            this.btnOpenTreasure.UseVisualStyleBackColor = true;
            this.btnOpenTreasure.Click += new System.EventHandler(this.OnOpenTreasureClicked);
            // 
            // btnStartPk
            // 
            this.btnStartPk.Location = new System.Drawing.Point(6, 762);
            this.btnStartPk.Margin = new System.Windows.Forms.Padding(6);
            this.btnStartPk.Name = "btnStartPk";
            this.btnStartPk.Size = new System.Drawing.Size(360, 72);
            this.btnStartPk.TabIndex = 7;
            this.btnStartPk.Text = "8 开始 PK";
            this.btnStartPk.UseVisualStyleBackColor = true;
            this.btnStartPk.Click += new System.EventHandler(this.OnStartPkClicked);
            // 
            // btnFinishPk
            // 
            this.btnFinishPk.Location = new System.Drawing.Point(6, 846);
            this.btnFinishPk.Margin = new System.Windows.Forms.Padding(6);
            this.btnFinishPk.Name = "btnFinishPk";
            this.btnFinishPk.Size = new System.Drawing.Size(360, 72);
            this.btnFinishPk.TabIndex = 8;
            this.btnFinishPk.Text = "9 完成 PK";
            this.btnFinishPk.UseVisualStyleBackColor = true;
            this.btnFinishPk.Click += new System.EventHandler(this.OnFinishPkClicked);
            // 
            // btnRanking
            // 
            this.btnRanking.Location = new System.Drawing.Point(6, 930);
            this.btnRanking.Margin = new System.Windows.Forms.Padding(6);
            this.btnRanking.Name = "btnRanking";
            this.btnRanking.Size = new System.Drawing.Size(360, 72);
            this.btnRanking.TabIndex = 20;
            this.btnRanking.Text = "11 积分排行榜";
            this.btnRanking.UseVisualStyleBackColor = true;
            this.btnRanking.Click += new System.EventHandler(this.OnShowRankingClicked);
            // 
            // btnNearestTreasureHint
            // 
            this.btnNearestTreasureHint.Location = new System.Drawing.Point(6, 1014);
            this.btnNearestTreasureHint.Margin = new System.Windows.Forms.Padding(6);
            this.btnNearestTreasureHint.Name = "btnNearestTreasureHint";
            this.btnNearestTreasureHint.Size = new System.Drawing.Size(360, 72);
            this.btnNearestTreasureHint.TabIndex = 10;
            this.btnNearestTreasureHint.Text = "11 最邻近分析（技能）";
            this.btnNearestTreasureHint.UseVisualStyleBackColor = true;
            this.btnNearestTreasureHint.Click += new System.EventHandler(this.OnNearestTreasureHintClicked);
            // 
            // btResetTreasures
            // 
            this.btResetTreasures.Location = new System.Drawing.Point(6, 1098);
            this.btResetTreasures.Margin = new System.Windows.Forms.Padding(6);
            this.btResetTreasures.Name = "btResetTreasures";
            this.btResetTreasures.Size = new System.Drawing.Size(360, 72);
            this.btResetTreasures.TabIndex = 11;
            this.btResetTreasures.Text = "12 重新布设宝物";
            this.btResetTreasures.UseVisualStyleBackColor = true;
            this.btResetTreasures.Click += new System.EventHandler(this.btResetTreasures_Click);
            // 
            // btViewKeShiYu
            // 
            this.btViewKeShiYu.Location = new System.Drawing.Point(6, 1182);
            this.btViewKeShiYu.Margin = new System.Windows.Forms.Padding(6);
            this.btViewKeShiYu.Name = "btViewKeShiYu";
            this.btViewKeShiYu.Size = new System.Drawing.Size(360, 72);
            this.btViewKeShiYu.TabIndex = 14;
            this.btViewKeShiYu.Text = "可视域分析";
            this.btViewKeShiYu.UseVisualStyleBackColor = true;
            this.btViewKeShiYu.Click += new System.EventHandler(this.OnViewshedClicked);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 1266);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(360, 72);
            this.button1.TabIndex = 13;
            this.button1.Text = "通视分析";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnLineOfSightClicked);
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(3, 1347);
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(375, 90);
            this.trackBar1.TabIndex = 15;
            this.trackBar1.Visible = false;
            // 
            // btGpsListenOpen
            // 
            this.btGpsListenOpen.Location = new System.Drawing.Point(3, 1443);
            this.btGpsListenOpen.Name = "btGpsListenOpen";
            this.btGpsListenOpen.Size = new System.Drawing.Size(363, 61);
            this.btGpsListenOpen.TabIndex = 21;
            this.btGpsListenOpen.Text = "打开GPS监听";
            this.btGpsListenOpen.UseVisualStyleBackColor = true;
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(6, 1569);
            this.lblHint.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(360, 271);
            this.lblHint.TabIndex = 10;
            this.lblHint.Text = "说明：\r\n1. 本宿主使用固定示例数据。\r\n2. ArcEngine 目前是预留适配层，当前结果为本地占位演示。\r\n3. 手机端协议已经定义，本演示只做本地消息仿" +
    "真。";
            // 
            // groupMessage
            // 
            this.groupMessage.Controls.Add(this.layoutMessage);
            this.groupMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupMessage.Location = new System.Drawing.Point(2156, 6);
            this.groupMessage.Margin = new System.Windows.Forms.Padding(6);
            this.groupMessage.Name = "groupMessage";
            this.groupMessage.Padding = new System.Windows.Forms.Padding(6);
            this.groupMessage.Size = new System.Drawing.Size(710, 1242);
            this.groupMessage.TabIndex = 2;
            this.groupMessage.TabStop = false;
            this.groupMessage.Text = "消息展示";
            // 
            // layoutMessage
            // 
            this.layoutMessage.ColumnCount = 1;
            this.layoutMessage.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutMessage.Controls.Add(this.groupMessageSummary, 0, 0);
            this.layoutMessage.Controls.Add(this.groupJson, 0, 1);
            this.layoutMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutMessage.Location = new System.Drawing.Point(6, 34);
            this.layoutMessage.Margin = new System.Windows.Forms.Padding(6);
            this.layoutMessage.Name = "layoutMessage";
            this.layoutMessage.RowCount = 2;
            this.layoutMessage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.layoutMessage.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F));
            this.layoutMessage.Size = new System.Drawing.Size(698, 1202);
            this.layoutMessage.TabIndex = 0;
            // 
            // groupMessageSummary
            // 
            this.groupMessageSummary.Controls.Add(this.txtMessageSummary);
            this.groupMessageSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupMessageSummary.Location = new System.Drawing.Point(6, 6);
            this.groupMessageSummary.Margin = new System.Windows.Forms.Padding(6);
            this.groupMessageSummary.Name = "groupMessageSummary";
            this.groupMessageSummary.Padding = new System.Windows.Forms.Padding(6);
            this.groupMessageSummary.Size = new System.Drawing.Size(686, 408);
            this.groupMessageSummary.TabIndex = 0;
            this.groupMessageSummary.TabStop = false;
            this.groupMessageSummary.Text = "消息摘要";
            // 
            // txtMessageSummary
            // 
            this.txtMessageSummary.BackColor = System.Drawing.Color.White;
            this.txtMessageSummary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessageSummary.Location = new System.Drawing.Point(6, 34);
            this.txtMessageSummary.Margin = new System.Windows.Forms.Padding(6);
            this.txtMessageSummary.Multiline = true;
            this.txtMessageSummary.Name = "txtMessageSummary";
            this.txtMessageSummary.ReadOnly = true;
            this.txtMessageSummary.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessageSummary.Size = new System.Drawing.Size(674, 368);
            this.txtMessageSummary.TabIndex = 0;
            // 
            // groupJson
            // 
            this.groupJson.Controls.Add(this.txtJson);
            this.groupJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupJson.Location = new System.Drawing.Point(6, 426);
            this.groupJson.Margin = new System.Windows.Forms.Padding(6);
            this.groupJson.Name = "groupJson";
            this.groupJson.Padding = new System.Windows.Forms.Padding(6);
            this.groupJson.Size = new System.Drawing.Size(686, 770);
            this.groupJson.TabIndex = 1;
            this.groupJson.TabStop = false;
            this.groupJson.Text = "输入消息 / 输出消息 JSON";
            // 
            // txtJson
            // 
            this.txtJson.BackColor = System.Drawing.Color.White;
            this.txtJson.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtJson.Location = new System.Drawing.Point(6, 34);
            this.txtJson.Margin = new System.Windows.Forms.Padding(6);
            this.txtJson.Multiline = true;
            this.txtJson.Name = "txtJson";
            this.txtJson.ReadOnly = true;
            this.txtJson.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtJson.Size = new System.Drawing.Size(674, 730);
            this.txtJson.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(443, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1704, 1248);
            this.tabControl1.TabIndex = 3;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtStatus);
            this.tabPage1.Location = new System.Drawing.Point(8, 39);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1688, 1201);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "当前游戏状态";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtStatus
            // 
            this.txtStatus.BackColor = System.Drawing.Color.White;
            this.txtStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStatus.Location = new System.Drawing.Point(3, 3);
            this.txtStatus.Margin = new System.Windows.Forms.Padding(6);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(1682, 1195);
            this.txtStatus.TabIndex = 1;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.axMapControl1);
            this.tabPage2.Controls.Add(this.axTOCControl1);
            this.tabPage2.Controls.Add(this.axToolbarControl1);
            this.tabPage2.Controls.Add(this.axLicenseControl1);
            this.tabPage2.Location = new System.Drawing.Point(8, 39);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1688, 1201);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "地图";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // axMapControl1
            // 
            this.axMapControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axMapControl1.Location = new System.Drawing.Point(219, 31);
            this.axMapControl1.Name = "axMapControl1";
            this.axMapControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axMapControl1.OcxState")));
            this.axMapControl1.Size = new System.Drawing.Size(1466, 1167);
            this.axMapControl1.TabIndex = 4;
            // 
            // axTOCControl1
            // 
            this.axTOCControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.axTOCControl1.Location = new System.Drawing.Point(3, 31);
            this.axTOCControl1.Name = "axTOCControl1";
            this.axTOCControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axTOCControl1.OcxState")));
            this.axTOCControl1.Size = new System.Drawing.Size(216, 1167);
            this.axTOCControl1.TabIndex = 3;
            // 
            // axToolbarControl1
            // 
            this.axToolbarControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.axToolbarControl1.Location = new System.Drawing.Point(3, 3);
            this.axToolbarControl1.Name = "axToolbarControl1";
            this.axToolbarControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axToolbarControl1.OcxState")));
            this.axToolbarControl1.Size = new System.Drawing.Size(1682, 28);
            this.axToolbarControl1.TabIndex = 2;
            // 
            // axLicenseControl1
            // 
            this.axLicenseControl1.Enabled = true;
            this.axLicenseControl1.Location = new System.Drawing.Point(759, -36);
            this.axLicenseControl1.Name = "axLicenseControl1";
            this.axLicenseControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axLicenseControl1.OcxState")));
            this.axLicenseControl1.Size = new System.Drawing.Size(32, 32);
            this.axLicenseControl1.TabIndex = 1;
            // 
            // groupLog
            // 
            this.groupLog.Controls.Add(this.txtLog);
            this.groupLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLog.Location = new System.Drawing.Point(6, 1272);
            this.groupLog.Margin = new System.Windows.Forms.Padding(6);
            this.groupLog.Name = "groupLog";
            this.groupLog.Padding = new System.Windows.Forms.Padding(6);
            this.groupLog.Size = new System.Drawing.Size(2872, 481);
            this.groupLog.TabIndex = 1;
            this.groupLog.TabStop = false;
            this.groupLog.Text = "日志区";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.White;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(6, 34);
            this.txtLog.Margin = new System.Windows.Forms.Padding(6);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(2860, 441);
            this.txtLog.TabIndex = 0;
            // 
            // btnTyson
            // 
            this.btnTyson.Location = new System.Drawing.Point(3, 1510);
            this.btnTyson.Name = "btnTyson";
            this.btnTyson.Size = new System.Drawing.Size(363, 56);
            this.btnTyson.TabIndex = 22;
            this.btnTyson.Text = "13显示区域";
            this.btnTyson.UseVisualStyleBackColor = true;
            this.btnTyson.Click += new System.EventHandler(this.btnTyson_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2884, 1759);
            this.Controls.Add(this.layoutRoot);
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GIS 探索答题游戏框架答辩演示台";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.layoutRoot.ResumeLayout(false);
            this.layoutTop.ResumeLayout(false);
            this.groupSteps.ResumeLayout(false);
            this.panelSteps.ResumeLayout(false);
            this.panelSteps.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupMessage.ResumeLayout(false);
            this.layoutMessage.ResumeLayout(false);
            this.groupMessageSummary.ResumeLayout(false);
            this.groupMessageSummary.PerformLayout();
            this.groupJson.ResumeLayout(false);
            this.groupJson.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.axMapControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axTOCControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axToolbarControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axLicenseControl1)).EndInit();
            this.groupLog.ResumeLayout(false);
            this.groupLog.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.TabPage tabPage2;
        private ESRI.ArcGIS.Controls.AxMapControl axMapControl1;
        private ESRI.ArcGIS.Controls.AxTOCControl axTOCControl1;
        private ESRI.ArcGIS.Controls.AxToolbarControl axToolbarControl1;
        private ESRI.ArcGIS.Controls.AxLicenseControl axLicenseControl1;
        private System.Windows.Forms.Button btResetTreasures;
        private System.Windows.Forms.Button btnStopPosition;
        private System.Windows.Forms.Button btViewKeShiYu;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.Button btGpsListenOpen;
        private System.Windows.Forms.Button btnTyson;
    }
}
