namespace Slot_Simulator
{
    partial class frmStatistics
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmStatistics));
            this.panelChart = new System.Windows.Forms.Panel();
            this.listViewResults = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonPlaySession = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.printDataButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // panelChart
            // 
            this.panelChart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelChart.Location = new System.Drawing.Point(12, 12);
            this.panelChart.Name = "panelChart";
            this.panelChart.Size = new System.Drawing.Size(749, 551);
            this.panelChart.TabIndex = 1;
            this.panelChart.Paint += new System.Windows.Forms.PaintEventHandler(this.panelChart_Paint);
            // 
            // listViewResults
            // 
            this.listViewResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewResults.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewResults.Location = new System.Drawing.Point(767, 12);
            this.listViewResults.Name = "listViewResults";
            this.listViewResults.Size = new System.Drawing.Size(320, 461);
            this.listViewResults.TabIndex = 3;
            this.listViewResults.UseCompatibleStateImageBehavior = false;
            this.listViewResults.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Category";
            this.columnHeader1.Width = 156;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Result";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader2.Width = 134;
            // 
            // buttonPlaySession
            // 
            this.buttonPlaySession.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlaySession.Location = new System.Drawing.Point(767, 479);
            this.buttonPlaySession.Name = "buttonPlaySession";
            this.buttonPlaySession.Size = new System.Drawing.Size(85, 23);
            this.buttonPlaySession.TabIndex = 5;
            this.buttonPlaySession.Text = "&Play Session";
            this.buttonPlaySession.UseVisualStyleBackColor = true;
            this.buttonPlaySession.Click += new System.EventHandler(this.buttonPlaySession_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReset.Location = new System.Drawing.Point(860, 479);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(85, 23);
            this.buttonReset.TabIndex = 6;
            this.buttonReset.Text = "&Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // printDataButton
            // 
            this.printDataButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.printDataButton.Location = new System.Drawing.Point(767, 508);
            this.printDataButton.Name = "printDataButton";
            this.printDataButton.Size = new System.Drawing.Size(85, 23);
            this.printDataButton.TabIndex = 7;
            this.printDataButton.Text = "Print &Data";
            this.printDataButton.UseVisualStyleBackColor = true;
            this.printDataButton.Click += new System.EventHandler(this.printDataButton_Click);
            // 
            // frmStatistics
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1099, 575);
            this.Controls.Add(this.printDataButton);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonPlaySession);
            this.Controls.Add(this.listViewResults);
            this.Controls.Add(this.panelChart);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmStatistics";
            this.Text = "Statistics";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmStatistics_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmStatistics_KeyDown);
            this.Resize += new System.EventHandler(this.frmStatistics_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelChart;
        private System.Windows.Forms.ListView listViewResults;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button buttonPlaySession;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button printDataButton;

    }
}