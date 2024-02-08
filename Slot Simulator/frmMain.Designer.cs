namespace Slot_Simulator
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuItemRun = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemRunSimulation = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemRunBingo = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemRunBatch = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemWindows = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemWindowsStatistics = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItemWindowsMaximize = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemWindowsRestore = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCoinIn = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCoinInSet40xBet = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCoinIn20 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCoinIn50 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCoinIn100 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCoinIn500 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCoinIn1000 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemCoinInCashOut = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemOptionsZipPlay = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemOptionsAutoPlay = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMisc = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMiscResetStatistics = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemMiscNewSession = new System.Windows.Forms.ToolStripMenuItem();
            this.soundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.muteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bankSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bS1ToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bS2ToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bS3ToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bS4ToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bS5ToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bS6ToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bS8ToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bS10ToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameMessageBox = new System.Windows.Forms.TextBox();
            this.creditsTB = new System.Windows.Forms.TextBox();
            this.betTB = new System.Windows.Forms.TextBox();
            this.winTB = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemRun,
            this.menuItemWindows,
            this.menuItemCoinIn,
            this.menuItemOptions,
            this.menuItemMisc,
            this.soundToolStripMenuItem,
            this.bankSizeToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1455, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuItemRun
            // 
            this.menuItemRun.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemRunSimulation,
            this.menuItemRunBingo,
            this.menuItemRunBatch});
            this.menuItemRun.Name = "menuItemRun";
            this.menuItemRun.Size = new System.Drawing.Size(40, 20);
            this.menuItemRun.Text = "&Run";
            // 
            // menuItemRunSimulation
            // 
            this.menuItemRunSimulation.Name = "menuItemRunSimulation";
            this.menuItemRunSimulation.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuItemRunSimulation.Size = new System.Drawing.Size(200, 22);
            this.menuItemRunSimulation.Text = "&Simulation";
            this.menuItemRunSimulation.Click += new System.EventHandler(this.menuItemRunSimulation_Click);
            // 
            // menuItemRunBingo
            // 
            this.menuItemRunBingo.Name = "menuItemRunBingo";
            this.menuItemRunBingo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.menuItemRunBingo.Size = new System.Drawing.Size(200, 22);
            this.menuItemRunBingo.Text = "&Time On Device";
            this.menuItemRunBingo.Click += new System.EventHandler(this.menuItemRunBingo_Click);
            // 
            // menuItemRunBatch
            // 
            this.menuItemRunBatch.Name = "menuItemRunBatch";
            this.menuItemRunBatch.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.menuItemRunBatch.Size = new System.Drawing.Size(200, 22);
            this.menuItemRunBatch.Text = "&Batch";
            this.menuItemRunBatch.Click += new System.EventHandler(this.menuItemRunBatch_Click);
            // 
            // menuItemWindows
            // 
            this.menuItemWindows.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemWindowsStatistics,
            this.toolStripSeparator1,
            this.menuItemWindowsMaximize,
            this.menuItemWindowsRestore});
            this.menuItemWindows.Name = "menuItemWindows";
            this.menuItemWindows.Size = new System.Drawing.Size(68, 20);
            this.menuItemWindows.Text = "&Windows";
            // 
            // menuItemWindowsStatistics
            // 
            this.menuItemWindowsStatistics.Name = "menuItemWindowsStatistics";
            this.menuItemWindowsStatistics.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.menuItemWindowsStatistics.Size = new System.Drawing.Size(169, 22);
            this.menuItemWindowsStatistics.Text = "&Statistics";
            this.menuItemWindowsStatistics.Click += new System.EventHandler(this.menuItemWindowsStatistics_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(166, 6);
            // 
            // menuItemWindowsMaximize
            // 
            this.menuItemWindowsMaximize.Name = "menuItemWindowsMaximize";
            this.menuItemWindowsMaximize.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.menuItemWindowsMaximize.Size = new System.Drawing.Size(169, 22);
            this.menuItemWindowsMaximize.Text = "&Maximize";
            this.menuItemWindowsMaximize.Click += new System.EventHandler(this.menuItemWindowsMaximize_Click);
            // 
            // menuItemWindowsRestore
            // 
            this.menuItemWindowsRestore.Name = "menuItemWindowsRestore";
            this.menuItemWindowsRestore.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.menuItemWindowsRestore.Size = new System.Drawing.Size(169, 22);
            this.menuItemWindowsRestore.Text = "&Restore";
            this.menuItemWindowsRestore.Click += new System.EventHandler(this.menuItemWindowsRestore_Click);
            // 
            // menuItemCoinIn
            // 
            this.menuItemCoinIn.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemCoinInSet40xBet,
            this.menuItemCoinIn20,
            this.menuItemCoinIn50,
            this.menuItemCoinIn100,
            this.menuItemCoinIn500,
            this.menuItemCoinIn1000,
            this.menuItemCoinInCashOut});
            this.menuItemCoinIn.Name = "menuItemCoinIn";
            this.menuItemCoinIn.Size = new System.Drawing.Size(57, 20);
            this.menuItemCoinIn.Text = "&Coin In";
            // 
            // menuItemCoinInSet40xBet
            // 
            this.menuItemCoinInSet40xBet.Name = "menuItemCoinInSet40xBet";
            this.menuItemCoinInSet40xBet.Size = new System.Drawing.Size(163, 22);
            this.menuItemCoinInSet40xBet.Text = "Reset Buy In";
            this.menuItemCoinInSet40xBet.Click += new System.EventHandler(this.menuItemResetBuyIn_Click);
            // 
            // menuItemCoinIn20
            // 
            this.menuItemCoinIn20.Name = "menuItemCoinIn20";
            this.menuItemCoinIn20.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D1)));
            this.menuItemCoinIn20.Size = new System.Drawing.Size(163, 22);
            this.menuItemCoinIn20.Text = "$20";
            this.menuItemCoinIn20.Click += new System.EventHandler(this.menuItemCoinIn_Click);
            // 
            // menuItemCoinIn50
            // 
            this.menuItemCoinIn50.Name = "menuItemCoinIn50";
            this.menuItemCoinIn50.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D2)));
            this.menuItemCoinIn50.Size = new System.Drawing.Size(163, 22);
            this.menuItemCoinIn50.Text = "$50";
            this.menuItemCoinIn50.Click += new System.EventHandler(this.menuItemCoinIn_Click);
            // 
            // menuItemCoinIn100
            // 
            this.menuItemCoinIn100.Name = "menuItemCoinIn100";
            this.menuItemCoinIn100.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D3)));
            this.menuItemCoinIn100.Size = new System.Drawing.Size(163, 22);
            this.menuItemCoinIn100.Text = "$100";
            this.menuItemCoinIn100.Click += new System.EventHandler(this.menuItemCoinIn_Click);
            // 
            // menuItemCoinIn500
            // 
            this.menuItemCoinIn500.Name = "menuItemCoinIn500";
            this.menuItemCoinIn500.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D4)));
            this.menuItemCoinIn500.Size = new System.Drawing.Size(163, 22);
            this.menuItemCoinIn500.Text = "$500";
            this.menuItemCoinIn500.Click += new System.EventHandler(this.menuItemCoinIn_Click);
            // 
            // menuItemCoinIn1000
            // 
            this.menuItemCoinIn1000.Name = "menuItemCoinIn1000";
            this.menuItemCoinIn1000.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D5)));
            this.menuItemCoinIn1000.Size = new System.Drawing.Size(163, 22);
            this.menuItemCoinIn1000.Text = "$1000";
            this.menuItemCoinIn1000.Click += new System.EventHandler(this.menuItemCoinIn_Click);
            // 
            // menuItemCoinInCashOut
            // 
            this.menuItemCoinInCashOut.Name = "menuItemCoinInCashOut";
            this.menuItemCoinInCashOut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D0)));
            this.menuItemCoinInCashOut.Size = new System.Drawing.Size(163, 22);
            this.menuItemCoinInCashOut.Text = "&Cash Out";
            this.menuItemCoinInCashOut.Click += new System.EventHandler(this.menuItemCoinInCashOut_Click);
            // 
            // menuItemOptions
            // 
            this.menuItemOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemOptionsZipPlay,
            this.menuItemOptionsAutoPlay});
            this.menuItemOptions.Name = "menuItemOptions";
            this.menuItemOptions.Size = new System.Drawing.Size(61, 20);
            this.menuItemOptions.Text = "&Options";
            // 
            // menuItemOptionsZipPlay
            // 
            this.menuItemOptionsZipPlay.CheckOnClick = true;
            this.menuItemOptionsZipPlay.Name = "menuItemOptionsZipPlay";
            this.menuItemOptionsZipPlay.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.menuItemOptionsZipPlay.Size = new System.Drawing.Size(167, 22);
            this.menuItemOptionsZipPlay.Text = "&Zip Play";
            this.menuItemOptionsZipPlay.Click += new System.EventHandler(this.menuItemOptionsZipPlay_Click);
            // 
            // menuItemOptionsAutoPlay
            // 
            this.menuItemOptionsAutoPlay.CheckOnClick = true;
            this.menuItemOptionsAutoPlay.Name = "menuItemOptionsAutoPlay";
            this.menuItemOptionsAutoPlay.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.menuItemOptionsAutoPlay.Size = new System.Drawing.Size(167, 22);
            this.menuItemOptionsAutoPlay.Text = "&Auto Play";
            this.menuItemOptionsAutoPlay.Click += new System.EventHandler(this.menuItemOptionsAutoPlay_Click);
            // 
            // menuItemMisc
            // 
            this.menuItemMisc.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemMiscResetStatistics,
            this.menuItemMiscNewSession});
            this.menuItemMisc.Name = "menuItemMisc";
            this.menuItemMisc.Size = new System.Drawing.Size(44, 20);
            this.menuItemMisc.Text = "&Misc";
            // 
            // menuItemMiscResetStatistics
            // 
            this.menuItemMiscResetStatistics.Name = "menuItemMiscResetStatistics";
            this.menuItemMiscResetStatistics.Size = new System.Drawing.Size(183, 22);
            this.menuItemMiscResetStatistics.Text = "&Reset Statistics";
            this.menuItemMiscResetStatistics.Click += new System.EventHandler(this.menuItemMiscResetStatistics_Click);
            // 
            // menuItemMiscNewSession
            // 
            this.menuItemMiscNewSession.Name = "menuItemMiscNewSession";
            this.menuItemMiscNewSession.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.menuItemMiscNewSession.Size = new System.Drawing.Size(183, 22);
            this.menuItemMiscNewSession.Text = "&New Session";
            this.menuItemMiscNewSession.Click += new System.EventHandler(this.menuItemMiscNewSession_Click);
            // 
            // soundToolStripMenuItem
            // 
            this.soundToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.muteToolStripMenuItem});
            this.soundToolStripMenuItem.Name = "soundToolStripMenuItem";
            this.soundToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.soundToolStripMenuItem.Text = "So&und";
            // 
            // muteToolStripMenuItem
            // 
            this.muteToolStripMenuItem.Checked = true;
            this.muteToolStripMenuItem.CheckOnClick = true;
            this.muteToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.muteToolStripMenuItem.Name = "muteToolStripMenuItem";
            this.muteToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.muteToolStripMenuItem.Text = "Mu&te";
            this.muteToolStripMenuItem.Click += new System.EventHandler(this.muteToolStripMenuItem_Click);
            // 
            // bankSizeToolStripMenuItem
            // 
            this.bankSizeToolStripMenuItem.CheckOnClick = true;
            this.bankSizeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bS1ToolStripItem,
            this.bS2ToolStripItem,
            this.bS3ToolStripItem,
            this.bS4ToolStripItem,
            this.bS5ToolStripItem,
            this.bS6ToolStripItem,
            this.bS8ToolStripItem,
            this.bS10ToolStripItem});
            this.bankSizeToolStripMenuItem.Name = "bankSizeToolStripMenuItem";
            this.bankSizeToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.bankSizeToolStripMenuItem.Text = "B&ank Size";
            // 
            // bS1ToolStripItem
            // 
            this.bS1ToolStripItem.Checked = true;
            this.bS1ToolStripItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.bS1ToolStripItem.Name = "bS1ToolStripItem";
            this.bS1ToolStripItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D1)));
            this.bS1ToolStripItem.Size = new System.Drawing.Size(158, 22);
            this.bS1ToolStripItem.Text = "1";
            this.bS1ToolStripItem.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // bS2ToolStripItem
            // 
            this.bS2ToolStripItem.CheckOnClick = true;
            this.bS2ToolStripItem.Name = "bS2ToolStripItem";
            this.bS2ToolStripItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D2)));
            this.bS2ToolStripItem.Size = new System.Drawing.Size(158, 22);
            this.bS2ToolStripItem.Text = "2";
            this.bS2ToolStripItem.Click += new System.EventHandler(this.bS2ToolStripItem_Click);
            // 
            // bS3ToolStripItem
            // 
            this.bS3ToolStripItem.CheckOnClick = true;
            this.bS3ToolStripItem.Name = "bS3ToolStripItem";
            this.bS3ToolStripItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D3)));
            this.bS3ToolStripItem.Size = new System.Drawing.Size(158, 22);
            this.bS3ToolStripItem.Text = "3";
            this.bS3ToolStripItem.Click += new System.EventHandler(this.bS3ToolStripItem_Click);
            // 
            // bS4ToolStripItem
            // 
            this.bS4ToolStripItem.CheckOnClick = true;
            this.bS4ToolStripItem.Name = "bS4ToolStripItem";
            this.bS4ToolStripItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D4)));
            this.bS4ToolStripItem.Size = new System.Drawing.Size(158, 22);
            this.bS4ToolStripItem.Text = "4";
            this.bS4ToolStripItem.Click += new System.EventHandler(this.bS4ToolStripItem_Click);
            // 
            // bS5ToolStripItem
            // 
            this.bS5ToolStripItem.CheckOnClick = true;
            this.bS5ToolStripItem.Name = "bS5ToolStripItem";
            this.bS5ToolStripItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D5)));
            this.bS5ToolStripItem.Size = new System.Drawing.Size(158, 22);
            this.bS5ToolStripItem.Text = "5";
            this.bS5ToolStripItem.Click += new System.EventHandler(this.bS5ToolStripItem_Click);
            // 
            // bS6ToolStripItem
            // 
            this.bS6ToolStripItem.CheckOnClick = true;
            this.bS6ToolStripItem.Name = "bS6ToolStripItem";
            this.bS6ToolStripItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D6)));
            this.bS6ToolStripItem.Size = new System.Drawing.Size(158, 22);
            this.bS6ToolStripItem.Text = "6";
            this.bS6ToolStripItem.Click += new System.EventHandler(this.bS6ToolStripItem_Click);
            // 
            // bS8ToolStripItem
            // 
            this.bS8ToolStripItem.CheckOnClick = true;
            this.bS8ToolStripItem.Name = "bS8ToolStripItem";
            this.bS8ToolStripItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D8)));
            this.bS8ToolStripItem.Size = new System.Drawing.Size(158, 22);
            this.bS8ToolStripItem.Text = "8";
            this.bS8ToolStripItem.Click += new System.EventHandler(this.bS8ToolStripItem_Click);
            // 
            // bS10ToolStripItem
            // 
            this.bS10ToolStripItem.CheckOnClick = true;
            this.bS10ToolStripItem.Name = "bS10ToolStripItem";
            this.bS10ToolStripItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.D0)));
            this.bS10ToolStripItem.Size = new System.Drawing.Size(158, 22);
            this.bS10ToolStripItem.Text = "10";
            this.bS10ToolStripItem.Click += new System.EventHandler(this.bS10ToolStripItem_Click);
            // 
            // gameMessageBox
            // 
            this.gameMessageBox.BackColor = System.Drawing.Color.Black;
            this.gameMessageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gameMessageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameMessageBox.ForeColor = System.Drawing.Color.White;
            this.gameMessageBox.Location = new System.Drawing.Point(31, 37);
            this.gameMessageBox.Name = "gameMessageBox";
            this.gameMessageBox.ReadOnly = true;
            this.gameMessageBox.Size = new System.Drawing.Size(564, 19);
            this.gameMessageBox.TabIndex = 3;
            this.gameMessageBox.Text = "Top";
            this.gameMessageBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // creditsTB
            // 
            this.creditsTB.BackColor = System.Drawing.Color.Black;
            this.creditsTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.creditsTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.creditsTB.ForeColor = System.Drawing.Color.White;
            this.creditsTB.Location = new System.Drawing.Point(31, 74);
            this.creditsTB.MinimumSize = new System.Drawing.Size(0, 48);
            this.creditsTB.Multiline = true;
            this.creditsTB.Name = "creditsTB";
            this.creditsTB.ReadOnly = true;
            this.creditsTB.Size = new System.Drawing.Size(281, 48);
            this.creditsTB.TabIndex = 4;
            this.creditsTB.Text = "Top";
            this.creditsTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // betTB
            // 
            this.betTB.BackColor = System.Drawing.Color.Black;
            this.betTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.betTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.betTB.ForeColor = System.Drawing.Color.White;
            this.betTB.Location = new System.Drawing.Point(322, 74);
            this.betTB.MinimumSize = new System.Drawing.Size(0, 48);
            this.betTB.Multiline = true;
            this.betTB.Name = "betTB";
            this.betTB.ReadOnly = true;
            this.betTB.Size = new System.Drawing.Size(273, 48);
            this.betTB.TabIndex = 5;
            this.betTB.Text = "Top";
            this.betTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // winTB
            // 
            this.winTB.BackColor = System.Drawing.Color.Black;
            this.winTB.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.winTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 52F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.winTB.ForeColor = System.Drawing.Color.White;
            this.winTB.Location = new System.Drawing.Point(606, 43);
            this.winTB.Name = "winTB";
            this.winTB.ReadOnly = true;
            this.winTB.Size = new System.Drawing.Size(285, 79);
            this.winTB.TabIndex = 6;
            this.winTB.Text = "Top";
            this.winTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1455, 620);
            this.Controls.Add(this.winTB);
            this.Controls.Add(this.betTB);
            this.Controls.Add(this.creditsTB);
            this.Controls.Add(this.gameMessageBox);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Slot Simulator";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuItemRun;
        private System.Windows.Forms.ToolStripMenuItem menuItemRunSimulation;
        private System.Windows.Forms.ToolStripMenuItem menuItemRunBingo;
        private System.Windows.Forms.TextBox gameMessageBox;
        private System.Windows.Forms.ToolStripMenuItem menuItemWindows;
        private System.Windows.Forms.ToolStripMenuItem menuItemWindowsStatistics;
        private System.Windows.Forms.ToolStripMenuItem menuItemCoinIn;
        private System.Windows.Forms.ToolStripMenuItem menuItemCoinIn20;
        private System.Windows.Forms.ToolStripMenuItem menuItemCoinIn50;
        private System.Windows.Forms.ToolStripMenuItem menuItemCoinIn100;
        private System.Windows.Forms.ToolStripMenuItem menuItemCoinIn500;
        private System.Windows.Forms.ToolStripMenuItem menuItemCoinIn1000;
        private System.Windows.Forms.ToolStripMenuItem menuItemCoinInCashOut;
        private System.Windows.Forms.ToolStripMenuItem menuItemOptions;
        private System.Windows.Forms.ToolStripMenuItem menuItemOptionsZipPlay;
        private System.Windows.Forms.ToolStripMenuItem menuItemMisc;
        private System.Windows.Forms.ToolStripMenuItem menuItemMiscResetStatistics;
        private System.Windows.Forms.ToolStripMenuItem menuItemCoinInSet40xBet;
        private System.Windows.Forms.ToolStripMenuItem menuItemRunBatch;
        private System.Windows.Forms.ToolStripMenuItem menuItemMiscNewSession;
        private System.Windows.Forms.ToolStripMenuItem menuItemOptionsAutoPlay;
        private System.Windows.Forms.ToolStripMenuItem menuItemWindowsMaximize;
        private System.Windows.Forms.ToolStripMenuItem menuItemWindowsRestore;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TextBox creditsTB;
        private System.Windows.Forms.TextBox betTB;
        private System.Windows.Forms.TextBox winTB;
        private System.Windows.Forms.ToolStripMenuItem soundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem muteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bankSizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bS1ToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem bS2ToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem bS3ToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem bS4ToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem bS5ToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem bS6ToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem bS8ToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem bS10ToolStripItem;
    }
}

