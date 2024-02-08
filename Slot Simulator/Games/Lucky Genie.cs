using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Lucky_Genie : GameInfo
    {
        internal Lucky_Genie(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            List<int> fgWildMultiplierWeights = new List<int>();
            for (int row = 0; row < m_extraData["wild weight table"].Count(); row++)
            {
                m_fgWildMultipliers.Add(int.Parse(m_extraData["wild weight table"][row][0].ToString()));
                fgWildMultiplierWeights.Add(int.Parse(m_extraData["wild weight table"][row][1].ToString()));
            }
            m_fgWildMultipliersCutOffs = m.MakeCutoffs(fgWildMultiplierWeights);
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild;
        List<int> m_fgWildMultipliersCutOffs = new List<int>();
        List<int> m_fgWildMultipliers = new List<int>();
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            base.PreSpin(false);
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    case System.Windows.Forms.Keys.Oemtilde://Cheat Key "~"
                        ReelIndexes[0] = 9;
                        ReelIndexes[1] = 6;
                        ReelIndexes[2] = 6;
                        break;
                }
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
                    getFGReels();
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
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                        if(winsThisFG > 1000 * 50 * 25)
                        {
                            int index = 0;
                            index++;
                        }
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
                        if (m_shamrockBoosts > 0 && !InSimulation)
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
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private List<WinArgs> getProgressiveWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            return winsToReturn;
        }
        private void getFGReels()
        {
            int wildMultiplierIndex = m.RandomIndex(m_fgWildMultipliersCutOffs);
            for(int reel = 0; reel < ReelCount; reel++)
            {
                currentReelSet[reel] = Reels.ReelsAlt[wildMultiplierIndex][reel];
            }
            ReelIndexes = Reels.GetRandomIndexes(currentReelSet, currentCutoffSet);
            if (DoCustomStats)
            {
                if (m_fgWildMults.Keys.Contains(m_fgWildMultipliers[wildMultiplierIndex])) m_fgWildMults[m_fgWildMultipliers[wildMultiplierIndex]]++;
                else if (!m_fgWildMults.Keys.Contains(m_fgWildMultipliers[wildMultiplierIndex])) m_fgWildMults[m_fgWildMultipliers[wildMultiplierIndex]] = 1;
            }
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        SortedDictionary<int, ulong> m_fgWildMults;
        internal override void SetUpCustomStats()
        {
            m_fgWildMults = new SortedDictionary<int, ulong>();
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total Multiplier Counts" }));
            _results.Add(new List<string>(new string[] { "Multiplier", "Count" }));
            foreach (int multiplier in m_fgWildMults.Keys)
            {
                _results.Add(new List<string>(new string[] { multiplier.ToString(), m_fgWildMults[multiplier].ToString() }));
            }
            _results.Add(new List<string>());
        }
    }
}
