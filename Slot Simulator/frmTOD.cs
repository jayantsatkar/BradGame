using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Slot_Simulator
{
    public partial class frmTOD : Form
    {
        private int sessionsToPlay = 0;
        private int setsToPlay = 0;
        private int betLevel = 0;
        private int startingCredits = 0;
        private int sessionEndCredits = 0;
        private string outputFile = "";
        GameInfo m_gameInfo;
        BackgroundWorker m_backgroundWorker;
        string m_gameInfoFilePath;

        public frmTOD()
        {
            InitializeComponent();
        }

        private void frmTOD_Load(object sender, EventArgs e)
        {
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if(startButton.Text == "Start")
            {
                startButton.Text = "Cancel";
                enableDisable(true);
                readVariables();
                statusRTB.Text = "Time On Device Simulation Started";
                m_backgroundWorker = new BackgroundWorker();
                m_backgroundWorker = new BackgroundWorker();
                m_backgroundWorker.WorkerReportsProgress = true;
                m_backgroundWorker.WorkerSupportsCancellation = true;
                m_backgroundWorker.DoWork += new DoWorkEventHandler(BGWorker_DoWork);
                m_backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BGWorker_ProgressChanged);
                m_backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWorker_RunWorkerCompleted);
                m_backgroundWorker.RunWorkerAsync();
            }
            else if (startButton.Text == "Cancel")
            {
                startButton.Text = "Start";
                enableDisable(false);
                if (!m_backgroundWorker.CancellationPending) m_backgroundWorker.CancelAsync();
            }
        }

        private void frmTOD_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (startButton.Text != "Start")
            {
                if (!m_backgroundWorker.CancellationPending)
                    m_backgroundWorker.CancelAsync();
                e.Cancel = true;
            }
        }

        //non-form functions
        private void enableDisable(bool disable)
        {
            if(disable)
            {
                sessionsTB2.Enabled = false;
                setsTB2.Enabled = false;
                betTB2.Enabled = false;
                creditsTB2.Enabled = false;
                winTB2.Enabled = false;
                outputTB2.Enabled = false;
            }
            else if (!disable)
            {
                sessionsTB2.Enabled = true;
                setsTB2.Enabled = true;
                betTB2.Enabled = true;
                creditsTB2.Enabled = true;
                winTB2.Enabled = true;
                outputTB2.Enabled = true;
            }
        }

        private void readVariables()
        {
            sessionsToPlay = int.Parse(sessionsTB2.Text);
            setsToPlay = int.Parse(setsTB2.Text);
            betLevel = int.Parse(betTB2.Text);
            startingCredits = int.Parse(creditsTB2.Text);
            sessionEndCredits = int.Parse(winTB2.Text);
            outputFile = outputTB2.Text;
        }

        internal void InitializeGameInfo(GameInfo _gameInfo)
        {
            m_gameInfo = _gameInfo;
        }

        internal void InitializeGameInfo(string _gameInfoFilePath)
        {
            m_gameInfoFilePath = _gameInfoFilePath;
        }

        //background worker functions
        DateTime lastUpdate;

        void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            if (m_gameInfo == null)
            { 
                m_gameInfo = GameInfo.CreateGameInfo(m_gameInfoFilePath); 
            }

            lastUpdate = DateTime.Now;

            while(!bgWorker.CancellationPending && !allSetsRun)
            {
                m_gameInfo.InSimulation = true;
                m_gameInfo.DoDefaultWinDistributions = false;
                m_gameInfo.DoCustomStats = false;
                playGames();
            }
            m_gameInfo.InSimulation = false;
            m_gameInfo.DoDefaultWinDistributions = false;
            m_gameInfo.DoCustomStats = false;
        }

        void BGWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statusRTB.Text += "\n Completed Set " + (e.ProgressPercentage + 1) + "...";
            statusTB1.Text = getETA((e.ProgressPercentage+1));
        }

        private string getETA(int p)
        {
            DateTime now = DateTime.Now;
            TimeSpan changeInTime = now - lastUpdate;
            lastUpdate = now;
            DateTime willFinish = now.AddTicks(changeInTime.Ticks * (setsToPlay - p));
            return willFinish.ToString();
        }

        void BGWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            enableDisable(false);
            statusRTB.Text += "\n Completed";
            m_backgroundWorker = null;
            startButton.Text = "Start";
            printData();
            statusRTB.Text += "\n Data Printed";
            statusTB1.Text = "";
        }

        //TOD Functions and Play
        List<int> maxPaysForSessions = new List<int>();
        List<double> averageSpins = new List<double>();
        List<double> averageWins= new List<double>();
        List<double> paybackForSets = new List<double>();
        List<double> winningSessions = new List<double>();
        List<double> cashoutsW0Bonuses = new List<double>();
        List<double> cashoutW1Bonus = new List<double>();
        List<double> cashoutW2Bonuses = new List<double>();
        List<double> averageCashoutAmount = new List<double>();
        List<double> fivePercentFailures = new List<double>();
        List<double> sessionsWBonuses = new List<double>();
        List<double> sessionsWBonusesorProg = new List<double>();
        List<double> medianSpins = new List<double>();
        List<List<int>> spinsForAllSets = new List<List<int>>();
        List<ulong> sessionsThatRecovered = new List<ulong>();
        List<ulong> sessionsWithChanceToRecover = new List<ulong>();
        List<double> setRecoveryChance = new List<double>();
        bool allSetsRun = false;
        setDistribution setsForTOD;

        internal void playGames()
        {
            setsForTOD = new setDistribution(startingCredits, m_gameInfo.Bet);
            for(int aa = 0; aa < setsToPlay; aa++)
            {
                SetsStats currentSet = new SetsStats(m_gameInfo.Bet, startingCredits);
                for(int ab = 0; ab < sessionsToPlay; ab++)
                {
                    currentSet.sessionsPlayed++;
                    SessionStats session = new SessionStats(startingCredits, m_gameInfo.Bet, sessionEndCredits / m_gameInfo.Bet);
                    m_gameInfo.BetLevel = betLevel;
                    m_gameInfo.m_bankSize = 1;
                    while(session.currentCredits >= session.bet && session.currentCredits < sessionEndCredits)
                    {
                        m_gameInfo.PreSpin(m_gameInfo.m_stackShow);
                        while (m_gameInfo.PostSpin() != GameAction.End) ;
                        session.placeBet();
                        session.processWin(m_gameInfo.WinsThisGame, m_gameInfo.m_freeGamesPlayed > 0 ? true : false, m_gameInfo.m_progressiveWinPG > 0 ? true : false);
                    }
                    currentSet.processSession(session);
                }
                currentSet.processSet();
                setsForTOD.addSet(currentSet);
                m_backgroundWorker.ReportProgress(aa);
            }
            allSetsRun = true;
            setsForTOD.getOverallStatsForSets();
        }

        internal void printData()
        {
            List<List<string>> outputStrings = new List<List<string>>();
            StringBuilder resultantString = new StringBuilder();
            string directory = System.Environment.CurrentDirectory;
            string newFile = string.Format("{0}\\{1}", directory, outputFile);
            StreamWriter writeToFile = new StreamWriter(newFile, false);
            //double medianSpinsForAllSets = getSpinsMedian();
            outputStrings.Add(new List<string> { "Set #", "Payback %", "Max Win", "Average Spins", "Median Spins", "Average Win", "Average Cashout Amount", "% of Winning Sessions", "% of Cashouts w/ 0 Bonuses",
                                                "% of Cashouts w/ 1 Bonus", "% of Cashouts w/2+ Bonuses", "% of 5% of Failures", "% of Sessions w/ 1+ Bonus", "% of Sessions w/ 1+ Bonus or Prog", "% of Sessions That Recovered"});
            List<List<string>> additionalStringsToAdd = setsForTOD.printForTOD();
            for(int ac = 0; ac < additionalStringsToAdd.Count(); ac++)
            {
                outputStrings.Add(additionalStringsToAdd[ac]);
            }
            int maxCol = 1;
            foreach(List<string> set in outputStrings)
            {
                maxCol = Math.Max(maxCol, set.Count());
            }
            List<int> columnWidths = m.MakeNewList<int>(maxCol, 2);
            foreach(List<string> set in outputStrings)
            {
                for(int ad = 0; ad < set.Count(); ad++)
                {
                    columnWidths[ad] = Math.Max(columnWidths[ad], set[ad].Length);
                }
            }
            foreach(List<string> set in outputStrings)
            {
                for(int ae = 0; ae < set.Count(); ae++)
                {
                    if (ae == 0) resultantString.Append(set[ae].PadRight(columnWidths[ae] + 2));
                    else resultantString.Append(set[ae].PadLeft(columnWidths[ae] + 2));
                }
                resultantString.AppendLine();
            }
            writeToFile.Write(resultantString);
            writeToFile.Close();
        }

        internal double getSpinsMedian()
        {
            double valueToReturn = 0;
            List<int> superLargeSpinCountList = new List<int>();
            for(int aa = 0; aa < spinsForAllSets.Count(); aa++)
            {
                for(int ab = 0; ab < spinsForAllSets[aa].Count(); ab++)
                {
                    superLargeSpinCountList.Add(spinsForAllSets[aa][ab]);
                }
            }
            superLargeSpinCountList.Sort();
            if(superLargeSpinCountList.Count() % 2 == 0)
            {
                valueToReturn = (superLargeSpinCountList[superLargeSpinCountList.Count() / 2] + superLargeSpinCountList[(superLargeSpinCountList.Count() / 2) - 1]) / 2;
            }
            else if (superLargeSpinCountList.Count() % 2 == 1)
            {
                double midPoint = (superLargeSpinCountList.Count() / 2) - 0.5;
                int actualMid = (int)midPoint;
                valueToReturn = (double)superLargeSpinCountList[actualMid];
            }
            return valueToReturn;
        }

        internal void processCurrentSet(SetsStats _set)
        {
            maxPaysForSessions.Add(_set.maxPayForSet);
            averageSpins.Add(_set.averageSpins);
            spinsForAllSets.Add(_set.spinsThisSet);
            medianSpins.Add(_set.medianSpins);
            averageWins.Add(_set.averageWinThisSet);
            paybackForSets.Add(_set.rTP);
            winningSessions.Add(_set.winningSessions);
            cashoutsW0Bonuses.Add(_set.cashoutsW0Bonuses);
            cashoutW1Bonus.Add(_set.cashoutsW1Bonus);
            cashoutW2Bonuses.Add(_set.cashoutW2Bonuses);
            averageCashoutAmount.Add((double)_set.totalCashout / (double)_set.cashouts);
            fivePercentFailures.Add((double)_set.gamesWithin5Percent / (double)sessionsToPlay);
            sessionsWBonuses.Add((double)_set.sessionsWithBonuses / (double)sessionsToPlay);
            sessionsWBonusesorProg.Add((double)_set.sessionsWithBonusesorProg / (double)sessionsToPlay);
            sessionsThatRecovered.Add(_set.sessionsRecovered);
            sessionsWithChanceToRecover.Add(_set.sessionsBelowRecoveryLevel);
            setRecoveryChance.Add((double)_set.sessionsRecovered / (double)_set.sessionsBelowRecoveryLevel);
        }
    }
}
