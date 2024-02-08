namespace Slot_Simulator
{
    partial class Simulation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Simulation));
            this.richTextBoxResults = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTrials = new System.Windows.Forms.TextBox();
            this.checkBoxDefaultWinDistributions = new System.Windows.Forms.CheckBox();
            this.checkBoxWinsByPay = new System.Windows.Forms.CheckBox();
            this.checkBoxCustomStats = new System.Windows.Forms.CheckBox();
            this.textBoxStatus = new System.Windows.Forms.TextBox();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.listOfWinsCB = new System.Windows.Forms.CheckBox();
            this.labelBetLevel = new System.Windows.Forms.Label();
            this.tbBetLevel = new System.Windows.Forms.TextBox();
            this.overallCB = new System.Windows.Forms.CheckBox();
            this.todCB = new System.Windows.Forms.CheckBox();
            this.jackpotCB = new System.Windows.Forms.CheckBox();
            this.gameStatsCB = new System.Windows.Forms.CheckBox();
            this.stepthroughCB = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // richTextBoxResults
            // 
            this.richTextBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxResults.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxResults.Location = new System.Drawing.Point(12, 55);
            this.richTextBoxResults.Name = "richTextBoxResults";
            this.richTextBoxResults.ReadOnly = true;
            this.richTextBoxResults.Size = new System.Drawing.Size(980, 489);
            this.richTextBoxResults.TabIndex = 0;
            this.richTextBoxResults.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Trials";
            // 
            // textBoxTrials
            // 
            this.textBoxTrials.Location = new System.Drawing.Point(47, 6);
            this.textBoxTrials.Name = "textBoxTrials";
            this.textBoxTrials.Size = new System.Drawing.Size(73, 20);
            this.textBoxTrials.TabIndex = 2;
            this.textBoxTrials.Text = "1000000000";
            this.textBoxTrials.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // checkBoxDefaultWinDistributions
            // 
            this.checkBoxDefaultWinDistributions.AutoSize = true;
            this.checkBoxDefaultWinDistributions.Checked = true;
            this.checkBoxDefaultWinDistributions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDefaultWinDistributions.Location = new System.Drawing.Point(362, 32);
            this.checkBoxDefaultWinDistributions.Name = "checkBoxDefaultWinDistributions";
            this.checkBoxDefaultWinDistributions.Size = new System.Drawing.Size(142, 17);
            this.checkBoxDefaultWinDistributions.TabIndex = 3;
            this.checkBoxDefaultWinDistributions.Text = "Default Win Distributions";
            this.checkBoxDefaultWinDistributions.UseVisualStyleBackColor = true;
            // 
            // checkBoxWinsByPay
            // 
            this.checkBoxWinsByPay.AutoSize = true;
            this.checkBoxWinsByPay.Checked = true;
            this.checkBoxWinsByPay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxWinsByPay.Location = new System.Drawing.Point(592, 32);
            this.checkBoxWinsByPay.Name = "checkBoxWinsByPay";
            this.checkBoxWinsByPay.Size = new System.Drawing.Size(86, 17);
            this.checkBoxWinsByPay.TabIndex = 4;
            this.checkBoxWinsByPay.Text = "Wins By Pay";
            this.checkBoxWinsByPay.UseVisualStyleBackColor = true;
            // 
            // checkBoxCustomStats
            // 
            this.checkBoxCustomStats.AutoSize = true;
            this.checkBoxCustomStats.Checked = true;
            this.checkBoxCustomStats.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxCustomStats.Location = new System.Drawing.Point(684, 33);
            this.checkBoxCustomStats.Name = "checkBoxCustomStats";
            this.checkBoxCustomStats.Size = new System.Drawing.Size(88, 17);
            this.checkBoxCustomStats.TabIndex = 5;
            this.checkBoxCustomStats.Text = "Custom Stats";
            this.checkBoxCustomStats.UseVisualStyleBackColor = true;
            // 
            // textBoxStatus
            // 
            this.textBoxStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxStatus.Location = new System.Drawing.Point(214, 6);
            this.textBoxStatus.Name = "textBoxStatus";
            this.textBoxStatus.ReadOnly = true;
            this.textBoxStatus.Size = new System.Drawing.Size(535, 20);
            this.textBoxStatus.TabIndex = 6;
            this.textBoxStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonPreview
            // 
            this.buttonPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPreview.Enabled = false;
            this.buttonPreview.Location = new System.Drawing.Point(755, 4);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(75, 23);
            this.buttonPreview.TabIndex = 7;
            this.buttonPreview.Text = "&Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.buttonPreview_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Enabled = false;
            this.buttonExport.Location = new System.Drawing.Point(836, 4);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 8;
            this.buttonExport.Text = "&Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Location = new System.Drawing.Point(917, 4);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 9;
            this.buttonRun.Text = "&Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // listOfWinsCB
            // 
            this.listOfWinsCB.AutoSize = true;
            this.listOfWinsCB.Checked = true;
            this.listOfWinsCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.listOfWinsCB.Location = new System.Drawing.Point(778, 32);
            this.listOfWinsCB.Name = "listOfWinsCB";
            this.listOfWinsCB.Size = new System.Drawing.Size(81, 17);
            this.listOfWinsCB.TabIndex = 10;
            this.listOfWinsCB.Text = "List of Wins";
            this.listOfWinsCB.UseVisualStyleBackColor = true;
            // 
            // labelBetLevel
            // 
            this.labelBetLevel.AutoSize = true;
            this.labelBetLevel.Location = new System.Drawing.Point(126, 9);
            this.labelBetLevel.Name = "labelBetLevel";
            this.labelBetLevel.Size = new System.Drawing.Size(52, 13);
            this.labelBetLevel.TabIndex = 11;
            this.labelBetLevel.Text = "Bet Level";
            // 
            // tbBetLevel
            // 
            this.tbBetLevel.Location = new System.Drawing.Point(184, 6);
            this.tbBetLevel.Name = "tbBetLevel";
            this.tbBetLevel.Size = new System.Drawing.Size(24, 20);
            this.tbBetLevel.TabIndex = 12;
            this.tbBetLevel.Text = "1";
            this.tbBetLevel.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // overallCB
            // 
            this.overallCB.AutoSize = true;
            this.overallCB.Checked = true;
            this.overallCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.overallCB.Location = new System.Drawing.Point(8, 32);
            this.overallCB.Name = "overallCB";
            this.overallCB.Size = new System.Drawing.Size(104, 17);
            this.overallCB.TabIndex = 13;
            this.overallCB.Text = "Overall Statistics";
            this.overallCB.UseVisualStyleBackColor = true;
            // 
            // todCB
            // 
            this.todCB.AutoSize = true;
            this.todCB.Checked = true;
            this.todCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.todCB.Location = new System.Drawing.Point(510, 32);
            this.todCB.Name = "todCB";
            this.todCB.Size = new System.Drawing.Size(76, 17);
            this.todCB.TabIndex = 14;
            this.todCB.Text = "TOD Stats";
            this.todCB.UseVisualStyleBackColor = true;
            // 
            // jackpotCB
            // 
            this.jackpotCB.AutoSize = true;
            this.jackpotCB.Checked = true;
            this.jackpotCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.jackpotCB.Location = new System.Drawing.Point(247, 32);
            this.jackpotCB.Name = "jackpotCB";
            this.jackpotCB.Size = new System.Drawing.Size(109, 17);
            this.jackpotCB.TabIndex = 15;
            this.jackpotCB.Text = "Jackpot Statistics";
            this.jackpotCB.UseVisualStyleBackColor = true;
            // 
            // gameStatsCB
            // 
            this.gameStatsCB.AutoSize = true;
            this.gameStatsCB.Checked = true;
            this.gameStatsCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.gameStatsCB.Location = new System.Drawing.Point(118, 32);
            this.gameStatsCB.Name = "gameStatsCB";
            this.gameStatsCB.Size = new System.Drawing.Size(123, 17);
            this.gameStatsCB.TabIndex = 16;
            this.gameStatsCB.Text = "Full Game Stats Line";
            this.gameStatsCB.UseVisualStyleBackColor = true;
            // 
            // stepthroughCB
            // 
            this.stepthroughCB.AutoSize = true;
            this.stepthroughCB.Location = new System.Drawing.Point(865, 32);
            this.stepthroughCB.Name = "stepthroughCB";
            this.stepthroughCB.Size = new System.Drawing.Size(97, 17);
            this.stepthroughCB.TabIndex = 17;
            this.stepthroughCB.Text = "Step - Through";
            this.stepthroughCB.UseVisualStyleBackColor = true;
            this.stepthroughCB.CheckedChanged += new System.EventHandler(this.stepthroughCB_CheckedChanged);
            // 
            // Simulation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 556);
            this.Controls.Add(this.stepthroughCB);
            this.Controls.Add(this.gameStatsCB);
            this.Controls.Add(this.jackpotCB);
            this.Controls.Add(this.todCB);
            this.Controls.Add(this.overallCB);
            this.Controls.Add(this.tbBetLevel);
            this.Controls.Add(this.labelBetLevel);
            this.Controls.Add(this.listOfWinsCB);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.buttonPreview);
            this.Controls.Add(this.textBoxStatus);
            this.Controls.Add(this.checkBoxCustomStats);
            this.Controls.Add(this.checkBoxWinsByPay);
            this.Controls.Add(this.checkBoxDefaultWinDistributions);
            this.Controls.Add(this.textBoxTrials);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBoxResults);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Simulation";
            this.Text = "Simulation";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSimulation_FormClosing);
            this.Load += new System.EventHandler(this.frmSimulation_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBoxResults;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxTrials;
        private System.Windows.Forms.CheckBox checkBoxDefaultWinDistributions;
        private System.Windows.Forms.CheckBox checkBoxWinsByPay;
        private System.Windows.Forms.CheckBox checkBoxCustomStats;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.CheckBox listOfWinsCB;
        private System.Windows.Forms.Label labelBetLevel;
        private System.Windows.Forms.TextBox tbBetLevel;
        private System.Windows.Forms.CheckBox overallCB;
        private System.Windows.Forms.CheckBox todCB;
        private System.Windows.Forms.CheckBox jackpotCB;
        private System.Windows.Forms.CheckBox gameStatsCB;
        private System.Windows.Forms.CheckBox stepthroughCB;
    }
}