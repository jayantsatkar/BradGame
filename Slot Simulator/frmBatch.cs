using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Threading;

namespace Slot_Simulator
{
    public partial class frmBatch : Form
    {
        public frmBatch()
        {
            InitializeComponent();
        }
        //Variables
        BackgroundWorker m_bgWorkerMain;
        List<FileInfo> m_filesLeft;
        Dictionary<BackgroundWorker, bool> m_bgWorkerThreadsStatus;
        Dictionary<FileInfo, ListViewItem> m_fileInfoToListViewItems;
        List<FileInfo> m_toExport;
        int m_threadsPaused, m_slaveThreadNum, m_maxThreads = 4, m_updateInterval;
        long m_trials;
        int m_betLevel = 1;
        int m_updateIntervalChangedNumber;
        //
        private void frmBatch_Load(object sender, EventArgs e)
        {
            m_updateInterval = 60;
            m_updateIntervalChangedNumber = 0;
            textBoxUpdateInterval_TextChanged(null, null);
            textBoxThreads.Text = m_maxThreads.ToString();
            buttonExport.Enabled = false;
            RefreshFilesLeft();
        }
        private void buttonThreadMinus_Click(object sender, EventArgs e)
        {
            m_maxThreads = Math.Max(1, m_maxThreads - 1);
            textBoxThreads.Text = m_maxThreads.ToString();
        }
        private void buttonThreadPlus_Click(object sender, EventArgs e)
        {
            m_maxThreads++;
            textBoxThreads.Text = m_maxThreads.ToString();
        }
        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                ListViewItem item = listViewResults.SelectedItems[0];
                foreach (FileInfo fileInfo in m_fileInfoToListViewItems.Keys)
                    if (item == m_fileInfoToListViewItems[fileInfo] && !m_toExport.Contains(fileInfo))
                    {
                        m_toExport.Add(fileInfo);
                        break;
                    }
            }
        }
        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (buttonRun.Text == "&Run")
            {
                RefreshFilesLeft();
                buttonExport.Enabled = true;
                textBoxTrials.Enabled = false;
                betLevelTB.Enabled = false;
                buttonRun.Text = "Cancel";
                m_trials = long.Parse(textBoxTrials.Text);
                m_betLevel = int.Parse(betLevelTB.Text);
                m_bgWorkerThreadsStatus = new Dictionary<BackgroundWorker, bool>();
                m_bgWorkerMain = new BackgroundWorker();
                m_bgWorkerMain.WorkerSupportsCancellation = true;
                m_bgWorkerMain.WorkerReportsProgress = true;
                m_bgWorkerMain.DoWork += new DoWorkEventHandler(BGWorkerMain_DoWork);
                m_bgWorkerMain.ProgressChanged += new ProgressChangedEventHandler(BGWorkerMain_ProgressChanged);
                m_bgWorkerMain.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWorkerMain_RunWorkerCompleted);
                m_bgWorkerMain.RunWorkerAsync();
            }
            else
            {
                if (!m_bgWorkerMain.CancellationPending)
                    m_bgWorkerMain.CancelAsync();
            }
        }
        private void textBoxUpdateInterval_TextChanged(object sender, EventArgs e)
        {
            int interval;
            if(int.TryParse(textBoxUpdateInterval.Text,out interval))
            {
                m_updateInterval = interval;
                m_updateIntervalChangedNumber = m.RandomInteger(1000000);
            }
        }
        //Main Thread
        void BGWorkerMain_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorkerMain = sender as BackgroundWorker;
            m_slaveThreadNum = 0;
            while (m_bgWorkerThreadsStatus.Count > 0 || m_filesLeft.Count > 0)
            {
                //Add Thread
                if (m_bgWorkerThreadsStatus.Count - m_threadsPaused < m_maxThreads)
                {
                    while (m_bgWorkerThreadsStatus.Count - m_threadsPaused < m_maxThreads && !(m_threadsPaused == 0 && m_filesLeft.Count == 0))
                        if (m_threadsPaused > 0)
                        {
                            foreach (BackgroundWorker bgWorkerToStop in new List<BackgroundWorker>(m_bgWorkerThreadsStatus.Keys))
                                if (!m_bgWorkerThreadsStatus[bgWorkerToStop])
                                {
                                    m_bgWorkerThreadsStatus[bgWorkerToStop] = true;
                                    m_threadsPaused--;
                                    break;
                                }
                        }
                        else if (m_filesLeft.Count != 0)
                        {
                            BackgroundWorker bgWorkerThread = new BackgroundWorker();
                            bgWorkerThread.WorkerSupportsCancellation = true;
                            bgWorkerThread.DoWork += new DoWorkEventHandler(BGWorkerSlaveSim_DoWork);
                            bgWorkerThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGWorkerSlaveSim_RunWorkerCompleted);
                            FileInfo nextFile = m_filesLeft[0];
                            m_filesLeft.Remove(nextFile);
                            bgWorkerThread.RunWorkerAsync(nextFile);
                            m_bgWorkerThreadsStatus[bgWorkerThread] = true;
                        }
                }
                //Pause Thread
                else if (m_bgWorkerThreadsStatus.Count - m_threadsPaused > m_maxThreads)
                {
                    while (m_bgWorkerThreadsStatus.Count - m_threadsPaused > m_maxThreads)
                        foreach (BackgroundWorker bgWorkerToStop in new List<BackgroundWorker>(m_bgWorkerThreadsStatus.Keys))
                            if (m_bgWorkerThreadsStatus[bgWorkerToStop])
                            {
                                m_bgWorkerThreadsStatus[bgWorkerToStop] = false;
                                m_threadsPaused++;
                                break;
                            }
                }
                Thread.Sleep(1000);
                if (bgWorkerMain.CancellationPending)
                {
                    foreach (BackgroundWorker bgWorkerThread in m_bgWorkerThreadsStatus.Keys)
                        bgWorkerThread.CancelAsync();
                    return;
                }
            }
        }
        void BGWorkerMain_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                BatchReportProgressArg reportProgressArg = e.UserState as BatchReportProgressArg;
                ListViewItem listViewItem = m_fileInfoToListViewItems[reportProgressArg.FileInfo];
                List<string> statusUpdate = reportProgressArg.Param as List<string>;
                for (int i = 0; i < 4; i++)
                    listViewItem.SubItems[i + 1].Text = statusUpdate[i];
            }
            else if (e.ProgressPercentage == 1)
            {
                SimulationArgs simArgs = e.UserState as SimulationArgs;
                ExcelFile.CreateExcel(simArgs.GetResults());
            }
        }
        void BGWorkerMain_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            buttonRun.Text = "&Run";
            buttonExport.Enabled = false;
            textBoxTrials.Enabled = true;
        }
        //Slave Thread
        void BGWorkerSlaveSim_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorkerThread = sender as BackgroundWorker;
            FileInfo fileInfo = e.Argument as FileInfo;
            int threadNum = m_slaveThreadNum++;
            List<string> status = new List<string>(new string[] { "Elapsed", "Time Left", "0.00%", "-" } );
            m_bgWorkerMain.ReportProgress(0, new BatchReportProgressArg(fileInfo, status));
            GameInfo gameInfo = GameInfo.CreateGameInfo(fileInfo.FullName);
            SimulationArgs simArg = new SimulationArgs(true, true, true, true, true, true, true, true, false, gameInfo, m_trials, m_betLevel);
            //Set Up
            simArg.SetUpStats();
            DateTime start = DateTime.Now;
            DateTime lastUpdate = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddSeconds(1);
            int tillNextCheck = 1000;
            int updateIntervalChangedNumber = m_updateIntervalChangedNumber;
            while (!bgWorkerThread.CancellationPending && !simArg.RunTrialAndIsFinished())
            {
                if (m_toExport.Contains(fileInfo))
                {
                    m_toExport.Remove(fileInfo);
                    m_bgWorkerMain.ReportProgress(1, simArg);
                }
                if (tillNextCheck-- >= 0)
                {
                    tillNextCheck = 1000;
                    if (DateTime.Now > nextUpdate || updateIntervalChangedNumber != m_updateIntervalChangedNumber)
                    {
                        updateIntervalChangedNumber = m_updateIntervalChangedNumber;
                        status[0] = (DateTime.Now - start).ToString(@"d\ \-\ hh\:mm\:ss");
                        status[1] = simArg.ETA;
                        status[2] = string.Format("{0:0.0000%}", simArg.CurrentProgress);
                        status[3] = string.Format("{0:0.0000%}", simArg.CurrentRTP);
                        m_bgWorkerMain.ReportProgress(0, new BatchReportProgressArg(fileInfo, status));
                        nextUpdate = DateTime.Now.AddSeconds(m_updateInterval);
                    }
                }
            }
            status[0] = (DateTime.Now - start).ToString(@"d\ \-\ hh\:mm\:ss");
            status[1] = "-";
            status[2] = "100%";
            status[3] = string.Format("{0:0.0000%}", simArg.CurrentRTP);
            m_bgWorkerMain.ReportProgress(0, new BatchReportProgressArg(fileInfo, status));
            gameInfo.InSimulation = false;
            gameInfo.DoDefaultWinDistributions = false;
            gameInfo.DoCustomStats = false;
            e.Result = simArg;
        }
        void BGWorkerSlaveSim_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bgWorkerThread = sender as BackgroundWorker;
            if (!bgWorkerThread.CancellationPending)
            {
                SimulationArgs simArg = e.Result as SimulationArgs;
                if (simArg.IsFinished)
                    ExcelFile.SaveExcel(simArg.GetResults(), string.Format(@"{0}\z {1} Simulation - Bet Level {2}.xlsx", m.Directory, simArg.GameInfo.FileNameWithoutGameData, simArg.GameInfo.BetLevel));
            }
            m_bgWorkerThreadsStatus.Remove(bgWorkerThread);
        }
        //Private functions///////////////////////////////////////
        private void RefreshFilesLeft()
        {
            m_filesLeft = new List<FileInfo>();
            foreach (FileInfo fileInfo in new DirectoryInfo(m.Directory).GetFiles())
                if ((fileInfo.Extension == ".xlsx" || fileInfo.Extension == ".xls") && fileInfo.Name.Substring(0, 1) != "z" && fileInfo.Name.Substring(0, 1) != "~")
                    m_filesLeft.Add(fileInfo);
            listViewResults.Items.Clear();
            m_fileInfoToListViewItems = new Dictionary<FileInfo, ListViewItem>();
            m_toExport = new List<FileInfo>();
            foreach (FileInfo fileInfo in m_filesLeft)
            {
                ListViewItem item = new ListViewItem(new string[] { fileInfo.Name, "-", "-", "-", "-" });
                m_fileInfoToListViewItems[fileInfo] = item;
                listViewResults.Items.Add(item);
            }
        }
    }
}
