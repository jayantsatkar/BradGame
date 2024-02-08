using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Golden_Wins_Lucky_Scroll : GameInfo
    {
        internal Golden_Wins_Lucky_Scroll(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wi");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_bgReplacementCutoffs = new List<int>();
            m_bgReplacementSymbols = new List<List<int>>();
            List<int> bgReplacementWeights = new List<int>();
            for (int aa = 0; aa < m_extraData["bonus substitution"].Count(); aa++)
            {
                List<int> replaceCombo = new List<int>();
                replaceCombo.Add(Reels.Symbols.IndexOf(m_extraData["bonus substitution"][aa][0]));
                replaceCombo.Add(Reels.Symbols.IndexOf(m_extraData["bonus substitution"][aa][1]));
                replaceCombo.Add(Reels.Symbols.IndexOf(m_extraData["bonus substitution"][aa][2]));
                bgReplacementWeights.Add(int.Parse(m_extraData["bonus substitution"][aa][3]));
                m_bgReplacementSymbols.Add(replaceCombo);
            }
            m_bgReplacementCutoffs = m.MakeCutoffs(bgReplacementWeights);
            for (int ab = 0; ab < ReelCount; ab++)
            {
                List<int> wildStops = new List<int>();
                for (int ad = 0; ad < Reels.ReelsAlt[1][ab].Count(); ad++)
                {
                    int wildCount = 0;
                    if (Reels.ReelsAlt[1][ab][ad] == m_symbolWild) wildCount++;
                    int currentStop = ad;
                    if (currentStop != Reels.ReelsAlt[1][ab].Count - 1 && currentStop != Reels.ReelsAlt[1][ab].Count - 2)
                    {
                        for (int ac = 1; ac < Dimensions[ab]; ac++)
                        {
                            if (Reels.ReelsAlt[1][ab][currentStop + ac] == m_symbolWild) wildCount++;
                        }
                        if (wildCount == Dimensions[ab]) wildStops.Add(ad);
                    }
                }
                m_fullWildReelStops.Add(wildStops);
            }
            for(int ae = 0; ae < 5; ae++)
            {
                m_dummyWildReel.Add(m_symbolWild);
            }
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild;
        List<int> m_bgReplacementCutoffs;
        List<List<int>> m_bgReplacementSymbols;
        int m_fgReelStripIndex = -1;
        List<List<int>> m_fullWildReelStops = new List<List<int>>();
        List<List<int>> m_nudgeScreenSymbols = new List<List<int>>();
        List<int> m_dummyWildReel = new List<int>();
        List<List<int>> m_reSpinReelSet = new List<List<int>>();
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_fgReelStripIndex = -1;
            m_nudgeScreenSymbols = new List<List<int>>();
            m_reSpinReelSet = new List<List<int>>();
            base.PreSpin(false);
            getBaseSubstitution();
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
            bool nudgehappened = false;
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
                    if(m_freeGamesLeft > 0)
                    {
                        int goldSymbols = CountSymbolsOnScreen(screenSymbols, Reels.Symbols.IndexOf("bg"));
                        int silverSymbols = CountSymbolsOnScreen(screenSymbols, Reels.Symbols.IndexOf("bs"));
                        if (goldSymbols == 3) m_fgReelStripIndex = 2;
                        else if (silverSymbols == 3) m_fgReelStripIndex = 1;
                        else if (goldSymbols != 3 && silverSymbols != 3) m_fgReelStripIndex = 0;
                    }
                    break;
                case DefaultGameStates.FGPreSpin:
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    //Set Up Free Game
                    getFGReelStrips();
                    ReelIndexes = Reels.GetRandomIndexes(currentReelSet, currentCutoffSet);
                    m_reSpinReelSet = new List<List<int>>();
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                    }
                    m_progressiveWinFG = 0;
                    nudgehappened = false;
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    int wildCountOnScreen = 0;
                    for (int ae = 0; ae < screenSymbols.Count(); ae++)
                    {
                        if (screenSymbols[ae].Contains(m_symbolWild)) wildCountOnScreen++;
                    }
                    if (m_fgReelStripIndex == 2 && !nudgehappened && wildCountOnScreen > 0)
                    {
                        m_nudgeScreenSymbols = screenSymbols;
                        nudgehappened = true;
                        m_gameState = DefaultGameStates.ReSpinPreSpin;
                        ActionPauseInMilliSeconds = 0;
                        return GameAction.Wait;
                    }
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
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
                case DefaultGameStates.ReSpinPreSpin:
                    SpinOrder = cNudgeSpinOrder;
                    m_reSpinReelSet = new List<List<int>>();
                    m_reSpinReelSet = getReSpinReelSet();
                    currentReelSet = m_reSpinReelSet;
                    ReelIndexes = getFGNudgeIndexes(m_nudgeScreenSymbols);
                    m_gameState = DefaultGameStates.ReSpinPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.ReSpinPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
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
            if (DoCustomStats)
            {
            }
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
        }
        internal override List<List<int>> GetScreenSymbols(List<List<int>> ReelSet)
        {
            List<List<int>> reels = ReelSet;
            List<List<int>> screenSymbols = new List<List<int>>();
            List<int> currentInsertionSet = m_bgReplacementSymbols[m_insertionIndex];
            for(int reelNum = 0; reelNum < ReelCount; reelNum++)
            {
                List<int> reel = reels[reelNum];
                List<int> reelSymbols = new List<int>();
                for(int i = 0; i < Dimensions[reelNum]; i++)
                {
                    int index = (ReelIndexes[reelNum] + i + reel.Count) % reel.Count;
                    int symbolIndex = reel[index];
                    if (symbolIndex == Reels.Symbols.IndexOf("r1")) reelSymbols.Add(currentInsertionSet[0]);
                    else if (symbolIndex == Reels.Symbols.IndexOf("r2")) reelSymbols.Add(currentInsertionSet[1]);
                    else if (symbolIndex == Reels.Symbols.IndexOf("r3")) reelSymbols.Add(currentInsertionSet[2]);
                    else if (symbolIndex != Reels.Symbols.IndexOf("r1") && symbolIndex != Reels.Symbols.IndexOf("r2") && symbolIndex != Reels.Symbols.IndexOf("r3")) reelSymbols.Add(symbolIndex);
                }
                screenSymbols.Add(reelSymbols);
            }
            return screenSymbols;
        }
        internal override string GetSymbolName(int _reelNum, int _index, int _cellNumIfStopped, bool _spinStop)
        {
            List<List<int>> reels = currentReelSet;
            List<int> currentInsertionSet = m_bgReplacementSymbols[m_insertionIndex];
            int index = (_index + reels[_reelNum].Count) % reels[_reelNum].Count;
            int symbolIndex = reels[_reelNum][index];
            if (symbolIndex == Reels.Symbols.IndexOf("r1")) symbolIndex = currentInsertionSet[0];
            else if (symbolIndex == Reels.Symbols.IndexOf("r2")) symbolIndex = currentInsertionSet[1];
            else if (symbolIndex == Reels.Symbols.IndexOf("r3")) symbolIndex = currentInsertionSet[2];
            return Reels.Symbols[symbolIndex];
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private void getBaseSubstitution()
        {
            m_insertionIndex = m.RandomIndex(m_bgReplacementCutoffs);
        }
        private void getFGReelStrips()
        {
            if (m_fgReelStripIndex == 0)
            {
                currentReelSet = Reels.ReelsFG;
                currentCutoffSet = Reels.ReelsFGCutoffs;
            }
            else if (m_fgReelStripIndex == 1)
            {
                currentReelSet = Reels.ReelsAlt[0];
                currentCutoffSet = Reels.ReelsFGCutoffs;
            }
            else if (m_fgReelStripIndex == 2)
            {
                currentReelSet = Reels.ReelsAlt[1];
                currentCutoffSet = Reels.ReelsFGCutoffs;
            }
        }
        private List<int> getFGNudgeIndexes(List<List<int>> _screenSymbols)
        {
            List<int> reelIndexesToReturn = new List<int>();
            for(int aa = 0; aa < ReelCount; aa++)
            {
                reelIndexesToReturn.Add(ReelIndexes[aa]);
                if (_screenSymbols[aa].Contains(m_symbolWild)) reelIndexesToReturn[aa] = 0;
            }
            return reelIndexesToReturn;
        }
        private List<List<int>> getReSpinReelSet()
        {
            List<List<int>> reelSetToReturn = new List<List<int>>();
            for (int aa = 0; aa < ReelCount; aa++)
            {
                if (m_nudgeScreenSymbols[aa].Contains(m_symbolWild)) reelSetToReturn.Add(m_dummyWildReel);
                else if (!m_nudgeScreenSymbols[aa].Contains(m_symbolWild)) reelSetToReturn.Add(currentReelSet[aa]);
            }
            return reelSetToReturn;
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        internal override void SetUpCustomStats()
        {
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
        }
    }
}
