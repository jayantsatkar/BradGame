using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Queen_of_Egypt : GameInfo
    {
        internal Queen_of_Egypt(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_symbolP1 = Reels.Symbols.IndexOf("p1");
            m_symbolP2 = Reels.Symbols.IndexOf("p2");
            m_symbolP3 = Reels.Symbols.IndexOf("p3");
            m_symbolP4 = Reels.Symbols.IndexOf("p4");
            m_progressiveSymbols = new List<int> { m_symbolP1, m_symbolP2, m_symbolP3, m_symbolP4 };
            m_stackTrigger = int.Parse(m_extraData["stack trigger"][0][0].ToString());
            if (m_progressiveType == "must hit") setCurrentProgAwards(true, true);
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_symbolP1, m_symbolP2, m_symbolP3, m_symbolP4;
        List<double> m_currentProgressiveAmounts = new List<double>();
        List<string> m_progressiveNames = new List<string>();
        List<int> m_progressiveResets = new List<int>();
        List<double> m_progressiveIncrements = new List<double>();
        int[] m_progressiveSymbolCount = new int[] { 0, 0, 0, 0 };
        List<int> m_progressiveSymbols;
        int m_stackTrigger = -1;
        protected int[] cSuperStreamsSpinOrder = { 6, 7, 8, 9, 10 };
        ulong m_totalFreeSpinsPlayed = 0;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_insertionIndex = -1;
            m_stackShow = m.RandomInteger(m_stackTrigger) == 0;
            base.PreSpin(m_stackShow);
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    case System.Windows.Forms.Keys.Oemtilde://Cheat Key "~"
                        int freeGames = 0;
                        while (freeGames == 0)
                        {
                            List<WinArgs> dummyWins = new List<WinArgs>();
                            base.PreSpin(false);
                            CalculateWins(dummyWins, GetScreenSymbols(currentReelSet), "scatter", ref freeGames);
                        }
                        break;
                    case System.Windows.Forms.Keys.Q: //Cheat Key "Q"
                        m_stackShow = true;
                        base.PreSpin(true);
                        break;
                }
                if (m_stackShow)
                {
                    m.gameMessageBox.Text = "Super Streams";
                    SpinOrder = m_stackShow ? cSuperStreamsSpinOrder : cDefaultSpinOrder;
                }
            }
            if (InSimulation && DoCustomStats)
            {
                m_statsBySymbol.IncGames();
                m_statsWithNoStacks.IncGames();
                m_stacksNoProgressives.IncGames();
            }
            if (m_progressiveType != "none")
            {
                for(int aa = 0; aa < m_progressives.Count(); aa++)
                {
                    m_progressives[aa].Increment(Bet);
                    if(!InSimulation) frmProg.ProgressiveValueTBs[aa].Text = string.Format("{0:$0,0.00}", (double)m_progressives[aa].CurrentValue / 100);
                }
            }
            m_progressiveSymbolCount = new int[] {0, 0, 0, 0};
            m_gameState = DefaultGameStates.PGPostSpin;
        }
        internal override GameAction PostSpin()
        {
            int oldFreeGames = m_freeGamesLeft;
            SpinOrder = m_stackShow ? cSuperStreamsSpinOrder : cDefaultSpinOrder;
            List<List<int>> screenSymbols;
            switch (m_gameState)
            {
                case DefaultGameStates.PGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    if (m_progressiveType != "none")
                    {
                        List<WinArgs> progWins = new List<WinArgs>();
                        for (int ab = 0; ab < m_progressiveSymbolCount.Length; ab++)
                        {
                            m_progressiveSymbolCount[ab] = CountSymbolsOnScreen(screenSymbols, m_progressiveSymbols[ab]);
                        }
                        for (int ac = 0; ac < m_progressiveSymbols.Count(); ac++)
                        {
                            if (m_progressiveSymbolCount[ac] == 12)
                            {
                                progWins = getProgressiveWin(m_progressives[ac]);
                                if (!InSimulation) frmProg.ProgressiveValueTBs[ac].Text = string.Format("{0:$0,0.00}", (double)m_progressives[ac].CurrentValue / 100);
                            }
                        }
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        m_progressiveWinPG = CountTheseWins(progWins);
                    }
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinsThisPG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 0);
                        if (m_stackShow) m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 1);
                    }
                    if(DoCustomStats)
                    {
                        int m_stackWin = 0;
                        m_stackWin = CountTheseWins(WinsToShow);
                        if (m_stackShow)
                        {
                            m_statsBySymbol.StoreGame(m_stackWin, Reels.SSSymbolsBG[m_insertionIndex][1]);
                            m_stacksNoProgressives.StoreGame(m_stackWin - m_progressiveWinPG, Reels.SSSymbolsBG[m_insertionIndex][1]);
                        }
                        else if (!m_stackShow) m_statsWithNoStacks.StoreGame(m_stackWin, 0);
                        if (m_stackShow && m_insertionIndex == 1) m_stacksNoProgressives.StoreGame(m_stackWin - m_progressiveWinPG, 0);
                    }
                    break;
                case DefaultGameStates.FGPreSpin:
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    //Set Up Free Game
                    m_progressiveSymbolCount = new int[] { 0, 0, 0, 0 };
                    m_insertionIndex = -1;
                    GetFGReelStrips(true);
                    SpinOrder = cSuperStreamsSpinOrder;
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                    }
                    m_progressiveWinFG = 0;
                    m_totalFreeSpinsPlayed++;
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    if (m_progressiveType != "none")
                    {
                        List<WinArgs> progWins = new List<WinArgs>();
                        for (int ab = 0; ab < m_progressiveSymbolCount.Length; ab++)
                        {
                            m_progressiveSymbolCount[ab] = CountSymbolsOnScreen(screenSymbols, m_progressiveSymbols[ab]);
                        }
                        for (int ac = 0; ac < m_progressiveSymbols.Count(); ac++)
                        {
                            if (m_progressiveSymbolCount[ac] == 12)
                            {
                                progWins = getProgressiveWin(m_progressives[ac]);
                                if (!InSimulation) frmProg.ProgressiveValueTBs[ac].Text = string.Format("{0:$0,0.00}", (double)m_progressives[ac].CurrentValue / 100);
                            }
                        }
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        m_progressiveWinFG = CountTheseWins(progWins);
                        m_totalProgressiveWinFG += m_progressiveWinFG;
                    }
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                        if (m_progressiveType != "none" && m_progressiveWinFG > 0)
                        {
                            if (m_winsByType.Keys.Contains(m_progressiveWinFG)) m_winsByType[m_progressiveWinFG][8]++;
                            else if (!m_winsByType.Keys.Contains(m_progressiveWinFG)) m_winsByType[m_progressiveWinFG] = new long[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                            m_fgWithProgressives++;
                        }
                    }
                    if(DoCustomStats)
                    {
                        int winsThisFGNoProg = 0; 
                        winsThisFGNoProg = CountTheseWins(WinsToShow);
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
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private void setCurrentProgAwards(bool setTop, bool setBottom)
        {
        }

        private List<WinArgs> getProgressiveWin(ProgressiveData _progressiveLevel)
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            winsToReturn.Add(new WinArgs(_progressiveLevel.Name, (int)_progressiveLevel.GetProgressiveAndReset()));
            return winsToReturn;
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        WinDistributionChart m_statsBySymbol, m_statsWithNoStacks, m_stacksNoProgressives;
        internal override void SetUpCustomStats()
        {
            m_statsBySymbol = new WinDistributionChart("Wins By Symbol in Super Streams", m.MakeIntHeaders("Symbol", "", 0, Reels.Symbols.Count()), Bet, m.Multipliers);
            m_statsWithNoStacks = new WinDistributionChart("Wins without Stacks", new List<string> { "No Stack" }, Bet, m.Multipliers);
            m_stacksNoProgressives = new WinDistributionChart("Wins without Progressives", m.MakeIntHeaders("Index ", "", 0, Reels.Symbols.Count()), Bet, m.Multipliers);
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            m_statsBySymbol.InputResults(_results);
            m_statsWithNoStacks.InputResults(_results);
            m_stacksNoProgressives.InputResults(_results);
            _results.Add(new List<string>());
        }
    }
}
