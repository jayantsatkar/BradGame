namespace Slot_Simulator
{
    partial class frmBatch
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBatch));
            this.buttonThreadPlus = new System.Windows.Forms.Button();
            this.buttonThreadMinus = new System.Windows.Forms.Button();
            this.textBoxThreads = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listViewResults = new System.Windows.Forms.ListView();
            this.columnHeaderGame = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderElapsed = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderLeft = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderProgress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRTP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.textBoxTrials = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxUpdateInterval = new System.Windows.Forms.TextBox();
            this.betLevelLabel = new System.Windows.Forms.Label();
            this.betLevelTB = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonThreadPlus
            // 
            this.buttonThreadPlus.Location = new System.Drawing.Point(124, 4);
            this.buttonThreadPlus.Name = "buttonThreadPlus";
            this.buttonThreadPlus.Size = new System.Drawing.Size(23, 23);
            this.buttonThreadPlus.TabIndex = 19;
            this.buttonThreadPlus.Text = "+";
            this.buttonThreadPlus.UseVisualStyleBackColor = true;
            this.buttonThreadPlus.Click += new System.EventHandler(this.buttonThreadPlus_Click);
            // 
            // buttonThreadMinus
            // 
            this.buttonThreadMinus.Location = new System.Drawing.Point(95, 4);
            this.buttonThreadMinus.Name = "buttonThreadMinus";
            this.buttonThreadMinus.Size = new System.Drawing.Size(23, 23);
            this.buttonThreadMinus.TabIndex = 18;
            this.buttonThreadMinus.Text = "-";
            this.buttonThreadMinus.UseVisualStyleBackColor = true;
            this.buttonThreadMinus.Click += new System.EventHandler(this.buttonThreadMinus_Click);
            // 
            // textBoxThreads
            // 
            this.textBoxThreads.Location = new System.Drawing.Point(64, 6);
            this.textBoxThreads.Name = "textBoxThreads";
            this.textBoxThreads.ReadOnly = true;
            this.textBoxThreads.Size = new System.Drawing.Size(25, 20);
            this.textBoxThreads.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Threads";
            // 
            // listViewResults
            // 
            this.listViewResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderGame,
            this.columnHeaderElapsed,
            this.columnHeaderLeft,
            this.columnHeaderProgress,
            this.columnHeaderRTP});
            this.listViewResults.Location = new System.Drawing.Point(12, 41);
            this.listViewResults.Name = "listViewResults";
            this.listViewResults.Size = new System.Drawing.Size(710, 280);
            this.listViewResults.TabIndex = 20;
            this.listViewResults.UseCompatibleStateImageBehavior = false;
            this.listViewResults.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderGame
            // 
            this.columnHeaderGame.Text = "Game File";
            this.columnHeaderGame.Width = 193;
            // 
            // columnHeaderElapsed
            // 
            this.columnHeaderElapsed.Text = "Elapsed";
            this.columnHeaderElapsed.Width = 124;
            // 
            // columnHeaderLeft
            // 
            this.columnHeaderLeft.Text = "Time Left";
            this.columnHeaderLeft.Width = 128;
            // 
            // columnHeaderProgress
            // 
            this.columnHeaderProgress.Text = "Progress";
            this.columnHeaderProgress.Width = 61;
            // 
            // columnHeaderRTP
            // 
            this.columnHeaderRTP.Text = "RTP";
            this.columnHeaderRTP.Width = 117;
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Location = new System.Drawing.Point(647, 12);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 22;
            this.buttonRun.Text = "&Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Enabled = false;
            this.buttonExport.Location = new System.Drawing.Point(566, 12);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 21;
            this.buttonExport.Text = "&Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // textBoxTrials
            // 
            this.textBoxTrials.Location = new System.Drawing.Point(191, 6);
            this.textBoxTrials.Name = "textBoxTrials";
            this.textBoxTrials.Size = new System.Drawing.Size(73, 20);
            this.textBoxTrials.TabIndex = 24;
            this.textBoxTrials.Text = "1000000000";
            this.textBoxTrials.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(153, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "Trials";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(270, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Update Interval";
            // 
            // textBoxUpdateInterval
            // 
            this.textBoxUpdateInterval.Location = new System.Drawing.Point(356, 6);
            this.textBoxUpdateInterval.Name = "textBoxUpdateInterval";
            this.textBoxUpdateInterval.Size = new System.Drawing.Size(32, 20);
            this.textBoxUpdateInterval.TabIndex = 26;
            this.textBoxUpdateInterval.Text = "60";
            this.textBoxUpdateInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxUpdateInterval.TextChanged += new System.EventHandler(this.textBoxUpdateInterval_TextChanged);
            // 
            // betLevelLabel
            // 
            this.betLevelLabel.AutoSize = true;
            this.betLevelLabel.Location = new System.Drawing.Point(394, 9);
            this.betLevelLabel.Name = "betLevelLabel";
            this.betLevelLabel.Size = new System.Drawing.Size(52, 13);
            this.betLevelLabel.TabIndex = 27;
            this.betLevelLabel.Text = "Bet Level";
            // 
            // betLevelTB
            // 
            this.betLevelTB.Location = new System.Drawing.Point(452, 4);
            this.betLevelTB.Name = "betLevelTB";
            this.betLevelTB.Size = new System.Drawing.Size(32, 20);
            this.betLevelTB.TabIndex = 28;
            this.betLevelTB.Text = "1";
            this.betLevelTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // frmBatch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 333);
            this.Controls.Add(this.betLevelTB);
            this.Controls.Add(this.betLevelLabel);
            this.Controls.Add(this.textBoxUpdateInterval);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxTrials);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.listViewResults);
            this.Controls.Add(this.buttonThreadPlus);
            this.Controls.Add(this.buttonThreadMinus);
            this.Controls.Add(this.textBoxThreads);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmBatch";
            this.Text = "Batch Runs";
            this.Load += new System.EventHandler(this.frmBatch_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonThreadPlus;
        private System.Windows.Forms.Button buttonThreadMinus;
        private System.Windows.Forms.TextBox textBoxThreads;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewResults;
        private System.Windows.Forms.ColumnHeader columnHeaderGame;
        private System.Windows.Forms.ColumnHeader columnHeaderElapsed;
        private System.Windows.Forms.ColumnHeader columnHeaderLeft;
        private System.Windows.Forms.ColumnHeader columnHeaderProgress;
        private System.Windows.Forms.ColumnHeader columnHeaderRTP;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.TextBox textBoxTrials;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxUpdateInterval;
        private System.Windows.Forms.Label betLevelLabel;
        private System.Windows.Forms.TextBox betLevelTB;
    }
}