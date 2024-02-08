using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Fu_Babies : GameInfo
    {
        internal Fu_Babies(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wi");//            m_symbolWild = Reels.Symbols.IndexOf("wi");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_symbolStack = Reels.Symbols.IndexOf("ss");
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_symbolStack;
        protected int[] cReSpinSpinOrder = { -1, 0, 1, 2, 3 };
        int reSpins = 0;
        bool reSpinNotOccured = false;
        bool featureOn = false;
        int[,] symbolHold = new int[5, 3];
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
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
                    List<WinArgs> progWins = new List<WinArgs>();
                    //progWins = getMustHitProgressiveWin();
                    //if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
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
                    featureOn = false;
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    if (screenSymbols[0][0] == screenSymbols[0][1] && screenSymbols[0][0] == screenSymbols[0][2])
                    {
                        //determine REsp
                        reSpins = 4;
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
                    for (int i = 1; i < 5; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (symbolHold[i, j] != 0)
                            {
                                screenSymbols[i][j] = symbolHold[i, j] - 1;
                            }
                        }
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
                    cReSpinSpinOrder = getNewSpinOrder(screenSymbols);
                    break;
            }
            //If Have Free Games, Play
            if (reSpins>0)//if there arre respins left nate
            {
                m_gameState = DefaultGameStates.ReSpinPreSpin;
                reSpins--;
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
        internal override void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            //List<List<int>> _FSscreenSymbols = GetScreenSymbols(currentReelSet);
            if (InFreeGames)
            {
                if (featureOn)
                {
                    for (int i = 1; i < 5; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (symbolHold[i, j] != 0)
                            {
                                _reelsPanel.SpriteDraw(Reels.Symbols[symbolHold[i, j] - 1], new Point(_reelsPanel.ReelOffsets[i].X, _reelsPanel.ReelOffsets[i].Y + j * SymbolHeight));//Reels.Symbols[symbolHold[i,j]-1]
                            }
                        }
                    }
                }
            }
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private int[] getNewSpinOrder(List<List<int>> _screenSymbols)
        {
                int[] spinOrderToReturn = { -1, 0, 1, 2, 3 };
                featureOn = true;
                //for (int i = 0; i < 5; i++)
                //{
                //    for (int j = 0; j < 3; j++)
                //    {
                //        symbolHold[i, j] = 0;
                //    }
                //}

                for (int aa = 0; aa < ReelCount; aa++)
                {
                    if (aa != 0)
                    {
                        if (_screenSymbols[aa][0] == _screenSymbols[0][0] || _screenSymbols[aa][0] == m_symbolWild)
                        {
                            symbolHold[aa, 0] = _screenSymbols[aa][0] + 1;
                        }
                        if (_screenSymbols[aa][1] == _screenSymbols[0][0] || _screenSymbols[aa][1] == m_symbolWild)
                        {
                            symbolHold[aa, 1] = _screenSymbols[aa][1] + 1;
                        }
                        if (_screenSymbols[aa][2] == _screenSymbols[0][0] || _screenSymbols[aa][2] == m_symbolWild)
                        {
                            symbolHold[aa, 2] = _screenSymbols[aa][2] + 1;
                        }
                    }
                }
            
            return spinOrderToReturn;
        }

        private void getReSpinReelIndexes()
        {
            for (int ab = 0; ab < ReelCount; ab++)
            {
                if (cReSpinSpinOrder[ab] != -1)
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