using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Quasi_Shot : GameInfo
    {
        internal Quasi_Shot(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild;
        int m_progCellNum = -1;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_progCellNum = -1;
            int randomDraw = m.RandomInteger(15);
            if (randomDraw < BetLevel) m_progCellNum = 0;
            else if (randomDraw < BetLevel * 2) m_progCellNum = 1;
            else if (randomDraw < BetLevel * 3) m_progCellNum = 2;
            base.PreSpin(false);
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
                }
            }
            if (m_progressiveType != "none")
            {
                for (int aa = 0; aa < m_progressives.Count(); aa++)
                {
                    m_progressives[aa].Increment(Bet);
                    if (!InSimulation) frmProg.ProgressiveValueTBs[aa].Text = string.Format("{0:$0,0.00}", (double)m_progressives[aa].CurrentValue / 100);
                }
            }
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
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    //edit the below loop to calculate the progressive award
                    if (m_progressiveType != "none")
                    {
                        List<WinArgs> progWins = new List<WinArgs>();
                        List<WinArgs> possibleProgs = new List<WinArgs>();
                        foreach(WinArgs winsToReview in WinsToShow)
                        {
                            if (winsToReview.SymbolIndex != m_symbolScatter)
                            {
                                if (winsToReview.Count == 5 && winsToReview.CellNums[4].Contains(m_progCellNum)) possibleProgs.Add(winsToReview);
                            }
                        }
                        progWins = getProgressiveWin(possibleProgs, screenSymbols);
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        m_progressiveWinPG = CountTheseWins(progWins);
                    }
                    //adds all wins into the win list
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinsThisPG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 0);
                    }
                    break;
                case DefaultGameStates.FGPreSpin:
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    //Set Up Free Game
                    GetFGReelStrips(false);
                    m_progCellNum = -1;
                    int randomDraw = m.RandomInteger(15);
                    if (randomDraw < BetLevel) m_progCellNum = 0;
                    else if (randomDraw < BetLevel * 2) m_progCellNum = 1;
                    else if (randomDraw < BetLevel * 3) m_progCellNum = 2;
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                    }
                    m_progressiveWinFG = 0;
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
                        List<WinArgs> possibleProgs = new List<WinArgs>();
                        foreach (WinArgs winsToReview in WinsToShow)
                        {
                            if (winsToReview.SymbolIndex != m_symbolScatter)
                            {
                                if (winsToReview.Count == 5 && winsToReview.CellNums[4].Contains(m_progCellNum)) possibleProgs.Add(winsToReview);
                            }
                        }
                        progWins = getProgressiveWin(possibleProgs, screenSymbols);
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
            if (m_freeGamesLeft == 0 && m_freeGamesPlayed > 0 && !InSimulation) m.gameMessageBox.Text = string.Format("{0} Total Free Spins Bonus Win", WinsThisGame);
            if(DoCustomStats)
            {
            }
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
        }
        internal override void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            if (m_progCellNum != -1)
            {
                _reelsPanel.SpriteDraw("progframe", new Point(_reelsPanel.ReelOffsets[4].X, _reelsPanel.ReelOffsets[4].Y + m_progCellNum * SymbolHeight));
            }
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private List<WinArgs> getProgressiveWin(List<WinArgs> possibleProgWins, List<List<int>> _screenSymbols)
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
                foreach (WinArgs winToCheck in possibleProgWins)
                {
                    int wildCount = 0;
                    for (int aa = 0; aa < ReelCount; aa++)
                    {
                        int cellNum = winToCheck.CellNums[aa][0];
                        if (_screenSymbols[aa][cellNum] == m_symbolWild) wildCount++;
                    }
                    if (wildCount > 0)
                    {
                        if (winToCheck.SymbolIndex > 5 && m_progCellNum == winToCheck.CellNums[4][0])
                        {
                            winsToReturn.Add(new WinArgs("Mini Progressive", winToCheck.SymbolIndex, m_progressives[3].GetProgressiveAndReset(), winToCheck.Count, winToCheck.Multiplier, winToCheck.CellNums,
                                                         winToCheck.IsScatter));
                            if (DoCustomStats)
                            {
                                if (InFreeGames) m_progressiveCountsFG["Mini"]++;
                                else if (!InFreeGames) m_progressiveCountsPG["Mini"]++;
                            }
                        }
                        else if (winToCheck.SymbolIndex <= 5 && m_progCellNum == winToCheck.CellNums[4][0])
                        {
                            winsToReturn.Add(new WinArgs("Minor Progressive", winToCheck.SymbolIndex, m_progressives[2].GetProgressiveAndReset(), winToCheck.Count, winToCheck.Multiplier, winToCheck.CellNums,
                                             winToCheck.IsScatter));
                            if (DoCustomStats)
                            {
                                if (InFreeGames) m_progressiveCountsFG["Minor"]++;
                                else if (!InFreeGames) m_progressiveCountsPG["Minor"]++;
                            }
                        }
                    }
                    else if (wildCount == 0)
                    {
                        if (winToCheck.SymbolIndex > 5 && m_progCellNum == winToCheck.CellNums[4][0])
                        {
                            winsToReturn.Add(new WinArgs("Major Progressive", winToCheck.SymbolIndex, m_progressives[1].GetProgressiveAndReset(), winToCheck.Count, winToCheck.Multiplier, winToCheck.CellNums,
                                                         winToCheck.IsScatter));
                            if (DoCustomStats)
                            {
                                if (InFreeGames) m_progressiveCountsFG["Major"]++;
                                else if (!InFreeGames) m_progressiveCountsPG["Major"]++;
                            }
                        }
                        else if (winToCheck.SymbolIndex <= 5 && m_progCellNum == winToCheck.CellNums[4][0])
                        {
                            winsToReturn.Add(new WinArgs("Grand Progressive", winToCheck.SymbolIndex, m_progressives[0].GetProgressiveAndReset(), winToCheck.Count, winToCheck.Multiplier, winToCheck.CellNums,
                                             winToCheck.IsScatter));
                            if (DoCustomStats)
                            {
                                if (InFreeGames) m_progressiveCountsFG["Grand"]++;
                                else if (!InFreeGames) m_progressiveCountsPG["Grand"]++;
                            }
                        }
                    }
                }
            return winsToReturn;
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        SortedDictionary<string, ulong> m_progressiveCountsPG;
        SortedDictionary<string, ulong> m_progressiveCountsFG;
        internal override void SetUpCustomStats()
        {
            m_progressiveCountsPG = new SortedDictionary<string, ulong>();
            m_progressiveCountsFG = new SortedDictionary<string, ulong>();
            for(int aa = 0; aa < m_progressives.Count(); aa++)
            {
                m_progressiveCountsPG.Add(m_progressives[aa].Name, 0);
                m_progressiveCountsFG.Add(m_progressives[aa].Name, 0);
            }
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Primary Progressive Information" }));
            _results.Add(new List<string>(new string[] { "Progressive", "Hits" }));
            foreach (string progType in m_progressiveCountsPG.Keys)
            {
                _results.Add(new List<string>(new string[] { progType, m_progressiveCountsPG[progType].ToString() }));
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Free Spins Progressive Information" }));
            _results.Add(new List<string>(new string[] { "Progressive", "Hits" }));
            foreach (string progType in m_progressiveCountsFG.Keys)
            {
                _results.Add(new List<string>(new string[] { progType, m_progressiveCountsFG[progType].ToString() }));
            }
        }
    }
}
