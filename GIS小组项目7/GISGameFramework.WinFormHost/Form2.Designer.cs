namespace QA
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.lblQuestionTitle = new System.Windows.Forms.Label();
            this.lblQuestionContent = new System.Windows.Forms.Label();
            this.lblQuestionType = new System.Windows.Forms.Label();
            this.lblCurrentScore = new System.Windows.Forms.Label();
            this.lblScoreValue = new System.Windows.Forms.Label();
            this.groupOptions = new System.Windows.Forms.GroupBox();
            this.cbOption4 = new System.Windows.Forms.CheckBox();
            this.cbOption3 = new System.Windows.Forms.CheckBox();
            this.cbOption2 = new System.Windows.Forms.CheckBox();
            this.cbOption1 = new System.Windows.Forms.CheckBox();
            this.rbOption4 = new System.Windows.Forms.RadioButton();
            this.rbOption3 = new System.Windows.Forms.RadioButton();
            this.rbOption2 = new System.Windows.Forms.RadioButton();
            this.rbOption1 = new System.Windows.Forms.RadioButton();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnPrev = new System.Windows.Forms.Button();
            this.lblCorrectAnswer = new System.Windows.Forms.Label();
            this.lblUserAnswer = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupResult = new System.Windows.Forms.GroupBox();
            this.groupOptions.SuspendLayout();
            this.groupResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Excel文件|.xls;.xlsx|所有文件|.";
            this.openFileDialog1.Title = "请选择答题数据库文件";
            // 
            // lblQuestionTitle
            // 
            this.lblQuestionTitle.AutoSize = true;
            this.lblQuestionTitle.Location = new System.Drawing.Point(12, 22);
            this.lblQuestionTitle.Name = "lblQuestionTitle";
            this.lblQuestionTitle.Size = new System.Drawing.Size(107, 18);
            this.lblQuestionTitle.TabIndex = 2;
            this.lblQuestionTitle.Text = "\t第 1 / 0 题";
            // 
            // lblQuestionContent
            // 
            this.lblQuestionContent.Location = new System.Drawing.Point(12, 50);
            this.lblQuestionContent.Name = "lblQuestionContent";
            this.lblQuestionContent.Size = new System.Drawing.Size(833, 61);
            this.lblQuestionContent.TabIndex = 3;
            this.lblQuestionContent.Text = "题目内容将显示在这里";
            // 
            // lblQuestionType
            // 
            this.lblQuestionType.AutoSize = true;
            this.lblQuestionType.ForeColor = System.Drawing.Color.RoyalBlue;
            this.lblQuestionType.Location = new System.Drawing.Point(173, 22);
            this.lblQuestionType.Name = "lblQuestionType";
            this.lblQuestionType.Size = new System.Drawing.Size(98, 18);
            this.lblQuestionType.TabIndex = 4;
            this.lblQuestionType.Text = "【单选题】";
            // 
            // lblCurrentScore
            // 
            this.lblCurrentScore.AutoSize = true;
            this.lblCurrentScore.Location = new System.Drawing.Point(443, 22);
            this.lblCurrentScore.Name = "lblCurrentScore";
            this.lblCurrentScore.Size = new System.Drawing.Size(98, 18);
            this.lblCurrentScore.TabIndex = 5;
            this.lblCurrentScore.Text = "当前得分：";
            // 
            // lblScoreValue
            // 
            this.lblScoreValue.AutoSize = true;
            this.lblScoreValue.ForeColor = System.Drawing.Color.Red;
            this.lblScoreValue.Location = new System.Drawing.Point(547, 22);
            this.lblScoreValue.Name = "lblScoreValue";
            this.lblScoreValue.Size = new System.Drawing.Size(17, 18);
            this.lblScoreValue.TabIndex = 6;
            this.lblScoreValue.Text = "0";
            // 
            // groupOptions
            // 
            this.groupOptions.Controls.Add(this.cbOption4);
            this.groupOptions.Controls.Add(this.cbOption3);
            this.groupOptions.Controls.Add(this.cbOption2);
            this.groupOptions.Controls.Add(this.cbOption1);
            this.groupOptions.Controls.Add(this.rbOption4);
            this.groupOptions.Controls.Add(this.rbOption3);
            this.groupOptions.Controls.Add(this.rbOption2);
            this.groupOptions.Controls.Add(this.rbOption1);
            this.groupOptions.Location = new System.Drawing.Point(27, 114);
            this.groupOptions.Name = "groupOptions";
            this.groupOptions.Size = new System.Drawing.Size(818, 159);
            this.groupOptions.TabIndex = 7;
            this.groupOptions.TabStop = false;
            this.groupOptions.Text = "请选择答案";
            // 
            // cbOption4
            // 
            this.cbOption4.AutoSize = true;
            this.cbOption4.Location = new System.Drawing.Point(403, 108);
            this.cbOption4.Name = "cbOption4";
            this.cbOption4.Size = new System.Drawing.Size(52, 22);
            this.cbOption4.TabIndex = 7;
            this.cbOption4.Text = "D.";
            this.cbOption4.UseVisualStyleBackColor = true;
            this.cbOption4.Visible = false;
            // 
            // cbOption3
            // 
            this.cbOption3.AutoSize = true;
            this.cbOption3.Location = new System.Drawing.Point(28, 108);
            this.cbOption3.Name = "cbOption3";
            this.cbOption3.Size = new System.Drawing.Size(52, 22);
            this.cbOption3.TabIndex = 6;
            this.cbOption3.Text = "C.";
            this.cbOption3.UseVisualStyleBackColor = true;
            this.cbOption3.Visible = false;
            // 
            // cbOption2
            // 
            this.cbOption2.AutoSize = true;
            this.cbOption2.Location = new System.Drawing.Point(404, 56);
            this.cbOption2.Name = "cbOption2";
            this.cbOption2.Size = new System.Drawing.Size(52, 22);
            this.cbOption2.TabIndex = 5;
            this.cbOption2.Text = "B.";
            this.cbOption2.UseVisualStyleBackColor = true;
            this.cbOption2.Visible = false;
            // 
            // cbOption1
            // 
            this.cbOption1.AutoSize = true;
            this.cbOption1.Location = new System.Drawing.Point(28, 57);
            this.cbOption1.Name = "cbOption1";
            this.cbOption1.Size = new System.Drawing.Size(52, 22);
            this.cbOption1.TabIndex = 4;
            this.cbOption1.Text = "A.";
            this.cbOption1.UseVisualStyleBackColor = true;
            this.cbOption1.Visible = false;
            // 
            // rbOption4
            // 
            this.rbOption4.AutoSize = true;
            this.rbOption4.Location = new System.Drawing.Point(404, 108);
            this.rbOption4.Name = "rbOption4";
            this.rbOption4.Size = new System.Drawing.Size(51, 22);
            this.rbOption4.TabIndex = 3;
            this.rbOption4.TabStop = true;
            this.rbOption4.Text = "D.";
            this.rbOption4.UseVisualStyleBackColor = true;
            this.rbOption4.Visible = false;
            // 
            // rbOption3
            // 
            this.rbOption3.AutoSize = true;
            this.rbOption3.Location = new System.Drawing.Point(28, 109);
            this.rbOption3.Name = "rbOption3";
            this.rbOption3.Size = new System.Drawing.Size(51, 22);
            this.rbOption3.TabIndex = 2;
            this.rbOption3.TabStop = true;
            this.rbOption3.Text = "C.";
            this.rbOption3.UseVisualStyleBackColor = true;
            this.rbOption3.Visible = false;
            // 
            // rbOption2
            // 
            this.rbOption2.AutoSize = true;
            this.rbOption2.Location = new System.Drawing.Point(403, 57);
            this.rbOption2.Name = "rbOption2";
            this.rbOption2.Size = new System.Drawing.Size(51, 22);
            this.rbOption2.TabIndex = 1;
            this.rbOption2.TabStop = true;
            this.rbOption2.Text = "B.";
            this.rbOption2.UseVisualStyleBackColor = true;
            this.rbOption2.Visible = false;
            // 
            // rbOption1
            // 
            this.rbOption1.AutoSize = true;
            this.rbOption1.Location = new System.Drawing.Point(28, 56);
            this.rbOption1.Name = "rbOption1";
            this.rbOption1.Size = new System.Drawing.Size(51, 22);
            this.rbOption1.TabIndex = 0;
            this.rbOption1.TabStop = true;
            this.rbOption1.Text = "A.";
            this.rbOption1.UseVisualStyleBackColor = true;
            this.rbOption1.Visible = false;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Location = new System.Drawing.Point(674, 295);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(101, 35);
            this.btnSubmit.TabIndex = 8;
            this.btnSubmit.Text = "提交答案";
            this.btnSubmit.UseVisualStyleBackColor = true;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(502, 355);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(88, 36);
            this.btnNext.TabIndex = 9;
            this.btnNext.Text = "下一题";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(241, 355);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(96, 36);
            this.btnPrev.TabIndex = 10;
            this.btnPrev.Text = "上一题";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // lblCorrectAnswer
            // 
            this.lblCorrectAnswer.AutoSize = true;
            this.lblCorrectAnswer.ForeColor = System.Drawing.Color.Green;
            this.lblCorrectAnswer.Location = new System.Drawing.Point(20, 24);
            this.lblCorrectAnswer.Name = "lblCorrectAnswer";
            this.lblCorrectAnswer.Size = new System.Drawing.Size(98, 18);
            this.lblCorrectAnswer.TabIndex = 12;
            this.lblCorrectAnswer.Text = "正确答案：";
            // 
            // lblUserAnswer
            // 
            this.lblUserAnswer.AutoSize = true;
            this.lblUserAnswer.Location = new System.Drawing.Point(307, 24);
            this.lblUserAnswer.Name = "lblUserAnswer";
            this.lblUserAnswer.Size = new System.Drawing.Size(98, 18);
            this.lblUserAnswer.TabIndex = 13;
            this.lblUserAnswer.Text = "您的答案：";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(0, 422);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(855, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 14;
            // 
            // groupResult
            // 
            this.groupResult.Controls.Add(this.lblCorrectAnswer);
            this.groupResult.Controls.Add(this.lblUserAnswer);
            this.groupResult.Location = new System.Drawing.Point(27, 279);
            this.groupResult.Name = "groupResult";
            this.groupResult.Size = new System.Drawing.Size(579, 54);
            this.groupResult.TabIndex = 15;
            this.groupResult.TabStop = false;
            this.groupResult.Text = "答题结果";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 443);
            this.Controls.Add(this.groupResult);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnPrev);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnSubmit);
            this.Controls.Add(this.groupOptions);
            this.Controls.Add(this.lblScoreValue);
            this.Controls.Add(this.lblCurrentScore);
            this.Controls.Add(this.lblQuestionType);
            this.Controls.Add(this.lblQuestionContent);
            this.Controls.Add(this.lblQuestionTitle);
            this.Name = "Form2";
            this.Text = "答题系统";
            this.groupOptions.ResumeLayout(false);
            this.groupOptions.PerformLayout();
            this.groupResult.ResumeLayout(false);
            this.groupResult.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label lblQuestionTitle;
        private System.Windows.Forms.Label lblQuestionContent;
        private System.Windows.Forms.Label lblQuestionType;
        private System.Windows.Forms.Label lblCurrentScore;
        private System.Windows.Forms.Label lblScoreValue;
        private System.Windows.Forms.GroupBox groupOptions;
        private System.Windows.Forms.CheckBox cbOption4;
        private System.Windows.Forms.CheckBox cbOption3;
        private System.Windows.Forms.CheckBox cbOption2;
        private System.Windows.Forms.CheckBox cbOption1;
        private System.Windows.Forms.RadioButton rbOption4;
        private System.Windows.Forms.RadioButton rbOption3;
        private System.Windows.Forms.RadioButton rbOption2;
        private System.Windows.Forms.RadioButton rbOption1;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Label lblCorrectAnswer;
        private System.Windows.Forms.Label lblUserAnswer;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.GroupBox groupResult;
    }
}