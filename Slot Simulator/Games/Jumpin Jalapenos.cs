using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class JumpinJalapenos : GameInfo
    {
        internal JumpinJalapenos(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wild");
            m_symbolScatter = Reels.Symbols.IndexOf("scatter");
            m_symbolInner = Reels.Symbols.IndexOf("inner");
            m_innerReel = m.GetSymbolIndexesFromDataTable(m_extraData["inner"], 0, Reels.Symbols);
            m_indexesWithWild = m.GetIndexesWithSymbol(m_symbolWild, Reels.ReelsFG, Dimensions, false);
            m_indexesWithWildFull = m.GetIndexesWithSymbol(m_symbolWild, Reels.ReelsFG, Dimensions, true);
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_symbolInner, m_inner;
        List<int> m_innerReel;
        List<List<int>> m_indexesWithWild, m_indexesWithWildFull;
        bool m_nudgeThisSpin;
        int m_gameMultiplier = 1;
        bool m_showStacks = false;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            base.PreSpin(m_showStacks);
            m_inner = m.RandomObject<int>(m_innerReel);
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    case System.Windows.Forms.Keys.Oemtilde:
                        //Cheat Key "~"
                        break;
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
                    screenSymbols = GetScreenSymbols(Reels.ReelsPG);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Post Game
                    CalculateLineWins(WinsToShow, screenSymbols, m_gameMultiplier);
                    CalculateScatterWins(WinsToShow, screenSymbols, ref m_freeGamesLeft, m_gameMultiplier);
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
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();
                    }
                    BonusCode = 1;
                    //Set Up Free Game
                    m_freeGameReels = Reels.ReelsFG;
                    m_freeGameCutoffs = Reels.ReelsFGCutoffs;
                    ReelIndexes = Reels.GetRandomIndexes(Reels.ReelsFG, Reels.ReelsFGCutoffs);
                    m_inner = m.RandomObject<int>(m_innerReel);
                    m_nudgeThisSpin = false;
                    for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                        if (m_indexesWithWild[reelNum].Contains(ReelIndexes[reelNum]) && !m_indexesWithWildFull[reelNum].Contains(ReelIndexes[reelNum]))
                        {
                            ReelIndexes[reelNum] = m_indexesWithWildFull[reelNum][0];
                            m_nudgeThisSpin = true;
                        }
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(Reels.ReelsFG);
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateLineWins(WinsToShow, screenSymbols, m_gameMultiplier);
                    CalculateScatterWins(WinsToShow, screenSymbols, ref m_freeGamesLeft, m_gameMultiplier);
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);
                        if(m_nudgeThisSpin)
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
                    if (symbolIndex == m_symbolInner)
                        symbolIndex = m_inner;
                    reelSymbols.Add(symbolIndex);
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
            return Reels.Symbols[symbolIndex];
        }
        //Bingo/////////////////////////////////////////////////////////////////////////////////////
        /*
        internal override List<byte> GetBingoCodes() { return new List<byte>(new byte[] { 0, 1 }); }
        internal override List<BingoReelIndexArg> FindTopPrize(byte _bonusCode, BackgroundWorker _bgWorker, BackgroundWorker _bgWorkerMain)
        {
            BingoReelIndexArg topPrize = new BingoReelIndexArg(_bonusCode, 0);
            List<BingoReelIndexArg> topPrizes = new List<BingoReelIndexArg>();
            topPrizes.Add(topPrize);
            return topPrizes;
        }
        internal override List<double> GetSegmentRatios()
        {
            List<double> ratios = new List<double>();
            for (int i = 1; i <= m.SegmentNum; i++)
                ratios.Add((i + 3) * 10);
            //30 to 130. So best will be 4x more than worst
            return ratios;
        }
        internal override void CalculateBingoConfiguration(BinaryWriter _writer)
        {
            //Entry Information/////////////////
            //INT Pay Amount
            //BYTE Bonus Code: 
            //INT Score
            //INT Weight
            //CHAR LIST for each of 5 reels
            ReelIndexes = Reels.GetRandomIndexes();
            List<List<int>> screenSymbols = GetScreenSymbols();
            List<WinArgs> wins = new List<WinArgs>();
            int freeGames = 0;
            CalculateLineWins(wins, screenSymbols);
            CalculateScatterWins(wins, screenSymbols, ref freeGames);
            byte bonusCode = freeGames == 0 ? (byte)0 : (byte)1;
            int winAmount = wins.Count > 0 ? CountTheseWins(wins) : 0;
            WriteBingoReelEntryMaybe(_writer, winAmount, bonusCode, GetScore(screenSymbols), 1);
        }
        private int GetScore(List<List<int>> _screenSymbols)
        {
            return CountSymbolsOnScreen(_screenSymbols, m_symbolWild);
        }//*/
        //Private Functions/////////////////////////////////////////////////////////////////////////

        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        internal override void SetUpCustomStats()
        {
        }
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
        }
    }
}
