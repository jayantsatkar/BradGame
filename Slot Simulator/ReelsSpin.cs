using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Threading;
using System.Media;
using System.Windows.Forms;

namespace Slot_Simulator
{
    internal enum ReelState { Ready, Spinning, Waiting, ShowingWins, Animating, ShowingWinsAtTheEndOfAGame }
    class ReelsSpin
    {
        private ReelsPanel m_reelsPanel;
        public bool m_soundStatus = true;
        internal ReelState ReelState;
        private GameInfo m_gameInfo;
        private BackgroundWorker m_bgwSpin;
        private const int cMoveReelIterationDelay = 33;
        private frmMain frmMain;
        internal ReelsSpin(ReelsPanel _reelsPanel, frmMain _frmMain)
        {
            frmMain = _frmMain;
            m_reelsPanel = _reelsPanel;
        }
        ~ReelsSpin()
        {

        }
        internal void Spin()
        {
            if (ReelState == Slot_Simulator.ReelState.Ready || ReelState == ReelState.ShowingWinsAtTheEndOfAGame)
            {
                if (m_bgwSpin != null && !m_bgwSpin.CancellationPending) m_bgwSpin.CancelAsync();
                m_reelsPanel.SetWinLines(null);
                ReelState = ReelState.Spinning;
                if (m.ZipPlay)
                {
                    SetReels(null, -1, true, true);
                    m_reelsPanel.Invalidate();
                    BGWSpinOverCallBack_RunWorkerCompleted(null, null);
                }
                else
                {
                    m_bgwSpin = new BackgroundWorker();
                    m_bgwSpin.WorkerSupportsCancellation = true;
                    m_bgwSpin.DoWork += new DoWorkEventHandler(BGWSpin_DoWork);
                    m_bgwSpin.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWSpinOverCallBack_RunWorkerCompleted);
                    m_bgwSpin.RunWorkerAsync();
                    
                }
            }
        }
        internal void FastSpin()
        {
            if (ReelState == Slot_Simulator.ReelState.Ready || ReelState == ReelState.ShowingWinsAtTheEndOfAGame)
            {
                frmMain.Bet();
                if (m_bgwSpin != null && !m_bgwSpin.CancellationPending) m_bgwSpin.CancelAsync();
                m_reelsPanel.SetWinLines(null);
                ReelState = ReelState.Spinning;
                while (m_gameInfo.PostSpin() != GameAction.End) ;
                ReelState = ReelState.Ready;
                SetReels(null, -1, true, true);
                m_reelsPanel.Invalidate();
                frmMain.AddWinToMeter(true);
            }
        }
        internal bool FastSpinAndDoNotRegisterIfBonus()
        {
            if (ReelState == Slot_Simulator.ReelState.Ready || ReelState == ReelState.ShowingWinsAtTheEndOfAGame)
            {
                frmMain.Bet();
                if (m_bgwSpin != null && !m_bgwSpin.CancellationPending) m_bgwSpin.CancelAsync();
                m_reelsPanel.SetWinLines(null);
                ReelState = ReelState.Spinning;
                while (m_gameInfo.PostSpin() != GameAction.End) ;
                ReelState = ReelState.Ready;
                SetReels(null, -1, true, true);
                m_reelsPanel.Invalidate();
                if (m_gameInfo.BonusCode > 0) return true;
                frmMain.AddWinToMeter(true);
            }
            return false;
        }
        internal void SetWinLinesIfAny()
        {
            if (m_gameInfo.WinsToShow != null && m_gameInfo.WinsToShow.Count > 0)
                StartWinLoop(true);
        }
        internal void CancelSpin(bool _dontWait = false)
        {
            if (_dontWait)
            {
                m_bgwSpin.RunWorkerCompleted -= BGWSpinOverCallBack_RunWorkerCompleted;
                m_bgwSpin.CancelAsync();
                BGWSpinOverCallBack_RunWorkerCompleted(null, null);
            }
            else
                m_bgwSpin.CancelAsync();
        }
        internal void SetGameInfo(GameInfo _gameInfo)
        {
            m_gameInfo = _gameInfo;
            SetReels();
        }
        //Background Worker

        List<int> m_spinOffsets174Start = new List<int>(new int[] { 30, 67, 77, });
        List<int> m_spinOffsets174Continue = new List<int>(new int[] { 78, 77, 78, 76, 78, 77, 78, 76, 78, });
        List<int> m_spinOffsets174Stop = new List<int>(new int[] { 78, 77, 78, 76, 78, 77, 78, 76, 76, 39, 30, -19, -15, -13, -8, -8, -4, });

        List<int> m_spinOffsets213Start = new List<int>(new int[] { 37, 82, 94, });
        List<int> m_spinOffsets213Continue = new List<int>(new int[] { 95, 94, 95, 95, 95, 94, 95, 94, 95, });
        List<int> m_spinOffsets213Stop = new List<int>(new int[] { 95, 94, 95, 94, 95, 94, 95, 94, 93, 48, 37, -23, -18, -16, -10, -10, -5, });

        List<int> m_spinOffsets290Start = new List<int>(new int[] { 50, 112, 128, });
        List<int> m_spinOffsets290Continue = new List<int>(new int[] { 129, 128, 129, 129, 129, 129, 129, 129, 129, });
        List<int> m_spinOffsets290Stop = new List<int>(new int[] { 129, 129, 129, 128, 129, 129, 129, 128, 128, 65, 50, -31, -25, -22, -14, -14, -7, });

        List<int> m_spinOffsets80Start = new List<int>(new int[] { 28, 62, 70, });
        List<int> m_spinOffsets80Continue = new List<int>(new int[] { 71, 71, 71, 71, 72, 71, 71, 71, 71, });
        List<int> m_spinOffsets80Stop = new List<int>(new int[] { 71, 71, 71, 71, 71, 71, 55, 44, 35, 17, 14, -9, -7, -6, -4, -4, -1, });

        List<int> m_spinOffsets142Start = new List<int>(new int[] { 24, 55, 63, });
        List<int> m_spinOffsets142Continue = new List<int>(new int[] { 95, 94, 95, 95, 95, 94, 95, 95, 94, });
        List<int> m_spinOffsets142Stop = new List<int>(new int[] { 95, 95, 95, 95, 95, 95, 95, 95, 64, 53, 29, -15, -12, -10, -7, -7, -3, });

        List<int> m_spinOffsets160Start = new List<int>(new int[] { 27, 62, 71, });
        List<int> m_spinOffsets160Continue = new List<int>(new int[] { 89, 89, 89, 89, 89, 89, 89, 89, 88, });
        List<int> m_spinOffsets160Stop = new List<int>(new int[] { 89, 89, 89, 89, 89, 89, 89, 89, 60, 51, 29, -14, -12, -9, -7, -7, -3, });

        List<int> m_spinOffsets128Start = new List<int>(new int[] { 21, 50, 57, });
        List<int> m_spinOffsets128Continue = new List<int>(new int[] { 71, 71, 71, 71, 72, 71, 71, 71, 71, });
        List<int> m_spinOffsets128Stop = new List<int>(new int[] { 71, 71, 71, 71, 71, 71, 71, 71, 48, 40, 23, -11, -9, -7, -5, -5, -2, });

        List<int> m_spinOffsets170Start = new List<int>(new int[] { 29, 66, 75, });
        List<int> m_spinOffsets170Continue = new List<int>(new int[] { 76, 75, 76, 75, 76, 75, 76, 75, 76, });
        List<int> m_spinOffsets170Stop = new List<int>(new int[] { 76, 75, 76, 75, 76, 75, 76, 75, 75, 38, 29, -18, -15, -13, -8, -8, -4, });

        List<int> m_spinOffsets156Start = new List<int>(new int[] { 27, 60, 69, });
        List<int> m_spinOffsets156Continue = new List<int>(new int[] { 70, 69, 69, 70, 69, 69, 70, 69, 69, });
        List<int> m_spinOffsets156Stop = new List<int>(new int[] { 70, 69, 69, 70, 69, 69, 70, 69, 67, 35, 27, -17, -13, -12, -7, -7, -4, });

        List<int> m_spinOffsets250Start = new List<int>(new int[] { 44, 96, 110, });
        List<int> m_spinOffsets250Continue = new List<int>(new int[] { 112, 110, 112, 110, 112, 110, 112, 110, 112, });
        List<int> m_spinOffsets250Stop = new List<int>(new int[] { 112, 110, 112, 110, 112, 110, 112, 110, 109, 57, 43, -27, -21, -19, -12, -12, -6, });

        List<int> m_spinOffsets220Start = new List<int>(new int[] { 38, 85, 97, });
        List<int> m_spinOffsets220Continue = new List<int>(new int[] { 98, 97, 98, 98, 98, 98, 98, 97, 98, });
        List<int> m_spinOffsets220Stop = new List<int>(new int[] { 98, 97, 98, 97, 98, 98, 98, 97, 96, 50, 38, -24, -19, -17, -10, -10, -5, });

        List<int> m_spinOffsets226Start = new List<int>(new int[] { 39, 87, 100, });
        List<int> m_spinOffsets226Continue = new List<int>(new int[] { 101, 100, 101, 100, 101, 100, 100, 100, 101, });
        List<int> m_spinOffsets226Stop = new List<int>(new int[] { 101, 100, 100, 100, 101, 100, 100, 100, 99, 51, 39, -24, -19, -17, -11, -11, -5, });

        List<int> m_spinOffsets184Start = new List<int>(new int[] { 30, 72, 82, });
        List<int> m_spinOffsets184Continue = new List<int>(new int[] { 102, 102, 102, 102, 104, 102, 102, 102, 102, });
        List<int> m_spinOffsets184Stop = new List<int>(new int[] { 102, 102, 102, 102, 102, 102, 102, 102, 69, 58, 33, -16, -13, -10, -7, -7, -3, });

        List<int> m_spinOffsets210Start = new List<int>(new int[] { 36, 81, 93, });
        List<int> m_spinOffsets210Continue = new List<int>(new int[] { 93, 93, 94, 93, 94, 93, 93, 93, 94, });
        List<int> m_spinOffsets210Stop = new List<int>(new int[] { 93, 93, 94, 93, 94, 93, 94, 93, 92, 47, 36, -23, -18, -16, -10, -10, -5, });

        List<int> m_spinOffsets284Start = new List<int>(new int[] { 50, 109, 125, });
        List<int> m_spinOffsets284Continue = new List<int>(new int[] { 127, 126, 126, 126, 127, 126, 126, 126, 126, });
        List<int> m_spinOffsets284Stop = new List<int>(new int[] { 126, 126, 126, 126, 126, 126, 126, 126, 124, 64, 49, -31, -24, -21, -13, -13, -7, });

        List<int> m_spinOffsets330Start = new List<int>(new int[] { 57, 127, 146, });
        List<int> m_spinOffsets330Continue = new List<int>(new int[] { 147, 146, 147, 147, 147, 146, 147, 146, 147, });
        List<int> m_spinOffsets330Stop = new List<int>(new int[] { 147, 146, 147, 146, 147, 146, 147, 146, 144, 74, 57, -36, -28, -25, -15, -15, -8, });

        void BGWSpin_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgwSpin = sender as BackgroundWorker;
            int reelCount = m_gameInfo.ReelCount;
            List<List<int>> reelMovements;
            SortedDictionary<int, int> soundIndexesToReels;
            List<int> indexes;
            int maxOffsetListLength = 0;
            switch (m_gameInfo.SymbolHeight)
            {
                case 80:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets80Start, m_spinOffsets80Continue, m_spinOffsets80Stop, 2, 8, 7);
                    break;
                case 128:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets128Start, m_spinOffsets128Continue, m_spinOffsets128Stop, 1, 5, 5);
                    break;
                case 142:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets142Start, m_spinOffsets142Continue, m_spinOffsets142Stop, 1, 6, 6);
                    break;
                case 160:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets160Start, m_spinOffsets160Continue, m_spinOffsets160Stop, 1, 5, 5);
                    break;
                case 170:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets170Start, m_spinOffsets170Continue, m_spinOffsets170Stop, 1, 4, 4);
                    break;
                case 156:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets156Start, m_spinOffsets156Continue, m_spinOffsets156Stop, 1, 4, 4);
                    break;
                case 174:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets174Start, m_spinOffsets174Continue, m_spinOffsets174Stop, 1, 4, 4);
                    break;
                case 210:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets210Start, m_spinOffsets210Continue, m_spinOffsets210Stop, 1, 4, 4);
                    break;
                case 213:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets213Start, m_spinOffsets213Continue, m_spinOffsets213Stop, 1, 4, 4);
                    break;
                case 220:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets220Start, m_spinOffsets220Continue, m_spinOffsets220Stop, 1, 4, 4);
                    break;
                case 226:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets226Start, m_spinOffsets226Continue, m_spinOffsets226Stop, 1, 4, 4);
                    break;
                case 250:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets250Start, m_spinOffsets250Continue, m_spinOffsets250Stop, 1, 4, 4);
                    break;
                case 284:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets284Start, m_spinOffsets284Continue, m_spinOffsets284Stop, 1, 4, 4);
                    break;
                case 290:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets290Start, m_spinOffsets290Continue, m_spinOffsets290Stop, 1, 4, 4);
                    break;
                case 184:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets184Start, m_spinOffsets184Continue, m_spinOffsets184Stop, 1, 5, 5);
                    break;
                case 330:
                    CreateReelMovements(out reelMovements, out indexes, out soundIndexesToReels, m_spinOffsets330Start, m_spinOffsets330Continue, m_spinOffsets330Stop, 1, 4, 4);
                    break;
                default: throw new ArgumentException("Unknown symbol height: " + m_gameInfo.SymbolHeight.ToString());
            }
            int onSoundIndexesIndex = 0;
            List<int> soundIndexes = new List<int>(soundIndexesToReels.Keys);
            foreach (List<int> reelMovementsForReel in reelMovements) maxOffsetListLength = Math.Max(maxOffsetListLength, reelMovementsForReel.Count);
            if (m_gameInfo.ActionPauseInMilliSeconds > 0)
                Thread.Sleep(m_gameInfo.ActionPauseInMilliSeconds);
            //Set Reels for non spinning reels
            for (int reelNum = 0; reelNum < reelCount; reelNum++)
                if (reelMovements[reelNum].Count == 0)
                {
                    SetReels(indexes, reelNum);
                }
            //Play 0 sounds if
            Sound m_soundPlayer = new Sound();
            if (soundIndexes[onSoundIndexesIndex] == 0)
            {
                m_soundPlayer.PlaySound(m_gameInfo.Sounds[soundIndexesToReels[soundIndexes[onSoundIndexesIndex]]], m_soundStatus);
                onSoundIndexesIndex++;
            }
            m_reelsPanel.Invalidate();
            DateTime startSpinTime = DateTime.Now;
            int iterations = 0;
            DateTime nextSpin = startSpinTime.AddMilliseconds(cMoveReelIterationDelay * ++iterations);
            //Go through the frames and display
            for (int frame = 0; frame < maxOffsetListLength && !bgwSpin.CancellationPending; frame++)
            {
                DateTime now = DateTime.Now;
                if (nextSpin > now)
                    Thread.Sleep(nextSpin - now);
                nextSpin = startSpinTime.AddMilliseconds(cMoveReelIterationDelay * ++iterations);
                //Maybe Play Sound
                if (onSoundIndexesIndex < soundIndexes.Count && soundIndexes[onSoundIndexesIndex] == frame)
                {
                    m_soundPlayer.PlaySound(m_gameInfo.Sounds[soundIndexesToReels[soundIndexes[onSoundIndexesIndex]]], m_soundStatus);
                    onSoundIndexesIndex++;
                }
                //
                for (int reelNum = 0; reelNum < reelCount; reelNum++)
                {
                    if (frame < reelMovements[reelNum].Count)
                    {
                        MoveReel(reelNum, indexes, reelMovements[reelNum][frame], frame == reelMovements[reelNum].Count - 1);
                    }
                }
                //if(frame == nextSoundFrame) TBD
                m_reelsPanel.Invalidate();
            } 
            SetReels(null, -1, true, true);
            m_reelsPanel.Invalidate();
        }
        void BGWWait_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(m_gameInfo.ActionPauseInMilliSeconds);
        }
        void WinLoop_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgwSpin = sender as BackgroundWorker;
            bool gameOver = (bool)e.Argument;
            List<WinArgs> winsToShow = m_gameInfo.WinsToShow;
            int onWin = 0;
            if (m_gameInfo.ActionPauseInMilliSeconds > 0)
            {
                m_gameInfo.ActionPauseInMilliSeconds = 0;
                Thread.Sleep(m_gameInfo.ActionPauseInMilliSeconds);
            }
            while (!bgwSpin.CancellationPending)
            {
                //Initial SetWinLines
                bgwSpin.ReportProgress(onWin, winsToShow);
                //Subsequent SetWinLines with maybe animations
                DateTime startSpinTime = DateTime.Now;
                for (int iterations = 1; iterations <= m_gameInfo.ShowWinDelayInFrames && !bgwSpin.CancellationPending; iterations++)
                {
                    DateTime nextSpin = startSpinTime.AddMilliseconds(m_gameInfo.ShowWinIterationDelay * iterations);
                    DateTime now = DateTime.Now;
                    if (nextSpin > now)
                        Thread.Sleep(nextSpin - now);
                    nextSpin = startSpinTime.AddMilliseconds(m_gameInfo.ShowWinIterationDelay * (iterations + 1));
                    
                    if (onWin < winsToShow.Count)
                    {
                        m_reelsPanel.SetWinAnimationIndex(iterations);
                        m_reelsPanel.Invalidate();
                    }
                }
                
                //cShowWinDelayFrames

                if (onWin >= winsToShow.Count)
                {
                    if (!gameOver) break;
                    else
                    {
                        if (frmMain.AutoPlay)
                            break;
                        onWin = 0;
                    }
                }
                else
                    onWin++;
            }
        }
        void WinLoop_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorker bgwSpin = sender as BackgroundWorker;
            int winToShowIndex = e.ProgressPercentage;
            List<WinArgs> winArgs = e.UserState as List<WinArgs>;
            if (winToShowIndex >= winArgs.Count)
            {
                m_reelsPanel.SetWinLines(null);
                if (!m_gameInfo.InFreeGames) m.gameMessageBox.Text = string.Format("{0} Total Credits Won", m_gameInfo.WinsThisGame);
                else if (m_gameInfo.InFreeGames) m.gameMessageBox.Text = string.Format("{0} Credits Won", m_gameInfo.WinsThisSpin);
            }
            else
            {
                m_reelsPanel.SetWinLines(winArgs[winToShowIndex]);
                m.gameMessageBox.Text = string.Format(winArgs[winToShowIndex].ToString());
            }
            m_reelsPanel.Invalidate();
        }
        void Animate_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgwSpin = sender as BackgroundWorker;
            while (!bgwSpin.CancellationPending && m_gameInfo.Animating)
                Thread.Sleep(33);
        }
        void BGWSpinOverCallBack_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ReelState = ReelState.Ready;
            switch (m_gameInfo.PostSpin())
            {
                case GameAction.Spin:
                    Spin();                    
                    break;
                case GameAction.Animate:
                    ReelState = ReelState.Animating;
                    StartAnimateLoop();
                    break;
                case GameAction.ShowWinsInBetweenGames:
                    if (m_gameInfo.WinsToShow.Count > 0)
                    {
                        m_gameInfo.UpdateWins();
                        m.winsTB.Text = m_gameInfo.WinsThisGame.ToString();
                        StartWinLoop(false);
                    }
                    else
                    {
                        ReelState = ReelState.Waiting;
                        m_gameInfo.ActionPauseInMilliSeconds = Math.Max(200, m_gameInfo.ActionPauseInMilliSeconds);
                        m_bgwSpin = new BackgroundWorker();
                        m_bgwSpin.DoWork += BGWWait_DoWork;
                        m_bgwSpin.WorkerSupportsCancellation = true;
                        m_bgwSpin.RunWorkerCompleted += BGWSpinOverCallBack_RunWorkerCompleted;
                        m_bgwSpin.RunWorkerAsync();
                    }
                    break;
                case GameAction.End:
                    if (m_gameInfo.InFreeGames)
                    {
                        m_gameInfo.UpdateWins();
                    }
                    frmMain.AddWinToMeter();
                    if (m_gameInfo.WinsToShow.Count > 0)
                        StartWinLoop(true);
                    else if (frmMain.AutoPlay)
                        frmMain.Spin();
                    break;
                case GameAction.Wait:
                    ReelState = ReelState.Waiting;
                    m_gameInfo.ActionPauseInMilliSeconds = Math.Max(0, m_gameInfo.ActionPauseInMilliSeconds);
                    m_bgwSpin = new BackgroundWorker();
                    m_bgwSpin.DoWork += BGWWait_DoWork;
                    m_bgwSpin.WorkerSupportsCancellation = true;
                    m_bgwSpin.RunWorkerCompleted += BGWSpinOverCallBack_RunWorkerCompleted;
                    m_bgwSpin.RunWorkerAsync();
                    break;
            }
        }
        void BGWSpinOverAutoPlayCallBack_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (frmMain.AutoPlay)
                frmMain.Spin();
        }
        //Private//////////////////////////////////////////////////////////////////////
        private void StartWinLoop(bool _gameOver)
        {
            Sound m_soundOverride = new Sound();
            ReelState = _gameOver ? ReelState.ShowingWinsAtTheEndOfAGame : ReelState.ShowingWins;
            int winAmount = _gameOver ? m_gameInfo.WinsThisGame : m_gameInfo.WinsThisSpin;
            if (winAmount >= 20 * m_gameInfo.Bet)
            {
                m_soundOverride.PlaySound(SoundTypes.Win7, m_soundStatus);
            }
            else if (winAmount >= 10 * m_gameInfo.Bet)
            {
                m_soundOverride.PlaySound(SoundTypes.Win6, m_soundStatus);
            }
            else if (winAmount >= 5 * m_gameInfo.Bet)
            {
                m_soundOverride.PlaySound(SoundTypes.Win5, m_soundStatus);
            }
            else if (winAmount >= 2 * m_gameInfo.Bet)
            {
                m_soundOverride.PlaySound(SoundTypes.Win4, m_soundStatus);
            }
            else if (winAmount >= 1 * m_gameInfo.Bet)
            {
                m_soundOverride.PlaySound(SoundTypes.Win3, m_soundStatus);
            }
            else if (winAmount >= .5 * m_gameInfo.Bet)
            {
                m_soundOverride.PlaySound(SoundTypes.Win2, m_soundStatus);
            }
            else if (winAmount > 0)
            {
                m_soundOverride.PlaySound(SoundTypes.Win1, m_soundStatus);
            }
            m_bgwSpin = new BackgroundWorker();
            m_bgwSpin.WorkerReportsProgress = true;
            m_bgwSpin.WorkerSupportsCancellation = true;
            m_bgwSpin.DoWork += WinLoop_DoWork;
            m_bgwSpin.ProgressChanged += WinLoop_ProgressChanged;
            if (!_gameOver)
                m_bgwSpin.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWSpinOverCallBack_RunWorkerCompleted);
            else if(frmMain.AutoPlay)
                m_bgwSpin.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWSpinOverAutoPlayCallBack_RunWorkerCompleted);
            m_bgwSpin.RunWorkerAsync(_gameOver);
        }
        private void StartAnimateLoop()
        {
            m_reelsPanel.StartAnimationThread();
            m_bgwSpin = new BackgroundWorker();
            m_bgwSpin.WorkerSupportsCancellation = true;
            m_bgwSpin.DoWork += Animate_DoWork;
            m_bgwSpin.RunWorkerAsync();
            m_bgwSpin.RunWorkerCompleted += BGWSpinOverCallBack_RunWorkerCompleted;
        }
        private void MoveReel(int _reelNum, List<int> _indexes, int _movement, bool _spinStop)
        {
            m_reelsPanel.Stopped = false;
            m_reelsPanel.SymbolOffsets[_reelNum] += _movement;
            bool setReel = false;
            if (m_reelsPanel.SymbolOffsets[_reelNum] >= 0)
            {
                m_reelsPanel.SymbolOffsets[_reelNum] -= m_gameInfo.SymbolHeight;
                _indexes[_reelNum]--;
                setReel = true;
            }
            else if (m_reelsPanel.SymbolOffsets[_reelNum] <= -m_gameInfo.SymbolHeight)
            {
                m_reelsPanel.SymbolOffsets[_reelNum] += m_gameInfo.SymbolHeight;
                _indexes[_reelNum]++;
                setReel = true;
            }
            if (setReel)
            {
                SetReels(_indexes, _reelNum, _spinStop);
            }
        }
        private void SetReels(List<int> _indexes = null, int _reelNum = -1, bool _reelStop = false, bool _spinStop = false)
        {
            if (_indexes == null)
            {
                _indexes = m_gameInfo.ReelIndexes;
                for (int reelNum = 0; reelNum < _indexes.Count; reelNum++)
                    m_reelsPanel.SymbolOffsets[reelNum] = 0;
                m_reelsPanel.Stopped = true;
            }
            if (_reelNum == -1)
            {
                List<int> dimensions = m_gameInfo.Dimensions;
                for (int reelNum = 0; reelNum < dimensions.Count; reelNum++)
                    for (int i = 0; i < dimensions[reelNum] + 1; i++)
                        m_reelsPanel.SymbolsOnReel[reelNum][i] = m_gameInfo.GetSymbolName(reelNum, _indexes[reelNum] + i, _reelStop ? i : -1, _spinStop);
            }
            else
            {
                for (int i = 0; i < m_gameInfo.Dimensions[_reelNum] + 1; i++)
                    m_reelsPanel.SymbolsOnReel[_reelNum][i] = m_gameInfo.GetSymbolName(_reelNum, _indexes[_reelNum] + i, _reelStop ? i : -1, _spinStop);
            }
        }
        private void CreateReelMovements(out List<List<int>> _reelMovements, out List<int> _indexes, out SortedDictionary<int, int> _soundIndexesToReels, List<int> _spinOffsetsStart, List<int> _spinOffsetsContinue, List<int> _spinOffsetsStop, int _symbolsStart, int _symbolsContinue, int _symbolsStop)
        {
            _indexes = m.MakeNewList<int>(m_gameInfo.ReelCount, 0);
            _reelMovements = new List<List<int>>();
            _soundIndexesToReels = new SortedDictionary<int,int>();
            for (int reelNum = 0; reelNum < m_gameInfo.ReelCount; reelNum++)
            {
                int extraContinues = m_gameInfo.SpinOrder[reelNum];
                List<int> reelMovementsForReel = new List<int>();
                if (extraContinues >= 0)
                {
                    reelMovementsForReel.AddRange(_spinOffsetsStart);
                    for (int i = 0; i < extraContinues; i++)
                        reelMovementsForReel.AddRange(_spinOffsetsContinue);
                    reelMovementsForReel.AddRange(_spinOffsetsStop);
                    _indexes[reelNum] = m_gameInfo.ReelIndexes[reelNum] + _symbolsStart + _symbolsContinue * extraContinues + _symbolsStop;
                    int soundIndex = _spinOffsetsStart.Count + extraContinues * _spinOffsetsContinue.Count + 11;//Hardcoded 11th index on the spinOffsetsStop
                    _soundIndexesToReels[soundIndex] = reelNum;
                }
                else
                {
                    _indexes[reelNum] = m_gameInfo.ReelIndexes[reelNum];
                    _soundIndexesToReels[0] = reelNum;
                }
                _reelMovements.Add(reelMovementsForReel);
            }
        }
    }
}
