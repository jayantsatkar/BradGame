using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Dynamite_Dinos : GameInfo
    {
        internal Dynamite_Dinos(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wi");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            /*List<int> progressiveWeights = new List<int>();
            for (int row = 0; row < m_extraData["progressive weights"].Count(); row++)
            {
                progressiveWeights.Add(int.Parse(m_extraData["progressive weights"][row][0].ToString()));
            }*/
            //m_progressiveCutoffs = m.MakeCutoffs(progressiveWeights);
            //m_progressiveChance = double.Parse(m_extraData["progressive chance"][0][0].ToString());
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild;
        //List<int> m_progressiveCutoffs = new List<int>();
        //double m_progressiveChance = 0;
        //bool m_progressiveCheat = false;
        List<List<int>> m_screenSymbolsForDraw;
        int patternPickProg = 1;
        int patternPickSecond = 1;
        int start = 0;
        List<int> sticky0 = new List<int> { 0, 0, 0, 0, 0 };
        List<int> sticky1 = new List<int> { 0, 0, 0, 0, 0 };
        //List<int> previousSticky = new List<int>{0,0,0,0,0};
        int notPaid = 1;
        Random hit = new Random();
        ///////////////////////
        List<List<int>> screenSymbols = new List<List<int>>();  
        ///////////////////
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_dajidaliCheat = false;
            base.PreSpin(false);
            if (m_freeGamesLeft == 0)
            {
                screenSymbols = GetScreenSymbols(currentReelSet);
                for (int reelNum = 0; reelNum < 5; reelNum++)
                {
                    for (int cellNum = 0; cellNum < 5; cellNum++)
                    {
                        screenSymbols[reelNum][cellNum] = Reels.Symbols.IndexOf("bl");
                    }
                }
                m_screenSymbolsForDraw = screenSymbols;
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
                            screenSymbols = GetScreenSymbols(currentReelSet);
                            for (int reelNum = 0; reelNum < 5; reelNum++)
                            {
                                for (int cellNum = 0; cellNum < 2; cellNum++)
                                {
                                    screenSymbols[reelNum][cellNum] = Reels.Symbols.IndexOf("bl");
                                }

                            }
                            CalculateWins(dummyWins, screenSymbols, "scatter", ref freeGames);
                        }
                        break;
                    case System.Windows.Forms.Keys.Q://Cheat Key "Q"
                        int wilds = 0;
                        while (wilds == 0)
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
            //screenSymbols = GetScreenSymbols(currentReelSet);
            //screenSymbols = ChangeScreenSymbols(currentReelSet, patternPick, sticky0, sticky1);
            //sticky0 = CreateNewSticky0(screenSymbols);
            //sticky1 = CreateNewSticky1(screenSymbols);
            //previousSticky = sticky;
            //m_screenSymbolsForDraw = screenSymbols;
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
                    start = 1;
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    patternPickProg = GetPattern(0); //Prog line
                    patternPickSecond = GetPattern(1); //Second line
                    picks[patternPickSecond - 1] = picks[patternPickSecond - 1] + 1;
                    screenSymbols = ChangeScreenSymbols(currentReelSet, patternPickProg, patternPickSecond, sticky0, sticky1);
                    m_screenSymbolsForDraw = screenSymbols;
                    sticky0 = CreateNewSticky0(screenSymbols);
                    sticky1 = CreateNewSticky1(screenSymbols);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    //edit the below loop to calculate the progressive award
                    //if (screenSymbols[0][0] > 13)
                    //{
                    //     List<WinArgs> progWins = safariWilds(screenSymbols[0][0]);
                    //     if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                    //     m_progressiveWinPG = CountTheseWins(progWins);
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
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
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
                    screenSymbols = ChangeScreenSymbols(currentReelSet, patternPickProg, patternPickSecond, sticky0, sticky1);
                    sticky0 = CreateNewSticky0(screenSymbols);
                    sticky1 = CreateNewSticky1(screenSymbols);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    //if (screenSymbols[0][0] > 13 && notPaid == 1)
                    //{
                    //    List<WinArgs> progWins = safariWilds(screenSymbols[0][0]);
                    //    if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                    //    m_progressiveWinPG = CountTheseWins(progWins);
                    //    notPaid = 0;
                    //}
                    Wins.AddRange(WinsToShow);
                    m_screenSymbolsForDraw = screenSymbols;
                    symbolAvg[m_freeGamesPlayed + 1] = symbolAvg[m_freeGamesPlayed] + (10 - CountSymbolsOnScreen(screenSymbols, Reels.Symbols.IndexOf("bl")));
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
                if (m_freeGamesPlayed == 0)
                {
                    notPaid = 1;
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
            if (!InSimulation) frmProg.ProgressiveValueTBs[prizeInteger].Text = string.Format("{0:$0,0.00}", (double)m_progressives[prizeInteger].CurrentValue / 100);
            return winsToReturn;
        }
        private List<List<int>> ChangeScreenSymbols(List<List<int>> ReelSet, int patternTop, int patternSecond, List <int> hold0, List <int> hold1)
        {
            //Random multiplierFinder = new Random();
            List<List<int>> reels = ReelSet;
            List<string> insertsTop = new List<string> { "p1", "p2", "p3", "p4" };
            List<string> insertsSecond = new List<string> { "h1", "h2", "h3", "h4", "h5" };
            List<List<int>> screenSymbols = new List<List<int>>();
            List<List<int>> currentInsertionSet = InFreeGames ? Reels.SSSymbolsFG : Reels.SSSymbolsBG;
            int count = new int();
            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
            {
                //Counts number of dynamite symbols on the screen
                List<int> reel1 = reels[reelNum];
                count = 0;
                for (int i = 2; i < 5; i++)
                {
                    int index = (ReelIndexes[reelNum] + i + reel1.Count) % reel1.Count;
                    int symbolIndex = reel1[index];
                    
                    if (symbolIndex == Reels.Symbols.IndexOf("dy"))
                    {
                        count++;
                    }
                }
                //Prevents over drawing when free spins occurs
                //if (m_gameState == DefaultGameStates.FGPreSpin)
                //{
                //    count = 0;
                //}
                List<int> reel = reels[reelNum];
                List<int> reelSymbols = new List<int>();
                for (int i = 0; i < 5; i++)
                {
                    int index = (ReelIndexes[reelNum] + i + reel.Count) % reel.Count;
                    int symbolIndex = reel[index];
                    if (InFreeGames && i < 2)
                    {
                        //Top line code for free spins
                        if (hold1[reelNum] == 0 && i == 0 || (hold1[reelNum] == 1 && hold0[reelNum] == 0 && count == 0 && i == 0)) //Top line with second line still covered
                        {
                            symbolIndex = Reels.Symbols.IndexOf("bl");
                        }
                        else if (hold1[reelNum] == 1 && i == 0 && count > 0) //Top line with second line uncovered
                        {
                            if (reelNum == 0) { symbolIndex = Reels.Symbols.IndexOf(insertsTop[patternTop - 1]); }
                            if (reelNum > 0) { symbolIndex = Reels.Symbols.IndexOf("wi"); }
                        }
                        else if (hold0[reelNum] == 1 && i == 0) //Top line already uncovered
                        {
                            if (reelNum == 0) { symbolIndex = Reels.Symbols.IndexOf(insertsTop[patternTop - 1]); }
                            if (reelNum > 0) { symbolIndex = Reels.Symbols.IndexOf("wi"); }
                        }
                        //Second line code for free spins
                        if (hold0[reelNum] == 1 && i == 1)
                        {
                            if (reelNum == 0) { symbolIndex = Reels.Symbols.IndexOf(insertsSecond[patternTop - 1]); }
                            if (reelNum > 0) { symbolIndex = Reels.Symbols.IndexOf("wi"); }
                        }
                        if ((hold1[reelNum] == 0 && i == 1 && count > 0) || (hold1[reelNum] == 1 && i == 1)) //Second line with variations
                        {
                            if (reelNum == 0) { symbolIndex = Reels.Symbols.IndexOf(insertsSecond[patternSecond - 1]); }
                            if (reelNum > 0) { symbolIndex = Reels.Symbols.IndexOf("wi"); }
                        }
                        else if (hold1[reelNum] == 0 && count == 0) //Second line covered
                        {
                            symbolIndex = Reels.Symbols.IndexOf("bl");
                        }
                    }
                    if (!InFreeGames && i < 2) 
                    {
                        if (i == 0 || count == 0) //Top line always covered in base game
                        {
                            symbolIndex = Reels.Symbols.IndexOf("bl");
                        }
                        if (i == 1 && count > 0) //Second line code for base game
                        {
                            if (reelNum == 0) { symbolIndex = Reels.Symbols.IndexOf(insertsSecond[patternSecond - 1]); }
                            if (reelNum > 0) { symbolIndex = Reels.Symbols.IndexOf("wi"); }
                        }
                    }
                    reelSymbols.Add(symbolIndex == m_insertionSymbol ? currentInsertionSet[m_insertionIndex][reelNum] : symbolIndex);
                }
                screenSymbols.Add(reelSymbols);
            }
            return screenSymbols;
        }
        private List<int> CreateNewSticky0(List<List<int>> visuals)
        {
            List<int> hold = new List<int> { 0, 0, 0, 0, 0 };
            for (int reelNum = 0; reelNum < 5; reelNum++)
            {
                int i = 0;
                if(visuals[reelNum][i] != Reels.Symbols.IndexOf("bl"))
                {
                    hold[reelNum] = 1;
                }
                if(visuals[reelNum][i] == Reels.Symbols.IndexOf("bl"))
                {
                    hold[reelNum] = 0;
                }

            }
            return hold;
        }
        private List<int> CreateNewSticky1(List<List<int>> visuals)
        {
            List<int> hold = new List<int> { 0, 0, 0, 0, 0 };
            for (int reelNum = 0; reelNum < 5; reelNum++)
            {
                int i = 1;
                if (visuals[reelNum][i] != Reels.Symbols.IndexOf("bl"))
                {
                    hold[reelNum] = 1;
                }
                if (visuals[reelNum][i] == Reels.Symbols.IndexOf("bl"))
                {
                    hold[reelNum] = 0;
                }

            }
            return hold;
        }

        internal override void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            //if ((m_gameState == DefaultGameStates.PGPostSpin || m_gameState == DefaultGameStates.FGPostSpin || m_gameState == DefaultGameStates.FGPreSpin) && start == 1)
            //{
            if(start == 1)
            {
                for (int reelNum = 0; reelNum < 5; reelNum++)
                {
                    for (int cellNum = 0; cellNum < 2; cellNum++)
                    {
                        //This one draws symbols after a spin ends in the base or free games
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("p1"))
                        {
                            _reelsPanel.SpriteDraw("p1", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("p2"))
                        {
                            _reelsPanel.SpriteDraw("p2", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("p3"))
                        {
                            _reelsPanel.SpriteDraw("p3", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("p4"))
                        {
                            _reelsPanel.SpriteDraw("p4", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("h1"))
                        {
                            _reelsPanel.SpriteDraw("h1", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("h2"))
                        {
                            _reelsPanel.SpriteDraw("h2", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("h3"))
                        {
                            _reelsPanel.SpriteDraw("h3", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("h4"))
                        {
                            _reelsPanel.SpriteDraw("h4", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("h5"))
                        {
                            _reelsPanel.SpriteDraw("h5", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("wi"))
                        {
                            _reelsPanel.SpriteDraw("wi", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("bl"))
                        {
                            _reelsPanel.SpriteDraw("bl", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("l1"))
                        {
                            _reelsPanel.SpriteDraw("l1", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("l2"))
                        {
                            _reelsPanel.SpriteDraw("l2", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("l3"))
                        {
                            _reelsPanel.SpriteDraw("l3", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("l4"))
                        {
                            _reelsPanel.SpriteDraw("l4", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("l5"))
                        {
                            _reelsPanel.SpriteDraw("l5", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("l6"))
                        {
                            _reelsPanel.SpriteDraw("l6", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == Reels.Symbols.IndexOf("dy"))
                        {
                            _reelsPanel.SpriteDraw("dy", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                    }
                }
            }
        }
        internal override void CustomDrawBeforeDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            //if ((m_gameState == DefaultGameStates.PGPostSpin || m_gameState == DefaultGameStates.FGPostSpin || m_gameState == DefaultGameStates.FGPreSpin) && start == 1)
            if( start == 1)
            {
                for (int reelNum = 0; reelNum < 5; reelNum++)
                {
                    for (int cellNum = 0; cellNum < 2; cellNum++)
                    {
                        //This one draws symbols before a spin begins in the base or free games
                        //if (m_screenSymbolsForDraw[reelNum][cellNum] > -1)
                        //{
                            _reelsPanel.SpriteDraw("bl", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        //}
                    }
                }
            }
        }
        private int GetPattern(int row)
        {
            int pattern = 4;
            //Random pick = new Random();
            //List<int> weightsProg = new List<int> { 1, 84, 1737, 10000 };
            //List<int> weightsRow = new List<int> { 1, 3, 6, 10, 15 };
            //if (row == 0) //Top row
            //{
            //    int pick2 = hit.Next(0, weightsProg[3]);
            //    for (int j = 0; j < 4; j++)
            //    {
            //        if (pick2 < weightsProg[j])
            //        {
            //            pattern = j + 1;
            //            return pattern;
            //        }
            //    }
            //}
            //if (row == 1) //Second row
            //{
            //    int pick2 = hit.Next(0, weightsRow[4]);
            //    for (int j = 0; j < 5; j++)
            //    {
            //        if (pick2 < weightsRow[j])
            //        {
            //            pattern = j + 1;
            //            return pattern;
            //        }
            //    }
            //}
            return pattern;
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        List<int> picks = new List<int> { 0, 0, 0, 0, 0 };
        List<int> symbolAvg = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            _results.Add(new List<string>());
            for (int i = 0; i < 5; i++)
            {
                _results.Add(new List<string>(new string[] {picks[i].ToString() }));
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>());
            for (int i = 0; i < 15; i++)
            {
                _results.Add(new List<string>(new string[] { symbolAvg[i].ToString() }));
            }
            _results.Add(new List<string>());
        }
    }
}
