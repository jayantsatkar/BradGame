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
    public partial class frmStatistics : Form
    {
        public int m_numberOfSessions;
        public int m_numberOfCashouts;
        public int m_numberOfSpins;
        public int m_buyIn;
        private List<double> sessionRTPs = new List<double>();
        private List<int> numberOfSpins = new List<int>();
        private List<int> maxWin = new List<int>();
        private List<double> hitFrequency = new List<double>();
        public frmStatistics()
        {
            InitializeComponent();
        }
        frmMain frmMain;
        private void frmStatistics_Load(object sender, EventArgs e)
        {
            KeyPreview = true;
            panelChart.BackColor = Color.Black;
            m_penMainAxis = new Pen(Color.Green, 1);
            m_penMinorAxis = new Pen(Color.FromArgb(70, 70, 70), 1);
            m_penMajorAxis = new Pen(Color.DarkRed, 1);
            m_penWinLine = new Pen(Color.Teal, 2);
            m_penTheoreticalLine = new Pen(Color.Yellow, 2);
            m_penSeperator = new Pen(Color.Teal, 3);
            m_brushBarDictionary = new Dictionary<string, Brush>();
            m_brushBarDictionary["green"] = Brushes.Green;
            m_brushBarDictionary["yellow"] = Brushes.Yellow;
            m_brushBarDictionary["orange"] = Brushes.Orange;
            m_brushBarDictionary["dark orange"] = Brushes.DarkOrange;
            m_brushBarDictionary["red"] = Brushes.Red;
            m_brushBarDictionary["blue"] = Brushes.Blue;
            m_brushBarDictionary["purple"] = Brushes.Purple;
            m_brushBarDictionary["white"] = Brushes.White;
            m_brushBarDictionary["gray"] = Brushes.Gray;
            m_brushBarDictionary["grey"] = Brushes.Gray;
            m_brushText = Brushes.Gray;
            m_brushGraphBG = new SolidBrush(Color.FromArgb(24, 24, 24));
            m_fontGraphText = new Font(new FontFamily("Arial"), 10, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        private void frmStatistics_Resize(object sender, EventArgs e)
        {
            panelChart.Invalidate();
        }
        private void frmStatistics_KeyDown(object sender, KeyEventArgs e)
        {
                 switch (e.KeyCode)
                 {
                     case Keys.S:
                     case Keys.Oemtilde:
                     case Keys.Q:
                     case Keys.W:
                     case Keys.E:
                         frmMain.GameInfo.CheatKey = e.KeyCode;
                         frmMain.Spin();
                         break;
                 }
        }
        private void buttonPlaySession_Click(object sender, EventArgs e)
        {
            m_buyIn = m.GetBuyInOfBet(frmMain.GameInfo.Bet);
            frmMain.ResetTo(m_buyIn);
            frmMain.ResetStatistics();
            for (int i = 0; i < 400; i++)
            {
                if (!frmMain.FastSpin(false))
                    break;
                if (m_gameStatistics.MaxChange >= m_buyIn)
                    break;
            }
            frmMain.SetWinLinesIfAny();
            DisplayStatistics();
        }
        public void buttonReset_Click(object sender, EventArgs e)
        {
            frmMain.ResetToNewSession();
            frmMain.ResetStatistics();
            DisplayStatistics();
        }
        internal void RegisterfrmMain(frmMain _frmMain)
        {
            frmMain = _frmMain;
        }
        //Stats
        GameStatistics m_gameStatistics;
        internal void SetGameStatistics(GameStatistics _gameStatistics) { m_gameStatistics = _gameStatistics; }
        internal void DisplayStatistics(GameStatistics _gameStatistics = null)
        {
            if (_gameStatistics != null)
                m_gameStatistics = _gameStatistics;
            listViewResults.Items.Clear();
            AddStatistic("Session Statistics");
            AddStatistic("Session Number", string.Format("{0:0,0}", m_numberOfSessions + 1));
            AddStatistic("Amount Invested", m.ToMoneyString(m_buyIn - m_gameStatistics.CoinOut + m_gameStatistics.CoinIn));
            AddStatistic("Coin In", m.ToMoneyString(m_gameStatistics.CoinIn));
            AddStatistic("Coin Out", m.ToMoneyString(m_gameStatistics.CoinOut));
            AddStatistic("RTP", string.Format("{0:0.00%}", m_gameStatistics.CoinOut / m_gameStatistics.CoinIn));
            AddStatistic("Games Played", string.Format("{0:0}", m_gameStatistics.TotalGames));
            AddStatistic("Games Won", string.Format("{0:0}", m_gameStatistics.TotalWinNum));
            AddStatistic("Hit Freq", string.Format("{0:0.00%}", m_gameStatistics.TotalWinNum / m_gameStatistics.TotalGames));
            AddStatistic("Std Dev", string.Format("{0:0.00}", m.GetStandardDeviation(m_gameStatistics.Bet, m_gameStatistics.Wins)));
            AddStatistic("Volatility Index", string.Format("{0:0.00}", m.GetStandardDeviation(m_gameStatistics.Bet, m_gameStatistics.Wins)*1.96));
            //AddStatistic("Cash Status");
            //AddStatistic("Total Buy-In", m.ToMoneyString(-m_gameStatistics.NextBuyIn));
            //AddStatistic("Maximum Profit", m.ToMoneyString(m_gameStatistics.MaxChange));
            //AddStatistic("Maximum Loss", m.ToMoneyString(m_gameStatistics.MinChange));
            //AddStatistic("Overall Information");
            AddStatistic("Sessions Played", string.Format("{0:0}", m_numberOfSessions + 1));
            //AddStatistic("Total Amount Invested", string.Format("{0:0}", 0));
           // AddStatistic("Total Coin In", string.Format("{0:0}", 0));
            //AddStatistic("Total Coin Out", string.Format("{0:0}", 0));
            //AddStatistic("Total Return %", string.Format("{0:0.00%}", 0));
           // AddStatistic("Total Games Played", string.Format("{0:0,0}", m_numberOfSpins));
            //AddStatistic("Total Games Won", string.Format("{0:0,0}", 0));
            //AddStatistic("Total Hit Frequency", string.Format("{0:0.00%}", 0));
            AddStatistic("# of Cashouts", string.Format("{0:0,0}", m_numberOfCashouts));
            AddStatistic("% of Cashouts", string.Format("{0:0.00%}", m_numberOfSessions == 0 ? 0 : ((double)m_numberOfCashouts / (double)m_numberOfSessions)));
            AddStatistic(string.Format("Avg. # of Spins"), string.Format("{0:0.00}", m_numberOfSpins / (m_numberOfSessions+1)));
            panelChart.Invalidate();
        }
        //List
        private int m_subCategoryIndex = 0;
        private List<Color> m_subCategoryColorList = new List<Color>(new Color[] { Color.FromArgb(240, 240, 240), Color.FromArgb(204, 255, 204) });
        private Color m_colorHeaderBack = Color.FromArgb(0, 0, 128);
        private Color m_colorHeaderFore = Color.FromArgb(236, 236, 10);
        private void AddStatistic(string _category, string _result = null)
        {
            ListViewItem item;
            if (_result == null)
            {
                item = new ListViewItem(_category);
                item.BackColor = m_colorHeaderBack;
                item.ForeColor = m_colorHeaderFore;
                m_subCategoryIndex = 0;
            }
            else
            {
                item = new ListViewItem(new string[] { _category, _result });
                item.BackColor = m_subCategoryColorList[(m_subCategoryIndex++) % m_subCategoryColorList.Count];
            }
            listViewResults.Items.Add(item);
        }
        //
        private float m_minYValue, m_maxYValue, m_yDist;
        private int m_minXValue, m_maxXValue, m_xDist;
        private int m_XPadding, m_YPadding, m_YPaddingBetweenGraphs;
        private Pen m_penMainAxis, m_penMinorAxis, m_penMajorAxis, m_penWinLine, m_penTheoreticalLine, m_penSeperator;
        private Brush m_brushText, m_brushGraphBG;
        private Dictionary<string, Brush> m_brushBarDictionary;
        private Font m_fontGraphText;
        private const int m_barAreaHeight = 120;
        private List<float> m_barAreaWinHeights = new List<float>(new float[] { 0, 15, 40, 60, 80, 100, 120 });
        private List<float> m_barAreaWinCutoffs = new List<float>(new float[] { 0, 1, 5, 10, 20, 50, 100 });
        private const int cXAxisLines = 40;
        private const int cXAsisMax = 400;
        private PointF GetPoint(float _x, float _y) { return new PointF(GetX(_x), GetY(_y)); }
        private float GetX(float _x) { return (_x - m_minXValue) / m_xDist * (panelChart.Width - 2 * m_XPadding) + m_XPadding; }
        private float GetY(float _y) { return (m_maxYValue - _y) / m_yDist * (panelChart.Height - m_barAreaHeight - 2 * m_YPadding - m_YPaddingBetweenGraphs) + m_YPadding; }
        private void panelChart_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                int gamesPlayed = m_gameStatistics.GameHistory.Count;

                m_maxYValue = Math.Max(m.BankRollForStatistics, m_gameStatistics.MaxChange);
                m_minYValue = Math.Min(-m.BankRollForStatistics, m_gameStatistics.MinChange);
                m_minXValue = Math.Max(0, gamesPlayed - cXAsisMax);
                m_maxXValue = m_minXValue + cXAsisMax;
                m_xDist = m_maxXValue - m_minXValue;
                m_yDist = m_maxYValue - m_minYValue;
                m_XPadding = 40;
                m_YPadding = 20;
                m_YPaddingBetweenGraphs = 30;
                //int 
                Graphics g = e.Graphics;
                //Main Graph///////////////////////////////////////////////////////////////////////////////
                //Background Main
                g.FillRectangle(m_brushGraphBG, m_XPadding, GetY(m_maxYValue), panelChart.Width - 2 * m_XPadding, GetY(m_minYValue) - GetY(m_maxYValue));
                //Main Axis
                g.DrawLine(m_penMainAxis, GetPoint(m_minXValue, 0), GetPoint(m_maxXValue, 0));
                //Minor/Major Axis Y
                int lineCount = 0;
                for (float y = m.BankRollForStatistics / 4; y < m_maxYValue; y += m.BankRollForStatistics / 4)
                {
                    lineCount++;
                    g.DrawLine(lineCount % 4 == 0 ? m_penMajorAxis : m_penMinorAxis, GetPoint(m_minXValue, y), GetPoint(m_maxXValue, y));
                    g.DrawString(string.Format("${0:0}", y / 100), m_fontGraphText, m_brushText, GetX(m_minXValue) - m_XPadding + 5, GetY(y) - 5);
                }
                lineCount = 0;
                for (float y = -m.BankRollForStatistics / 4; y > m_minYValue; y += -m.BankRollForStatistics / 4)
                {
                    lineCount++;
                    if (lineCount % 4 == 0)
                    {
                        int buyInIndex = lineCount / 4 - 1;
                        g.DrawLine(m_penMinorAxis, GetPoint(m_minXValue, y), GetPoint(m_maxXValue, y));
                        if (buyInIndex == 0)
                            g.DrawLine(m_penMajorAxis, GetPoint(m_minXValue, y), GetPoint(m_gameStatistics.BuyIns[buyInIndex][0], y));
                        else if(buyInIndex != 0)
                        {
                            g.DrawLine(m_penMajorAxis, GetPoint(m_gameStatistics.BuyIns[buyInIndex - 1][0], m_gameStatistics.BuyIns[buyInIndex - 1][1]), GetPoint(m_gameStatistics.BuyIns[buyInIndex - 1][0], y));
                            g.DrawLine(m_penMajorAxis, GetPoint(m_gameStatistics.BuyIns[buyInIndex - 1][0], y), GetPoint(m_gameStatistics.BuyIns[buyInIndex][0], y));
                        }
                    }
                    else g.DrawLine(m_penMinorAxis, GetPoint(m_minXValue, y), GetPoint(m_maxXValue, y));
                    g.DrawString(string.Format("-${0:0}", -y / 100), m_fontGraphText, m_brushText, GetX(m_minXValue) - m_XPadding + 5, GetY(y) - 5);
                }
                //Minor/Major Axis X
                for (float x = m_minXValue; x <= m_maxXValue; x += cXAxisLines)
                    g.DrawLine(m_penMinorAxis, GetX(x), m_YPadding, GetX(x), panelChart.Height - 2 * m_YPadding - m_barAreaHeight);
                //Theoretical line
                g.DrawLine(m_penTheoreticalLine,
                     GetPoint(m_minXValue, -m_gameStatistics.Bet * m_minXValue * (1 - m_gameStatistics.ExpectedRTP)),
                    GetPoint(m_maxXValue, -m_gameStatistics.Bet * m_maxXValue * (1 - m_gameStatistics.ExpectedRTP)));
                //Fill in between & bottom
                g.FillRectangle(Brushes.Black, m_XPadding, panelChart.Height - m_barAreaHeight - m_YPadding - m_YPaddingBetweenGraphs, panelChart.Width - 2 * m_XPadding + 1, m_YPaddingBetweenGraphs);
                g.FillRectangle(Brushes.Black, m_XPadding, panelChart.Height - m_YPadding, panelChart.Width - 2 * m_XPadding + 1, m_YPadding);
                //Label
                for (float x = m_minXValue; x <= m_maxXValue; x += cXAxisLines)
                    g.DrawString(string.Format("#{0:0}", x), m_fontGraphText, m_brushText, GetX(x) - 10, panelChart.Height - m_barAreaHeight - m_YPaddingBetweenGraphs - m_YPadding);
                //Bottom Graph///////////////////////////////////////////////////////////////////////////////
                //Background Bars
                g.FillRectangle(m_brushGraphBG, m_XPadding, panelChart.Height - m_barAreaHeight - m_YPadding, panelChart.Width - 2 * m_XPadding, m_barAreaHeight);
                //Minor/Major Axis X
                for (float x = m_minXValue; x <= m_maxXValue; x += cXAxisLines)
                    g.DrawLine(m_penMinorAxis, GetX(x), panelChart.Height - m_YPadding - m_barAreaHeight, GetX(x), panelChart.Height - m_YPadding);
                //Win Bars Axis
                for (int i = 1; i < m_barAreaWinHeights.Count; i++)
                {
                    float winHeight = m_barAreaWinHeights[i];
                    float y = panelChart.Height - winHeight - m_YPadding;
                    g.DrawLine(m_penMinorAxis, m_XPadding, y, panelChart.Width - m_XPadding, y);
                    g.DrawString(string.Format("{0:0}x", m_barAreaWinCutoffs[i]), m_fontGraphText, m_brushText, 5, y);
                }
                //Win Line
                List<PointF> winPattern = new List<PointF>();
                float winLineY, winLineX;
                if (m_minXValue == 0)
                {
                    winLineY = m_gameStatistics.GameHistory[m_minXValue].At;
                    winLineX = m_minXValue;
                }
                else
                {
                    winLineY = m_gameStatistics.GameHistory[m_minXValue - 1].At;
                    winLineX = m_minXValue - 1;
                }
                for (int gameIndex = m_minXValue; gameIndex < m_gameStatistics.GameHistory.Count; gameIndex++)
                {
                    GameHistory game = m_gameStatistics.GameHistory[gameIndex];
                    winLineY += game.Change;
                    winPattern.Add(GetPoint(winLineX, winLineY));
                    DrawBar(g, winLineX, (float)game.Win / m_gameStatistics.Bet, game.Brush);
                    winLineX++;
                }
                if (winPattern.Count > 1)
                    g.DrawLines(m_penWinLine, winPattern.ToArray());
                //Buy ins
                foreach (List<float> buyin in m_gameStatistics.BuyIns)
                    if (buyin[0] >= m_minXValue && buyin[0] <= m_maxXValue && buyin[1] >= m_minYValue && buyin[1] <= m_maxYValue)
                    {
                        float x = GetX(buyin[0]);
                        float y = GetY(buyin[1]);
                        float radius = 6;
                        g.FillEllipse(Brushes.DarkRed, x - radius - 1, y - radius - 1, 2 * radius, 2 * radius);
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void DrawBar(Graphics _g, float _x, float _winTimesBet, string _brush)
        {
            if (_winTimesBet > 0)
            {
                int halfOfWidth = 2;
                PointF barX = GetPoint(_x, 0);
                float winBarHeight = -1;
                for (int i = 1; i < m_barAreaWinCutoffs.Count; i++)
                    if (_winTimesBet < m_barAreaWinCutoffs[i])
                    {
                        winBarHeight = (_winTimesBet - m_barAreaWinCutoffs[i - 1]) / (m_barAreaWinCutoffs[i] - m_barAreaWinCutoffs[i - 1]) * (m_barAreaWinHeights[i] - m_barAreaWinHeights[i - 1]) + m_barAreaWinHeights[i - 1];
                        break;
                    }
                if (winBarHeight == -1)
                    winBarHeight = m_barAreaHeight;
                
                
                string brushString;
                if (_brush == "")
                {
                    brushString = brushString = "red";
                }
                else
                    brushString = _brush;
                Brush winBrush = m_brushBarDictionary.ContainsKey(_brush) ? m_brushBarDictionary[brushString] : m_brushBarDictionary["gray"];
                _g.FillRectangle(winBrush, barX.X - halfOfWidth, panelChart.Height - winBarHeight - m_YPadding, 2 * halfOfWidth, winBarHeight);
            }
        }

        public void storeStatistics()
        {
            sessionRTPs.Add(m_gameStatistics.CoinOut / m_gameStatistics.CoinIn);
            int previousSpins = numberOfSpins.Sum();
            numberOfSpins.Add(m_numberOfSessions == 0 ? m_numberOfSpins : m_numberOfSpins - previousSpins);
            hitFrequency.Add((double)m_gameStatistics.TotalWinNum / (double)m_gameStatistics.TotalGames);
        }

        private void printStatistics()
        {
            List<List<string>> outputStrings = new List<List<string>>();
            StringBuilder resultantString = new StringBuilder();
            string directory = System.Environment.CurrentDirectory;
            string newFile = string.Format("{0}\\{1}", directory, "output-SessionData.txt");
            StreamWriter writeToFile = new StreamWriter(newFile, false);
            outputStrings.Add(new List<string> { "Session #", "Return %", "Number of Spins", "Hit Frequency" });
            for(int aa = 0; aa < sessionRTPs.Count(); aa++)
            {
                List<string> sessionString = new List<string>();
                sessionString.Add(string.Format("Session {0}", aa));
                sessionString.Add(string.Format("{0:0.00%}", sessionRTPs[aa]));
                sessionString.Add(string.Format("{0:0}", numberOfSpins[aa]));
                sessionString.Add(string.Format("{0:0.00%}", hitFrequency[aa]));
                outputStrings.Add(sessionString);
            }
            int maxCol = 1;
            foreach(List<string> session in outputStrings)
            {
                maxCol = Math.Max(maxCol, session.Count());
            }
            List<int> columnWidths = m.MakeNewList<int>(maxCol, 2);
            foreach(List<string> session in outputStrings)
            {
                for(int ab = 0; ab < session.Count(); ab++)
                {
                    columnWidths[ab] = Math.Max(columnWidths[ab], session[ab].Length);
                }
            }
            foreach(List<string> session in outputStrings)
            {
                for(int ac = 0; ac < session.Count(); ac++)
                {
                    if (ac == 0) resultantString.Append(session[ac].PadRight(columnWidths[ac] + 2));
                    else resultantString.Append(session[ac].PadLeft(columnWidths[ac] + 2));
                }
                resultantString.AppendLine();
            }
            writeToFile.Write(resultantString);
            writeToFile.Close();
        }

        private void printDataButton_Click(object sender, EventArgs e)
        {
            printStatistics();
        }
    }
    internal class GameStatistics
    {
        internal float Bet, TotalGames, CoinIn, CoinOut, MinChange, MaxChange, Change, ExpectedRTP, TotalWinNum;
        internal SortedDictionary<int, int> GamesSinceLast;
        internal SortedDictionary<int, long> Wins;
        internal List<GameHistory> GameHistory;
        internal List<List<float>> BuyIns;
        internal float NextBuyIn;
        internal GameStatistics(int _startingBet, float _expectedRTP)
        {
            ExpectedRTP = _expectedRTP;
            Bet = _startingBet;
            BuyIns = new List<List<float>>();
            BuyIns.Add(new List<float> { m.BankRollForStatistics });
            NextBuyIn = -m.BankRollForStatistics;
            TotalGames = 0;
            TotalWinNum = 0;
            CoinIn = 0;
            CoinOut = 0;
            MinChange = 0;
            MaxChange = 0;
            Change = 0;
            Wins = new SortedDictionary<int, long>();
            GamesSinceLast = new SortedDictionary<int, int>();
            GameHistory = new List<GameHistory>();
            GameHistory.Add(new GameHistory(0, 0, 0));
        }
        internal void RegisterWin(int _bet, int _win, string _brush)
        {
            Bet = _bet;
            TotalGames++;
            CoinIn += _bet;
            CoinOut += _win;
            if (_win > 0)
                TotalWinNum++;
            if (Change < NextBuyIn + Bet)
            {
                BuyIns.Add(new List<float>(new float[] { GameHistory.Count - 1, Change }));
                NextBuyIn += -m.BankRollForStatistics;
            }
            foreach (int eachBonusCode in new List<int>(GamesSinceLast.Keys))
                GamesSinceLast[eachBonusCode]++;
            Change += _win - Bet;
            MinChange = Math.Min(MinChange, Change);
            MaxChange = Math.Max(MaxChange, Change);
            if (!Wins.ContainsKey(_win)) Wins[_win] = 0;
            Wins[_win]++;
            GameHistory.Add(new GameHistory(_bet, _win, (int)Change, _brush));
        }
    }
    internal class GameHistory
    {
        internal int Win, Change, At;
        internal string Brush;
        internal GameHistory(int _bet, int _win, int _at, string _brush = "")
        {
            Win = _win;
            At = _at;
            Change = _win - _bet;
            Brush = _brush;
        }
    }
}
