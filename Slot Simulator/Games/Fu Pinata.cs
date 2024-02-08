using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Fu_Pinata : GameInfo
    {
        internal Fu_Pinata(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wi");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            for (int row = 0; row < m_extraData["free spins by choice"].Count(); row++)
            {
                m_freeSpinsByChoice.Add(int.Parse(m_extraData["free spins by choice"][row][0].ToString()));
                m_freeSpinsByChoiceMultipliers.Add(int.Parse(m_extraData["free spins by choice"][row][1].ToString()));
            }
            for (int row = 0; row < m_extraData["free spins to simulate"].Count(); row++)
            {
                m_currentFreeGameChoice = int.Parse(m_extraData["free spins to simulate"][row][0].ToString());
                //m_fixedFreeGameChoice = int.Parse(m_extraData["free spins to simulate"][row][0].ToString());
            }
            frmChoose = new frmChooseVolatility();
            frmChoose.m_choiceStrings = new List<string>();
            frmChoose.m_choiceStrings.Add("12 SPINS \r\n with \r\n WILDS \r\n up to \r\n 3X");
            frmChoose.m_choiceStrings.Add("9 SPINS \r\n with \r\n WILDS \r\n up to \r\n 4X");
            frmChoose.m_choiceStrings.Add("6 SPINS \r\n with \r\n WILDS \r\n up to \r\n 5X");
            frmChoose.m_choiceStrings.Add("RANDOM");
            frmChoose.createChoices();
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild;
        List<int> m_freeSpinsByChoice = new List<int>();
        List<int> m_freeSpinsByChoiceMultipliers = new List<int>();
        int m_currentFreeGameChoice = new int();
        int m_currentFreeGameMultiplier = new int();
        int wildsAppear = new int();
        int keepTrack = 0;
        //int m_fixedFreeGameChoice = new int(); //used for simulations of random free spins
        //int m_currentFreeGameChoice = 2;
        List<List<int>> m_screenSymbolsForDraw, screenSymbols;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            base.PreSpin(false);
            m_screenSymbolsForDraw = new List<List<int>>();
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
            screenSymbols = new List<List<int>>(); 
            switch (m_gameState)
            {
                case DefaultGameStates.PGPostSpin:
                    
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    //edit the below loop to calculate the progressive award
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
                            if (m_currentFreeGameChoice != 3)
                            {
                                m_currentFreeGameMultiplier = m_freeSpinsByChoiceMultipliers[m_currentFreeGameChoice];
                            }
                            if (m_currentFreeGameChoice == 3)
                            {
                                m_currentFreeGameChoice = m.RandomInteger(3, true); //dumb code
                                m_currentFreeGameMultiplier = m.RandomInteger(3, true) + 3; //more dumb code
                            }
                            m_freeGamesLeft = m_freeSpinsByChoice[m_currentFreeGameChoice];
                        }
                    }
                    //if (m_progressiveType != "none")
                    //{
                    //    List<WinArgs> progWins = new List<WinArgs>();
                    //    if (m_progressiveType == "must hit") progWins = getMustHitProgressiveWin();
                    //    else if (m_progressiveType == "shamrock fortunes")
                    //    {
                    //        progWins = getShamrockFortunesWin();
                    //    }
                    //    if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                    //}
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
                    if (!InSimulation)
                    {
                        m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                        //switch (CheatKey)
                        //{
                        //    case System.Windows.Forms.Keys.B://Cheat Key "~"
                        //        //int freeGames = 0;
                        //        int WinsThisGame1 = 0;
                        //        while (WinsThisGame1 < Bet * 100)
                        //        {
                        //            List<WinArgs> dummyWins1 = new List<WinArgs>();
                        //            base.PreSpin(false);
                        //            CalculateWins(dummyWins1, screenSymbols, "normal", ref m_freeGamesLeft);
                        //            WinsThisGame1 = CountTheseWins(Wins);
                        //        }
                        //        break;
                        //}
                    }
                    //if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    //Set Up Free Game
                    //GetFGReelStrips(false);
                    GetAltReelStrips(m_currentFreeGameChoice, false);
                    m_screenSymbolsForDraw = new List<List<int>>();
                    screenSymbols = new List<List<int>>();
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    //screenSymbols = ChangeScreenSymbols(currentReelSet); //Created to change wilds to wild multipliers
                    m_screenSymbolsForDraw = screenSymbols;
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
                    wildsAppear = CountSymbolsOnScreen(screenSymbols, m_symbolWild);
                    screenSymbols = ChangeScreenSymbols(currentReelSet); //Created to change wilds to wild multipliers
                    m_screenSymbolsForDraw = screenSymbols;
                    //GetFGReelStrips(false);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    foreach (WinArgs specificWin in WinsToShow)
                    {
                        if (specificWin.SymbolIndex == m_symbolScatter)
                        {
                            if (specificWin.Count >= 3)
                            {
                                if (m_currentFreeGameChoice != 3)
                                {
                                    m_currentFreeGameMultiplier = m_freeSpinsByChoiceMultipliers[m_currentFreeGameChoice];
                                }
                                if (m_currentFreeGameChoice == 3)
                                {
                                    m_currentFreeGameChoice = m.RandomInteger(3, true); //dumb code
                                    m_currentFreeGameMultiplier = m.RandomInteger(3, true) + 3; //more dumb code
                                }
                                m_freeGamesLeft += (m_freeSpinsByChoice[m_currentFreeGameChoice] - 1); //
                            }
                        }
                    }
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                        //if(winsThisFG > 1000 * 50 * 25)
                        //{
                        //    int index = 0;
                        //    index++;
                        //}
                    }
                    if (DoDefaultWinDistributions)
                    if (DoCustomStats)
                    {
                        if (InFreeGames && CountTheseWins(WinsToShow) > 0 && wildsAppear > 0) wildCount++;
                        if (InFreeGames && CountTheseWins(WinsToShow) > 0 && wildsAppear == 1) wildCountOne++;
                        if (InFreeGames && CountTheseWins(WinsToShow) > 0 && wildsAppear == 2) wildCountTwo++;
                        if (InFreeGames && wildsAppear > 0) totalWildCount++;
                        if (InFreeGames && wildsAppear == 1) totalWildCountOne++;
                        if (InFreeGames && wildsAppear == 2) totalWildCountTwo++;
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
                if (!InSimulation && m_freeGamesPlayed == 0)
                {
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    //screenSymbols = ChangeScreenSymbols(currentReelSet); //Created to change wilds to wild multipliers
                    m_screenSymbolsForDraw = screenSymbols;
                    var dialogResult = frmChoose.ShowDialog();
                    m_currentFreeGameChoice = frmChoose.m_currentChoice;
                    if (m_currentFreeGameChoice != 3)
                    {
                        m_currentFreeGameMultiplier = m_freeSpinsByChoiceMultipliers[m_currentFreeGameChoice];
                    }
                    if (m_currentFreeGameChoice == 3)
                    {
                        m_currentFreeGameChoice = m.RandomInteger(3, true); //dumb code
                        m_currentFreeGameMultiplier = m.RandomInteger(3, true) + 3; //more dumb code
                    }
                    //m_currentFreeGameMultiplier = m_freeSpinsByChoiceMultipliers[m_currentFreeGameChoice];
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
        private List<List<int>> ChangeScreenSymbols(List<List<int>> ReelSet)
        {
            Random multiplierFinder = new Random();
            List<List<int>> reels = ReelSet;
            List<List<int>> screenSymbols = new List<List<int>>();
            List<List<int>> currentInsertionSet = InFreeGames ? Reels.SSSymbolsFG : Reels.SSSymbolsBG;
            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
            {
                List<int> reel = reels[reelNum];
                List<int> reelSymbols = new List<int>();
                int wildMultiplierIndex = 0;
                for (int i = 0; i < Dimensions[reelNum]; i++)
                {
                    int index = (ReelIndexes[reelNum] + i + reel.Count) % reel.Count;
                    int symbolIndex = reel[index];
                    if (symbolIndex == Reels.Symbols.IndexOf("wi"))
                    {
                        wildMultiplierIndex = multiplierFinder.Next(1,m_currentFreeGameMultiplier + 1);
                        keepTrack += wildMultiplierIndex;
                        switch (wildMultiplierIndex)
                        {
                            case 1:
                                symbolIndex = Reels.Symbols.IndexOf("w1");
                                break;
                            case 2:
                                symbolIndex = Reels.Symbols.IndexOf("w2");
                                break;
                            case 3:
                                symbolIndex = Reels.Symbols.IndexOf("w3");
                                break;
                            case 4:
                                symbolIndex = Reels.Symbols.IndexOf("w4");
                                break;
                            case 5:
                                symbolIndex = Reels.Symbols.IndexOf("w5");
                                break;
                            case 6:
                                symbolIndex = Reels.Symbols.IndexOf("w6");
                                break;
                            case 7:
                                symbolIndex = Reels.Symbols.IndexOf("w7");
                                break;
                            case 8:
                                symbolIndex = Reels.Symbols.IndexOf("w8");
                                break;
                            case 9:
                                symbolIndex = Reels.Symbols.IndexOf("w9");
                                break;
                            case 10:
                                symbolIndex = Reels.Symbols.IndexOf("w10");
                                break;
                            default:
                                break;
                        }
                    }
                    reelSymbols.Add(symbolIndex == m_insertionSymbol ? currentInsertionSet[m_insertionIndex][reelNum] : symbolIndex);
                }
                screenSymbols.Add(reelSymbols);
            }
            return screenSymbols;
        }
        internal override void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            if (InFreeGames)
            {
                for (int reelNum = 0; reelNum < 5; reelNum++)
                {
                    for (int cellNum = 0; cellNum < 3; cellNum++)
                    {
                        //This one draws symbols after a spin ends in the base or free games
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w1") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w1", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w2") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w2", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w3") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w3", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w4") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w4", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w5") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w5", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w6") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w6", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w7") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w7", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w8") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w8", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w9") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w9", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("w10") && (m_gameState == DefaultGameStates.FGPreSpin || m_gameState == DefaultGameStates.FGPostSpin))
                        {
                            _reelsPanel.SpriteDraw("w10", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                            //_reelsPanel.SpriteDraw(Reels.Symbols[m_screenSymbolsForDraw[reelNum][cellNum]], new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                            //_reelsPanel.SpriteDraw("l1", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                    }
                }
            }
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        int wildCount = 0;
        int wildCountOne = 0;
        int wildCountTwo = 0;
        int totalWildCount = 0;
        int totalWildCountOne = 0;
        int totalWildCountTwo = 0;
        //internal override void SetUpCustomStats()
        //{
        //    int wildCount = 0;
        //}
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total Multiplier Counts" }));
            _results.Add(new List<string>(new string[] { "Number of Wilds", "Hits", "Appears" }));
            _results.Add(new List<string>(new string[] { "Any", wildCount.ToString(), totalWildCount.ToString() }));
            _results.Add(new List<string>(new string[] { "One Wild", wildCountOne.ToString(), totalWildCountOne.ToString() }));
            _results.Add(new List<string>(new string[] { "Two Wilds", wildCountTwo.ToString(), totalWildCountTwo.ToString() }));
            _results.Add(new List<string>(new string[] { keepTrack.ToString() }));
            _results.Add(new List<string>());
        }
    }
}
