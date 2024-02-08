﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Phoenix_Wins : GameInfo
    {
        internal Phoenix_Wins(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wi");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_symbolStack = Reels.Symbols.IndexOf("ss");
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_symbolStack;
        protected int[] cReSpinSpinOrder = { -1, 0, 1, 2, 3 };
        int reSpins = 0;
        bool reSpinNotOccured = false;
        bool m_progressiveCheat = false;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_progressiveCheat = false;
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
                    case System.Windows.Forms.Keys.Q://Cheat Key "Q"
                        int wilds = 0;
                        while(wilds == 0)
                        {
                            base.PreSpin(false);
                            wilds = CountSymbolsOnScreen(GetScreenSymbols(currentReelSet), m_symbolWild);
                        }
                        m_dajidaliCheat = true;
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
                    if ((CountSymbolsOnScreen(screenSymbols, m_symbolWild) > 0  && m.RandomDouble(1) < m_progressiveChance * BetLevel) || m_progressiveCheat)
                    {
                        List<WinArgs> progWins = getDaJiDaLiWin();
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        m_progressiveWinPG = CountTheseWins(progWins);
                    }
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
                    GetFGReelStrips(true);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                    }
                    m_progressiveWinFG = 0;
                    reSpins = 0;
                    reSpinNotOccured = false;
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    if (screenSymbols[0][0] == screenSymbols[0][1] && screenSymbols[0][0] == screenSymbols[0][2] && screenSymbols[0][1] == screenSymbols[0][2])
                    {
                        cReSpinSpinOrder = getNewSpinOrder(screenSymbols);
                        m_gameState = DefaultGameStates.ReSpinPreSpin;
                        reSpinNotOccured = true;
                        break;
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
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("Re-Spin Feature");
                    SpinOrder = cReSpinSpinOrder;
                    getReSpinReelIndexes();
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
            if (reSpinNotOccured && reSpins == 0)
            {
                m_gameState = DefaultGameStates.ReSpinPreSpin;
                reSpins++;
                return GameAction.Wait;
            }
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
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private List<WinArgs> getProgressiveWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            int prizeInteger = m.RandomIndex(m_progressiveCutoffs);
            winsToReturn.Add(new WinArgs(m_progressives[prizeInteger].Name, m_progressives[prizeInteger].GetProgressiveAndReset()));
            if(!InSimulation) frmProg.ProgressiveValueTBs[prizeInteger].Text = string.Format("{0:$0,0.00}", (double)m_progressives[prizeInteger].CurrentValue / 100);
            return winsToReturn;
        }

        private int[] getNewSpinOrder(List<List<int>> _screenSymbols)
        {
            int[] spinOrderToReturn = { -1, 0, 1, 2, 3 };
            int currentSpinNumber = 0;
            for (int aa = 0; aa < ReelCount; aa++)
            {
                if(aa != 0)
                {
                    if(_screenSymbols[aa].Contains(_screenSymbols[0][0]) || _screenSymbols[aa].Contains(m_symbolWild)) spinOrderToReturn[aa] = -1;
                    else if (!_screenSymbols[aa].Contains(_screenSymbols[0][0]) && !_screenSymbols[aa].Contains(m_symbolWild)) spinOrderToReturn[aa] = currentSpinNumber++;
                }
            }
            return spinOrderToReturn;
        }

        private void getReSpinReelIndexes()
        {
            for(int ab = 0; ab < ReelCount; ab++)
            {
                if(cReSpinSpinOrder[ab] != -1)
                {
                    ReelIndexes[ab] = m.RandomIndex(currentCutoffSet[ab]);
                }
            }
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
