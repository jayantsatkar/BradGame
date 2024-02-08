using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Fu_Cat : GameInfo
    {
        internal Fu_Cat(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wi");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_symbolBonusWild = Reels.Symbols.IndexOf("bw");
            m_pgWildChance = int.Parse(m_extraData["bg wild chance"][0][0].ToString());
            m_fgWildChance = int.Parse(m_extraData["fg wild chance"][0][0].ToString());
            m_reelConfigurations = new List<string[]>();
            List<int> m_reelConfigWeights = new List<int>();
            for(int row = 0; row < m_extraData["reel configuration weights"].Count(); row++)
            {
                string[] arrayToAdd = new string[] { "", "", "", "", "" };
                arrayToAdd[0] = m_extraData["reel configuration weights"][row][0].ToString();
                arrayToAdd[1] = m_extraData["reel configuration weights"][row][1].ToString();
                arrayToAdd[2] = m_extraData["reel configuration weights"][row][2].ToString();
                arrayToAdd[3] = m_extraData["reel configuration weights"][row][3].ToString();
                arrayToAdd[4] = m_extraData["reel configuration weights"][row][4].ToString();
                m_reelConfigurations.Add(arrayToAdd);
                m_reelConfigWeights.Add(int.Parse(m_extraData["reel configuration weights"][row][5].ToString()));
            }
            m_reelConfigCutOffs = m.MakeCutoffs(m_reelConfigWeights);
            m_wildConfigurations = new List<string[]>();
            List<int> m_wildConfigWeights = new List<int>();
            for(int row = 0; row < m_extraData["wild configuration weights"].Count(); row++)
            {
                string[] arrayToAdd = new string[] { "", "", "" };
                arrayToAdd[0] = m_extraData["wild configuration weights"][row][0].ToString();
                arrayToAdd[1] = m_extraData["wild configuration weights"][row][1].ToString();
                arrayToAdd[2] = m_extraData["wild configuration weights"][row][2].ToString();
                m_wildConfigurations.Add(arrayToAdd);
                m_wildConfigWeights.Add(int.Parse(m_extraData["wild configuration weights"][row][3].ToString()));
            }
            m_wildConfigCutOffs = m.MakeCutoffs(m_wildConfigWeights);
            m_currentWildStates = m.MakeNewEmptyLists<int>(ReelCount);
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_symbolBonusWild;
        int m_pgWildChance, m_fgWildChance;
        List<string[]> m_reelConfigurations;
        List<int> m_reelConfigCutOffs;
        List<string[]> m_wildConfigurations;
        List<int> m_wildConfigCutOffs;
        List<List<int>> m_currentWildStates;
        int m_guaranteedFG;
        bool m_pgWilds = false;
        bool m_fgWilds = false;
        List<List<int>> m_screenSymbolsForDraw;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_pgWilds = false;
            m_fgWilds = false;
            m_guaranteedFG = -1;
            m_currentWildStates = new List<List<int>>();
            for (int aa = 0; aa < ReelCount; aa++)
            {
                List<int> dummyList = new List<int>();
                for(int ab = 0; ab < Dimensions[aa]; ab++)
                {
                    dummyList.Add(0);
                }
                m_currentWildStates.Add(dummyList);
            }
            m_pgWilds = m.RandomInteger(m_pgWildChance) == 0;
            m_screenSymbolsForDraw = new List<List<int>>();
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
                        m_pgWilds = true;
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
            if(m_pgWilds)
            {
                getWildConfiguration();
                m_screenSymbolsForDraw = GetScreenSymbols(currentReelSet);
                for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                {
                    for (int cellNum = 0; cellNum < Dimensions[reelNum]; cellNum++)
                    {
                        if (m_currentWildStates[reelNum][cellNum] == 1) m_screenSymbolsForDraw[reelNum][cellNum] = m_screenSymbolsForDraw[reelNum][cellNum] == m_symbolScatter ? m_symbolBonusWild : m_symbolWild;
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
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    if(m_pgWilds)
                    {
                        for(int reelNum = 0; reelNum < ReelCount; reelNum++)
                        {
                            for(int cellNum = 0; cellNum < Dimensions[reelNum]; cellNum++)
                            {
                                if (m_currentWildStates[reelNum][cellNum] == 1) screenSymbols[reelNum][cellNum] = screenSymbols[reelNum][cellNum] == m_symbolScatter ? m_symbolBonusWild : m_symbolWild;
                            }
                        }
                    }
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    //edit the below loop to calculate the progressive award
                    List<WinArgs> progWins = new List<WinArgs>();
                    progWins = getMustHitProgressiveWin();
                    if(progWins.Count() > 0) WinsToShow.AddRange(progWins);
                    //adds all wins into the win list
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinsThisPG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 0);
                        if (m_pgWilds) m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 1);
                    }
                    if (m_freeGamesLeft > 0) m_guaranteedFG = m.RandomInteger(m_freeGamesLeft);
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
                    m_fgWilds = false;
                    m_pgWilds = false;
                    m_currentWildStates = new List<List<int>>();
                    for (int aa = 0; aa < ReelCount; aa++)
                    {
                        List<int> dummyList = new List<int>();
                        for(int ab = 0; ab < Dimensions[aa]; ab++)
                        {
                            dummyList.Add(0);
                        }
                        m_currentWildStates.Add(dummyList);
                    }
                    m_fgWilds = m.RandomInteger(m_fgWildChance) == 0 || m_guaranteedFG == m_freeGamesPlayed;
                    m_screenSymbolsForDraw = new List<List<int>>();
                    if(m_fgWilds)
                    {
                        getWildConfiguration();
                        m_screenSymbolsForDraw = GetScreenSymbols(currentReelSet);
                        for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                        {
                            for (int cellNum = 0; cellNum < Dimensions[reelNum]; cellNum++)
                            {
                                if (m_currentWildStates[reelNum][cellNum] == 1) m_screenSymbolsForDraw[reelNum][cellNum] = m_screenSymbolsForDraw[reelNum][cellNum] == m_symbolScatter ? m_symbolBonusWild : m_symbolWild;
                            }
                        }
                    }
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    if (m_fgWilds)
                    {
                        for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                        {
                            for (int cellNum = 0; cellNum < Dimensions[reelNum]; cellNum++)
                            {
                                if (m_currentWildStates[reelNum][cellNum] == 1) screenSymbols[reelNum][cellNum] = screenSymbols[reelNum][cellNum] == m_symbolScatter ? m_symbolBonusWild : m_symbolWild;
                            }
                        }
                        m_screenSymbolsForDraw = screenSymbols;
                    }
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
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
            if(m_pgWilds || m_fgWilds)
            {
                for(int reelNum = 0; reelNum < ReelCount; reelNum++)
                {
                    for(int cellNum = 0; cellNum < Dimensions[reelNum]; cellNum++)
                    {
                        if (m_screenSymbolsForDraw[reelNum][cellNum] == m_symbolWild)
                        {
                            _reelsPanel.SpriteDraw("wi", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                        else if (m_screenSymbolsForDraw[reelNum][cellNum] == m_symbolBonusWild)
                        {
                            _reelsPanel.SpriteDraw("bw", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        }
                    }
                }
            }
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private void getWildConfiguration()
        {
            int reelConfigIndex = -1;
            int wildConfigIndex = -1;
            reelConfigIndex = m.RandomIndex(m_reelConfigCutOffs);
            for(int aa = 0; aa < ReelCount; aa++)
            {
                wildConfigIndex = -1;
                if(m_reelConfigurations[reelConfigIndex][aa] == "x")
                {
                    wildConfigIndex = m.RandomIndex(m_wildConfigCutOffs);
                    for(int ab = 0; ab < Dimensions[aa]; ab++)
                    {
                        if (m_wildConfigurations[wildConfigIndex][ab] == "x") m_currentWildStates[aa][ab] = 1;
                        else if (m_wildConfigurations[wildConfigIndex][ab] != "x") m_currentWildStates[aa][ab] = 0;
                    }
                }
                else if (m_reelConfigurations[reelConfigIndex][aa] != "x")
                {
                    for(int ab = 0; ab < Dimensions[aa]; ab++)
                    {
                        m_currentWildStates[aa][ab] = 0;
                    }
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
