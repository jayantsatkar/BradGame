using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Emerald_Fairy : GameInfo
    {
        internal Emerald_Fairy(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wi");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            List<string> alternateReel1 = new List<string>();
            for (int aa = 0; aa < m_extraData["alt reel 1"].Count(); aa++)
            {
                alternateReel1.Add(m_extraData["alt reel 1"][aa][0]);
            }
            altReel1 = new List<int>();
            for (int ab = 0; ab < alternateReel1.Count(); ab++)
            {
                altReel1.Add(Reels.Symbols.IndexOf(alternateReel1[ab]));
            }
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild;
        protected int[] cSuperStreamsSpinOrder = { 6, 7, 8, 9, 10 };
        int m_oldInsertionIndex = -1;
        List<int> altReel1;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_insertionIndex = -1;
            m_oldInsertionIndex = -1;
            InFreeGames = false;
            getReelSetsAndIndexes();
            m_freeGamesLeft = 0;
            m_freeGamesPlayed = 0;
            m_progressiveWinPG = 0;
            m_progressiveWinFG = 0;
            m_totalProgressiveWinFG = 0;
            m_maxProgressiveWinThisFG = 0;
            Wins = new List<WinArgs>();
            WinsToShow = Wins;
            ActionPauseInMilliSeconds = 0;
            BonusCode = 0;
            SpinOrder = cDefaultSpinOrder;
            Animating = false;
            m_shamrockBoosts = 0;
            m_currentBankedGame = 1;
            if (DoDefaultWinDistributions)
            {
                m_statsWinDistributionPaidGame.IncGames();
                m_statsWinDistributionPaidGameHigh.IncGames();
                m_statsWinDistributionPG.IncGames();
            }
            if (!InSimulation)
            {
                m.gameMessageBox.Text = "";
            }
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
            if (m_insertionIndex >= 5 && !InSimulation)
            {
                m.gameMessageBox.Text = "Super Streams with " + Reels.Symbols[Reels.SSSymbolsBG[m_insertionIndex][1]];
                SpinOrder = m_insertionIndex >= 5 ? cSuperStreamsSpinOrder : cDefaultSpinOrder;
            }
            if (m_insertionIndex >= 5 && InSimulation && DoCustomStats)
            {
                m_statsBySymbol.IncGames();
                m_statsByNumberOfSymbol.IncGames();
            }
            if (m_progressiveType != "none")
            {
                if (m_progressiveType != "shamrock fortunes") incrementProgressives();
                else if (m_progressiveType == "shamrock fortunes")
                {
                    m_shamrockBoosts = 0;
                    for (int aa = 0; aa < m_progressives.Count(); aa++)
                    {
                        bool didItBoost = m_progressives[aa].IncrementShamrock(Bet, aa, BetLevel);
                        if (didItBoost) m_shamrockBoosts++;
                        if (!InSimulation)
                        {
                            frmProg.ProgressiveValueTBs[aa].Text = string.Format("{0:$0,0.00}", (double)m_progressives[aa].CurrentValue / 100);
                        }
                    }
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
                    if (m_shamrockBoosts > 0 && !InSimulation)
                    {
                        m_shamrockBoosts = 0;
                        m.gameMessageBox.Text = "Boost Won!";
                        m_gameState = DefaultGameStates.PGPostSpin;
                        ActionPauseInMilliSeconds = 2000;
                        return GameAction.Wait;
                    }
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    //edit the below loop to calculate the progressive award
                    if (m_progressiveType != "none")
                    {
                        List<WinArgs> progWins = new List<WinArgs>();
                        if (m_progressiveType == "must hit") progWins = getMustHitProgressiveWin();
                        else if (m_progressiveType == "shamrock fortunes")
                        {
                            progWins = getShamrockFortunesWin();
                        }
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
                        if (m_insertionIndex >= 1)
                        {
                            m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 1);
                            m_statsBySymbol.StoreGame(m_statsWinsThisPG, Reels.SSSymbolsBG[m_insertionIndex][1] - 1);
                            int stackSymbolCount = 0;
                            for(int aa = 1; aa < 4; aa++)
                            {
                                for(int ab = 0; ab < screenSymbols[aa].Count(); ab++)
                                {
                                    if (screenSymbols[aa][ab] == Reels.SSSymbolsBG[m_insertionIndex][1]) stackSymbolCount++;
                                }
                            }
                            m_statsByNumberOfSymbol.StoreGame(m_statsWinsThisPG, stackSymbolCount);
                        }
                    }
                    break;
                case DefaultGameStates.FGPreSpin:
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    //Set Up Free Game
                    m_insertionIndex = -1;
                    SpinOrder = cDefaultSpinOrder;
                    getFGReelStrips();
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                        m_statsBySymbolFG.IncGames();
                        m_statsByNumberOfSymbolFG.IncGames();
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
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                        m_statsBySymbolFG.StoreGame(winsThisFG, m_insertionIndex);
                        int stackSymbolCount = 0;
                        for(int aa = 1; aa < 4; aa++)
                        {
                            for(int ab = 0; ab < screenSymbols[aa].Count(); ab++)
                            {
                                if (screenSymbols[aa][ab] == m_insertionIndex + 1) stackSymbolCount++;
                            }
                        }
                        m_statsByNumberOfSymbolFG.StoreGame(winsThisFG, stackSymbolCount);
                    }
                    //Decrement Free Games Left
                    m_freeGamesLeft--;
                    m_freeGamesPlayed++;
                    break;
                case DefaultGameStates.BankGameState:
                    List<WinArgs> dummyWins = new List<WinArgs>();
                    dummyWins = getShamrockFortunesWin();
                    m_currentBankedGame++;
                    if (m_currentBankedGame < m_bankSize)
                    {
                        m_shamrockBoosts = 0;
                        m_shamrockBoosts = playShamrockFortunes();
                        if (!InSimulation)
                        {
                            for (int ac = 0; ac < m_progressives.Count(); ac++)
                            {
                                frmProg.ProgressiveValueTBs[ac].Text = string.Format("{0:$0,0.00}", (double)m_progressives[ac].CurrentValue / 100);
                            }
                        }
                        if(m_shamrockBoosts > 0 && !InSimulation)
                        {
                            ActionPauseInMilliSeconds = 2000;
                            m.gameMessageBox.Text = string.Format("Machine {0} Boosted The Jackpots!", m_currentBankedGame);
                            m_gameState = DefaultGameStates.BankGameState;
                            return GameAction.Wait;
                        }
                    }
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
            if (!InSimulation && m_bankSize > 1 && m_currentBankedGame < m_bankSize)
            {
                m_gameState = DefaultGameStates.BankGameState;
                return GameAction.Wait;
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
            List<List<int>> currentInsertionSet = InFreeGames ? Reels.SSSymbolsFG : Reels.SSSymbolsBG;
            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
            {
                List<int> reel = reels[reelNum];
                List<int> reelSymbols = new List<int>();
                for (int i = 0; i < Dimensions[reelNum]; i++)
                {
                    int index = (ReelIndexes[reelNum] + i + reel.Count) % reel.Count;
                    int symbolIndex = reel[index];
                    if (symbolIndex == m_insertionSymbol || symbolIndex == Reels.Symbols.IndexOf("re1")) symbolIndex = currentInsertionSet[m_insertionIndex][reelNum];
                    reelSymbols.Add(symbolIndex);
                }
                screenSymbols.Add(reelSymbols);
            }
            return screenSymbols;
        }

        internal override string GetSymbolName(int _reelNum, int _index, int _cellNumIfStopped, bool _spinStop)
        {
            List<List<int>> reels = currentReelSet;
            List<List<int>> currentInsertionSet = InFreeGames ? Reels.SSSymbolsFG : Reels.SSSymbolsBG;
            int index = (_index + reels[_reelNum].Count) % reels[_reelNum].Count;
            int symbolIndex = reels[_reelNum][index];
            if (symbolIndex == m_insertionSymbol || symbolIndex == Reels.Symbols.IndexOf("re1")) symbolIndex = currentInsertionSet[m_insertionIndex][_reelNum];
            return Reels.Symbols[symbolIndex];
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private void getReelSetsAndIndexes()
        {
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            m_insertionIndex = getInsertionIndex(Reels.SSCutoffsBG);
            for (int aa = 0; aa < ReelCount; aa++)
            {
                if (aa == 0)
                {
                    newReelSet.Add(m_insertionIndex >= 1 ? altReel1 : Reels.ReelsPG[aa]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsPGCutoffs[aa]);
                }
                else if (aa == 4)
                {
                    newReelSet.Add(Reels.ReelsPG[aa]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsPGCutoffs[aa]);
                }
                else if (aa != 0 && aa != 4)
                {
                    if (Reels.SSSymbolsBG[m_insertionIndex][aa] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsPG[aa][Reels.SSInsertionInfoIndicesBG[m_insertionIndex][aa] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsPGCutoffs[aa][Reels.SSInsertionInfoIndicesBG[m_insertionIndex][aa] + 1]);
                    }
                    else if (Reels.SSSymbolsBG[m_insertionIndex][aa] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsPG[aa]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsPGCutoffs[aa]);
                    }
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
        }

        private void getFGReelStrips()
        {
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            if (m_freeGamesPlayed == 0)
            {
                m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                m_oldInsertionIndex = m_insertionIndex;
            }
            else if (m_freeGamesPlayed != 0) m_insertionIndex = m_oldInsertionIndex;
            for (int aa = 0; aa < ReelCount; aa++)
            {
                if (aa == 0 || aa == 4)
                {
                    newReelSet.Add(Reels.ReelsFG[aa]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFGCutoffs[aa]);
                }
                else if (aa != 0 && aa != 4)
                {
                    if (Reels.SSSymbolsFG[m_insertionIndex][aa] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsFG[aa][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][aa] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsFGCutoffs[aa][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][aa] + 1]);
                    }
                    else if (Reels.SSSymbolsFG[m_insertionIndex][aa] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsFG[aa]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFGCutoffs[aa]);
                    }
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        WinDistributionChart m_statsBySymbolFG, m_statsByNumberOfSymbolFG, m_statsBySymbol, m_statsByNumberOfSymbol;
        internal override void SetUpCustomStats()
        {
            m_statsBySymbol = new WinDistributionChart("Wins By Symbol in Super Streams", m.MakeIntHeaders("Symbol", "", 1, 5), Bet, m.Multipliers);
            m_statsByNumberOfSymbol = new WinDistributionChart("Wins By Symbol Count on Middle Reel", m.MakeIntHeaders("", "", 0, 10), Bet, m.Multipliers);
            m_statsBySymbolFG = new WinDistributionChart("Wins By Symbol in Super Streams FG", m.MakeIntHeaders("Symbol", "", 1, 5), Bet, m.Multipliers);
            m_statsByNumberOfSymbolFG = new WinDistributionChart("Wins By Symbol Count on Middle Reel FG", m.MakeIntHeaders("", "", 0, 10), Bet, m.Multipliers);
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            m_statsBySymbol.InputResults(_results);
            m_statsByNumberOfSymbol.InputResults(_results);
            m_statsBySymbolFG.InputResults(_results);
            m_statsByNumberOfSymbolFG.InputResults(_results);
        }
    }
}
