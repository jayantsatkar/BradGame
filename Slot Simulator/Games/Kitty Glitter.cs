using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class KittyGlitter : GameInfo
    {
        internal KittyGlitter(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("wild");
            m_symbolScatter = Reels.Symbols.IndexOf("scatter");
            m_symbolScatter2 = Reels.Symbols.IndexOf("scatter2");
            m_symbolPICa = Reels.Symbols.IndexOf("pic-a");
            m_symbolPICb = Reels.Symbols.IndexOf("pic-b");
            m_symbolPICc = Reels.Symbols.IndexOf("pic-c");
            m_symbolPICd = Reels.Symbols.IndexOf("pic-d");
            m_indexesWithScatter2FG = m.GetIndexesWithSymbol(m_symbolScatter2, Reels.ReelsFG, Dimensions, false);
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolScatter2, m_symbolWild, m_symbolPICa, m_symbolPICb, m_symbolPICc, m_symbolPICd;
        int m_charmCount;
        List<List<int>> m_indexesWithScatter2FG;
        int m_gameMultiplier = 1;
        bool m_showStacks = false;
        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            base.PreSpin(m_showStacks);
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    case System.Windows.Forms.Keys.Oemtilde:
                        int freeGames = 0;
                        while (freeGames == 0)
                        {
                            ReelIndexes = Reels.GetRandomIndexes(Reels.ReelsPG, Reels.ReelsPGCutoffs);
                            CalculateScatterWins(null, GetScreenSymbols(Reels.ReelsPG), ref freeGames, m_gameMultiplier);
                        }
                        break;
                }
            }
            m_charmCount = 0;
            m_gameState = DefaultGameStates.PGPostSpin;
        }
        internal override GameAction PostSpin()
        {
            int oldFreeGames = m_freeGamesLeft;
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
                    m_gameState = DefaultGameStates.FGPostSpin;
                    return GameAction.Spin;
                case DefaultGameStates.FGPostSpin:
                    if (m_indexesWithScatter2FG[4].Contains(ReelIndexes[4]))
                        m_charmCount++;
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
                    if (InFreeGames && reelNum > 0)
                    {
                        if ((m_charmCount >= 3 && symbolIndex == m_symbolPICa) ||
                            (m_charmCount >= 6 && symbolIndex == m_symbolPICb) ||
                            (m_charmCount >= 9 && symbolIndex == m_symbolPICc) ||
                            (m_charmCount >= 12 && symbolIndex == m_symbolPICd))
                            symbolIndex = m_symbolWild;
                    }
                    reelSymbols.Add(symbolIndex);
                }
                screenSymbols.Add(reelSymbols);
            }
            return screenSymbols;
        }
        //Bingo
        /*
        internal override List<byte> GetBingoCodes() { return new List<byte>(new byte[] { 0, 1 }); }
        internal override List<BingoReelIndexArg> FindTopPrize(byte _bonusCode, BackgroundWorker _bgWorker, BackgroundWorker _bgWorkerMain)
        {
            BingoReelIndexArg topPrize = new BingoReelIndexArg(_bonusCode, 0);
            List<BingoReelIndexArg> topPrizes = new List<BingoReelIndexArg>();
            topPrizes.Add(topPrize);
            return topPrizes;
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
