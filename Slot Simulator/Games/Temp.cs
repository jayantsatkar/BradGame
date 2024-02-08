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
    class Temp : GameInfo
    {
        internal Temp(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_symbolSpecial = Reels.Symbols.IndexOf("h1");         
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        private ReelsSpin m_reelsSpin;
        int m_symbolScatter, m_symbolWild, m_symbolSpecial, fg_triggers = 0, fg_retriggers = 0, current_game_num = 0;
        protected int[] cReSpinSpinOrder = { -1, 0, 1, 2, -1 };
        int[,] hold_symbol = new int[5, 6] { { 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0 }};
        int[] hold_reels = new int[5] { 0, 0, 0, 0, 0 };
        bool respin = false, continue_respin = false, first_respin = true;

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
            /*if (m_progressiveType != "none")
            {
                for (int aa = 0; aa < m_progressives.Count(); aa++)
                {
                    if (m_progressiveType == "shamrock fortunes") m_progressives[aa].IncrementShamrock(Bet, aa, BetLevel);
                    else if (m_progressiveType != "shamrock fortunes") m_progressives[aa].Increment(Bet);
                    if (!InSimulation) frmProg.ProgressiveValueTBs[aa].Text = string.Format("{0:$0,0.00}", (double)m_progressives[aa].CurrentValue / 100);
                }
            }*/
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

                    //edit the below loop to calculate the progressive award
                    /*if (m_progressiveType != "none")
                    {
                        List<WinArgs> progWins = new List<WinArgs>();
                        if (m_progressiveType == "dajidali") progWins = getDaJiDaLiWin();
                        else if (m_progressiveType == "must hit") progWins = getMustHitProgressiveWin();
                        else if (m_progressiveType == "shamrock fortunes") progWins = getShamrockFortunesWin();
                        else if (m_progressiveType == "custom") progWins = getProgressiveWin(screenSymbols);
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        m_progressiveWinPG = CountTheseWins(progWins);
                    }*/

                    //adds all wins into the win list
                    Wins.AddRange(WinsToShow);

                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinsThisPG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 0);                        
                    }

                    if (m_freeGamesLeft > 0)
                    {
                        fg_triggers++;
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
       
                    //Reset hold screen & respin
                    for (int reel = 0; reel < ReelCount; reel++)
                    {
                        for (int row = 0; row < Dimensions[reel]; row++)
                        {
                            hold_symbol[reel, row] = 0;                            
                        }
                    }
                    respin = false;
                    continue_respin = false;

                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    //First screen evaluation
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);

                    for (int i = 1; i < ReelCount - 1; i++)
                        hold_reels[i] = 0;
                    hold_reels[0] = hold_reels[ReelCount - 1] = 1;

                    getRespin(screenSymbols);
                    if(respin == true)
                    {
                        continue_respin = true;
                        current_game_num = 0;
                        first_respin = true;

                        //Remove the H1 wins because those aren't calculated until the respin feature is over
                        foreach (WinArgs specificWin in WinsToShow)
                        {
                            if (specificWin.SymbolIndex == m_symbolSpecial)
                            {
                                WinsToShow.Remove(specificWin);
                                break;
                            }
                        }

                        Wins.AddRange(WinsToShow);

                        if (!InSimulation) MessageBox.Show("RESPIN");
                        m_gameState = DefaultGameStates.ReSpinPreSpin;
                        return GameAction.ShowWinsInBetweenGames;
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
                case DefaultGameStates.ReSpinPreSpin:                                      
                    //Respin reels 2 - 4                    
                    SpinOrder = cReSpinSpinOrder;
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("Respin: {0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    while (continue_respin == true)
                    {
                        screenSymbols = GetScreenSymbols(currentReelSet);

                        //check to see if new H1 symbols appeared - which would cause the respin count to reset
                        if (first_respin != true)
                        {
                            if (getRespin(screenSymbols) == true)
                                current_game_num = 0;
                        }
                        first_respin = false;

                        SpinOrder = cReSpinSpinOrder;

                        if (current_game_num < num_respins)
                        {                  
                            //get a new screen for the reels that are respinning 
                            for (int reel = 1; reel < ReelCount - 1; reel++)
                            {
                                if (hold_reels[reel] == 0)
                                    ReelIndexes[reel] = Reels.GetRandomIndexesGivenReel(currentReelSet, currentCutoffSet, reel);
                            }
  
                            for (int reel = 1; reel < ReelCount - 1; reel++)
                            {
                                for (int row = 0; row < Dimensions[reel]; row++)
                                {
                                    if (hold_symbol[reel, row] == 1)
                                        screenSymbols[reel][row] = m_symbolSpecial;
                                }
                            }

                            //increment current respin game number   
                            current_game_num++;

                            return GameAction.Spin;
                        }
                        else
                            break;
                    }

                    //we're done respinning the middle 3 reels. check to see if we're awarding progressive or not
                    screenSymbols = GetScreenSymbols(currentReelSet);

                    for (int reel = 1; reel < ReelCount - 1; reel++)
                    {
                        for (int row = 0; row < Dimensions[reel]; row++)
                        {
                            if (hold_symbol[reel, row] == 1)
                                screenSymbols[reel][row] = m_symbolSpecial;
                        }
                    }
                    WinsToShow = new List<WinArgs>();
                    List<WinArgs> fgProgWins = new List<WinArgs>();
                    //fgProgWins = getProgressiveWin(screenSymbols);
                    if (fgProgWins.Count() > 0)
                    {
                        //MessageBox.Show("prog");
                        WinsToShow.AddRange(fgProgWins);
                        m_progressiveWinFG = CountTheseWins(fgProgWins);

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
                        respin = false;
                        continue_respin = false;
                        SpinOrder = cDefaultSpinOrder;
                        break;
                    }
                    else
                    {
                        //Middle reels are not spinning anymore
                        for (int i = 1; i < ReelCount - 1; i++)
                            hold_reels[i] = 1;

                        //Check if H1 is on reel 1. If not, respin reel.
                        if (!screenSymbols[0].Contains(m_symbolSpecial))
                        {
                            ReelIndexes[0] = Reels.GetRandomIndexesGivenReel(currentReelSet, currentCutoffSet, 0);
                            hold_reels[0] = 0;
                        }

                        //Check if H1 is on reel 5. If not, respin reel.
                        if (!screenSymbols[ReelCount - 1].Contains(m_symbolSpecial))
                        {
                            ReelIndexes[ReelCount - 1] = Reels.GetRandomIndexesGivenReel(currentReelSet, currentCutoffSet, ReelCount - 1);
                            hold_reels[ReelCount - 1] = 0;
                        }

                        int index = 0;
                        //Set new spin order. -1 means reel doesn't spin.
                        for (int reel = 0; reel < ReelCount; reel++)
                        {
                            if (hold_reels[reel] == 1)
                                cReSpinSpinOrder[reel] = -1;
                            else
                            {
                                cReSpinSpinOrder[reel] = index;
                                index++;
                            }
                        }
                        SpinOrder = cReSpinSpinOrder;

                        m_gameState = DefaultGameStates.ReSpinPostSpin;
                        return GameAction.Spin;
                    }
                    break;
                case DefaultGameStates.ReSpinPostSpin:
                    //should only get here if we didn't get a progressive
                    WinsToShow = new List<WinArgs>();
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    for (int reel = 1; reel < ReelCount - 1; reel++)
                    {
                        for (int row = 0; row < Dimensions[reel]; row++)
                        {
                            if (hold_symbol[reel, row] == 1)
                                screenSymbols[reel][row] = m_symbolSpecial;
                        }
                    }

                    CalculateAnyH1Wins(WinsToShow, screenSymbols);            

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
                    respin = false;
                    continue_respin = false;
                    SpinOrder = cDefaultSpinOrder;
                    break;
            }
            //If Have Free Games, Play
            if (m_freeGamesLeft > 0)
            {
                int freeGamesWonThisSpin = InFreeGames ? m_freeGamesLeft - oldFreeGames + 1 : m_freeGamesLeft;
                if (InFreeGames && freeGamesWonThisSpin > 0)
                {
                    fg_retriggers++;
                }

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
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
        }

        //Private Functions/////////////////////////////////////////////////////////////////////////
        internal override void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            if (InFreeGames)
            {
                for (int i = 0; i < ReelCount; i++)
                {
                    for (int j = 0; j < Dimensions[i]; j++)
                    {
                        if (hold_symbol[i, j] == 1)
                        {
                            _reelsPanel.SpriteDraw(Reels.Symbols[m_symbolSpecial], new Point(_reelsPanel.ReelOffsets[i].X, _reelsPanel.ReelOffsets[i].Y + j * SymbolHeight));
                        }
                    }
                }
            }
        }

        private List<WinArgs> getProgressiveWin(List<List<int>> _screenSymbols)
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            int count = 0, index = 0;

            for (int reel = 1; reel < ReelCount - 1; reel++)
            {
                if (_screenSymbols[reel].Contains(m_symbolSpecial))
                {
                    foreach (int i in _screenSymbols[reel])
                    {
                        if (i == m_symbolSpecial)
                            count++;
                    }
                }
            }

            //If all 3 middle reels consist of the symbol, then the progressive is awarded
            if (count == (Dimensions[1] + Dimensions[2] + Dimensions[3]))
            {
                index = m.RandomIndex(m_progressiveCutoffs);
                winsToReturn.Add(new WinArgs(m_progressives[index].Name, m_progressives[index].GetProgressiveAndReset()));
                if (!InSimulation) 
                    frmProg.ProgressiveValueTBs[index].Text = string.Format("{0:$0,0.00}", (double)m_progressives[index].CurrentValue / 100);
            }   

            return winsToReturn;
        }

        private bool getRespin(List<List<int>> _screenSymbols)
        {
            bool restart = false;
            int count = 0, index = 0;

            if (respin == false)                             //First time we check if there's a respin
            {
                for (int reel = 1; reel < ReelCount - 1; reel++)
                {
                    count = 0;
                    for (int row = 0; row < Dimensions[reel]; row++)
                    {
                        if (_screenSymbols[reel][row] == m_symbolSpecial && hold_symbol[reel, row] == 0)
                        {
                            hold_symbol[reel, row] = 1;
                            restart = true;
                            count++;
                        }
                    }

                    if (count == Dimensions[reel])
                    {
                        hold_reels[reel] = 1;
                        respin = true;
                    }
                }
            }
            else                                           //Subsequent respins
            {
                for (int reel = 1; reel < ReelCount - 1; reel++)
                {
                    count = 0;
                    if (_screenSymbols[reel].Contains(m_symbolSpecial))
                    {
                        for (int row = 0; row < Dimensions[reel]; row++)
                        {
                            if (_screenSymbols[reel][row] == m_symbolSpecial && hold_symbol[reel, row] == 0)
                            {
                                hold_symbol[reel, row] = 1;
                                restart = true;
                                count++;
                            }
                            else if (hold_symbol[reel, row] == 1)
                            {
                                count++;
                            }
                        }
                    }

                    if (count == Dimensions[reel])
                    {
                        hold_reels[reel] = 1;
                    }
                }
            }

            //Set new spin order. -1 means reel doesn't spin.
            for (int reel = 0; reel < ReelCount; reel++)
            {
                if (hold_reels[reel] == 1)
                    cReSpinSpinOrder[reel] = -1;
                else
                {
                    cReSpinSpinOrder[reel] = index;
                    index++;
                }             
            }

            if (hold_reels[1] == 1 && hold_reels[2] == 1 && hold_reels[3] == 1)
                continue_respin = false;

            return restart;
        }

        private void CalculateAnyH1Wins(List<WinArgs> _wins, List<List<int>> _screenSymbols)
        {
            string _prefix = InFreeGames ? cFreeGamePrefix : "";
            Dictionary<int, PayArgs> symbolToPayArg = InFreeGames ? m_symbolToPayArgFG : m_symbolToPayArgPG;
            if (symbolToPayArg.ContainsKey(m_symbolSpecial))
            {
                PayArgs payArg = symbolToPayArg[m_symbolSpecial];
                int match = 0;
                int count = 1;
                for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                {
                    int countThisReel = 0;
                    foreach (int symbol in _screenSymbols[reelNum])
                        if (payArg.CanBe.ContainsKey(symbol))
                        {
                            countThisReel++;
                        }
                    if (countThisReel > 0)
                    {
                        match++;
                        count *= countThisReel;
                    }
                    else break;
                }
                if (match > 0 && payArg.Pays[match - 1] > 0)
                {
                    List<List<int>> cellNums = null;
                    if (!InSimulation)
                    {
                        cellNums = new List<List<int>>();
                        for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                        {
                            List<int> cells = new List<int>();
                            if (reelNum < match)
                                for (int i = 0; i < _screenSymbols[reelNum].Count; i++)
                                    if (payArg.CanBe.ContainsKey(_screenSymbols[reelNum][i]))
                                        cells.Add(i);
                            cellNums.Add(cells);
                        }
                    }
                    _wins.Add(new WinArgs(_prefix == "" ? payArg.Name : _prefix + payArg.Name, payArg.CanBeWithoutMultipliers[0], payArg.Pays[match - 1] * count * BetLevel, match, 1, cellNums));
                }
            }
        }

        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        /*All custom statistics should be declared here.  See Butterfly Game for example.*/
        /*Function to initialize all custom stats*/
        internal override void SetUpCustomStats()
        {
        }
        /*Function to display custom stats*/
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
        }
    }
}
