using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Butterfly_Game : GameInfo
    {
        internal Butterfly_Game(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_symbolSuper = Reels.Symbols.IndexOf("sb");
            m_dummyWildReel = new List<int>();
            for(int aa = 0; aa < 5; aa++)
            {
                m_dummyWildReel.Add(m_symbolWild);
            }
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_symbolSuper;
        int m_superFreeGames, m_superFreeGamesPlayed, m_wildExpansionCount, m_totalSFGWin, m_expandedWilds, m_totalFGWin;
        List<int> m_dummyWildReel;
        List<List<int>> m_reSpinReelSet;
        bool m_expansionOccured = false;
        List<List<int>> m_nudgeScreenSymbols;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_superFreeGames = 0;
            m_superFreeGamesPlayed = 0;
            m_reSpinReelSet = new List<List<int>>();
            m_nudgeScreenSymbols = new List<List<int>>();
            m_expansionOccured = false;
            m_wildExpansionCount = 0;
            m_totalSFGWin = 0;
            m_totalFGWin = 0;
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
                incrementProgressives();
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
                    if (m_progressiveType != "none" && !StepThrough)
                    {
                        List<WinArgs> progWins = new List<WinArgs>();
                        progWins = getMustHitProgressiveWin();
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
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES and {2} SUPER FREE GAMES", 
                                                                             m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed, m_superFreeGames);
                    //Set Up Free Game
                    GetFGReelStrips(false);
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
                    Wins.AddRange(WinsToShow);
                    if (CountSymbolsOnScreen(screenSymbols, m_symbolSuper) > 0) m_superFreeGames += CountSymbolsOnScreen(screenSymbols, m_symbolSuper);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                        m_totalFGWin += winsThisFG;
                    }
                    //Decrement Free Games Left
                    m_freeGamesLeft--;
                    m_freeGamesPlayed++;
                    break;
                case DefaultGameStates.SuperFGPreSpin:
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} SUPER FREE GAMES", m_superFreeGamesPlayed + 1, m_superFreeGames + m_superFreeGamesPlayed);
                    //Set Up Free Game
                    getSuperFGReelStrips();
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                    }
                    m_progressiveWinFG = 0;
                    m_expansionOccured = false;
                    m_nudgeScreenSymbols = new List<List<int>>();
                    m_gameState = DefaultGameStates.SuperFGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.SuperFGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    int wildCountOnScreen = 0;
                    for (int ab = 0; ab < screenSymbols.Count(); ab++)
                    {
                        if (screenSymbols[ab].Contains(m_symbolWild)) wildCountOnScreen++;
                    }
                    if(!m_expansionOccured && wildCountOnScreen > 0)
                    {
                        m_nudgeScreenSymbols = screenSymbols;
                        m_expansionOccured = true;
                        m_gameState = DefaultGameStates.ReSpinPreSpin;
                        ActionPauseInMilliSeconds = 0;
                        return GameAction.Wait;
                    }
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisSFG = CountTheseWins(WinsToShow);
                        m_totalSFGWin += winsThisSFG;
                        m_singleSFGWins.StoreGame(winsThisSFG, 0);
                        m_singleSFGWins.StoreGame(winsThisSFG, 1);
                    }
                    //Decrement Free Games Left
                    m_superFreeGames--;
                    m_superFreeGamesPlayed++;
                    break;
                case DefaultGameStates.ReSpinPreSpin:
                    SpinOrder = cNudgeSpinOrder;
                    m_expandedWilds = 0;
                    m_reSpinReelSet = new List<List<int>>();
                    m_reSpinReelSet = getreSpinReelSet();
                    currentReelSet = m_reSpinReelSet;
                    m_wildExpansionCount++;
                    ReelIndexes = getSFGNudgeIndexes(m_nudgeScreenSymbols);
                    m_gameState = DefaultGameStates.ReSpinPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.ReSpinPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisSFG = CountTheseWins(WinsToShow);
                        m_singleSFGWins.StoreGame(winsThisSFG, 2);
                        m_singleSFGWins.StoreGame(winsThisSFG, 0);
                        m_totalSFGWin += winsThisSFG;
                        if (m_howManyWilds.Keys.Contains(m_expandedWilds)) m_howManyWilds[m_expandedWilds]++;
                        else if (!m_howManyWilds.Keys.Contains(m_expandedWilds)) m_howManyWilds[m_expandedWilds] = 1;
                    }
                    //Decrement Free Games Left
                    m_superFreeGames--;
                    m_superFreeGamesPlayed++;
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
            if(m_superFreeGames > 0)
            {
                if(DoCustomStats && m_superFreeGamesPlayed == 0)
                {
                    m_totalSFGWins.IncGames();
                    if (m_superGamesWon.Keys.Contains(m_superFreeGames)) m_superGamesWon[m_superFreeGames]++;
                    else if (!m_superGamesWon.Keys.Contains(m_superFreeGames)) m_superGamesWon[m_superFreeGames] = 1;
                    m_totalSFGWin = 0;
                }
                m_gameState = DefaultGameStates.SuperFGPreSpin;
                return GameAction.ShowWinsInBetweenGames;
            }
            //Sum up wins and Stats
            WinsThisGame = CountTheseWins(Wins);
            AfterGameStatsCollection();
            if (m_freeGamesLeft == 0 && m_freeGamesPlayed > 0 && !InSimulation) m.gameMessageBox.Text = string.Format("{0} Total Free Spins Bonus Win", WinsThisGame);
            if (DoCustomStats)
            {
                if(m_freeGamesLeft == 0 && m_freeGamesPlayed > 0)
                {
                    if (m_totalFGWinCounts.Keys.Contains(m_totalFGWin)) m_totalFGWinCounts[m_totalFGWin]++;
                    else if (!m_totalFGWinCounts.Keys.Contains(m_totalFGWin)) m_totalFGWinCounts[m_totalFGWin] = 1;
                }
                if(m_superFreeGames == 0 && m_superFreeGamesPlayed > 0)
                {
                    m_totalSFGStats.storeWin(m_totalSFGWin);
                    if (m_wildExpansions.Keys.Contains(m_wildExpansionCount)) m_wildExpansions[m_wildExpansionCount]++;
                    else if (!m_wildExpansions.Keys.Contains(m_wildExpansionCount)) m_wildExpansions[m_wildExpansionCount] = 1;
                    m_totalSFGWins.StoreGame(m_totalSFGWin, 0);
                    if (m_totalSFGWinCounts.Keys.Contains(m_totalSFGWin)) m_totalSFGWinCounts[m_totalSFGWin]++;
                    else if (!m_totalSFGWinCounts.Keys.Contains(m_totalSFGWin)) m_totalSFGWinCounts[m_totalSFGWin] = 1;
                    if(m_sfgWinTotalsBySpinCount.Keys.Contains(m_superFreeGamesPlayed))
                    {
                        if (m_sfgWinTotalsBySpinCount[m_superFreeGamesPlayed].Keys.Contains(m_totalSFGWin)) m_sfgWinTotalsBySpinCount[m_superFreeGamesPlayed][m_totalSFGWin]++;
                        else if (!m_sfgWinTotalsBySpinCount[m_superFreeGamesPlayed].Keys.Contains(m_totalSFGWin)) m_sfgWinTotalsBySpinCount[m_superFreeGamesPlayed][m_totalSFGWin] = 1;
                    }
                    else if (!m_sfgWinTotalsBySpinCount.Keys.Contains(m_superFreeGamesPlayed))
                    {
                        m_sfgWinTotalsBySpinCount[m_superFreeGamesPlayed] = new SortedDictionary<int, ulong>();
                        m_sfgWinTotalsBySpinCount[m_superFreeGamesPlayed][m_totalSFGWin] = 1;
                    }
                    if (m_sfgWildExpandBySpinCount.Keys.Contains(m_superFreeGamesPlayed))
                    {
                        if (m_sfgWildExpandBySpinCount[m_superFreeGamesPlayed].Keys.Contains(m_wildExpansionCount)) m_sfgWildExpandBySpinCount[m_superFreeGamesPlayed][m_wildExpansionCount]++;
                        else if (!m_sfgWildExpandBySpinCount[m_superFreeGamesPlayed].Keys.Contains(m_wildExpansionCount)) m_sfgWildExpandBySpinCount[m_superFreeGamesPlayed][m_wildExpansionCount] = 1;
                    }
                    else if (!m_sfgWildExpandBySpinCount.Keys.Contains(m_superFreeGamesPlayed))
                    {
                        m_sfgWildExpandBySpinCount[m_superFreeGamesPlayed] = new SortedDictionary<int, ulong>();
                        m_sfgWildExpandBySpinCount[m_superFreeGamesPlayed][m_wildExpansionCount] = 1;
                    }
                }
                
            }
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private void getSuperFGReelStrips()
        {
            currentReelSet = Reels.ReelsAlt[0];
            currentCutoffSet = currentCutoffSet;
            ReelIndexes = Reels.GetRandomIndexes(currentReelSet, currentCutoffSet);
        }
        private List<List<int>> getreSpinReelSet()
        {
            List<List<int>> reelSetReturn = new List<List<int>>();
            for(int ac = 0; ac < ReelCount; ac++)
            {
                if (m_nudgeScreenSymbols[ac].Contains(m_symbolWild))
                {
                    reelSetReturn.Add(m_dummyWildReel);
                    m_expandedWilds++;
                }
                else if (!m_nudgeScreenSymbols[ac].Contains(m_symbolWild)) reelSetReturn.Add(currentReelSet[ac]);
            }
            return reelSetReturn;
        }
        private List<int> getSFGNudgeIndexes(List<List<int>> _screenSymbols)
        {
            List<int> reelIndexesToReturn = new List<int>();
            for(int ad = 0; ad < ReelCount; ad++)
            {
                reelIndexesToReturn.Add(ReelIndexes[ad]);
                if (_screenSymbols[ad].Contains(m_symbolWild)) reelIndexesToReturn[ad] = 0;
            }
            return reelIndexesToReturn;
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        WinDistributionChart m_totalSFGWins, m_singleSFGWins;
        SortedDictionary<int, ulong> m_superGamesWon;
        SortedDictionary<int, ulong> m_wildExpansions;
        SortedDictionary<int, ulong> m_howManyWilds;
        SortedDictionary<int, ulong> m_totalSFGWinCounts;
        SortedDictionary<int, SortedDictionary<int, ulong>> m_sfgWinTotalsBySpinCount;
        SortedDictionary<int, SortedDictionary<int, ulong>> m_sfgWildExpandBySpinCount;
        SortedDictionary<int, ulong> m_totalFGWinCounts;
        WinStatsInfo m_totalSFGStats;
        internal override void SetUpCustomStats()
        {
            m_totalSFGWins = new WinDistributionChart("Super Free Games Total Wins", new List<string> { "Wins" }, Bet, m.Multipliers);
            m_singleSFGWins = new WinDistributionChart("Super Free Games Single Spin Wins", new List<string> { "Overall", "No Expansion", "Expansion" }, Bet, m.Multipliers);
            m_superGamesWon = new SortedDictionary<int, ulong>();
            m_wildExpansions = new SortedDictionary<int, ulong>();
            m_howManyWilds = new SortedDictionary<int, ulong>();
            m_totalSFGWinCounts = new SortedDictionary<int, ulong>();
            m_sfgWinTotalsBySpinCount = new SortedDictionary<int, SortedDictionary<int, ulong>>();
            m_sfgWildExpandBySpinCount = new SortedDictionary<int, SortedDictionary<int, ulong>>();
            m_totalFGWinCounts = new SortedDictionary<int, ulong>();
            m_totalSFGStats = new WinStatsInfo("Super Free Games", Bet, true);
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            m_totalSFGWins.InputResults(_results);
            m_singleSFGWins.InputResults(_results);
            m_totalSFGStats.InputResults(_results);
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total Super Free Games Counts" }));
            _results.Add(new List<string>(new string[] { "Spins", "Counts" }));
            foreach(int spinCount in m_superGamesWon.Keys)
            {
                _results.Add(new List<string>(new string[] { spinCount.ToString(), m_superGamesWon[spinCount].ToString() }));
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total Wild Expansion Counts" }));
            _results.Add(new List<string>(new string[] { "Expansions", "Counts" }));
            foreach(int expansionCount in m_wildExpansions.Keys)
            {
                _results.Add(new List<string>(new string[] { expansionCount.ToString(), m_wildExpansions[expansionCount].ToString() }));
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "How Many Wilds on a Spin" }));
            _results.Add(new List<string>(new string[] { "Expansions", "Counts" }));
            foreach (int expansionCount in m_howManyWilds.Keys)
            {
                _results.Add(new List<string>(new string[] { expansionCount.ToString(), m_howManyWilds[expansionCount].ToString() }));
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total SFG Win Counts" }));
            _results.Add(new List<string>(new string[] { "Win Amount", "Count" }));
            foreach (int winAmount in m_totalSFGWinCounts.Keys)
            {
                _results.Add(new List<string>(new string[] { winAmount.ToString(), m_totalSFGWinCounts[winAmount].ToString() }));
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total SFG Win Counts by Spin Count" }));
            _results.Add(new List<string>(new string[] { "Spin Count", "Win Amount", "Count" }));
            foreach(int spinCount in m_sfgWinTotalsBySpinCount.Keys)
            {
                foreach(int winAmount in m_sfgWinTotalsBySpinCount[spinCount].Keys)
                {
                    _results.Add(new List<string>(new string[] { spinCount.ToString(), winAmount.ToString(), m_sfgWinTotalsBySpinCount[spinCount][winAmount].ToString() }));
                }
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total SFG Expansion by Spin Count" }));
            _results.Add(new List<string>(new string[] { "Spin Count", "Expansions", "Count" }));
            foreach (int spinCount in m_sfgWildExpandBySpinCount.Keys)
            {
                foreach (int wildExpand in m_sfgWildExpandBySpinCount[spinCount].Keys)
                {
                    _results.Add(new List<string>(new string[] { spinCount.ToString(), wildExpand.ToString(), m_sfgWildExpandBySpinCount[spinCount][wildExpand].ToString() }));
                }
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total FG Win Counts" }));
            _results.Add(new List<string>(new string[] { "Win Amount", "Count" }));
            foreach (int winAmount in m_totalFGWinCounts.Keys)
            {
                _results.Add(new List<string>(new string[] { winAmount.ToString(), m_totalFGWinCounts[winAmount].ToString() }));
            }
            _results.Add(new List<string>());
        }
    }
}
