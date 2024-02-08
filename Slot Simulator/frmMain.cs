using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Slot_Simulator
{
    public partial class frmMain : Form
    {
        internal bool AutoPlay;
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint SetThreadExecutionState([In] uint esFlags);
        public frmMain()
        {
            InitializeComponent();
            //0x80000000 | 0x00000004 - Continuous | User Present - For XP
            //0x80000000 | 0x00000002 | 0x00000001 - Continuous | Display | System - For Vista and after
            //SetThreadExecutionState(0x80000000 | 0x00000002 | 0x00000001 | 0x00000004);
        }
        internal GameInfo GameInfo;
        public int m_betLevel = 1;
        private int m_creditsTotal = 2000;
        private ReelsPanel reelsPanel;
        private ReelsSpin m_reelsSpin;
        private frmStatistics frmStatistics;
        private GameStatistics m_gameStatistics;
        private Sound m_soundPlayer;
        //private NumberBox m_numberBoxBet, m_numberBoxCreditWin, m_numberBoxCreditTotal;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0112) // WM_SYSCOMMAND
            {
                // Check your window state here
                if (m.WParam == new IntPtr(0xF030)) // Maximize event - SC_MAXIMIZE from Winuser.h
                {
                    menuItemWindowsMaximize_Click(null, null);
                    return;
                }
            }
            base.WndProc(ref m);
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            m.gameMessageBox = gameMessageBox;

            reelsPanel = new ReelsPanel(this);
            reelsPanel.Left = 0;
            reelsPanel.Top = 24;
            m.ReelsPanel = reelsPanel;
            m_reelsSpin = new ReelsSpin(reelsPanel, this);
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(m.Directory);
                bool loaded = false;
                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    switch (fileInfo.Extension)
                    {
                        case ".xls":
                        case ".xlsx":
                            string firstChar = fileInfo.Name.Substring(0, 1);
                            if (firstChar != "~" && firstChar != "z")
                            {
                                GameInfo = GameInfo.CreateGameInfo(fileInfo.FullName);
                                loaded = true;
                            }
                            break;
                    }
                    if (loaded) break;
                }
                List<DirectoryInfo> imageDirectories = new List<DirectoryInfo>();
                string mainDirectory = GameInfo.GameName;
                DirectoryInfo directoryToLookAt = new DirectoryInfo(string.Format(@"{0}\img\{1}", m.Directory, mainDirectory));
                if(directoryToLookAt.Exists)
                {
                    foreach(FileInfo fileInfo in directoryToLookAt.GetFiles())
                    {
                        switch (fileInfo.Extension)
                        {
                            case ".png":
                            case ".jpg":
                                ReelImages.AddImage(fileInfo, GameInfo.conversionRatio);
                                break;
                        }
                    }
                }
                else if (!directoryToLookAt.Exists)
                {
                    //string altDirectory = string.Format("{0}{1}{2}{3}{4} Images", GameInfo.Dimensions[0], GameInfo.Dimensions[1], GameInfo.Dimensions[2], GameInfo.Dimensions[3], GameInfo.Dimensions[4]);
                    string altDirectory = string.Format("{0} Images", m.MaxInt(GameInfo.Dimensions));
                    DirectoryInfo newDirectory = new DirectoryInfo(string.Format(@"{0}\img\{1}", m.Directory, altDirectory));
                    if(newDirectory.Exists)
                    {
                        foreach (FileInfo fileInfo in newDirectory.GetFiles())
                        {
                            switch (fileInfo.Extension)
                            {
                                case ".png":
                                case ".jpg":
                                    ReelImages.AddImage(fileInfo, GameInfo.conversionRatio);
                                    break;
                            }
                        }
                    }
                    else if (!newDirectory.Exists)
                    {
                        DirectoryInfo backupDirectory = new DirectoryInfo(string.Format(@"{0}\img", m.Directory));
                        foreach (FileInfo fileInfo in backupDirectory.GetFiles())
                        {
                            switch (fileInfo.Extension)
                            {
                                case ".png":
                                case ".jpg":
                                    ReelImages.AddImage(fileInfo, GameInfo.conversionRatio);
                                    break;
                            }
                        }
                    }
                }
                setAllDisplayItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            m.winsTB = winTB;
            menuItemMiscNewSession_Click(null, null);
            reelsPanel.Select();
        }
        private void setAllDisplayItems()
        {
            //GameInfo.SymbolHeight = ReelImages.imageDimensions["l4"][1]; //Commented out because didn't see why we were changing the height and it was causing errors. This may need to be investigated more in the future.
            GameInfo.SymbolWidth = ReelImages.imageDimensions["l4"][0];
            GameInfo.BackgroundHeight = ReelImages.imageDimensions["background"][1];
            GameInfo.BackgroundWidth = ReelImages.imageDimensions["background"][0];
            if (m.MaxInt(GameInfo.Dimensions) == 3)
            {
                if (ReelImages.originalBackgroundHeight == 1208)
                {
                    GameInfo.ReelsStartX = 37;
                    GameInfo.ReelsStartY = 60;
                    GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 4;
                }
                else if (ReelImages.originalBackgroundHeight == 1080)
                {
                    GameInfo.ReelsStartX = 39;
                    GameInfo.ReelsStartY = 9;
                    GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 3;
                }
                else
                {
                    GameInfo.ReelsStartX = 39;
                    GameInfo.ReelsStartY = 9;
                    GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 3;
                }
            }
            else if (m.MaxInt(GameInfo.Dimensions) == 4)
            {
                if (ReelImages.originalBackgroundHeight == 1208)
                {
                    GameInfo.ReelsStartX = 37;
                    GameInfo.ReelsStartY = 60;
                    GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth));
                }
                else if (ReelImages.originalBackgroundHeight == 1080)
                {
                    GameInfo.ReelsStartX = 39;
                    GameInfo.ReelsStartY = 9;
                    GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 3;
                }
                else
                {
                    GameInfo.ReelsStartX = 39;
                    GameInfo.ReelsStartY = 9;
                    GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 3;
                }
            }
            else if (m.MaxInt(GameInfo.Dimensions) == 5)
            {
                GameInfo.ReelsStartX = 49;
                GameInfo.ReelsStartY = 9;
                GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 3;
            }
            else if (m.MaxInt(GameInfo.Dimensions) == 6)
            {
                GameInfo.ReelsStartX = 41;
                GameInfo.ReelsStartY = 10;
                GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 3;                
            }
            else if (m.MaxInt(GameInfo.Dimensions) == 7 || m.MaxInt(GameInfo.Dimensions) == 8 || m.MaxInt(GameInfo.Dimensions) == 9)
            {
                GameInfo.ReelsStartX = 20;
                GameInfo.ReelsStartY = 11;
                GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 3;
            }
            else
            {
                GameInfo.ReelsStartX = 39;
                GameInfo.ReelsStartY = 15;
                GameInfo.ReelSpacing = (int)((GameInfo.BackgroundWidth - (GameInfo.ReelsStartX * 2)) / (GameInfo.ReelCount * GameInfo.SymbolWidth)) + 3;
            }
            GameInfo.ProcessOversized();
            InitializeReels();
            if (GameInfo.m_progressiveType != "none")
            {
                GameInfo.frmProg = new frmProgressives();
                GameInfo.frmProg.leftStart = GameInfo.BackgroundWidth + 6;
                GameInfo.frmProg.progressiveType = GameInfo.m_progressiveType;
                GameInfo.frmProg.setProgressiveTextBoxes(GameInfo.m_progressives);
                GameInfo.frmProg.Show();
            }
        }
        private void menuItemRunSimulation_Click(object sender, EventArgs e)
        {
            Simulation frmSimulation = new Simulation();
            frmSimulation.InitializeGameInfo(GameInfo.FileNamePath);
            frmSimulation.Visible = true;

        }
        private void menuItemRunBingo_Click(object sender, EventArgs e)
        {
            frmTOD frmTOD = new frmTOD();
            frmTOD.InitializeGameInfo(GameInfo);
            frmTOD.Visible = true;
        }
        private void menuItemRunBatch_Click(object sender, EventArgs e)
        {
            frmBatch frmBatch = new frmBatch();
            frmBatch.Visible = true;
        }
        private void menuItemWindowsStatistics_Click(object sender, EventArgs e)
        {
            if (frmStatistics == null)
            {
                frmStatistics = new frmStatistics();
                frmStatistics.RegisterfrmMain(this);
                frmStatistics.DisplayStatistics(m_gameStatistics);
                frmStatistics.Visible = true;
            }
            else
            {
                frmStatistics.Close();
                frmStatistics = null;
            }
        }
        private void menuItemWindowsMaximize_Click(object sender, EventArgs e)
        {
            if (reelsPanel.Top == 24)
            {
                reelsPanel.Top -= 24;
                gameMessageBox.Top -= 24;
            }
            menuStrip1.Hide();
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(GameInfo.BackgroundWidth, GameInfo.BackgroundHeight);
            Left = GameInfo.MaximizedX;
            Top = GameInfo.MaximizedY;
        }
        private void menuItemWindowsRestore_Click(object sender, EventArgs e)
        {
            if (reelsPanel.Top == 0)
            {
                reelsPanel.Top += 24;
                gameMessageBox.Top += 24;
            }
            menuStrip1.Show();
            FormBorderStyle = FormBorderStyle.FixedSingle;
        }
        private void menuItemResetBuyIn_Click(object sender, EventArgs e)
        {
            ResetBuyIn();
        }
        private void menuItemCoinIn_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            int dollars = int.Parse(menuItem.Text.Substring(1));
            m_creditsTotal += dollars * 100;
            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");
        }
        private void menuItemCoinInCashOut_Click(object sender, EventArgs e)
        {
            m_creditsTotal = 0;
            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");
        }
        private void menuItemOptionsZipPlay_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            m.ZipPlay = menuItem.Checked;            
        }
        private void menuItemOptionsAutoPlay_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            AutoPlay = menuItem.Checked;     
        }
        private void menuItemMiscResetStatistics_Click(object sender, EventArgs e)
        {
            ResetStatistics();
            if (frmStatistics != null)
                frmStatistics.DisplayStatistics(m_gameStatistics);
        }
        private void menuItemMiscNewSession_Click(object sender, EventArgs e)
        {
            int baseBet = GameInfo.Bet / GameInfo.BetLevel;
            m.BankRollForStatistics = 50 * GameInfo.Bet;
            if (frmStatistics != null) frmStatistics.m_numberOfSessions++;
            menuItemMiscResetStatistics_Click(null, null);
            m_creditsTotal = m.BankRollForStatistics;
            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");
            m.ReelsPanel.Invalidate();
        }
        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D1: SetBetLevel(GameInfo.PossibleBetLevels[0]); m.ReelsPanel.Invalidate(); break;
                case Keys.D2: SetBetLevel(GameInfo.PossibleBetLevels[1]); m.ReelsPanel.Invalidate(); break;
                case Keys.D3: SetBetLevel(GameInfo.PossibleBetLevels[2]); m.ReelsPanel.Invalidate(); break;
                case Keys.D4: SetBetLevel(GameInfo.PossibleBetLevels[3]); m.ReelsPanel.Invalidate(); break;
                case Keys.D5: SetBetLevel(GameInfo.PossibleBetLevels[4]); m.ReelsPanel.Invalidate(); break;

                case Keys.S:
                case Keys.Oemtilde:
                case Keys.Q:
                case Keys.W:
                case Keys.E:
                case Keys.R:
                case Keys.T:
                case Keys.Y:
                case Keys.U:
                case Keys.I:
                case Keys.O:
                case Keys.P:
                    GameInfo.CheatKey = e.KeyCode;
                    Spin();
                    break;
            }
            return;
        }
        //Internal Functions/////////////////////////////////////////////////////////////////////////////////////////////////
        internal void AddWinToMeter(bool _skipStatsDisplayAndCountUp = false)
        {
            winTB.Text = GameInfo.WinsThisGame.ToString();
            m_creditsTotal += GameInfo.WinsThisGame;
            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");
            m_gameStatistics.RegisterWin(GameInfo.Bet, GameInfo.WinsThisGame, GameInfo.BonusCodesToColors.ContainsKey(GameInfo.BonusCode) ? GameInfo.BonusCodesToColors[GameInfo.BonusCode] : "");
            if (frmStatistics != null && !_skipStatsDisplayAndCountUp)
                frmStatistics.DisplayStatistics(m_gameStatistics);
        }
        internal void SetBetLevel(int _betLevel)
        {
            m_betLevel = _betLevel;
            GameInfo.BetLevel = m_betLevel;
            betTB.Text = GameInfo.Bet.ToString();
        }
        internal void Bet()
        {
            m_creditsTotal -= GameInfo.Bet;
            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");
        }
        internal void ResetStatistics()
        {
            m_gameStatistics = new GameStatistics(GameInfo.Bet, GameInfo.ExpectedRTP);
            if (frmStatistics != null)
                frmStatistics.SetGameStatistics(m_gameStatistics);
        }
        internal void ResetBuyIn()
        {
            m_creditsTotal = m.GetBuyInOfBet(GameInfo.Bet);
            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");
        }
        internal void ResetTo(int _credits)
        {
            m_creditsTotal = _credits;
            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");
        }
        internal void ResetToNewSession()
        {
            menuItemMiscNewSession_Click(null, null);
        }
        internal void AddCreditsTimesBet(int _timesBet)
        {
            m_creditsTotal += GameInfo.Bet * _timesBet;
            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");
        }
        internal void Spin(RandomType _randomType = RandomType.Random)
        {
            m_soundPlayer = new Sound();
            if(!GameInfo.InFreeGames) winTB.Text = "";
            switch (m_reelsSpin.ReelState)
            {
                case ReelState.Ready:
                case ReelState.ShowingWinsAtTheEndOfAGame:
                    m.SetRandomType(_randomType);
                    if (frmStatistics == null || !frmStatistics.Visible)
                    {
                        if (m_creditsTotal >= GameInfo.Bet)
                        {
                            GameInfo.PreSpin(GameInfo.m_stackShow);
                            m_reelsSpin.m_soundStatus = GameInfo.m_soundMute;
                            m_reelsSpin.Spin();
                            Bet();
                        }
                    }
                    else if (frmStatistics != null && frmStatistics.Visible)
                    {
                        if ((m_creditsTotal < GameInfo.Bet) || (m_creditsTotal >= 125 * GameInfo.Bet * GameInfo.BetLevel))
                        {
                            if ((m_creditsTotal >= 125 * GameInfo.Bet * GameInfo.BetLevel)) frmStatistics.m_numberOfCashouts++;
                            frmStatistics.storeStatistics();
                            ResetToNewSession();
                            ResetStatistics();
                            frmStatistics.DisplayStatistics();
                            frmStatistics.m_numberOfSpins++;
                            GameInfo.PreSpin(GameInfo.m_stackShow);
                            m_reelsSpin.m_soundStatus = GameInfo.m_soundMute;
                            m_reelsSpin.Spin();
                            Bet();
                        }
                        else if (m_creditsTotal >= GameInfo.Bet)
                        {
                            GameInfo.PreSpin(GameInfo.m_stackShow);
                            frmStatistics.m_numberOfSpins++;
                            m_reelsSpin.m_soundStatus = GameInfo.m_soundMute;
                            m_reelsSpin.Spin();
                            Bet();
                        }
                    }
                        break;
                case ReelState.ShowingWins:
                case ReelState.Waiting:
                    m_reelsSpin.CancelSpin(true);
                    break;
                case ReelState.Spinning:
                    m_reelsSpin.CancelSpin(false);
                    break;
            }   
        }
        internal bool FastSpin(bool _rebuy = true)
        {
            GameInfo.CheatKey = Keys.S;
            if (frmStatistics != null)
                frmStatistics.SetGameStatistics(m_gameStatistics);
            if (m_reelsSpin.ReelState == ReelState.Ready || m_reelsSpin.ReelState == ReelState.ShowingWinsAtTheEndOfAGame)
            {
                if (m_creditsTotal < GameInfo.Bet)
                {
                    if (_rebuy)
                        m_creditsTotal += m.GetBuyInOfBet(GameInfo.Bet);
                    else
                        return false;
                }
                m.SetRandomType(RandomType.Random);
                GameInfo.PreSpin(GameInfo.m_stackShow);
                m_reelsSpin.FastSpin();
                return true;
            }
            return false;
        }
        internal bool FastSpinAndIfBonusDoNotRegister()
        {
            GameInfo.CheatKey = Keys.S;
            if (frmStatistics != null)
                frmStatistics.SetGameStatistics(m_gameStatistics);
            if (m_reelsSpin.ReelState == ReelState.Ready || m_reelsSpin.ReelState == ReelState.ShowingWinsAtTheEndOfAGame)
            {
                if (m_creditsTotal < GameInfo.Bet)
                {
                    m_creditsTotal += m.GetBuyInOfBet(GameInfo.Bet);                     
                }
                m.SetRandomType(RandomType.SetRandom);
                GameInfo.SaveStateBeforeSpin();
                GameInfo.PreSpin(GameInfo.m_stackShow);
                if (m_reelsSpin.FastSpinAndDoNotRegisterIfBonus())
                {
                    m_creditsTotal += GameInfo.Bet;
                    GameInfo.ReloadStateToBeforeSpin();
                    return true;
                }
            }
            return false;
        }
        internal void SetWinLinesIfAny()
        {
            m_reelsSpin.SetWinLinesIfAny();
        }


        //Private Functions//////////////////////////////////////////////////////////////////////////////////////////////////
        private void InitializeReels()
        {
            int yOffset = 24;
            Width = GameInfo.BackgroundWidth + 6;
            Height = GameInfo.BackgroundHeight + 52;
            reelsPanel.SetGameInfo(GameInfo);
            reelsPanel.Width = GameInfo.BackgroundWidth;
            reelsPanel.Height = GameInfo.BackgroundHeight;
            m_reelsSpin.SetGameInfo(GameInfo);
            int dimensionMax = m.MaxInt(GameInfo.Dimensions);
            int top = GameInfo.ReelsStartY + yOffset;
            int bottom = top + dimensionMax * GameInfo.SymbolHeight;
            int bottomOther = bottom - yOffset;
            int left = GameInfo.ReelsStartX;
            int right = left + GameInfo.ReelCount * GameInfo.SymbolWidth + (GameInfo.ReelCount - 1) * GameInfo.ReelSpacing;
            int middle = (left + right) / 2;
            string dimensionString = "";
            for (int aa = 0; aa < GameInfo.Dimensions.Count(); aa++ )
            {
                if (aa < GameInfo.Dimensions.Count() - 1) dimensionString += (GameInfo.Dimensions[aa].ToString() + ",");
                else if (aa == GameInfo.Dimensions.Count() - 1) dimensionString += GameInfo.Dimensions[aa].ToString();
            }
            if (ReelImages.originalBackgroundHeight == 1080 && dimensionMax == 3)
            {
                gameMessageBox.Top = GameInfo.BackgroundHeight - 80;
                gameMessageBox.Width = 300;
                gameMessageBox.Left = (GameInfo.BackgroundWidth) - (gameMessageBox.Width / 2) - 175;
                betTB.Width = 155;
                betTB.Left = 320;
                betTB.Height = 48;
                creditsTB.Top = GameInfo.BackgroundHeight - 35;
                creditsTB.Left = 850;
                creditsTB.Width = 165;
                creditsTB.Height = 48;
                winTB.Top = GameInfo.BackgroundHeight - 65;
                winTB.Left = betTB.Right + 10;
                winTB.Width = 350;
            }
            else if (ReelImages.originalBackgroundHeight == 1080 && dimensionMax == 4)
            {
                gameMessageBox.Top = GameInfo.BackgroundHeight - 80;
                gameMessageBox.Width = 300;
                gameMessageBox.Left = (GameInfo.BackgroundWidth) - (gameMessageBox.Width / 2) - 175;
                betTB.Width = 155;
                betTB.Left = 310;
                betTB.Height = 48;
                creditsTB.Top = GameInfo.BackgroundHeight - 35;
                creditsTB.Left = 830;
                creditsTB.Width = 165;
                creditsTB.Height = 48;
                winTB.Top = GameInfo.BackgroundHeight - 65;
                winTB.Left = betTB.Right + 10;
                winTB.Width = 350;
            }
            else if (dimensionMax == 5)
            {
                gameMessageBox.Top = GameInfo.BackgroundHeight - 101;
                gameMessageBox.Width = 300;
                gameMessageBox.Left = (GameInfo.BackgroundWidth) - (gameMessageBox.Width / 2) - 175;
                betTB.Width = 155;
                betTB.Left = 413;
                betTB.Height = 48;
                creditsTB.Top = GameInfo.BackgroundHeight - 39;
                creditsTB.Left = 1056;
                creditsTB.Width = 165;
                creditsTB.Height = 48;
                winTB.Top = GameInfo.BackgroundHeight - 76;
                winTB.Left = 638;
                winTB.Width = 350;
            }
            else if (dimensionMax == 6)
            {
                gameMessageBox.Top = GameInfo.BackgroundHeight - 110;
                gameMessageBox.Width = 300;
                gameMessageBox.Left = (GameInfo.BackgroundWidth) - (gameMessageBox.Width / 2) - 175;
                betTB.Width = 155;
                betTB.Left = 350;
                betTB.Height = 48;
                creditsTB.Top = GameInfo.BackgroundHeight - 39;
                creditsTB.Left = 940;
                creditsTB.Width = 165;
                creditsTB.Height = 48;
                winTB.Top = GameInfo.BackgroundHeight - 76;
                winTB.Left = 550;
                winTB.Width = 350;
            }
            else if (dimensionMax == 7)
            {
                gameMessageBox.Top = GameInfo.BackgroundHeight - 65;
                gameMessageBox.Width = 250;
                gameMessageBox.Left = (GameInfo.BackgroundWidth) - (gameMessageBox.Width / 2) - 110;
                gameMessageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                betTB.Width = 100;
                betTB.Left = 190;
                betTB.Height = 30;
                creditsTB.Top = GameInfo.BackgroundHeight - 20;
                creditsTB.Left = 505;
                creditsTB.Width = 110;
                creditsTB.Height = 48;
                creditsTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                winTB.Top = GameInfo.BackgroundHeight - 47;
                winTB.Left = 300;
                winTB.Width = 200;
            }
            else if (dimensionMax == 8)
            {
                gameMessageBox.Top = GameInfo.BackgroundHeight - 75;
                gameMessageBox.Width = 250;
                gameMessageBox.Left = (GameInfo.BackgroundWidth) - (gameMessageBox.Width / 2) - 110;
                gameMessageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                betTB.Width = 100;
                betTB.Left = 190;
                betTB.Height = 30;
                creditsTB.Top = GameInfo.BackgroundHeight - 25;
                creditsTB.Left = 510;
                creditsTB.Width = 100;
                creditsTB.Height = 48;
                creditsTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                winTB.Top = GameInfo.BackgroundHeight - 50;
                winTB.Left = 300;
                winTB.Width = 200;
            }
            else if (dimensionMax == 9)
            {
                gameMessageBox.Top = GameInfo.BackgroundHeight - 90;
                gameMessageBox.Width = 250;
                gameMessageBox.Left = (GameInfo.BackgroundWidth) - (gameMessageBox.Width / 2) - 110;
                gameMessageBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                betTB.Width = 95;
                betTB.Left = 170;
                betTB.Height = 30;
                creditsTB.Top = GameInfo.BackgroundHeight - 35;
                creditsTB.Left = 470;
                creditsTB.Width = 100;
                creditsTB.Height = 48;
                creditsTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                winTB.Top = GameInfo.BackgroundHeight - 70;
                winTB.Left = 270;
                winTB.Width = 190;
            }
            else if (ReelImages.originalBackgroundHeight == 1208)
            {
                gameMessageBox.Top = GameInfo.BackgroundHeight - 100;
                gameMessageBox.Left = (GameInfo.BackgroundWidth / 2) - (gameMessageBox.Width / 2);
                creditsTB.Top = gameMessageBox.Top + 37;
                creditsTB.Left = gameMessageBox.Left;
                creditsTB.Width = 274;
                betTB.Width = 274;
                betTB.Left = creditsTB.Left + 290;
                winTB.Left = betTB.Right + 20;
                winTB.Width = 285;
            }
            
            gameMessageBox.Text = "Press s to spin the reels.";

            decimal valueToDisplay = (decimal)m_creditsTotal / (decimal)100;
            creditsTB.Text = valueToDisplay.ToString("c");

            betTB.Top = creditsTB.Top;
            betTB.Text = GameInfo.Bet.ToString();

            winTB.Text = "0";

            gameMessageBox.Visible = GameInfo.TopMessageShow;

            int reelHeight = dimensionMax * GameInfo.SymbolHeight;
            m_gameStatistics = new GameStatistics(GameInfo.Bet, GameInfo.ExpectedRTP);
            Text = GameInfo.GameName;
        }

        private void muteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GameInfo.m_soundMute = !GameInfo.m_soundMute;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            GameInfo.m_bankSize = 1;
            bS1ToolStripItem.Checked = true;
            bS2ToolStripItem.Checked = false;
            bS3ToolStripItem.Checked = false;
            bS4ToolStripItem.Checked = false;
            bS5ToolStripItem.Checked = false;
            bS6ToolStripItem.Checked = false;
            bS8ToolStripItem.Checked = false;
            bS10ToolStripItem.Checked = false;
        }

        private void bS2ToolStripItem_Click(object sender, EventArgs e)
        {
            GameInfo.m_bankSize = 2;
            bS1ToolStripItem.Checked = false;
            bS2ToolStripItem.Checked = true;
            bS3ToolStripItem.Checked = false;
            bS4ToolStripItem.Checked = false;
            bS5ToolStripItem.Checked = false;
            bS6ToolStripItem.Checked = false;
            bS8ToolStripItem.Checked = false;
            bS10ToolStripItem.Checked = false;
        }

        private void bS3ToolStripItem_Click(object sender, EventArgs e)
        {
            GameInfo.m_bankSize = 3;
            bS1ToolStripItem.Checked = false;
            bS2ToolStripItem.Checked = false;
            bS3ToolStripItem.Checked = true;
            bS4ToolStripItem.Checked = false;
            bS5ToolStripItem.Checked = false;
            bS6ToolStripItem.Checked = false;
            bS8ToolStripItem.Checked = false;
            bS10ToolStripItem.Checked = false;
        }

        private void bS4ToolStripItem_Click(object sender, EventArgs e)
        {
            GameInfo.m_bankSize = 4;
            bS1ToolStripItem.Checked = false;
            bS2ToolStripItem.Checked = false;
            bS3ToolStripItem.Checked = false;
            bS4ToolStripItem.Checked = true;
            bS5ToolStripItem.Checked = false;
            bS6ToolStripItem.Checked = false;
            bS8ToolStripItem.Checked = false;
            bS10ToolStripItem.Checked = false;
        }

        private void bS5ToolStripItem_Click(object sender, EventArgs e)
        {
            GameInfo.m_bankSize = 5;
            bS1ToolStripItem.Checked = false;
            bS2ToolStripItem.Checked = false;
            bS3ToolStripItem.Checked = false;
            bS4ToolStripItem.Checked = false;
            bS5ToolStripItem.Checked = true;
            bS6ToolStripItem.Checked = false;
            bS8ToolStripItem.Checked = false;
            bS10ToolStripItem.Checked = false;
        }

        private void bS6ToolStripItem_Click(object sender, EventArgs e)
        {
            GameInfo.m_bankSize = 6;
            bS1ToolStripItem.Checked = false;
            bS2ToolStripItem.Checked = false;
            bS3ToolStripItem.Checked = false;
            bS4ToolStripItem.Checked = false;
            bS5ToolStripItem.Checked = false;
            bS6ToolStripItem.Checked = true;
            bS8ToolStripItem.Checked = false;
            bS10ToolStripItem.Checked = false;
        }

        private void bS8ToolStripItem_Click(object sender, EventArgs e)
        {
            GameInfo.m_bankSize = 8;
            bS1ToolStripItem.Checked = false;
            bS2ToolStripItem.Checked = false;
            bS3ToolStripItem.Checked = false;
            bS4ToolStripItem.Checked = false;
            bS5ToolStripItem.Checked = false;
            bS6ToolStripItem.Checked = false;
            bS8ToolStripItem.Checked = true;
            bS10ToolStripItem.Checked = false;
        }

        private void bS10ToolStripItem_Click(object sender, EventArgs e)
        {
            GameInfo.m_bankSize = 10;
            bS1ToolStripItem.Checked = false;
            bS2ToolStripItem.Checked = false;
            bS3ToolStripItem.Checked = false;
            bS4ToolStripItem.Checked = false;
            bS5ToolStripItem.Checked = false;
            bS6ToolStripItem.Checked = false;
            bS8ToolStripItem.Checked = false;
            bS10ToolStripItem.Checked = true;
        }
    }
}
