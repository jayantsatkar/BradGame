using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Markov_Chains : GameInfo
    {
        internal Markov_Chains(ExcelFile _excelFile)
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
        int patternPick = 1;
        int start = 0;
        List <int> sticky = new List<int>{0,0,0,0,0};
        List<int> previousSticky = new List<int>{0,0,0,0,0};
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
            screenSymbols = GetScreenSymbols(currentReelSet);
            for (int reelNum = 0; reelNum < 5; reelNum++)
            {
                for (int cellNum = 0; cellNum < 4; cellNum++)
                {
                    screenSymbols[reelNum][cellNum] = Reels.Symbols.IndexOf("bl");
                }
            }
            m_screenSymbolsForDraw = screenSymbols;
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
                                for (int cellNum = 0; cellNum < 1; cellNum++)
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
                    patternPick = GetPattern(0);
                    screenSymbols = ChangeScreenSymbols(currentReelSet, patternPick, sticky);
                    sticky = CreateNewSticky(screenSymbols);
                    m_screenSymbolsForDraw = screenSymbols;
                    //sticky = new List<int> {0,0,0,0,0};
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    //edit the below loop to calculate the progressive award
                    if (sticky[1] + sticky[2] + sticky[3] == 3 && screenSymbols[1][0] > 13)
                    {
                        List<WinArgs> progWins = safariWilds(screenSymbols[1][0]);
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        if (screenSymbols[1][0] > 15)
                        {
                            progWins[0].Amount = progWins[0].Amount * BetLevel;
                        }
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
                    GetFGReelStrips(false);
                    if (m_freeGamesPlayed == 0)
                    {
                        screenSymbols = GetScreenSymbols(currentReelSet);
                        screenSymbols = ChangeScreenSymbols(screenSymbols, 14, new List<int> { 0, 0, 0, 0, 0 });
                        for (int reelNum = 0; reelNum < 5; reelNum++)
                        {
                            for (int cellNum = 0; cellNum < 4; cellNum++)
                            {
                                screenSymbols[reelNum][cellNum] = Reels.Symbols.IndexOf("bl");
                            }
                        }
                        m_screenSymbolsForDraw = screenSymbols;
                    }
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
                    screenSymbols = ChangeScreenSymbols(currentReelSet, patternPick, sticky);
                    sticky = CreateNewSticky(screenSymbols);
                    previousSticky = sticky;
                    m_screenSymbolsForDraw = screenSymbols;
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    if (sticky[1] + sticky[2] + sticky[3] == 3 && notPaid == 1 && screenSymbols[1][0] > 13)
                    {
                        List<WinArgs> progWins = safariWilds(screenSymbols[1][0]);
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        if (screenSymbols[1][0] > 15)
                        {
                            progWins[0].Amount = progWins[0].Amount * BetLevel;
                        }
                        m_progressiveWinPG = CountTheseWins(progWins);
                        notPaid = 0;
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
                if (m_freeGamesPlayed == 0)
                {
                    patternPick = GetPattern(1);
                    notPaid = 1;
                    sticky = new List<int> { 0, 0, 0, 0, 0 };
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
        private List<List<int>> ChangeScreenSymbols(List<List<int>> ReelSet, int pattern, List <int> hold)
        {
            //Random multiplierFinder = new Random();
            List<List<int>> reels = ReelSet;
            List<List<int>> screenSymbols = new List<List<int>>();
            List<List<int>> currentInsertionSet = InFreeGames ? Reels.SSSymbolsFG : Reels.SSSymbolsBG;
            ////////////////CHANGE VALUES HERE/////////////////
            int featureOdds = 34; //Base game feature odds
            int reelHitProb = 64; //Probability (%) the reels land in base game feature
            int reelHitProbFS = 12; //Probability (%) the reels land in free spins 
            ///////////////////////////////////////////////////
            List<string> inserts2 = new List<string> { "h1", "h2", "h3", "h4", "h5", "h1", "h2", "h3", "h4", "h5", "h1", "h2", "h3", "h4", "h5", "h1", "h2", "h3", "h4", "h5", "h1", "h2", "h3", "h4", "h5", "h1", "h2", "h3", "h4", "h5"}; //R1/R5 reel patterns
            List<string> inserts1 = new List<string> { "p1", "p1", "p1", "p1", "p1", "p2", "p2", "p2", "p2", "p2", "p3", "p3", "p3", "p3", "p3", "p4", "p4", "p4", "p4", "p4", "wi", "wi", "wi", "wi", "wi", "h1", "h2", "h3", "h4", "h5"}; //R2-R4 reel patterns
            int doesBaseHit = hit.Next(0,featureOdds);
            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
            {
                List<int> reel = reels[reelNum];
                List<int> reelSymbols = new List<int>();
                for (int i = 0; i < Dimensions[reelNum]; i++)
                {
                    int index = (ReelIndexes[reelNum] + i + reel.Count) % reel.Count;
                    int symbolIndex = reel[index];
                    if (hold[reelNum] == 0)
                        {
                            if (i == 0)
                            {
                                if(InFreeGames)
                                {
                                    int possiblehit = hit.Next(0,100);
                                    if (possiblehit < reelHitProbFS)
                                    {
                                        if (reelNum == 0 || reelNum == 4)
                                        {
                                            symbolIndex = Reels.Symbols.IndexOf(inserts2[pattern - 1]);
                                        }
                                        if (reelNum > 0 && reelNum < 4)
                                        {
                                            symbolIndex = Reels.Symbols.IndexOf(inserts1[pattern - 1]);
                                        }
                                    }
                                    if (possiblehit > reelHitProbFS - 1)
                                    {
                                        symbolIndex = Reels.Symbols.IndexOf("bl");
                                    }
                                }
                                if (!InFreeGames)
                                {
                                    int possiblehit = hit.Next(0,100);
                                    if (possiblehit < reelHitProb && doesBaseHit == 0)
                                    {
                                        if (reelNum == 0 || reelNum == 4)
                                        {
                                            symbolIndex = Reels.Symbols.IndexOf(inserts2[pattern - 1]);
                                        }
                                        if (reelNum > 0 && reelNum < 4)
                                        {
                                            symbolIndex = Reels.Symbols.IndexOf(inserts1[pattern - 1]);
                                        }
                                    }
                                    if (possiblehit > reelHitProb - 1 || doesBaseHit > 0)
                                    {
                                        symbolIndex = Reels.Symbols.IndexOf("bl");
                                    }
                                }
                            }
                    }
                    if (hold[reelNum] == 1)
                    {
                        if (InFreeGames)
                        {
                            if (i == 0)
                            {
                                if (reelNum == 0 || reelNum == 4)
                                {
                                   symbolIndex = Reels.Symbols.IndexOf(inserts2[pattern - 1]);
                                }
                                if (reelNum > 0 && reelNum < 4)
                                {
                                   symbolIndex = Reels.Symbols.IndexOf(inserts1[pattern - 1]);
                                }
                            }
                        }
                        if (!InFreeGames)
                        {
                            if (i == 0)
                            {
                                int possiblehit = hit.Next(0,100);
                                if (possiblehit < reelHitProb && doesBaseHit == 0)
                                {
                                    if (reelNum == 0 || reelNum == 4)
                                    {
                                        symbolIndex = Reels.Symbols.IndexOf(inserts2[pattern - 1]);
                                    }
                                    if (reelNum > 0 && reelNum < 4)
                                    {
                                        symbolIndex = Reels.Symbols.IndexOf(inserts1[pattern - 1]);
                                    }
                                }
                                if (possiblehit > reelHitProb - 1 || doesBaseHit > 0)
                                {
                                    symbolIndex = Reels.Symbols.IndexOf("bl");
                                }
                            }
                        }
                    }
                    reelSymbols.Add(symbolIndex == m_insertionSymbol ? currentInsertionSet[m_insertionIndex][reelNum] : symbolIndex);
                }
                screenSymbols.Add(reelSymbols);
            }
            return screenSymbols;
        }
        private List<int> CreateNewSticky(List<List<int>> visuals)
        {
            List<int> hold = new List<int>{0,0,0,0,0};
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

        internal override void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            if((m_gameState == DefaultGameStates.PGPostSpin || m_gameState == DefaultGameStates.FGPostSpin || m_gameState == DefaultGameStates.FGPreSpin) && start == 1)
                for (int reelNum = 0; reelNum < 5; reelNum++)
                {
                    int cellNum = 0;
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
                }
        }
        internal override void CustomDrawBeforeDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            //if ((m_gameState == DefaultGameStates.PGPostSpin || m_gameState == DefaultGameStates.FGPostSpin || m_gameState == DefaultGameStates.FGPreSpin) && start == 1)
            if (start == 1)
            {
                for (int reelNum = 0; reelNum < 5; reelNum++)
                {
                    for (int cellNum = 0; cellNum < 1; cellNum++)
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
        private int GetPattern(int i)
        {
            int pattern = 5;
            Random pick = new Random();
            ////////////CHANGE VALUES HERE////////////////
            List<int> weightsBase1 = new List<int> {2,4,6,8,10,208,406,604,802,1000,4800,8600,12400,16200,20000,36000,52000,68000,84000,100000,120000,140000,160000,180000,200000,260000,320000,380000,440000,500000}; //Rolling average weights for base game patterns
            List<int> weightsBase2 = new List<int> {4,8,12,16,20,416,812,1208,1604,2000,5800,9600,13400,17200,21000,37000,53000,69000,85000,101000,120822,140622,160422,180222,200100,260100,320100,380100,440100,500100}; //Rolling average weights for base game patterns
            List<int> weightsBase3 = new List<int> {6,12,18,24,30,624,1218,1812,2406,3000,6800,10600,14400,18200,22000,38000,54000,70000,86000,102000,121644,141244,160844,180444,200200,260200,320200,380200,440200,500200}; //Rolling average weights for base game patterns
            List<int> weightsBase5 = new List<int> {10,20,30,40,50,1040,2030,3020,4010,5000,8800,12600,16400,20200,24000,40000,56000,72000,88000,104000,123288,142488,161688,180888,200400,260400,320400,380400,440400,500400}; //Rolling average weights for base game patterns
            List<int> weightsBase10 = new List<int> { 20, 40, 60, 80, 100, 2080, 4060, 6040, 8020, 10000, 13800, 17600, 21400, 25200, 29000, 45000, 61000, 77000, 93000, 109000, 127397, 145597, 163797, 181997, 200900, 260900, 320900, 380900, 440900, 500900 }; //Rolling average weights for base game patterns

            List<int> weightsFS1 = new List<int> { 2, 4, 6, 8, 10, 208, 406, 604, 802, 1000, 4800, 8600, 12400, 16200, 20000, 36000, 52000, 68000, 84000, 100000, 120000, 140000, 160000, 180000, 200000, 260000, 320000, 380000, 440000, 500000 }; //Rolling average weights for base game patterns
            List<int> weightsFS2 = new List<int> { 4, 8, 12, 16, 20, 416, 812, 1208, 1604, 2000, 5800, 9600, 13400, 17200, 21000, 37000, 53000, 69000, 85000, 101000, 120822, 140622, 160422, 180222, 200100, 260100, 320100, 380100, 440100, 500100 }; //Rolling average weights for base game patterns
            List<int> weightsFS3 = new List<int> { 6, 12, 18, 24, 30, 624, 1218, 1812, 2406, 3000, 6800, 10600, 14400, 18200, 22000, 38000, 54000, 70000, 86000, 102000, 121644, 141244, 160844, 180444, 200200, 260200, 320200, 380200, 440200, 500200 }; //Rolling average weights for base game patterns
            List<int> weightsFS5 = new List<int> { 10, 20, 30, 40, 50, 1040, 2030, 3020, 4010, 5000, 8800, 12600, 16400, 20200, 24000, 40000, 56000, 72000, 88000, 104000, 123288, 142488, 161688, 180888, 200400, 260400, 320400, 380400, 440400, 500400 }; //Rolling average weights for base game patterns
            List<int> weightsFS10 = new List<int> { 20, 40, 60, 80, 100, 2080, 4060, 6040, 8020, 10000, 13800, 17600, 21400, 25200, 29000, 45000, 61000, 77000, 93000, 109000, 127397, 145597, 163797, 181997, 200900, 260900, 320900, 380900, 440900, 500900 }; //Rolling average weights for base game patterns








            //////////////////////////////////////////////
            if (BetLevel == 1)
            {
                int pickBase = hit.Next(0, weightsBase1[weightsBase1.Count() - 1]);
                int pickFS = hit.Next(0, weightsFS1[weightsFS1.Count() - 1]);
                if (i == 0) //Base Game
                {
                    for (int j = 0; j < weightsBase1.Count(); j++)
                    {
                        if (pickBase < weightsBase1[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                if (i == 1) //Free Spins
                {
                    for (int j = 0; j < weightsFS1.Count(); j++)
                    {
                        if (pickFS < weightsFS1[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                return pattern;
            }
            if (BetLevel == 2)
            {
                int pickBase = hit.Next(0, weightsBase2[weightsBase2.Count() - 1]);
                int pickFS = hit.Next(0, weightsFS2[weightsFS2.Count() - 1]);
                if (i == 0) //Base Game
                {
                    for (int j = 0; j < weightsBase2.Count(); j++)
                    {
                        if (pickBase < weightsBase2[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                if (i == 1) //Free Spins
                {
                    for (int j = 0; j < weightsFS2.Count(); j++)
                    {
                        if (pickFS < weightsFS2[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                return pattern;
            }
            if (BetLevel == 3)
            {
                int pickBase = hit.Next(0, weightsBase3[weightsBase3.Count() - 1]);
                int pickFS = hit.Next(0, weightsFS3[weightsFS3.Count() - 1]);
                if (i == 0) //Base Game
                {
                    for (int j = 0; j < weightsBase3.Count(); j++)
                    {
                        if (pickBase < weightsBase3[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                if (i == 1) //Free Spins
                {
                    for (int j = 0; j < weightsFS3.Count(); j++)
                    {
                        if (pickFS < weightsFS3[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                return pattern;
            }
            if (BetLevel == 5)
            {
                int pickBase = hit.Next(0, weightsBase5[weightsBase5.Count() - 1]);
                int pickFS = hit.Next(0, weightsFS5[weightsFS5.Count() - 1]);
                if (i == 0) //Base Game
                {
                    for (int j = 0; j < weightsBase5.Count(); j++)
                    {
                        if (pickBase < weightsBase5[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                if (i == 1) //Free Spins
                {
                    for (int j = 0; j < weightsFS5.Count(); j++)
                    {
                        if (pickFS < weightsFS5[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                return pattern;
            }
            if (BetLevel == 10)
            {
                int pickBase = hit.Next(0, weightsBase10[weightsBase5.Count() - 1]);
                int pickFS = hit.Next(0, weightsFS10[weightsFS5.Count() - 1]);
                if (i == 0) //Base Game
                {
                    for (int j = 0; j < weightsBase10.Count(); j++)
                    {
                        if (pickBase < weightsBase10[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                if (i == 1) //Free Spins
                {
                    for (int j = 0; j < weightsFS10.Count(); j++)
                    {
                        if (pickFS < weightsFS10[j])
                        {
                            pattern = j + 1;
                            return pattern;
                        }
                    }
                }
                return pattern;
            }
            return pattern;
            
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
           
        }
    }
}
