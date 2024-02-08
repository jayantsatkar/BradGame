using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Bone_Garden : GameInfo
    {
        internal Bone_Garden(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            List<int> fsWeights1 = new List<int>();
            List<int> fsWeights2 = new List<int>();
            m_fgReelStripCutoffs = new List<List<int>>();
            for(int aa = 0; aa < m_extraData["free spin symbol draw"].Count(); aa++)
            {
                fsWeights1.Add(int.Parse(m_extraData["free spin symbol draw"][aa][0].ToString()));
                fsWeights2.Add(int.Parse(m_extraData["free spin symbol draw"][aa][1].ToString()));
            }
            m_fgReelStripCutoffs.Add(m.MakeCutoffs(fsWeights1));
            m_fgReelStripCutoffs.Add(m.MakeCutoffs(fsWeights2));
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild;
        int m_currentFSReelIndex = -1;
        List<List<int>> m_fgReelStripCutoffs;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            base.PreSpin(true);
            m_currentFSReelIndex = -1;
            m_currentBankedGame = 1;
            ActionPauseInMilliSeconds = 0;
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
                if(m_progressiveType != "shamrock fortunes") incrementProgressives();
                else if (m_progressiveType == "shamrock fortunes")
                {
                    m_shamrockBoosts = 0;
                    for(int aa = 0; aa < m_progressives.Count(); aa++)
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
                    if (m_shamrockBoosts > 0) return GameAction.Wait;
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    //edit the below loop to calculate the progressive award
                    if (m_progressiveType == "shamrock fortunes")
                    {
                        List<WinArgs> progWins = new List<WinArgs>();
                        progWins = getShamrockFortunesWin();
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
                    getFSReelStrips();
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
                    }
                    break;
            }
            //If Have Free Games, Play
            if (m_freeGamesLeft > 0)
            {
                int freeGamesWonThisSpin = InFreeGames ? m_freeGamesLeft - oldFreeGames + 1 : m_freeGamesLeft;
                InFreeGames = true;
                if (m_freeGamesPlayed == 0) m_currentFSReelIndex = getFSReelIndex();
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
            if(!InSimulation && m_bankSize > 1 && m_currentBankedGame < m_bankSize)
            {
                m_gameState = DefaultGameStates.BankGameState;
                if (m_shamrockBoosts > 0) ActionPauseInMilliSeconds = 2000;
                return GameAction.Wait;
            }
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private void getFSReelStrips()
        {
            currentReelSet = Reels.ReelsAlt[m_currentFSReelIndex];
            currentCutoffSet = Reels.ReelsAltCutoffs[m_currentFSReelIndex];
            ReelIndexes = Reels.GetRandomIndexes(currentReelSet, currentCutoffSet);
        }

        private int getFSReelIndex()
        {
            int randomDraw = 0;
            randomDraw = m.RandomIndex(m_fgReelStripCutoffs[0]);
            int randomDraw2 = 0;
            randomDraw2 = m.RandomIndex(m_fgReelStripCutoffs[1]);
            return ((4 * randomDraw) + randomDraw2);
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
