using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class DragonsLaw : GameInfo
    {
        internal DragonsLaw(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wild");
            m_symbolScatter = Reels.Symbols.IndexOf("scatter");
            m_symbolInner = Reels.Symbols.IndexOf("inner");
            m_wildFeatureWildConfig = new RandomObjectArg<List<int>>(m.GetIntListFromDataTableZeroIndexedWhereXIsTrue(m_extraData["feature wild configuration"], 1, 5, "w"), m.GetIntCutoffsFromDataTable(m_extraData["feature wild configuration"], 0));
            m_wildFeatureReels = m.MakeNewEmptyLists<int>(4);
            for(int rowIndex=0;rowIndex<m_extraData["feature wild reels"].Count;rowIndex++)
                for (int colIndex = 0; colIndex < 4; colIndex++)
                {
                    int symbolIndex = Reels.Symbols.IndexOf(m_extraData["feature wild reels"][rowIndex][colIndex]);
                    if (symbolIndex != -1)
                        m_wildFeatureReels[colIndex].Add(symbolIndex);
                }
            m_innerReel = m.GetSymbolIndexesFromDataTable(m_extraData["inner reel"], 0, Reels.Symbols);
            m_featureCyclePG = 1 / double.Parse(m_extraGeneralData["pg feature cycle"]);
            m_featureCycleFG = 1 / double.Parse(m_extraGeneralData["fg feature cycle"]);
            m_currentWilds = m.MakeNewEmptyLists<int>(ReelCount);
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_symbolInner;
        RandomObjectArg<List<int>> m_wildFeatureWildConfig;
        List<List<int>> m_wildFeatureReels;
        List<List<int>> m_currentWilds;
        List<int> m_innerReel;
        bool m_feature;
        int m_guaranteedFeatureGame, m_inner;
        double m_featureCyclePG, m_featureCycleFG;
        int[] cExtendedSpinOrder = new int[] { 4, 5, 6, 7, 8 };
        int m_gameMultiplier = 1;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            base.PreSpin(m_stackShow);
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    case System.Windows.Forms.Keys.Oemtilde://Cheat Key "~"
                        //int freeGames = 0;
                        //while (freeGames == 0)
                        //{
                        //    ReelIndexes = Reels.GetRandomIndexes();
                        //    CalculateScatterWins(null, GetScreenSymbols(), ref freeGames);
                        //}
                        break;
                }
            }
            m_feature = m.RandomChance(m_featureCyclePG);
            m_inner = m.RandomObject<int>(m_innerReel);
            if (m_feature)
            {
                RefreshWilds();
                SpinOrder = cExtendedSpinOrder;
            }
            m_gameState = DefaultGameStates.PGPostSpin;
        }

        private void RefreshWilds()
        {
            for(int reelNum=0;reelNum<ReelCount;reelNum++)
                m_currentWilds[reelNum].Clear();
            List<int> wildReelNums = m_wildFeatureWildConfig.RandomPrize;
            List<int> wildReel = m_wildFeatureReels[wildReelNums.Count - 2];
            int wildReelLen = wildReel.Count;
            foreach (int wildReelNum in wildReelNums)
            {
                int randomIndex = m.RandomInteger(wildReel.Count);
                for (int i = 0; i < Dimensions[wildReelNum]; i++)
                    if (wildReel[(randomIndex + i) % wildReelLen] == m_symbolWild)
                        m_currentWilds[wildReelNum].Add(i);
            }
        }
        internal override GameAction PostSpin()
        {
            int oldFreeGames = m_freeGamesLeft;
            SpinOrder = cDefaultSpinOrder;
            List<List<int>> screenSymbols;
            switch (m_gameState)
            {
                case DefaultGameStates.PGPostSpin:
                    screenSymbols = GetScreenSymbols(Reels.ReelsPG);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateScatterWins(WinsToShow, screenSymbols, ref m_freeGamesLeft, m_gameMultiplier);
                    if (m_feature)
                    {
                        for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                            foreach (int cellIndex in m_currentWilds[reelNum])
                                screenSymbols[reelNum][cellIndex] = m_symbolWild;
                        BonusCode = 1;
                    }
                    CalculateLineWins(WinsToShow, screenSymbols, m_gameMultiplier);
                    Wins.AddRange(WinsToShow);
                    if (m_freeGamesLeft > 0)
                        m_guaranteedFeatureGame = m.RandomInteger(m_freeGamesLeft);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinsThisPG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 0);
                        if (m_feature)
                            m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 1);
                        if (DoCustomStats)
                        {
                            m_statsByWildFeaturePattern.IncGames();
                            if (m_feature)
                                m_statsByWildFeaturePattern.StoreGame(m_statsWinsThisPG, m_wildFeatureWildConfig.CurrentIndex);
                        }
                    }
                    break;
                case DefaultGameStates.FGPreSpin:
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                    }
                    m_feature = m_guaranteedFeatureGame == m_freeGamesPlayed || m.RandomChance(m_featureCycleFG);
                    if (m_feature)
                        RefreshWilds();
                    m_inner = m.RandomObject<int>(m_innerReel);
                    BonusCode = 2;
                    m_freeGameReels = Reels.ReelsFG;
                    m_freeGameCutoffs = Reels.ReelsFGCutoffs;
                    //Set Up Free Game
                    ReelIndexes = Reels.GetRandomIndexes(Reels.ReelsFG, Reels.ReelsFGCutoffs);
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(Reels.ReelsFG);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateScatterWins(WinsToShow, screenSymbols, ref m_freeGamesLeft, m_gameMultiplier);
                    if (m_feature)
                        for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                            foreach (int cellIndex in m_currentWilds[reelNum])
                                screenSymbols[reelNum][cellIndex] = m_symbolWild;
                    CalculateLineWins(WinsToShow, screenSymbols, m_gameMultiplier);
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                        if (m_feature)
                            m_statsWinDistributionFG.StoreGame(winsThisFG, 1);
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
            if (DoCustomStats)
            {
                m_statsByBonusCode.IncGames();
                m_statsByBonusCode.StoreGame(WinsThisGame, BonusCode);
            }
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
            //m.AddPayTableCount("Bonus", m.Multipliers.Count + 1, new List<string>(new string[] { "Bonus", }), _payTablesSeperated, _payTablesTotal);
        }
        internal override List<List<int>> GetScreenSymbols(List<List<int>> ReelSet)
        {
            List<List<int>> reels = ReelSet;
            List<List<int>> screenSymbols = new List<List<int>>();
            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
            {
                List<int> reel = reels[reelNum];
                List<int> reelSymbols = new List<int>();
                for (int i = -1; i < Dimensions[reelNum] - 1; i++)
                {
                    int index = (ReelIndexes[reelNum] + i + reel.Count) % reel.Count;
                    int symbolIndex = reel[index];
                    reelSymbols.Add(symbolIndex == m_symbolInner ? m_inner : symbolIndex);
                }
                screenSymbols.Add(reelSymbols);
            }
            return screenSymbols;
        }

        internal override string GetSymbolName(int _reelNum, int _index, int _cellNumIfStopped, bool _spinStop)
        {
            List<List<int>> reels = InFreeGames ? Reels.ReelsFG : Reels.ReelsPG;
            int index = (_index + reels[_reelNum].Count) % reels[_reelNum].Count;
            int symbolIndex = reels[_reelNum][index];
            if (symbolIndex == m_symbolInner)
                symbolIndex = m_inner;
            if (m_feature && _spinStop && m_currentWilds[_reelNum].Contains(_cellNumIfStopped))
                symbolIndex = m_symbolWild;
            return Reels.Symbols[symbolIndex];
        }
        internal override void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            if (m_feature && !_stopped)
                for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                    foreach (int cellNum in m_currentWilds[reelNum])
                        _reelsPanel.SpriteDraw("wild", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
        }

        //Private Functions/////////////////////////////////////////////////////////////////////////

        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        WinDistributionChart m_statsByBonusCode, m_statsByWildFeaturePattern;
        internal override void SetUpCustomStats()
        {
            m_statsByBonusCode = new WinDistributionChart("By BC", m.MakeIntHeaders("BC ", "", 0, 3), Bet, m.Multipliers);
            m_statsByWildFeaturePattern = new WinDistributionChart("By Wild Feature Pattern", m.MakeIntHeaders("Pattern ", "", 1, m_wildFeatureWildConfig.PrizeNum), Bet, m.Multipliers);
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            m_statsByBonusCode.InputResults(_results);
            m_statsByWildFeaturePattern.InputResults(_results);
        }
    }
}
