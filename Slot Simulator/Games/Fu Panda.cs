using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Fu_Panda : GameInfo
    {
        internal Fu_Panda(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_freeGamesForBonusSymbols = new Dictionary<int, int>();
            for(int rowIndex = 0; rowIndex < m_extraData["free game info"].Count; rowIndex++)
            {
                m_freeGamesForBonusSymbols.Add(int.Parse(m_extraData["free game info"][rowIndex][0]), int.Parse(m_extraData["free game info"][rowIndex][1]));
            }
            if (m_progressiveType == "must hit") setCurrentProgAwards(true, true);
            else if (m_progressiveType == "custom") setCurrentProgAwards(true, true);
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        //frmProgressives frmProg = new frmProgressives();
        int m_symbolScatter, m_symbolWild;
        Dictionary<int, int> m_freeGamesForBonusSymbols;
        List<double> m_currentProgressiveAmounts = new List<double>();
        int m_gameMultiplier = 1;
        bool m_showStacks = true;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            base.PreSpin(m_showStacks);
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    case System.Windows.Forms.Keys.Oemtilde://Cheat Key "~"
                        //int freeGames = 0;
                        //while (freeGames == 0)
                        //{
                        //    ReelIndexes = Reels.GetRandomIndexes();
                        //    CalculateScatterWins(null, GetScreenSymbols(), ref freeGames);
                        //}
                        break;

                }
                if (frmProg.Visible == false && m_progressiveType != "none")
                {
                    setProgressiveForm();
                }
            }
            if(m_progressiveType != "none")
            {
                incrementProgressives();
            }
            m_gameMultiplier = 1;
            m_gameState = DefaultGameStates.PGPostSpin;
        }
        internal override GameAction PostSpin()
        {
            int oldFreeGames = m_freeGamesLeft;
            SpinOrder = cDefaultSpinOrder;
            List<List<int>> screenSymbols;
            switch (m_gameState)
            {
                case DefaultGameStates.PGPostSpin:
                    screenSymbols = GetScreenSymbols(Reels.ReelsPG);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    foreach(WinArgs specificWin in WinsToShow)
                    {
                        if(specificWin.SymbolIndex == m_symbolScatter)
                        {
                            if (specificWin.Count >= 3) m_freeGamesLeft += m_freeGamesForBonusSymbols[specificWin.Count];
                        }
                    }
                    Wins.AddRange(WinsToShow);
                    if(m_progressiveType != "none")
                    {
                        List<WinArgs> progWins = getProgressiveWin();
                        if (progWins.Count() > 0) Wins.AddRange(progWins);
                        m_progressiveWinPG = CountTheseWins(progWins);
                    }
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinsThisPG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 0);
                    }
                    break;
                case DefaultGameStates.FGPreSpin:
                    m_freeGameReels = Reels.ReelsFG;
                    m_freeGameCutoffs = Reels.ReelsFGCutoffs;
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    ReelIndexes = Reels.GetRandomIndexes(Reels.ReelsFG, Reels.ReelsFGCutoffs);
                    m_insertionIndex = getInsertionIndex(Reels.SSCutoffsBG);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                    }
                    BonusCode = 1;
                    m_gameMultiplier = 2;
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(Reels.ReelsFG);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft, m_gameMultiplier);
                    foreach (WinArgs specificWin in WinsToShow)
                    {
                        if (specificWin.SymbolIndex == m_symbolScatter)
                        {
                            if (specificWin.Count >= 3) m_freeGamesLeft += m_freeGamesForBonusSymbols[specificWin.Count];
                        }
                    }
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                    }
                    //Decrement Free Games Left
                    m_freeGamesLeft--;
                    m_freeGamesPlayed++;
                    break;
            }
            //If Have Free Games, Play
            if (m_freeGamesLeft > 0)
            {
                int freeGamesWonThisSpin = InFreeGames ? m_freeGamesLeft - oldFreeGames + 1 : m_freeGamesLeft;
                if(m_freeGamesPlayed == 0 && DoCustomStats)
                {
                    if (m_freeGamesStarted.Keys.Contains(m_freeGamesLeft)) m_freeGamesStarted[m_freeGamesLeft]++;
                    else m_freeGamesStarted[m_freeGamesLeft] = 1;
                }
                InFreeGames = true;
                if (!InSimulation && freeGamesWonThisSpin > 0)
                    m.gameMessageBox.Text = string.Format("{0} FREE GAMES WON!!!", freeGamesWonThisSpin);
                m_gameState = DefaultGameStates.FGPreSpin;
                return GameAction.ShowWinsInBetweenGames;
            }
            //Sum up wins and Stats
            WinsThisGame = CountTheseWins(Wins);
            AfterGameStatsCollection();
            if (DoCustomStats)
            {
            }
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
            //m.AddPayTableCount("Bonus", m.Multipliers.Count + 1, new List<string>(new string[] { "Bonus", }), _payTablesSeperated, _payTablesTotal);
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        /*private void incrementProgressives()
        {
            if(m_progressiveType == "must hit")
            {
                m_currentProgressiveAmounts[0] += (double)Bet * .003600;
                m_currentProgressiveAmounts[1] += (double)Bet * .007030;
                if (!InSimulation)
                {
                }
            }
            else if (m_progressiveType == "custom")
            {
            }
        }*/

        private void setCurrentProgAwards(bool setTop, bool setBottom)
        {
            if(m_progressiveType == "must hit")
            {
                if (m_currentProgressiveAwards.Count() == 0)
                {
                    m_currentProgressiveAwards.Add(0);
                    m_currentProgressiveAwards.Add(0);
                    m_currentProgressiveAmounts.Add(400000.00);
                    m_currentProgressiveAmounts.Add(20000.00);
                }
                if(setTop)
                { 
                    int m_indexForTop = -1;
                    m_indexForTop = m.RandomInteger(1000);
                    if(m_indexForTop < 98) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 400000.00 + (double)(m_indexForTop*1000));
                    else if (m_indexForTop == 98 || m_indexForTop == 99) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 498000.00);
                    else if (m_indexForTop > 99) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 499000.00);
                }
                if (setBottom)
                {
                    int m_indexForBottom = -1;
                    m_indexForBottom = m.RandomInteger(1000);
                    if (m_indexForBottom < 39) m_currentProgressiveAwards[1] = (m.RandomDouble(750) + 20000.00 + (double)(750 * m_indexForBottom));
                    else if (m_indexForBottom >= 39) m_currentProgressiveAwards[1] = (m.RandomDouble(750) + 20000.00 + (double)(750 * 39));
                }
            }
            else if (m_progressiveType == "custom")
            {
            }
        }

        private List<WinArgs> getProgressiveWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            if(m_progressiveType == "must hit")
            {
                bool winning = true;
                while(winning)
                {
                    if(m_currentProgressiveAmounts[0] >= m_currentProgressiveAwards[0])
                    {
                        winsToReturn.Add(new WinArgs("Top Progressive", (int)m_currentProgressiveAwards[0]));
                        double difference = m_currentProgressiveAmounts[0] - m_currentProgressiveAwards[0];
                        m_currentProgressiveAmounts[0] = 400000.00 + difference;
                        setCurrentProgAwards(true, false);
                    }
                    if (m_currentProgressiveAmounts[1] >= m_currentProgressiveAwards[1])
                    {
                        winsToReturn.Add(new WinArgs("Bottom Progressive", (int)m_currentProgressiveAwards[1]));
                        double difference = m_currentProgressiveAmounts[1] - m_currentProgressiveAwards[1];
                        m_currentProgressiveAmounts[1] = 20000.00 + difference;
                        setCurrentProgAwards(false, true);
                    }
                    if (!InSimulation)
                    {
                    }
                    if (m_currentProgressiveAmounts[0] >= m_currentProgressiveAwards[0] || m_currentProgressiveAmounts[1] >= m_currentProgressiveAwards[1]) winning = true;
                    else if (m_currentProgressiveAmounts[0] < m_currentProgressiveAwards[0] && m_currentProgressiveAmounts[1] < m_currentProgressiveAwards[1]) winning = false;
                }
            }
            else if (m_progressiveType == "custom")
            {
            }
            return winsToReturn;
        }
        private void setProgressiveForm()
        {
            if (m_progressiveType != "none")
            {
            }
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        SortedDictionary<int, int> m_freeGamesStarted;
        internal override void SetUpCustomStats()
        {
            m_freeGamesStarted = new SortedDictionary<int, int>();
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            foreach(int freeGamesInitial in m_freeGamesStarted.Keys)
            {
                _results.Add(new List<string>(new string[] { freeGamesInitial.ToString(), m_freeGamesStarted[freeGamesInitial].ToString()}));
            }
        }
    }
}
