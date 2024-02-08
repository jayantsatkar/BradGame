using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Slot_Simulator
{
    public partial class Simulation : Form
    {
        public Simulation()
        {
            InitializeComponent();
        }
        //Variables
        GameInfo m_gameInfo;
        BackgroundWorker m_backgroundWorker;
        bool m_flagPreview, m_flagExport;
        string m_gameInfoFilePath;
        //
        private void frmSimulation_Load(object sender, EventArgs e)
        {
            m_flagPreview = false;
            m_flagExport = false;
        }
        internal void InitializeGameInfo(GameInfo _gameInfo)
        {
            m_gameInfo = _gameInfo;
        }
        internal void InitializeGameInfo(string _gameInfoFilePath)
        {
            m_gameInfoFilePath = _gameInfoFilePath;
        }
        private void buttonPreview_Click(object sender, EventArgs e)
        {
            m_flagPreview = true;
        }
        private void buttonExport_Click(object sender, EventArgs e)
        {
            m_flagExport = true;
        }
        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (buttonRun.Text == "&Run")
            {
                checkBoxDefaultWinDistributions.Enabled = false;
                checkBoxWinsByPay.Enabled = false;
                checkBoxCustomStats.Enabled = false;
                listOfWinsCB.Enabled = false;
                textBoxTrials.Enabled = false;
                buttonPreview.Enabled = true;
                buttonExport.Enabled = true;
                jackpotCB.Enabled = false;
                overallCB.Enabled = false;
                gameStatsCB.Enabled = false;
                todCB.Enabled = false;
                stepthroughCB.Enabled = false;
                tbBetLevel.Enabled = false;
                buttonRun.Text = "&Cancel";
                m_backgroundWorker = new BackgroundWorker();
                m_backgroundWorker.WorkerReportsProgress = true;
                m_backgroundWorker.WorkerSupportsCancellation = true;
                m_backgroundWorker.DoWork += new DoWorkEventHandler(BGWorker_DoWork);
                m_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BGWorker_ProgressChanged);
                m_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWorker_RunWorkerCompleted);
                m_backgroundWorker.RunWorkerAsync();
            }
            else
            {
                if (!m_backgroundWorker.CancellationPending)
                    m_backgroundWorker.CancelAsync();
            }
        }
        private void frmSimulation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (buttonRun.Text != "&Run")
            {
                if (!m_backgroundWorker.CancellationPending)
                    m_backgroundWorker.CancelAsync();
                e.Cancel = true;
            }
        }
        //Background Workers///////////////////////////////////////////////////////////////////////////////////////////////////
        void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            if (m_gameInfo == null)
                m_gameInfo = GameInfo.CreateGameInfo(m_gameInfoFilePath);

            SimulationArgs simArg = new SimulationArgs(
                overallCB.Checked,
                gameStatsCB.Checked,
                jackpotCB.Checked,
                checkBoxDefaultWinDistributions.Checked,
                todCB.Checked,
                checkBoxWinsByPay.Checked,
                checkBoxCustomStats.Checked,
                listOfWinsCB.Checked,
                stepthroughCB.Checked,
                m_gameInfo,
                int.Parse(textBoxTrials.Text),
                int.Parse(tbBetLevel.Text));

            //m_gameInfo.SetUpBingoSimulation(checkBoxUseBingo.Checked);
            //Set Up
            simArg.SetUpStats();
            DateTime lastUpdate = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddSeconds(30);
            int tillNextCheck = 1000;
            while (!bgWorker.CancellationPending && !simArg.RunTrialAndIsFinished())
            {
                if (m_flagPreview)
                {
                    m_flagPreview = false;
                    bgWorker.ReportProgress(0, simArg);
                }
                else if (m_flagExport)
                {
                    m_flagExport = false;
                    bgWorker.ReportProgress(1, simArg);
                }
                if (tillNextCheck-- >= 0)
                {
                    tillNextCheck = 1000;
                    if (DateTime.Now > nextUpdate)
                    {
                        bgWorker.ReportProgress(2, simArg);
                        nextUpdate = nextUpdate.AddSeconds(30);
                    }
                }
            }
            m_gameInfo.InSimulation = false;
            m_gameInfo.DoDefaultWinDistributions = false;
            m_gameInfo.DoCustomStats = false;
            e.Result = simArg;
        }
        void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SimulationArgs simArg = e.UserState as SimulationArgs;
            if (e.ProgressPercentage == 0)
            {
                richTextBoxResults.Text = simArg.GetResultsAsText();
                textBoxStatus.Text = simArg.ETA;
            }
            else if (e.ProgressPercentage == 1)
            {
                ExcelFile.CreateExcel(simArg.GetResults());
                textBoxStatus.Text = simArg.ETA;
            }
            else
            {
                textBoxStatus.Text = simArg.ETA;
            }
        }
        void BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SimulationArgs simArg = e.Result as SimulationArgs;
            checkBoxDefaultWinDistributions.Enabled = true;
            checkBoxWinsByPay.Enabled = true;
            checkBoxCustomStats.Enabled = true;
            textBoxTrials.Enabled = true;
            tbBetLevel.Enabled = true;
            listOfWinsCB.Enabled = true;
            jackpotCB.Enabled = true;
            overallCB.Enabled = true;
            todCB.Enabled = true;
            gameStatsCB.Enabled = true;
            stepthroughCB.Enabled = true;
            m_backgroundWorker = null;
            richTextBoxResults.Text = simArg.GetResultsAsText();
            buttonRun.Text = "&Run";
            if (simArg.IsFinished)
            {
                ExcelFile.SaveExcel(simArg.GetResults(), string.Format(@"{0}\z {1} Simulation - Bet Level {2}.xlsx", m.Directory, simArg.GameInfo.FileNameWithoutGameData, simArg.GameInfo.BetLevel));
            }
        }

        private void stepthroughCB_CheckedChanged(object sender, EventArgs e)
        {
            if(stepthroughCB.Checked)
            {
                overallCB.Checked = false;
                gameStatsCB.Checked = false;
                jackpotCB.Checked = false;
                checkBoxDefaultWinDistributions.Checked = false;
                todCB.Checked = false;
                checkBoxWinsByPay.Checked = true;
                checkBoxCustomStats.Checked = false;
                listOfWinsCB.Checked = false;
                textBoxTrials.Enabled = false;
            }
            else if (!stepthroughCB.Checked)
            {
                overallCB.Checked = true;
                gameStatsCB.Checked = true;
                jackpotCB.Checked = true;
                checkBoxDefaultWinDistributions.Checked = true;
                todCB.Checked = true;
                checkBoxWinsByPay.Checked = true;
                checkBoxCustomStats.Checked = true;
                listOfWinsCB.Checked = true;
                textBoxTrials.Enabled = true;
            }
        }
    }
    class SimulationArgs
    {
        long m_trials, m_trialsSoFar, m_totalCredits, m_trailsSinceLastETA;
        int m_betLevelForSim, m_betForSim;
        List<uint> m_winCategoryAmounts;
        List<double> m_multipliers;
        internal GameInfo GameInfo;
        bool m_defaultWinDistributions, m_winsByPay, m_customStats, m_listOfWins, m_overallStats, m_jackpotStats, m_tODStats, m_gameStats, m_stepThrough;
        Dictionary<string, Dictionary<string, PayCountArg>> m_payTablesSeperated;
        Dictionary<string, PayCountArg> m_payTablesTotal;
        SortedDictionary<int, long> m_wins;
        DateTime m_dateTimeSinceLastETA;
        object m_lock = new object();
        List<double> m_averageWins = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        List<double> m_variances = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        List<double> m_volatilityOfGame = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        List<int> m_maxPays = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        List<double> m_medianPays = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        List<double> m_1stQuartiles = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        List<double> m_3rdQuartiles = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        List<double> m_9thDeciles = new List<double> { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        gameStatsArg gameStatsLine;
        internal SimulationArgs(bool _overallStats, bool _dogameStatsLine, bool _jackpotStats, bool _defaultWinDistributions, bool _tOD, bool _winsByPay, bool _customStats, bool _listOfWinsCheck, bool _stepThrough,
                                GameInfo _gameInfo, long _trials, int _betLevel)
        {
            m_overallStats = _overallStats;
            m_jackpotStats = _jackpotStats;
            m_defaultWinDistributions = _defaultWinDistributions;
            m_tODStats = _tOD;
            m_winsByPay = _winsByPay;
            m_customStats = _customStats;
            m_listOfWins = _listOfWinsCheck;
            m_betLevelForSim = _betLevel;
            m_gameStats = _dogameStatsLine;
            m_stepThrough = _stepThrough;
            GameInfo = _gameInfo;
            GameInfo.BetLevel = m_betLevelForSim;
            GameInfo.m_bankSize = 1;
            if (!_stepThrough) m_trials = _trials;
            else if (_stepThrough) m_trials = (long)GameInfo.m_cycleSize;
            m_multipliers = m.Multipliers;
            m_betForSim = GameInfo.Bet * m_betLevelForSim;
            m_trialsSoFar = 0;
            m_totalCredits = 0;
            if (m_winsByPay)
                m_winCategoryAmounts = m.MultipliersToCredits(m_multipliers, GameInfo.Bet * m_betLevelForSim);
            m_trailsSinceLastETA = 0;
            m_dateTimeSinceLastETA = DateTime.Now;
            m_wins = new SortedDictionary<int, long>();
            gameStatsLine = new gameStatsArg();
        }
        internal bool IsFinished { get { return m_trials == m_trialsSoFar; } }
        internal string ETA
        {
            get
            {
                if (m_trailsSinceLastETA > 0)
                {
                    DateTime now = DateTime.Now;
                    TimeSpan changeInTime = now - m_dateTimeSinceLastETA;
                    m_dateTimeSinceLastETA = now;
                    DateTime willFinish = now.AddTicks(changeInTime.Ticks * (m_trials - m_trialsSoFar) / m_trailsSinceLastETA);
                    m_trailsSinceLastETA = 0;
                    return willFinish.ToString();
                }
                else return "Error";
            }
        }
        internal double CurrentRTP { get { return (double)m_totalCredits / m_trialsSoFar / GameInfo.Bet; } }
        internal double CurrentProgress { get { return (double)m_trialsSoFar / m_trials; } }
        internal void SetUpStats()
        {
            if(m_defaultWinDistributions) GameInfo.SetUpStats();
            if(m_customStats) GameInfo.SetUpCustomStats();
            if (m_tODStats) GameInfo.SetUpTODStats();
            GameInfo.InSimulation = true;
            GameInfo.DoDefaultWinDistributions = m_defaultWinDistributions;
            GameInfo.DoCustomStats = m_customStats;
            GameInfo.StepThrough = m_stepThrough;
            GameInfo.doTODStats = m_tODStats;
            if(m_stepThrough)
            {
                GameInfo.m_currentStepThroughStops = new List<int>();
                GameInfo.m_weightsForStepThrough = new List<int>();
                for(int aa = 0; aa < GameInfo.ReelCount; aa++)
                {
                    GameInfo.m_currentStepThroughStops.Add(GameInfo.Reels.ReelsPG[aa].Count() - 1);
                }
            }
            if(m_winsByPay)
                GameInfo.GetPayTableCounts(out m_payTablesSeperated, out m_payTablesTotal);
        }
        internal bool RunTrialAndIsFinished()
        {
            lock (m_lock)
            {
                GameInfo.PreSpin(GameInfo.m_stackShow);
                while (GameInfo.PostSpin() != GameAction.End) ;
                m_totalCredits += GameInfo.WinsThisGame;
                gameStatsLine.collectWinInfo(m_betForSim, GameInfo.WinsThisGame, GameInfo.WinsThisGame - GameInfo.m_statsWinsThisPG, GameInfo.m_freeGamesPlayed, GameInfo.SecondFeature);
                if (m_winsByPay && GameInfo.WinsThisGame > 0)
                {
                    foreach (Dictionary<string, PayCountArg> payTableGroup in m_payTablesSeperated.Values)
                    {
                        int category = 0;
                        int winTotal = 0;
                        foreach (WinArgs win in GameInfo.Wins)
                        {
                            if (payTableGroup.ContainsKey(win.Name))
                            {
                                winTotal += win.Amount;
                            }
                        }        
                        if (winTotal > 0)
                            for (category = 1; category < m_multipliers.Count; category++)
                                if (winTotal < m_winCategoryAmounts[category]) break;
                        foreach (WinArgs win in GameInfo.Wins)
                        {
                            if (payTableGroup.ContainsKey(win.Name))
                            {
                                if (m_stepThrough)
                                {
                                    ulong weightMultiplier = 1;
                                    for (int aa = 0; aa < GameInfo.ReelCount; aa++)
                                    {
                                        weightMultiplier *= (ulong)GameInfo.m_weightsForStepThrough[aa];
                                    }
                                    m_payTablesTotal[win.Name].WinAmounts[win.Count - 1][category] += (ulong)win.Amount * weightMultiplier;
                                    m_payTablesTotal[win.Name].Hits[win.Count - 1] += ((ulong)1 * weightMultiplier);
                                }
                                else if (!m_stepThrough)
                                {
                                    m_payTablesTotal[win.Name].WinAmounts[win.Count - 1][category] += (ulong)win.Amount;
                                    m_payTablesTotal[win.Name].Hits[win.Count - 1]++;
                                }
                            }
                        }
                    }
                }
                if (!m_wins.ContainsKey(GameInfo.WinsThisGame)) m_wins[GameInfo.WinsThisGame] = 0;
                m_wins[GameInfo.WinsThisGame]++;
                m_maxPays[0] = Math.Max(m_maxPays[0], GameInfo.WinsThisGame);
                m_maxPays[1] = Math.Max(m_maxPays[1], GameInfo.WinsThisGame - GameInfo.m_progressiveWinPG - GameInfo.m_progressiveWinFG);
                m_maxPays[2] = Math.Max(m_maxPays[2], GameInfo.m_statsWinsThisPG);
                m_maxPays[3] = Math.Max(m_maxPays[3], GameInfo.m_statsWinsThisPG - GameInfo.m_progressiveWinPG);
                m_maxPays[4] = Math.Max(m_maxPays[4], GameInfo.WinsThisGame - GameInfo.m_statsWinsThisPG);
                m_maxPays[5] = Math.Max(m_maxPays[5], GameInfo.WinsThisGame - GameInfo.m_statsWinsThisPG - GameInfo.m_totalProgressiveWinFG);
                m_maxPays[6] = Math.Max(m_maxPays[6], GameInfo.m_progressiveWinPG);
                m_maxPays[6] = Math.Max(m_maxPays[6], GameInfo.m_maxProgressiveWinThisFG);
                m_maxPays[7] = Math.Max(m_maxPays[7], GameInfo.m_progressiveWinPG);
                m_maxPays[8] = Math.Max(m_maxPays[8], GameInfo.m_maxProgressiveWinThisFG);
                m_trailsSinceLastETA++;
            }
            return ++m_trialsSoFar == m_trials;
        }
        internal List<List<string>> GetResults()
        {
            List<List<string>> results = new List<List<string>>();
            lock (m_lock)
            {
                results.Add(new List<string>(new string[] { GameInfo.GameName }));
                results.Add(new List<string>(new string[] { "Trials", m_trialsSoFar.ToString() }));
                results.Add(new List<string>(new string[] { "Bet", GameInfo.Bet.ToString() }));
                results.Add(new List<string>(new string[] { "Cycle Size", m_trials.ToString() }));
                results.Add(new List<string>(new string[] { "Trials Remaining", (m_trials - m_trialsSoFar).ToString() }));
                results.Add(new List<string>());
                if (m_overallStats)
                {
                    for (int aa = 0; aa < 9; aa++)
                    {
                        m_averageWins[aa] = getAverageWin(aa);
                        m_variances[aa] = getVariance(aa);
                        m_volatilityOfGame[aa] = getVolatility(aa);
                    }
                    foreach (int win in GameInfo.m_winsByType.Keys)
                        GameInfo.m_winsByType[win][6] = GameInfo.m_winsByType[win][7] + GameInfo.m_winsByType[win][8];

                    results.Add(new List<string>(new string[] { "Within 90% Tolerance", withinTolerance(.90).ToString() }));
                    results.Add(new List<string>(new string[] { "Within 95% Tolerance", withinTolerance(.95).ToString() }));

                    results.Add(new List<string>());
                    results.Add(new List<string>(new string[] { "", "Overall Game", "Overall Game No Progressives", "Base Game", "Base Game No Progressives", "Free Spins Bonus", 
                                                            "Free Spins Bonus No Progressives", "Progressives Overall", "Progressives Base Game", "Progressives Free Spins Bonus" }));
                    results.Add(new List<string>(new string[] { "Average Win (Credits)", string.Format("{0:0.00}", m_averageWins[0]), string.Format("{0:0.00}", m_averageWins[1]), 
                                                            string.Format("{0:0.00}", m_averageWins[2]), string.Format("{0:0.00}", m_averageWins[3]),  string.Format("{0:0.00}", m_averageWins[4]),
                                                            string.Format("{0:0.00}", m_averageWins[5]), string.Format("{0:0.00}", m_averageWins[6]), string.Format("{0:0.00}", m_averageWins[7]), 
                                                            string.Format("{0:0.00}", m_averageWins[8]) }));
                    results.Add(new List<string>(new string[] { "Average Win (Times Bet)", string.Format("{0:0.00}x", m_averageWins[0] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", m_averageWins[1] / GameInfo.Bet), string.Format("{0:0.00}x", m_averageWins[2] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", m_averageWins[3] / GameInfo.Bet), string.Format("{0:0.00}x", m_averageWins[4] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_averageWins[5] / GameInfo.Bet), string.Format("{0:0.00}x", m_averageWins[6] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_averageWins[7] / GameInfo.Bet), string.Format("{0:0.00}x", m_averageWins[8] / GameInfo.Bet) }));
                    results.Add(new List<string>(new string[] { "Variance", string.Format("{0:0.00}", m_variances[0]), string.Format("{0:0.00}", m_variances[1]), string.Format("{0:0.00}", m_variances[2]), 
                                                            string.Format("{0:0.00}", m_variances[3]), string.Format("{0:0.00}", m_variances[4]), string.Format("{0:0.00}", m_variances[5]),
                                                            string.Format("{0:0.00}", m_variances[6]), string.Format("{0:0.00}", m_variances[7]), string.Format("{0:0.00}", m_variances[8]) }));
                    results.Add(new List<string>(new string[] { "Volatility", string.Format("{0:0.00}", m_volatilityOfGame[0]), string.Format("{0:0.00}", m_volatilityOfGame[1]), 
                                                            string.Format("{0:0.00}", m_volatilityOfGame[2]), string.Format("{0:0.00}", m_volatilityOfGame[3]), string.Format("{0:0.00}", m_volatilityOfGame[4]),
                                                            string.Format("{0:0.00}", m_volatilityOfGame[5]), string.Format("{0:0.00}", m_volatilityOfGame[6]), string.Format("{0:0.00}", m_volatilityOfGame[7]),
                                                            string.Format("{0:0.00}", m_volatilityOfGame[8]) }));
                    results.Add(new List<string>(new string[] { "Max Pay (Credits)", string.Format("{0:0}", m_maxPays[0]), string.Format("{0:0}", m_maxPays[1]), string.Format("{0:0}", m_maxPays[2]), 
                                                            string.Format("{0:0}", m_maxPays[3]), string.Format("{0:0}", m_maxPays[4]), string.Format("{0:0}", m_maxPays[5]), 
                                                            string.Format("{0:0}", m_maxPays[6]), string.Format("{0:0}", m_maxPays[7]), string.Format("{0:0}", m_maxPays[8]) }));
                    results.Add(new List<string>(new string[] { "Max Pay (Times Bet)", string.Format("{0:0.00}x", (double)m_maxPays[0] / (double)GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", (double)m_maxPays[1] / GameInfo.Bet), string.Format("{0:0.00}x", (double)m_maxPays[2] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", (double)m_maxPays[3] / GameInfo.Bet), string.Format("{0:0.00}x", (double)m_maxPays[4] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", (double)m_maxPays[5] / GameInfo.Bet), string.Format("{0:0.00}x", (double)m_maxPays[6] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", (double)m_maxPays[7] / GameInfo.Bet), string.Format("{0:0.00}x", (double)m_maxPays[8] / GameInfo.Bet) }));
                    results.Add(new List<string>(new string[] { "Median (Credits)", string.Format("{0:0}", m_medianPays[0]), string.Format("{0:0}", m_medianPays[1]), string.Format("{0:0}", m_medianPays[2]),
                                                            string.Format("{0:0}", m_medianPays[3]), string.Format("{0:0}", m_medianPays[4]), string.Format("{0:0}", m_medianPays[5]),
                                                            string.Format("{0:0}", m_medianPays[6]), string.Format("{0:0}", m_medianPays[7]), string.Format("{0:0}", m_medianPays[8]) }));
                    results.Add(new List<string>(new string[] { "Median (Times Bet)", string.Format("{0:0.00}x", m_medianPays[0] / GameInfo.Bet), string.Format("{0:0.00}x", m_medianPays[1] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", m_medianPays[2]/ GameInfo.Bet), string.Format("{0:0.00}x", m_medianPays[3]/ GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_medianPays[4]/ GameInfo.Bet), string.Format("{0:0.00}x", m_medianPays[5]/ GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_medianPays[6]/ GameInfo.Bet), string.Format("{0:0.00}x", m_medianPays[7]/ GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_medianPays[8]/ GameInfo.Bet)}));
                    results.Add(new List<string>(new string[] { "1st Quartile (Credits)", string.Format("{0:0}", m_1stQuartiles[0]), string.Format("{0:0}", m_1stQuartiles[1]), 
                                                            string.Format("{0:0}", m_1stQuartiles[2]), string.Format("{0:0}", m_1stQuartiles[3]), string.Format("{0:0}", m_1stQuartiles[4]), 
                                                            string.Format("{0:0}", m_1stQuartiles[5]), string.Format("{0:0}", m_1stQuartiles[6]), string.Format("{0:0}", m_1stQuartiles[7]),
                                                            string.Format("{0:0}", m_1stQuartiles[8]) }));
                    results.Add(new List<string>(new string[] { "1st Quartile (Times Bet)", string.Format("{0:0.00}x", m_1stQuartiles[0] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", m_1stQuartiles[1] / GameInfo.Bet), string.Format("{0:0.00}x", m_1stQuartiles[2] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_1stQuartiles[3] / GameInfo.Bet), string.Format("{0:0.00}x", m_1stQuartiles[4] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_1stQuartiles[5] / GameInfo.Bet), string.Format("{0:0.00}x", m_1stQuartiles[6] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_1stQuartiles[7] / GameInfo.Bet), string.Format("{0:0.00}x", m_1stQuartiles[8] / GameInfo.Bet) }));
                    results.Add(new List<string>(new string[] { "3rd Quartile (Credits)", string.Format("{0:0}", m_3rdQuartiles[0]), string.Format("{0:0}", m_3rdQuartiles[1]), 
                                                            string.Format("{0:0}", m_3rdQuartiles[2]),  string.Format("{0:0}", m_3rdQuartiles[3]), string.Format("{0:0}", m_3rdQuartiles[4]),
                                                            string.Format("{0:0}", m_3rdQuartiles[5]), string.Format("{0:0}", m_3rdQuartiles[6]), string.Format("{0:0}", m_3rdQuartiles[7]),
                                                            string.Format("{0:0}", m_3rdQuartiles[8]) }));
                    results.Add(new List<string>(new string[] { "3rd Quartile (Times Bet)", string.Format("{0:0.00}x", m_3rdQuartiles[0] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", m_3rdQuartiles[1] / GameInfo.Bet),  string.Format("{0:0.00}x", m_3rdQuartiles[2] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", m_3rdQuartiles[3] / GameInfo.Bet),  string.Format("{0:0.00}x", m_3rdQuartiles[4] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_3rdQuartiles[5] / GameInfo.Bet),  string.Format("{0:0.00}x", m_3rdQuartiles[6] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_3rdQuartiles[7] / GameInfo.Bet),  string.Format("{0:0.00}x", m_3rdQuartiles[8] / GameInfo.Bet) }));
                    results.Add(new List<string>(new string[] { "9th Decile (Credits)", string.Format("{0:0}", m_9thDeciles[0]), string.Format("{0:0}", m_9thDeciles[1]), 
                                                            string.Format("{0:0}", m_9thDeciles[2]), string.Format("{0:0}", m_9thDeciles[3]), string.Format("{0:0}", m_9thDeciles[4]), 
                                                            string.Format("{0:0}", m_9thDeciles[5]), string.Format("{0:0}", m_9thDeciles[6]), string.Format("{0:0}", m_9thDeciles[7]),
                                                            string.Format("{0:0}", m_9thDeciles[8]) }));
                    results.Add(new List<string>(new string[] { "9th Decile (Times Bet)", string.Format("{0:0.00}x", m_9thDeciles[0] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x", m_9thDeciles[1] / GameInfo.Bet), string.Format("{0:0.00}x", m_9thDeciles[2] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_9thDeciles[3] / GameInfo.Bet), string.Format("{0:0.00}x", m_9thDeciles[4] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_9thDeciles[5] / GameInfo.Bet), string.Format("{0:0.00}x", m_9thDeciles[6] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x", m_9thDeciles[7] / GameInfo.Bet), string.Format("{0:0.00}x", m_9thDeciles[8] / GameInfo.Bet) }));
                    results.Add(new List<string>(new string[] { "9th Decile Range (Credits)", string.Format("{0:0} - {1:0}", m_9thDeciles[0], m_maxPays[0]), 
                                                            string.Format("{0:0} - {1:0}", m_9thDeciles[1], m_maxPays[1]), string.Format("{0:0} - {1:0}", m_9thDeciles[2], m_maxPays[2]),
                                                            string.Format("{0:0} - {1:0}", m_9thDeciles[3], m_maxPays[3]), string.Format("{0:0} - {1:0}", m_9thDeciles[4], m_maxPays[4]),
                                                            string.Format("{0:0} - {1:0}", m_9thDeciles[5], m_maxPays[5]), string.Format("{0:0} - {1:0}", m_9thDeciles[6], m_maxPays[6]),
                                                            string.Format("{0:0} - {1:0}", m_9thDeciles[7], m_maxPays[7]), string.Format("{0:0} - {1:0}", m_9thDeciles[8], m_maxPays[8]) }));
                    results.Add(new List<string>(new string[] { "9th Decile Range (Times Bet)", string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[0] / GameInfo.Bet, m_maxPays[0] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[1] / GameInfo.Bet, m_maxPays[1] / GameInfo.Bet), 
                                                            string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[2] / GameInfo.Bet, m_maxPays[2] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[3] / GameInfo.Bet, m_maxPays[3] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[4] / GameInfo.Bet, m_maxPays[4] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[5] / GameInfo.Bet, m_maxPays[5] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[6] / GameInfo.Bet, m_maxPays[6] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[7] / GameInfo.Bet, m_maxPays[7] / GameInfo.Bet),
                                                            string.Format("{0:0.00}x - {1:0.00}x", m_9thDeciles[8] / GameInfo.Bet, m_maxPays[8] / GameInfo.Bet) }));
                    results.Add(new List<string>(new string[] { "Median To Mean", string.Format("{0:0.00}", m_medianPays[0] / m_averageWins[0]), 
                                                            string.Format("{0:0.00}", m_medianPays[1] / m_averageWins[1]), string.Format("{0:0.00}", m_medianPays[2] / m_averageWins[2]), 
                                                            string.Format("{0:0.00}", m_medianPays[3] / m_averageWins[3]), string.Format("{0:0.00}", m_medianPays[4] / m_averageWins[4]), 
                                                            string.Format("{0:0.00}", m_medianPays[5] / m_averageWins[5]), string.Format("{0:0.00}", m_medianPays[6] / m_averageWins[6]),
                                                            string.Format("{0:0.00}", m_medianPays[7] / m_averageWins[7]), string.Format("{0:0.00}", m_medianPays[8] / m_averageWins[8]),}));
                    results.Add(new List<string>());
                }

                if(m_gameStats)
                {
                    getGameStatsLine();
                    gameStatsLine.InputResults(results, m_volatilityOfGame[1]);
                }

                if (m_jackpotStats) GameInfo.DisplayJackpotStats(results, m_trialsSoFar);

                if (m_defaultWinDistributions) GameInfo.DisplayStats(results);

                if (m_tODStats) GameInfo.DisplayTODStats(results);

                if (m_winsByPay)
                {
                    foreach (string payTableName in m_payTablesSeperated.Keys)
                    {
                        Dictionary<string, PayCountArg> payTable = m_payTablesSeperated[payTableName];
                        results.Add(new List<string>(new string[] { payTableName }));
                        ulong totalWin = 0;
                        ulong totalHits = 0;
                        double totalRTP = 0;
                        //Headers
                        List<string> headers = new List<string>(new string[] { "Name", "Pay", "Hits", "Total Pays", "RTP" });
                        headers.AddRange(m.MultipliersToLabels(m_multipliers));
                        results.Add(headers);
                        //Each Pay Symbol
                        foreach (string paySymbol in payTable.Keys)
                        {
                            for (int match = payTable[paySymbol].Pays.Count; match > 0; match--)
                                if (payTable[paySymbol].Pays[match - 1] > 0)
                                {
                                    string name = string.Format("{0} _ {1}", paySymbol, match);
                                    int pay = payTable[paySymbol].Pays[match - 1];
                                    ulong totalWinOfPaySymbol = 0;
                                    ulong totalHitsofPaySymbol = 0;
                                    double rTPOfPaySymbol = 0;
                                    List<ulong> winAmountsForPay = payTable[paySymbol].WinAmounts[match - 1];
                                    foreach (ulong winAmount in winAmountsForPay)
                                        totalWinOfPaySymbol += winAmount;
                                    totalWin += totalWinOfPaySymbol;
                                    if(GameInfo.m_normalEvaluationType != "line") totalHitsofPaySymbol = payTable[paySymbol].Hits[match - 1];
                                    else if (GameInfo.m_normalEvaluationType == "line") totalHitsofPaySymbol = payTable[paySymbol].Hits[match - 1] / (ulong)GameInfo.Lines;
                                    totalHits += totalHitsofPaySymbol;
                                    if (m_stepThrough) rTPOfPaySymbol = (double)totalWinOfPaySymbol / (double)GameInfo.m_cycleSizeForCalc / (double)GameInfo.Bet;
                                    else if (!m_stepThrough) rTPOfPaySymbol = (double)totalWinOfPaySymbol / (double)m_trialsSoFar / (double)GameInfo.Bet;
                                    totalRTP += rTPOfPaySymbol;
                                    List<string> row = new List<string>(new string[] { 
                                        name,
                                        pay.ToString(),
                                        totalHitsofPaySymbol.ToString(),
                                        totalWinOfPaySymbol.ToString(),
                                        string.Format("{0:0.0000%}",rTPOfPaySymbol)
                                    });
                                    //Rest
                                    for (int category = 0; category < winAmountsForPay.Count; category++)
                                    {
                                        double rTPofCategory = 0;
                                        if (m_stepThrough) rTPofCategory = (double)winAmountsForPay[category] / (double)GameInfo.m_cycleSizeForCalc / (double)GameInfo.Bet;
                                        else if (!m_stepThrough) rTPofCategory = (double)winAmountsForPay[category] / (double)m_trialsSoFar / (double)GameInfo.Bet;
                                        if (winAmountsForPay[category] == 0) row.Add("");
                                        else if (winAmountsForPay[category] > 0) row.Add(string.Format("{0:0.0000%}", rTPofCategory));
                                    }
                                    results.Add(row);
                                }
                        }
                        results.Add(new List<string>(new string[] { "Total", "", totalHits.ToString(), totalWin.ToString(), string.Format("{0:0.0000%}", totalRTP) }));
                        results.Add(new List<string>());
                    }
                }

                if (m_customStats) GameInfo.DisplayCustomStats(results);

                if(m_listOfWins) GameInfo.DisplayWinsByType(results);
            }
            return results;
        }
        internal string GetResultsAsText()
        {
            StringBuilder resultsString = new StringBuilder();
            List<List<string>> results = GetResults();
            int maxCol = 1;
            foreach (List<string> resultsRow in results)
                maxCol = Math.Max(maxCol, resultsRow.Count);
            List<int> columnWidths = m.MakeNewList<int>(maxCol, 2);
            foreach (List<string> resultsRow in results)
                for (int i = 0; i < resultsRow.Count; i++)
                    columnWidths[i] = Math.Max(columnWidths[i], resultsRow[i].Length);
            foreach (List<string> resultsRow in results)
            {
                for (int i = 0; i < resultsRow.Count; i++)
                {
                    if (i == 0)
                        resultsString.Append(resultsRow[i].PadRight(columnWidths[i] + 2));
                    else
                        resultsString.Append(resultsRow[i].PadLeft(columnWidths[i] + 2));
                }
                resultsString.AppendLine();
            }
            return resultsString.ToString();
        }

        internal double getAverageWin(int currentIndex)
        {
            double averageToReturn = 0;
            long currentHits = 0;
            long currentGames = 0;
            bool firstQuartile = false;
            bool medianRecorded = false;
            bool thirdQuartile = false;
            bool ninthDecile = false;
            if (currentIndex < 4) currentGames = m_trialsSoFar;
            else if (currentIndex == 4 || currentIndex == 5) currentGames = GameInfo.m_numberOfFreeGameSessions;
            else if (currentIndex == 6) currentGames = GameInfo.m_progressiveGames;
            else if (currentIndex == 7) currentGames = GameInfo.m_pgWithProgressives;
            else if (currentIndex == 8) currentGames = GameInfo.m_fgWithProgressives;
            if (currentIndex != 4 && currentIndex != 5)
            {
                foreach (int winAmount in GameInfo.m_winsByType.Keys)
                {
                    averageToReturn += (winAmount) * ((double)GameInfo.m_winsByType[winAmount][currentIndex] / (double)currentGames);
                    if (winAmount > 0) currentHits += GameInfo.m_winsByType[winAmount][currentIndex];
                    if (currentHits >= (((double)currentGames - GameInfo.m_winsByType[0][currentIndex]) / 2) && !medianRecorded)
                    {
                        m_medianPays[currentIndex] = winAmount;
                        medianRecorded = true;
                    }
                    if (currentHits >= (((double)currentGames - GameInfo.m_winsByType[0][currentIndex]) / 4) && !firstQuartile)
                    {
                        m_1stQuartiles[currentIndex] = winAmount;
                        firstQuartile = true;
                    }
                    if (currentHits >= ((((double)currentGames - GameInfo.m_winsByType[0][currentIndex]) / 4) * 3) && !thirdQuartile)
                    {
                        m_3rdQuartiles[currentIndex] = winAmount;
                        thirdQuartile = true;
                    }
                    if (currentHits >= ((((double)currentGames - GameInfo.m_winsByType[0][currentIndex]) / 10) * 9) && !ninthDecile)
                    {
                        m_9thDeciles[currentIndex] = winAmount;
                        ninthDecile = true;
                    }
                }
            }
            else if (currentIndex == 4 || currentIndex == 5)
            {
                foreach (int winAmount in GameInfo.m_winsByType.Keys)
                {
                    averageToReturn += (winAmount) * ((double)GameInfo.m_winsByType[winAmount][currentIndex] / (double)currentGames);
                    if (winAmount >= 0) currentHits += GameInfo.m_winsByType[winAmount][currentIndex];
                    if (currentHits >= (((double)currentGames) / 2) && !medianRecorded)
                    {
                        m_medianPays[currentIndex] = winAmount;
                        medianRecorded = true;
                    }
                    if (currentHits >= (((double)currentGames) / 4) && !firstQuartile)
                    {
                        m_1stQuartiles[currentIndex] = winAmount;
                        firstQuartile = true;
                    }
                    if (currentHits >= ((((double)currentGames) / 4) * 3) && !thirdQuartile)
                    {
                        m_3rdQuartiles[currentIndex] = winAmount;
                        thirdQuartile = true;
                    }
                    if (currentHits >= ((((double)currentGames) / 10) * 9) && !ninthDecile)
                    {
                        m_9thDeciles[currentIndex] = winAmount;
                        ninthDecile = true;
                    }
                }
            }
            return averageToReturn;
        }

        internal double getVariance(int currentIndex)
        {
            double varianceToReturn = 0;
            long currentGames = 0;
            if (currentIndex < 4) currentGames = m_trialsSoFar;
            else if (currentIndex == 4 || currentIndex == 5) currentGames = GameInfo.m_numberOfFreeGameSessions;
            else if (currentIndex == 6) currentGames = GameInfo.m_progressiveGames;
            else if (currentIndex == 7) currentGames = GameInfo.m_pgWithProgressives;
            else if (currentIndex == 8) currentGames = GameInfo.m_fgWithProgressives;
            foreach(int winAmount in GameInfo.m_winsByType.Keys)
            {
                varianceToReturn += ((winAmount - m_averageWins[currentIndex]) / GameInfo.Bet * (winAmount - m_averageWins[currentIndex]) / GameInfo.Bet * ((double)GameInfo.m_winsByType[winAmount][currentIndex] / (double)currentGames));
            }
            return varianceToReturn;
        }

        internal double getVolatility(int currentIndex)
        {
            return 1.645 * Math.Sqrt(m_variances[currentIndex]);
        }

        internal bool withinTolerance(double interval)
        {
            bool valueToReturn = true;
            double factor = 1.645;
            double payBack = GameInfo.m_gamePayback;
            if (interval == .95) factor = 1.96;
            double confidence = factor * Math.Sqrt(m_variances[0]) / Math.Sqrt((double)m_trialsSoFar);
            double lowerBound = payBack - confidence;
            double upperBound = payBack + confidence;
            if (CurrentRTP <= upperBound && CurrentRTP >= lowerBound) valueToReturn = true;
            else if (CurrentRTP > upperBound || CurrentRTP < lowerBound) valueToReturn = false;
            return valueToReturn;
        }

        internal void getGameStatsLine()
        {
            gameStatsLine.gameName = GameInfo.GameName;
            gameStatsLine.costToCover = m_betForSim;
            gameStatsLine.rTP = CurrentRTP;
            gameStatsLine.baseGame = (double)gameStatsLine.totalPGWin / ((double)m_trialsSoFar * (double)m_betForSim);
            gameStatsLine.bonusGame = CurrentRTP - gameStatsLine.baseGame;
            gameStatsLine.hitFreq = (double)gameStatsLine.totalHits / (double)m_trialsSoFar;
            gameStatsLine.baseGameFeatureFreq = (double)m_trialsSoFar / (double)gameStatsLine.baseGameFeatures;
            gameStatsLine.bonusFreq = (double)m_trialsSoFar / (double)gameStatsLine.freeSpinsBonusPlayed;
            gameStatsLine.bonusMedToMean = m_medianPays[4] / m_averageWins[4];
            gameStatsLine.bonusMean = m_averageWins[4] / (double)m_betForSim;
            gameStatsLine.bonus1stQuartile = m_1stQuartiles[4] / (double)m_betForSim;
            gameStatsLine.bonus2ndQuartile = m_medianPays[4] / (double)m_betForSim;
            gameStatsLine.bonus3rdQuartile = m_3rdQuartiles[4] / (double)m_betForSim;
            gameStatsLine.bonus9thDecile = m_9thDeciles[4] / (double)m_betForSim;
            gameStatsLine.tenTimesOdds = (double)m_trialsSoFar / (double)gameStatsLine.tenTimesHits;
            gameStatsLine.hundredTimesOdds = (double)m_trialsSoFar / (double)gameStatsLine.hundredTimesHits;
            gameStatsLine.maxWin = (double)m_maxPays[0] / (double)m_betForSim;
            gameStatsLine.baseVolatility = m_volatilityOfGame[3];
            gameStatsLine.totalVolatility = m_volatilityOfGame[0];
            if (m_tODStats)
            {
                for (int aa = 0; aa < GameInfo.m_setsForSimulation.Count; aa++)
                {
                    if (GameInfo.m_setsForSimulation[aa].startingBankroll == 50 * m_betForSim)
                    {
                        GameInfo.m_setsForSimulation[aa].getOverallStatsForSets();
                        gameStatsLine.normalizedTOD = GameInfo.m_setsForSimulation[aa].averageSpins;
                        gameStatsLine.medianTOD = GameInfo.m_setsForSimulation[aa].medianSpins;
                        gameStatsLine.cashoutPercent = GameInfo.m_setsForSimulation[aa].winningSessions;
                        gameStatsLine.cashoutMultiple = GameInfo.m_setsForSimulation[aa].averageCashoutAmount / (double)m_betForSim;
                        gameStatsLine.cashoutWBonus = 1 - GameInfo.m_setsForSimulation[aa].cashoutsW0Bonuses;
                        gameStatsLine.sessionWBonus = GameInfo.m_setsForSimulation[aa].sessionsWBonuses;
                    }
                }
            }
            else if (!m_tODStats)
            {
                gameStatsLine.normalizedTOD = 0;
                gameStatsLine.medianTOD = 0;
                gameStatsLine.cashoutPercent = 0;
                gameStatsLine.cashoutMultiple = 0;
                gameStatsLine.cashoutWBonus = 0;
                gameStatsLine.sessionWBonus = 0;
            }
        }
    }
}
