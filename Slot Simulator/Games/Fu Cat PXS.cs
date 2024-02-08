using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Fu_Cat_PXS : GameInfo
    {
        internal Fu_Cat_PXS(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_symbolBonusWild = Reels.Symbols.IndexOf("bw");
            m_symbolBonus2 = Reels.Symbols.IndexOf("b2");
            m_symbolBonus3 = Reels.Symbols.IndexOf("b3");
            m_symbolBonus4 = Reels.Symbols.IndexOf("b4");
            m_symbolBonus5 = Reels.Symbols.IndexOf("b5");
            m_symbolBonus8 = Reels.Symbols.IndexOf("b8");
            m_symbolBonus12 = Reels.Symbols.IndexOf("b1");
            m_symbolBonus20 = Reels.Symbols.IndexOf("b0");
            m_wildConfigurations = new List<List<int>>();
            m_wildConfigCutoffs = new List<int>();
            List<int> m_wildConfigWeights = new List<int>();
            List<int> m_wildConfigWeightsFG = new List<int>();
            for(int aa = 0; aa < m_extraData["reel configuration weights"].Count(); aa++)
            {
                List<int> newConfigList = new List<int>();
                for(int ab = 0; ab < 12; ab++)
                {
                    newConfigList.Add(int.Parse(m_extraData["reel configuration weights"][aa][ab].ToString()));
                }
                m_wildConfigurations.Add(newConfigList);
                m_wildConfigWeights.Add(int.Parse(m_extraData["reel configuration weights"][aa][12].ToString()));
                m_wildConfigWeightsFG.Add(int.Parse(m_extraData["reel configuration weights"][aa][13].ToString()));
            }
            m_wildConfigCutoffs = m.MakeCutoffs(m_wildConfigWeights);
            m_wildConfigCutoffsFG = m.MakeCutoffs(m_wildConfigWeightsFG);
            m_freeSpinCountMultipliers = new List<int>();
            m_freeSpinCountMultipliersCutoffs = new List<int>();
            List<int> m_freeSpinCountMultiplierWeights = new List<int>();
            for(int ac = 0; ac < m_extraData["free spin count multipliers"].Count(); ac++)
            {
                m_freeSpinCountMultipliers.Add(int.Parse(m_extraData["free spin count multipliers"][ac][0].ToString()));
                m_freeSpinCountMultiplierWeights.Add(int.Parse(m_extraData["free spin count multipliers"][ac][1].ToString()));
            }
            m_freeSpinCountMultipliersCutoffs = m.MakeCutoffs(m_freeSpinCountMultiplierWeights);
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_symbolBonusWild, m_symbolBonus2, m_symbolBonus3, m_symbolBonus4, m_symbolBonus5, m_symbolBonus8, m_symbolBonus12, m_symbolBonus20;
        List<List<int>> m_wildConfigurations;
        List<int> m_wildConfigCutoffs;
        List<int> m_freeSpinCountMultipliers;
        List<int> m_freeSpinCountMultipliersCutoffs;
        List<int> m_wildConfigCutoffsFG;
        List<List<int>> m_currentWildStates;
        List<List<int>> m_screenSymbolsForDraw;
        int m_pgWilds, m_fgWilds;
        int m_guaranteedSpin;
        int m_fgMultiplier;
        int m_oldFreeGamesLeft;
        int m_currentSessionWildFeatures;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            m_pgWilds = -1;
            m_fgWilds = -1;
            m_guaranteedSpin = -1;
            m_fgMultiplier = -1;
            m_oldFreeGamesLeft = -1;
            m_currentSessionWildFeatures = 0;
            m_fgMultiplier = m_freeSpinCountMultipliers[m.RandomIndex(m_freeSpinCountMultipliersCutoffs)];
            m_screenSymbolsForDraw = new List<List<int>>();
            m_currentWildStates = new List<List<int>>();
            for (int ad = 1; ad < ReelCount-1; ad++)
            {
                List<int> dummyList = new List<int>();
                for(int ae = 0; ae < Dimensions[ad]; ae++)
                {
                    dummyList.Add(0);
                }
                m_currentWildStates.Add(dummyList);
            }
            m_pgWilds = m.RandomIndex(m_wildConfigCutoffs);
            if(DoCustomStats)
            {
                if (m_totalWildIndexCounts.Keys.Contains(m_pgWilds)) m_totalWildIndexCounts[m_pgWilds]++;
                else if (!m_totalWildIndexCounts.Keys.Contains(m_pgWilds)) m_totalWildIndexCounts[m_pgWilds] = 1;
            }
            base.PreSpin(true);
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
                    case System.Windows.Forms.Keys.Q: //Cheat Key "Q"
                        while(m_pgWilds == 0)
                        {
                            m_pgWilds = m.RandomIndex(m_wildConfigCutoffs);
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
            m_screenSymbolsForDraw = GetScreenSymbols(currentReelSet);
            if (m_pgWilds > 0)
            {
                if(InSimulation && DoCustomStats) m_featureWinsByCount.IncGames();
                getWildInfo();
            }
            else if (m_pgWilds == 0)
            {
                if (InSimulation && DoCustomStats) m_pgWOFeature.IncGames();
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
                    if (m_freeGamesLeft > 0) m_freeGamesLeft *= m_fgMultiplier;
                    if(m_pgWilds > 0)
                    {
                        for(int aj = 1; aj < ReelCount - 1; aj++)
                        {
                            for(int ak = 0; ak < Dimensions[aj]; ak++)
                            {
                                if (m_currentWildStates[aj-1][ak] == 1) screenSymbols[aj][ak] = screenSymbols[aj][ak] == m_symbolScatter ? m_symbolBonusWild : m_symbolWild;
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
                        if (m_pgWilds > 0) m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 1);
                    }
                    if(DoCustomStats)
                    {
                        if (m_pgWilds == 0) m_pgWOFeature.StoreGame(m_statsWinsThisPG, 0);
                        else if (m_pgWilds > 0)
                        {
                            int wildCount = m_wildConfigurations[m_pgWilds].Sum();
                            m_featureWinsByCount.StoreGame(m_statsWinsThisPG, wildCount);
                            if (m_wildFeatureWins.Keys.Contains(m_statsWinsThisPG)) m_wildFeatureWins[m_statsWinsThisPG]++;
                            else if (!m_wildFeatureWins.Keys.Contains(m_statsWinsThisPG)) m_wildFeatureWins[m_statsWinsThisPG] = 1;
                        }
                    }
                    //if (m_freeGamesLeft > 0) m_guaranteedSpin = m.RandomInteger(m_freeGamesLeft);
                    break;
                case DefaultGameStates.FGPreSpin:
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    //Set Up Free Game
                    GetFGReelStrips(true);
                    m_fgMultiplier = -1;
                    m_fgMultiplier = m_freeSpinCountMultipliers[m.RandomIndex(m_freeSpinCountMultipliersCutoffs)];
                    m_oldFreeGamesLeft = -1;
                    m_oldFreeGamesLeft = m_freeGamesLeft;
                    m_fgWilds = -1;
                    m_currentWildStates = new List<List<int>>();
                    for (int ad = 1; ad < ReelCount - 1; ad++)
                    {
                        List<int> dummyList = new List<int>();
                        for (int al = 0; al < Dimensions[ad]; al++)
                        {
                            dummyList.Add(0);
                        }
                        m_currentWildStates.Add(dummyList);
                    }
                    if (m_guaranteedSpin == (m_freeGamesPlayed))
                    {
                        m_fgWilds = m.RandomIndex(m_wildConfigCutoffsFG);
                        while (m_fgWilds == 0)
                        {
                            m_fgWilds = m.RandomIndex(m_wildConfigCutoffsFG);
                        }
                    }
                    else if (m_guaranteedSpin != (m_freeGamesPlayed)) m_fgWilds = m.RandomIndex(m_wildConfigCutoffsFG);
                    m_screenSymbolsForDraw = new List<List<int>>();
                    m_screenSymbolsForDraw = GetScreenSymbols(currentReelSet);
                    if (m_fgWilds > 0)
                    {
                        getWildInfoFG();
                        if (DoCustomStats && InSimulation) m_featureWinsByCount.IncGames();
                    }
                    else if (m_fgWilds == 0)
                    {
                        if (InSimulation && DoCustomStats) m_fgWOFeature.IncGames();
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
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    if (m_oldFreeGamesLeft != m_freeGamesLeft)
                    {
                        int freeGameDifference = 0;
                        freeGameDifference = m_freeGamesLeft - m_oldFreeGamesLeft;
                        int freeGamesToAdd = 0;
                        freeGamesToAdd = m_fgMultiplier * freeGameDifference;
                        m_freeGamesLeft = m_oldFreeGamesLeft + freeGamesToAdd;
                    }
                    if (m_fgWilds > 0)
                    {
                        for (int aj = 1; aj < ReelCount - 1; aj++)
                        {
                            for (int ak = 0; ak < Dimensions[aj]; ak++)
                            {
                                if (m_currentWildStates[aj-1][ak] == 1) screenSymbols[aj][ak] = screenSymbols[aj][ak] == m_symbolScatter ? m_symbolBonusWild : m_symbolWild;
                            }
                        }
                    }
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                        if (m_fgWilds > 0)
                        {
                            m_statsWinDistributionFG.StoreGame(winsThisFG, 1);
                        }
                    }
                    if(DoCustomStats)
                    {
                        int winsThisFG2 = CountTheseWins(WinsToShow);
                        if (m_fgWilds > 0)
                        {
                            int wildCount = m_wildConfigurations[m_fgWilds].Sum();
                            m_featureWinsByCount.StoreGame(winsThisFG2, wildCount);
                            if (m_wildFeatureWins.Keys.Contains(winsThisFG2)) m_wildFeatureWins[winsThisFG2]++;
                            else if (!m_wildFeatureWins.Keys.Contains(winsThisFG2)) m_wildFeatureWins[winsThisFG2] = 1;
                            if(winsThisFG2 > 0) m_currentSessionWildFeatures++;
                        }
                        else if (m_fgWilds == 0) m_fgWOFeature.StoreGame(winsThisFG2, 0);
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
                if (m_freeGamesPlayed == 0)
                {
                    m_freeGameSessions++;
                    m_currentSessionWildFeatures = 0;
                    m_guaranteedSpin = m.RandomInteger(8);
                }
                m_gameState = DefaultGameStates.FGPreSpin;
                return GameAction.ShowWinsInBetweenGames;
            }
            //Sum up wins and Stats
            WinsThisGame = CountTheseWins(Wins);
            AfterGameStatsCollection();
            if (m_freeGamesLeft == 0 && m_freeGamesPlayed > 0 && !InSimulation) m.gameMessageBox.Text = string.Format("{0} Total Free Spins Bonus Win", WinsThisGame);
            if (DoCustomStats)
            {
                if (m_currentSessionWildFeatures == 0 && m_freeGamesPlayed > 0 && m_freeGamesLeft == 0) m_freeGamesWOFeatures++;
                if (m_freeGamesLeft == 0 && m_freeGamesPlayed > 0)
                {
                    if (m_totalFGCounts.Keys.Contains(m_freeGamesPlayed)) m_totalFGCounts[m_freeGamesPlayed]++;
                    else if (!m_totalFGCounts.Keys.Contains(m_freeGamesPlayed)) m_totalFGCounts[m_freeGamesPlayed] = 1;
                    if(m_wildFeaturesInFG.Keys.Contains(m_freeGamesPlayed))
                    {
                        if (m_wildFeaturesInFG[m_freeGamesPlayed].Keys.Contains(m_currentSessionWildFeatures)) m_wildFeaturesInFG[m_freeGamesPlayed][m_currentSessionWildFeatures]++;
                        else if (!m_wildFeaturesInFG[m_freeGamesPlayed].Keys.Contains(m_currentSessionWildFeatures))
                        {
                            m_wildFeaturesInFG[m_freeGamesPlayed][m_currentSessionWildFeatures] = 1;
                        }
                    }
                    else if(!m_wildFeaturesInFG.Keys.Contains(m_freeGamesPlayed))
                    {
                        m_wildFeaturesInFG[m_freeGamesPlayed] = new SortedDictionary<int, ulong>();
                        m_wildFeaturesInFG[m_freeGamesPlayed][m_currentSessionWildFeatures] = 1;
                    }
                }
            }
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
        }
        internal override void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped)
        {
            if(m_pgWilds > 0 || m_fgWilds > 0)
            {
                for(int reelNum = 1; reelNum < ReelCount - 1; reelNum++)
                {
                    for(int cellNum = 0; cellNum < Dimensions[reelNum]; cellNum++)
                    {
                        if(m_screenSymbolsForDraw[reelNum][cellNum] == m_symbolWild) _reelsPanel.SpriteDraw("ww", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                        else if (m_screenSymbolsForDraw[reelNum][cellNum] == m_symbolBonusWild) _reelsPanel.SpriteDraw("bw", new Point(_reelsPanel.ReelOffsets[reelNum].X, _reelsPanel.ReelOffsets[reelNum].Y + cellNum * SymbolHeight));
                    }
                }
            }
            if(m_fgMultiplier > 1)
            {
                for(int cellNum = 0; cellNum < Dimensions[3]; cellNum++)
                {
                    if(m_screenSymbolsForDraw[3][cellNum] == m_symbolScatter)
                    {
                        if(m_fgMultiplier == 2) _reelsPanel.SpriteDraw("b2", new Point(_reelsPanel.ReelOffsets[3].X, _reelsPanel.ReelOffsets[3].Y + cellNum * SymbolHeight));
                        else if (m_fgMultiplier == 3) _reelsPanel.SpriteDraw("b3", new Point(_reelsPanel.ReelOffsets[3].X, _reelsPanel.ReelOffsets[3].Y + cellNum * SymbolHeight));
                        else if (m_fgMultiplier == 4) _reelsPanel.SpriteDraw("b4", new Point(_reelsPanel.ReelOffsets[3].X, _reelsPanel.ReelOffsets[3].Y + cellNum * SymbolHeight));
                        else if (m_fgMultiplier == 5) _reelsPanel.SpriteDraw("b5", new Point(_reelsPanel.ReelOffsets[3].X, _reelsPanel.ReelOffsets[3].Y + cellNum * SymbolHeight));
                        else if (m_fgMultiplier == 8) _reelsPanel.SpriteDraw("b8", new Point(_reelsPanel.ReelOffsets[3].X, _reelsPanel.ReelOffsets[3].Y + cellNum * SymbolHeight));
                        else if (m_fgMultiplier == 12) _reelsPanel.SpriteDraw("b1", new Point(_reelsPanel.ReelOffsets[3].X, _reelsPanel.ReelOffsets[3].Y + cellNum * SymbolHeight));
                        else if (m_fgMultiplier == 20) _reelsPanel.SpriteDraw("b0", new Point(_reelsPanel.ReelOffsets[3].X, _reelsPanel.ReelOffsets[3].Y + cellNum * SymbolHeight));
                    }
                }
            }
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private List<WinArgs> getProgressiveWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            return winsToReturn;
        }
        private void getWildInfo()
        {
            for (int af = 1; af < ReelCount - 1; af++)
            {
                for(int ag = 0; ag < Dimensions[af]; ag++)
                {
                    m_currentWildStates[af - 1][ag] = m_wildConfigurations[m_pgWilds][(af - 1) * 4 + ag];
                }
            }
            m_screenSymbolsForDraw = GetScreenSymbols(currentReelSet);
            for(int ah = 1; ah < ReelCount - 1; ah++)
            {
                for(int ai = 0; ai < Dimensions[ah]; ai++)
                {
                    if (m_currentWildStates[ah - 1][ai] == 1) m_screenSymbolsForDraw[ah][ai] = m_screenSymbolsForDraw[ah][ai] == m_symbolScatter ? m_symbolBonusWild : m_symbolWild;
                }
            }
        }
        private void getWildInfoFG()
        {
            for (int af = 1; af < ReelCount - 1; af++)
            {
                for (int ag = 0; ag < Dimensions[af]; ag++)
                {
                    m_currentWildStates[af - 1][ag] = m_wildConfigurations[m_fgWilds][(af - 1) * 4 + ag];
                }
            }
            m_screenSymbolsForDraw = GetScreenSymbols(currentReelSet);
            for (int ah = 1; ah < ReelCount - 1; ah++)
            {
                for (int ai = 0; ai < Dimensions[ah]; ai++)
                {
                    if (m_currentWildStates[ah - 1][ai] == 1) m_screenSymbolsForDraw[ah][ai] = m_screenSymbolsForDraw[ah][ai] == m_symbolScatter ? m_symbolBonusWild : m_symbolWild;
                }
            }
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        WinDistributionChart m_pgWOFeature, m_fgWOFeature, m_featureWinsByCount;
        ulong m_freeGamesWOFeatures, m_freeGameSessions;
        SortedDictionary<int, ulong> m_wildFeatureWins;
        SortedDictionary<int, SortedDictionary<int, ulong>> m_wildFeaturesInFG;
        SortedDictionary<int, ulong> m_totalFGCounts;
        SortedDictionary<int, ulong> m_totalWildIndexCounts;
        internal override void SetUpCustomStats()
        {
            m_pgWOFeature = new WinDistributionChart("Primary Game Wins without Feature", new List<string> { "Wins" }, Bet, m.Multipliers);
            m_fgWOFeature = new WinDistributionChart("Free Game Wins without Feature", new List<string> { "Wins" }, Bet, m.Multipliers);
            m_featureWinsByCount = new WinDistributionChart("Feature Wins by Wild Counts", m.MakeIntHeaders("", "", 0, 13), Bet, m.Multipliers);
            m_freeGamesWOFeatures = 0;
            m_freeGameSessions = 0;
            m_wildFeatureWins = new SortedDictionary<int, ulong>();
            m_wildFeaturesInFG = new SortedDictionary<int, SortedDictionary<int, ulong>>();
            m_totalFGCounts = new SortedDictionary<int, ulong>();
            m_totalWildIndexCounts = new SortedDictionary<int, ulong>();
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            m_pgWOFeature.InputResults(_results);
            m_fgWOFeature.InputResults(_results);
            m_featureWinsByCount.InputResults(_results);
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Free Game Sessions without Features:", m_freeGamesWOFeatures.ToString() }));
            _results.Add(new List<string>(new string[] { "Free Game Sessions:", m_freeGameSessions.ToString() }));
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total Free Game Counts"}));
            _results.Add(new List<string>(new string[] { "Spins", "Counts"}));
            foreach(int spinCount in m_totalFGCounts.Keys)
            {
                _results.Add(new List<string>(new string[] {spinCount.ToString(), m_totalFGCounts[spinCount].ToString() }));
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Wild Index", "Count" }));
            foreach(int wildIndex in m_totalWildIndexCounts.Keys)
            {
                _results.Add(new List<string>(new string[] { wildIndex.ToString(), m_totalWildIndexCounts[wildIndex].ToString() }));
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Total Free Game Wild Features" }));
            _results.Add(new List<string>(new string[] { "Spins", "Wild Features", "Count" }));
            foreach(int spins in m_wildFeaturesInFG.Keys)
            {
                foreach(int wildFeatures in m_wildFeaturesInFG[spins].Keys)
                {
                    _results.Add(new List<string>(new string[] { spins.ToString(), wildFeatures.ToString(), m_wildFeaturesInFG[spins][wildFeatures].ToString() }));
                }
            }
            _results.Add(new List<string>());
            _results.Add(new List<string>(new string[] { "Feature Win Stats" }));
            _results.Add(new List<string>(new string[] { "Win Amount", "Hits" }));
            foreach (int winAmount in m_wildFeatureWins.Keys)
            {
                _results.Add(new List<string>(new string[] { winAmount.ToString(), m_wildFeatureWins[winAmount].ToString() }));
            }
            _results.Add(new List<string>());
        }
    }
}
