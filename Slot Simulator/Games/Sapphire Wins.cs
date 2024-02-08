using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Slot_Simulator
{
    class Sapphire_Wins : GameInfo
    {
        internal Sapphire_Wins(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            for(int row = 0; row < m_extraData["free spins by choice"].Count(); row++)
            {
                m_freeSpinsByChoice.Add(int.Parse(m_extraData["free spins by choice"][row][0].ToString()));
            }
            List<List<int>> m_freeSpinWeights = new List<List<int>>();
            for (int aa = 0; aa < 3; aa++)
            {
                m_freeSpinMultipliers.Add(new List<int>());
                m_freeSpinWeights.Add(new List<int>());
            }
            for (int row = 1; row < m_extraData["choice multipliers and weights"].Count(); row++)
            {
                m_freeSpinMultipliers[0].Add(int.Parse(m_extraData["choice multipliers and weights"][row][0]));
                m_freeSpinWeights[0].Add(int.Parse(m_extraData["choice multipliers and weights"][row][1]));
                m_freeSpinMultipliers[1].Add(int.Parse(m_extraData["choice multipliers and weights"][row][2]));
                m_freeSpinWeights[1].Add(int.Parse(m_extraData["choice multipliers and weights"][row][3]));
                m_freeSpinMultipliers[2].Add(int.Parse(m_extraData["choice multipliers and weights"][row][4]));
                m_freeSpinWeights[2].Add(int.Parse(m_extraData["choice multipliers and weights"][row][5]));
            }
            for(int aa = 0; aa < m_freeSpinWeights.Count(); aa++)
            {
                m_freeSpinMultiplierCutoffs.Add(m.MakeCutoffs(m_freeSpinWeights[aa]));
            }
            frmChoose = new frmChooseVolatility();
            frmChoose.m_choiceStrings = new List<string>();
            frmChoose.m_choiceStrings.Add("20 spins \r\n with \r\n All Wins \r\n 2x - 10x");
            frmChoose.m_choiceStrings.Add("15 spins \r\n with \r\n All Wins \r\n 4x - 50x");
            frmChoose.m_choiceStrings.Add("8 spins \r\n with \r\n All Wins \r\n 6x - 100x");
            frmChoose.createChoices();
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild;
        List<int> m_freeSpinsByChoice = new List<int>();
        List<List<int>> m_freeSpinMultiplierCutoffs = new List<List<int>>();
        List<List<int>> m_freeSpinMultipliers = new List<List<int>>();
        int m_currentFreeGameChoice = 0;
        int m_currentMultiplier = 1;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_currentFreeGameChoice = getFreeGameChoice();
            m_currentMultiplier = 1;
            m_dajidaliCheat = false;
            base.PreSpin(true);
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    case System.Windows.Forms.Keys.Oemtilde://Cheat Key "~"
                        int scatterCount = 0;
                        while (scatterCount != 3)
                        {
                            base.PreSpin(true);
                            scatterCount = CountSymbolsOnScreen(GetScreenSymbols(currentReelSet), m_symbolScatter);
                        }
                        break;
                    case System.Windows.Forms.Keys.Q://Cheat Key "Q"
                        int wilds = 0;
                        while(wilds == 0)
                        {
                            base.PreSpin(true);
                            wilds = CountSymbolsOnScreen(GetScreenSymbols(currentReelSet), m_symbolWild);
                        }
                        m_dajidaliCheat = true;
                        break;
                    case System.Windows.Forms.Keys.W://Cheat Key "Q"
                        int scatterCount1 = 0;
                        m_currentFreeGameChoice = 0;
                        while (scatterCount1 != 3)
                        {
                            base.PreSpin(true);
                            scatterCount1 = CountSymbolsOnScreen(GetScreenSymbols(currentReelSet), m_symbolScatter);
                        }
                        break;
                    case System.Windows.Forms.Keys.E://Cheat Key "Q"
                        int scatterCount2 = 0;
                        m_currentFreeGameChoice = 1;
                        while (scatterCount2 != 3)
                        {
                            base.PreSpin(true);
                            scatterCount2 = CountSymbolsOnScreen(GetScreenSymbols(currentReelSet), m_symbolScatter);
                        }
                        break;
                    case System.Windows.Forms.Keys.R://Cheat Key "Q"
                        int scatterCount3 = 0;
                        m_currentFreeGameChoice = 2;
                        while (scatterCount3 != 3)
                        {
                            base.PreSpin(true);
                            scatterCount3 = CountSymbolsOnScreen(GetScreenSymbols(currentReelSet), m_symbolScatter);
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
                    int dummyFreeGames = 0;
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref dummyFreeGames);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref dummyFreeGames);
                    if (CountSymbolsOnScreen(screenSymbols, m_symbolWild) > 0)
                    {
                        List<WinArgs> progWins = getDaJiDaLiWin();
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        m_progressiveWinPG = CountTheseWins(progWins);
                    }
                    foreach (WinArgs specificWin in WinsToShow)
                    {
                        if (specificWin.SymbolIndex == m_symbolScatter)
                        {
                            if (specificWin.Count >= 3)
                            {
                                m_freeGamesLeft = m_freeSpinsByChoice[m_currentFreeGameChoice];
                            }
                        }
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
                    m_currentMultiplier = getFreeGameMultiplier();
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES: All Wins {2}x ", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed, m_currentMultiplier);
                    m_progressiveWinFG = 0;
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    int dummyFreeSpins = 0;
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref dummyFreeSpins, m_currentMultiplier);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref dummyFreeSpins, m_currentMultiplier);
                    foreach (WinArgs specificWin in WinsToShow)
                    {
                        if (specificWin.SymbolIndex == m_symbolScatter)
                        {
                            if (specificWin.Count >= 3)
                            {
                                m_freeGamesLeft += m_freeSpinsByChoice[m_currentFreeGameChoice];
                            }
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
                InFreeGames = true;
                if(!InSimulation && m_freeGamesPlayed == 0)
                {
                    var dialogResult = frmChoose.ShowDialog();
                    m_currentFreeGameChoice = frmChoose.m_currentChoice;
                    m_freeGamesLeft = m_freeSpinsByChoice[m_currentFreeGameChoice];
                }
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
        private int getFreeGameChoice()
        {
            return m.RandomInteger(3);
        }
        private int getFreeGameMultiplier()
        {
            int indexChosen = m.RandomIndex(m_freeSpinMultiplierCutoffs[m_currentFreeGameChoice]);
            return m_freeSpinMultipliers[m_currentFreeGameChoice][indexChosen];
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
