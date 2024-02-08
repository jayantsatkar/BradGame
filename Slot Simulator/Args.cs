using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using System.IO;
using System.Threading;

namespace Slot_Simulator
{
    enum ReelType { PG, PG1, PG2, FG, Alt, FG5, FG6, FG7, FG8 };
    enum GameAction { Spin, ShowWinsInBetweenGames, Animate, End, Wait, HoldAndSpin };
    class WildCanBeArg
    {
        internal Dictionary<int, int> Wilds;
        internal List<int> CanBe;
        internal WildCanBeArg(Dictionary<int, int> _wilds, List<int> _canBe)
        {
            Wilds = _wilds;
            CanBe = _canBe;
        }
    }
    class PayArgs
    {
        internal string Name;
        internal Dictionary<int, int> CanBe;
        internal List<int> CanBeWithoutMultipliers;
        internal List<int> Pays;
        internal bool IsScatter;
        internal PayArgs(string _name, Dictionary<int, int> _canBe, List<int> _pays, bool _isScatter)
        {
            Name = _name;
            CanBe = _canBe;
            Pays = _pays;
            IsScatter = _isScatter;
            CanBeWithoutMultipliers = new List<int>(CanBe.Keys);
        }
        public override string ToString()
        {
            StringBuilder label = new StringBuilder();
            label.Append(string.Format("{0}: ",Name));
            foreach(int pay in Pays)
                if(pay > 0)
                    label.Append(string.Format("{0} , ", pay));
            if (IsScatter)
                label.Append("IsScatter");
            return label.ToString();
        }
    }
    class WinArgs
    {
        internal string Name;
        internal List<List<int>> CellNums;
        internal int Amount, Count, Multiplier, SymbolIndex;
        internal bool IsScatter;
        internal WinArgs(string _name, int _symbolIndex, int _amount, int _count, int _multiplier, List<List<int>> _cellNums, bool _isScatter = false)
        {
            Name = _name;
            SymbolIndex = _symbolIndex;
            Amount = _amount;
            Count = _count;
            Multiplier = _multiplier;
            CellNums = _cellNums;
            IsScatter = _isScatter;
        }
        internal WinArgs(string _name, int _amount)
        {
            Name = _name;
            Amount = _amount;
            SymbolIndex = -1;
            Count = 1;
            CellNums = null;
            IsScatter = false;
        }
        public override string ToString()
        {
            return string.Format("{0} {1} - {2} Credits", Count, Name, Amount);
        }
    }
    class PayCountArg
    {
        internal List<int> Pays;
        internal List<List<ulong>> WinAmounts;
        internal List<ulong> Hits;
        internal PayCountArg(List<int> _pays, int _categoryCount)
        {
            Pays = _pays;
            WinAmounts = new List<List<ulong>>();
            Hits = new List<ulong>();
            for (int i = 0; i < Pays.Count; i++)
            {
                WinAmounts.Add(m.MakeNewList<ulong>(_categoryCount, 0));
                Hits.Add(0);
            }
        }
    }
    class RandomObjectArg<T>
    {
        internal List<T> Prizes;
        internal List<int> Cutoffs;
        internal List<double> CutoffsDouble;
        internal int CurrentIndex, TotalWeight;
        internal double TotalWeightDouble;
        internal T CurrentPrize;
        internal Dictionary<T, int> PrizeToWeight;
        internal Dictionary<T, double> PrizeToWeightDouble;
        internal int PrizeNum { get { return Prizes.Count; } }
        internal RandomObjectArg(List<T> _prizes, List<int> _cutoffs)
        {
            Prizes = _prizes;
            Cutoffs = _cutoffs;
            RefreshRandomPrize();
            PrizeToWeight = new Dictionary<T, int>();
            for (int i = 0; i < Prizes.Count; i++)
                PrizeToWeight[Prizes[i]] = i == 0 ? Cutoffs[i] : Cutoffs[i] - Cutoffs[i - 1];
            TotalWeight = Cutoffs[Cutoffs.Count - 1];
        }
        internal RandomObjectArg(List<T> _prizes, List<double> _cutoffs)
        {
            Prizes = _prizes;
            CutoffsDouble = _cutoffs;
            RefreshRandomPrize();
            PrizeToWeightDouble = new Dictionary<T, double>();
            for (int i = 0; i < Prizes.Count; i++)
                PrizeToWeightDouble[Prizes[i]] = i == 0 ? CutoffsDouble[i] : CutoffsDouble[i] - CutoffsDouble[i - 1];
            TotalWeightDouble = CutoffsDouble[CutoffsDouble.Count - 1];
        }
        private List<int> m_normalizationFactors = null;
        private List<int> m_normalizationCounts, m_m_normalizationCountsBackup;
        internal RandomObjectArg(List<T> _prizes, List<int> _weights, List<int> _normalizationFactors)
        {
            for (int i = 0; i < _weights.Count; i++)
                _weights[i] *= _normalizationFactors[i];
            Prizes = _prizes;
            Cutoffs = m.MakeCutoffs(_weights);
            RefreshRandomPrize();
            PrizeToWeight = new Dictionary<T, int>();
            for (int i = 0; i < Prizes.Count; i++)
                PrizeToWeight[Prizes[i]] = i == 0 ? Cutoffs[i] : Cutoffs[i] - Cutoffs[i - 1];
            TotalWeight = Cutoffs[Cutoffs.Count - 1];
            m_normalizationFactors = _normalizationFactors;
            m_normalizationCounts = m.MakeNewList<int>(Prizes.Count, 0);
        }
        internal RandomObjectArg(List<T> _prizes, List<int> _cutoffs, int _normalizationFactor)
        {
            Prizes = _prizes;
            Cutoffs = _cutoffs;
            RefreshRandomPrize();
            PrizeToWeight = new Dictionary<T, int>();
            for (int i = 0; i < Prizes.Count; i++)
                PrizeToWeight[Prizes[i]] = i == 0 ? Cutoffs[i] : Cutoffs[i] - Cutoffs[i - 1];
            TotalWeight = Cutoffs[Cutoffs.Count - 1];
            m_normalizationFactors = m.MakeNewList<int>(Cutoffs.Count, _normalizationFactor);
            m_normalizationCounts = m.MakeNewList<int>(Prizes.Count, 0);
        }
        internal void RefreshRandomPrize()
        {
            if (m_normalizationFactors != null)
            {
                while (true)
                {
                    int index = Cutoffs == null ? m.RandomIndex(CutoffsDouble) : m.RandomIndex(Cutoffs);
                    m_normalizationCounts[index]++;
                    if (m_normalizationCounts[index] >= m_normalizationFactors[index])
                    {
                        CurrentIndex = index;
                        CurrentPrize = Prizes[CurrentIndex];
                        m_normalizationCounts[index] = 0;
                        break;
                    }
                }
            }
            else
            {
                CurrentIndex = Cutoffs == null ? m.RandomIndex(CutoffsDouble) : m.RandomIndex(Cutoffs);
                CurrentPrize = Prizes[CurrentIndex];
            }
        }
        internal void RefreshPsuedoRandomPrize(bool _incrementIndex = true)
        {
            if (m_normalizationFactors != null)
            {
                while (true)
                {
                    int index = m.RandomIndex(Cutoffs, true);
                    m_normalizationCounts[index]++;
                    if (m_normalizationCounts[index] >= m_normalizationFactors[index])
                    {
                        CurrentIndex = index;
                        CurrentPrize = Prizes[CurrentIndex];
                        m_normalizationCounts[index] = 0;
                        break;
                    }
                }
            }
            else
            {
                CurrentIndex = m.RandomIndex(Cutoffs, true, _incrementIndex);
                CurrentPrize = Prizes[CurrentIndex];
            }
        }
        internal void SetPrize(int _prizeIndex)
        {
            CurrentIndex = _prizeIndex;
            CurrentPrize = Prizes[CurrentIndex];
        }
        internal T RandomPrize
        {
            get
            {
                RefreshRandomPrize();
                return CurrentPrize;
            }
        }
        internal T PsuedoRandomPrize
        {
            get
            {
                RefreshPsuedoRandomPrize();
                return CurrentPrize;
            }
        }
        internal T PsuedoRandomPrizeWithoutIncrementing
        {
            get
            {
                RefreshPsuedoRandomPrize(false);
                return CurrentPrize;
            }
        }
        internal int RandomIndex
        {
            get
            {
                RefreshRandomPrize();
                return CurrentIndex;
            }
        }
        internal void SaveState()
        {
            m_m_normalizationCountsBackup = new List<int>(m_normalizationCounts);
        }
        internal void ReloadState()
        {
            m_normalizationCounts = m_m_normalizationCountsBackup;
        }
    }
    class FreeGameCounter
    {
        SortedDictionary<int, long> m_freeGameCounts;
        long m_totalBonuses;
        internal FreeGameCounter()
        {
            m_freeGameCounts = new SortedDictionary<int, long>();
            m_totalBonuses = 0;
        }
        internal void StoreFreeGamesForThisPaidGame(int _freeGames)
        {
            if (!m_freeGameCounts.ContainsKey(_freeGames))
                m_freeGameCounts[_freeGames] = 0;
            m_freeGameCounts[_freeGames]++;
            m_totalBonuses++;
        }
        internal void InputResults(List<List<string>> _results)
        {
            _results.Add(new List<string>(new string[] { "Free Games Played", "Count", "Chance", "Chance At or Above" }));
            double chancesLeft = 1;
            foreach (int freeGamesPlayed in m_freeGameCounts.Keys)
            {
                double currentChance = m_freeGameCounts[freeGamesPlayed] / m_totalBonuses;

                _results.Add(new List<string>(new string[] { 
                    freeGamesPlayed.ToString(),
                    m_freeGameCounts[freeGamesPlayed].ToString(),
                    string.Format("{0:0.000000%}",currentChance),
                    string.Format("{0:0.000000%}",chancesLeft),
                }));
                chancesLeft -= currentChance;
            }
            _results.Add(new List<string>());
        }
    }
    class WinDistributionChart
    {
        internal int Bet, HeaderCount, WinCategoryMultipliersCount, WinCategoryCount;
        internal long TotalOutOf;
        internal string Name;
        internal List<string> Headers;
        internal List<double> WinCategoryMultipliers;
        internal List<uint> WinCategoryAmounts;
        internal List<List<ulong>> WinCategoryWinsAmounts, WinCategoryCounts, CategoryOutOf;
        internal WinDistributionChart(string _name, List<string> _headers, int _bet, List<double> _multipliers)
        {
            Name = _name;
            Headers = _headers;
            Bet = _bet;
            WinCategoryMultipliers = _multipliers;
            HeaderCount = Headers.Count;
            WinCategoryMultipliersCount = WinCategoryMultipliers.Count;
            WinCategoryCount = WinCategoryMultipliersCount + 1;
            WinCategoryAmounts = m.MultipliersToCredits(WinCategoryMultipliers, Bet);
            WinCategoryCounts = new List<List<ulong>>();
            WinCategoryWinsAmounts = new List<List<ulong>>();
            CategoryOutOf = new List<List<ulong>>();
            for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
            {
                WinCategoryCounts.Add(m.MakeNewList<ulong>(WinCategoryAmounts.Count + 1, 0));
                WinCategoryWinsAmounts.Add(m.MakeNewList<ulong>(WinCategoryAmounts.Count + 1, 0));
                CategoryOutOf.Add(m.MakeNewList<ulong>(WinCategoryAmounts.Count + 1, 0));
            }
            TotalOutOf = 0;
        }
        internal void IncGames() { TotalOutOf++; }
        internal void StoreGame(int _winAmount, int _headerIndex = 0)
        {
            int category = 0;
            if (_winAmount > 0)
                for (category = 1; category < WinCategoryMultipliersCount; category++)
                    if (_winAmount < WinCategoryAmounts[category]) break;
            WinCategoryCounts[_headerIndex][category]++;
            WinCategoryWinsAmounts[_headerIndex][category] += (ulong)_winAmount;
        }
        internal void InputResults(List<List<string>> _results)
        {
            //Headers
            List<string> headers = new List<string>();
            headers.Add(Name);
            for (int i = 0; i < 5; i++)
                foreach (string header in Headers)
                    headers.Add(header);
            _results.Add(headers);
            //
            List<List<ulong>> winCategoryCountsAbove = new List<List<ulong>>();
            for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
            {
                List<ulong> winCategoryCountsAboveSub = m.MakeNewList<ulong>(WinCategoryCount, 0);
                for (int category = 0; category < WinCategoryCount; category++)
                    for (int j = 0; j <= category; j++)
                        winCategoryCountsAboveSub[j] += WinCategoryCounts[headerNum][category];
                winCategoryCountsAbove.Add(winCategoryCountsAboveSub);
            }
            List<double> totalWins = m.MakeNewList<double>(HeaderCount, 0);
            List<ulong> totalCounts = m.MakeNewList<ulong>(HeaderCount, 0);
            for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
                for (int category = 0; category < WinCategoryCount; category++)
                {
                    totalWins[headerNum] += WinCategoryWinsAmounts[headerNum][category];
                    totalCounts[headerNum] += WinCategoryCounts[headerNum][category];
                }
            //Each Win
            List<string> categoryLabels = m.MultipliersToLabels(WinCategoryMultipliers);
            for (int category = 0; category < WinCategoryCount; category++)
            {
                List<string> row = new List<string>(new string[] { categoryLabels[category] });
                //At
                for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
                    row.Add(string.Format("{0:0.0000%}", (double)WinCategoryCounts[headerNum][category] / totalCounts[headerNum]));
                //Above
                for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
                    row.Add(string.Format("{0:0.0000%}", (double)winCategoryCountsAbove[headerNum][category] / totalCounts[headerNum]));
                //Distribution
                for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
                    row.Add(string.Format("{0:0.0000%}", (double)WinCategoryWinsAmounts[headerNum][category] / totalCounts[headerNum] / Bet / ((double)TotalOutOf / totalCounts[headerNum])));
                //Count
                for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
                    row.Add(WinCategoryCounts[headerNum][category].ToString());
                //Odds
                for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
                {
                    if(winCategoryCountsAbove[headerNum][category] != 0) row.Add(string.Format("{0:0.0000}", (decimal)totalCounts[headerNum] / winCategoryCountsAbove[headerNum][category]));
                    else if (winCategoryCountsAbove[headerNum][category] == 0) row.Add("0.0000");
                }
                _results.Add(row);
            }
            //RTP
            List<string> rtps = new List<string>(new string[] { "RTP" });
            for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
                rtps.Add(string.Format("{0:0.0000%}", (double)totalWins[headerNum] / totalCounts[headerNum] / Bet));
            _results.Add(rtps);
            //Cycle
            List<string> cycles = new List<string>(new string[] { "Cycle" });
            for (int headerNum = 0; headerNum < HeaderCount; headerNum++)
                cycles.Add(string.Format("{0:0.0000}", (double)TotalOutOf/totalCounts[headerNum]));
            _results.Add(cycles);
            _results.Add(new List<string>());
        }
    }
    public class ProgressiveData
    {
        internal string Name;
        internal int Reset;
        double m_increment;
        internal double m_currentValue;
        internal ulong m_timesHit;
        internal int CurrentValue { get { return (int)m_currentValue; } }
        internal ProgressiveData(string _name, int _reset, double _increment)
        {
            Name = _name;
            Reset = _reset;
            m_increment = _increment;
            m_currentValue = _reset;
            m_timesHit = 0;
        }
        internal void Increment(long _coinIn)
        {
            m_currentValue += _coinIn * m_increment;
        }
        internal bool IncrementShamrock(long _coinIn, int _progLevel, int _betLevel)
        {
            bool valueToReturn = false;
            if (_progLevel == 0)
            {
                List<int> MegaBoost = new List<int> { 5993, 5997, 6000 };
                List<List<int>> MegaBoostTable1 = new List<List<int>> { new List<int> { 90, 120, 150 }, new List<int> { 13, 14, 15 } };
                List<List<int>> MegaBoostTable2 = new List<List<int>> { new List<int> { 105, 150, 225, 300 }, new List<int> { 12, 13, 14, 15 } };
                int randomBoost = m.RandomInteger(MegaBoost[2]);
                for(int i = 0; i < 3; i++)
                {
                    if (randomBoost < MegaBoost[i])
                    {
                        if (i == 0) { m_currentValue += _coinIn * m_increment; valueToReturn = false; return valueToReturn; }
                        if (i == 1)
                        {
                            int incrementBoost = m.RandomInteger(10);
                            for (int j = 0; j < 3; j++)
                            {
                                if (incrementBoost < MegaBoostTable1[1][j]) { m_currentValue += MegaBoostTable1[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                        if (i == 2)
                        {
                            int incrementBoost = m.RandomInteger(13);
                            for (int j = 0; j < 4; j++)
                            {
                                if (incrementBoost < MegaBoostTable2[1][j]) { m_currentValue += MegaBoostTable2[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }  
                    }
                }
            }
            if (_progLevel == 1)
            {
                List<int> MajorBoost = new List<int> { 2562, 2566, 2569, 2570 };
                List<List<int>> MajorBoostTable1 = new List<List<int>> { new List<int> { 30,45,60,75}, new List<int> { 2,4,6,7 } };
                List<List<int>> MajorBoostTable2 = new List<List<int>> { new List<int> { 45,60,75,90,105,120,150 }, new List<int> { 7,14,19,21,22,23,24 } };
                List<List<int>> MajorBoostTable3 = new List<List<int>> { new List<int> { 75,90,105,120,135,150,225 }, new List<int> { 5,7,8,10,11,12,13 } };
                int randomBoost = m.RandomInteger(MajorBoost[3]);
                for (int i = 0; i < 4; i++)
                {
                    if (randomBoost < MajorBoost[i])
                    {
                        if (i == 0) { m_currentValue += _coinIn * m_increment; valueToReturn = false; return valueToReturn; }
                        if (i == 1)
                        {
                            int incrementBoost = m.RandomInteger(7);
                            for (int j = 0; j < 4; j++)
                            {
                                if (incrementBoost < MajorBoostTable1[1][j]) { m_currentValue += MajorBoostTable1[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                        if (i == 2)
                        {
                            int incrementBoost = m.RandomInteger(14);
                            for (int j = 0; j < 7; j++)
                            {
                                if (incrementBoost < MajorBoostTable2[1][j]) { m_currentValue += MajorBoostTable2[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                        if (i == 3)
                        {
                            int incrementBoost = m.RandomInteger(13);
                            for (int j = 0; j < 7; j++)
                            {
                                if (incrementBoost < MajorBoostTable3[1][j]) { m_currentValue += MajorBoostTable3[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                    }
                }
            }
            if (_progLevel == 2)
            {
                List<int> MinorBoost = new List<int> { 2102, 2107, 2109, 2110 };
                List<List<int>> MinorBoostTable1 = new List<List<int>> { new List<int> { 15, 30, 45, 60, 75,90 }, new List<int> { 3,5,8,10,11,12 } };
                List<List<int>> MinorBoostTable2 = new List<List<int>> { new List<int> { 30,60,75,90,105 }, new List<int> { 4,7,9,11,12 } };
                List<List<int>> MinorBoostTable3 = new List<List<int>> { new List<int> { 60, 75, 90, 105, 120, 135, 150, 180 }, new List<int> { 4,7,10,12,13,14,15,16 } };
                int randomBoost = m.RandomInteger(MinorBoost[3]);
                for (int i = 0; i < 4; i++)
                {
                    if (randomBoost < MinorBoost[i])
                    {
                        if (i == 0) { m_currentValue += _coinIn * m_increment; valueToReturn = false; return valueToReturn; }
                        if (i == 1)
                        {
                            int incrementBoost = m.RandomInteger(13);
                            for (int j = 0; j < 6; j++)
                            {
                                if (incrementBoost < MinorBoostTable1[1][j]) { m_currentValue += MinorBoostTable1[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                        if (i == 2)
                        {
                            int incrementBoost = m.RandomInteger(12);
                            for (int j = 0; j < 5; j++)
                            {
                                if (incrementBoost < MinorBoostTable2[1][j]) { m_currentValue += MinorBoostTable2[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                        if (i == 3)
                        {
                            int incrementBoost = m.RandomInteger(16);
                            for (int j = 0; j < 8; j++)
                            {
                                if (incrementBoost < MinorBoostTable3[1][j]) { m_currentValue += MinorBoostTable3[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                    }
                }
            }
            if (_progLevel == 3)
            {
                List<int> MiniBoost = new List<int> { 1226,1235,1239,1240 };
                List<List<int>> MiniBoostTable1 = new List<List<int>> { new List<int> { 15, 30}, new List<int> { 1, 2 } };
                List<List<int>> MiniBoostTable2 = new List<List<int>> { new List<int> { 15, 30, 45, 60, 75}, new List<int> { 2,4,5,6,7 } };
                List<List<int>> MiniBoostTable3 = new List<List<int>> { new List<int> { 60,90,150 }, new List<int> { 2,3,4 } };
                int randomBoost = m.RandomInteger(MiniBoost[3]);
                for (int i = 0; i < 4; i++)
                {
                    if (randomBoost < MiniBoost[i])
                    {
                        if (i == 0) { m_currentValue += _coinIn * m_increment; valueToReturn = false; return valueToReturn; }
                        if (i == 1)
                        {
                            int incrementBoost = m.RandomInteger(20);
                            for (int j = 0; j < 2; j++)
                            {
                                if (incrementBoost < MiniBoostTable1[1][j]) { m_currentValue += MiniBoostTable1[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                        if (i == 2)
                        {
                            int incrementBoost = m.RandomInteger(6);
                            for (int j = 0; j < 5; j++)
                            {
                                if (incrementBoost < MiniBoostTable2[1][j]) { m_currentValue += MiniBoostTable2[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                        if (i == 3)
                        {
                            int incrementBoost = m.RandomInteger(4);
                            for (int j = 0; j < 3; j++)
                            {
                                if (incrementBoost < MiniBoostTable3[1][j]) { m_currentValue += MiniBoostTable3[0][j] * _betLevel; valueToReturn = true; return valueToReturn; }
                            }
                        }
                    }
                }
            }
            return valueToReturn;
        }
        internal int GetProgressiveAndReset()
        {
            int prog = (int)m_currentValue;
            m_currentValue = Reset + (m_currentValue - prog);
            m_timesHit++;
            return prog;
        }
        internal void getMustHitProgressiveandReset(double currentAward)
        {
            m_currentValue = Reset + (m_currentValue - currentAward);
            m_timesHit++;
        }
    }
    class BatchReportProgressArg
    {
        internal FileInfo FileInfo;
        internal object Param;
        internal BatchReportProgressArg(FileInfo _fileInfo, object _param)
        {
            FileInfo = _fileInfo;
            Param = _param;
        }
    }
    public class pickSciptData
    {
        internal List<List<string>> m_scripts;
        internal List<int> m_scriptCutOffs;
        internal pickSciptData(List<List<string>> _scripts, List<int> _scriptCutOffs)
        {
            m_scripts = _scripts;
            m_scriptCutOffs = _scriptCutOffs;
        }
        internal int getScript()
        {
            return m.RandomIndex(m_scriptCutOffs);
        }
    }
    class WinStatsInfo
    {
        internal SortedDictionary<int, ulong> m_wins;
        internal int maxPay;
        internal int bet;
        internal double average, variance, median, firstQuart, thirdQuart, ninthDec;
        string name;
        ulong currentGames;
        bool includeZeroWins;
        internal WinStatsInfo(string _name, int _bet, bool _includeZero)
        {
            name = _name;
            bet = _bet;
            includeZeroWins = _includeZero;
            m_wins = new SortedDictionary<int, ulong>();
            average = 0;
            variance = 0;
            maxPay = 0;
            median = 0;
            firstQuart = 0;
            thirdQuart = 0;
            ninthDec = 0;
            currentGames = 0;
        }
        internal void storeWin(int _winAmount)
        {
            if (m_wins.Keys.Contains(_winAmount)) m_wins[_winAmount]++;
            else if (!m_wins.Keys.Contains(_winAmount)) m_wins[_winAmount] = 1;
            if (_winAmount > maxPay) maxPay = _winAmount;
            currentGames++;
        }
        internal void InputResults(List<List<string>> _results)
        {
            getStats();
            _results.Add(new List<string>(new string[] { "", name}));
            _results.Add(new List<string>(new string[] { "Average Win (Credits)", string.Format("{0:0.00}", average)}));
            _results.Add(new List<string>(new string[] { "Average Win (Times Bet)", string.Format("{0:0.00}", average / (double)bet) }));
            _results.Add(new List<string>(new string[] { "Variance", string.Format("{0:0.00}", variance) }));
            _results.Add(new List<string>(new string[] { "Volatility", string.Format("{0:0.00}", 1.645 * Math.Sqrt(variance)) }));
            _results.Add(new List<string>(new string[] { "Max Pay (Credits)", string.Format("{0 : 0}", maxPay) }));
            _results.Add(new List<string>(new string[] { "Max Pay (Times Bet)", string.Format("{0:0.00}", (double)(maxPay / bet)) }));
            _results.Add(new List<string>(new string[] { "Median (Credits)", string.Format("{0:0}", median) }));
            _results.Add(new List<string>(new string[] { "Median (Times Bet)", string.Format("{0:0.00}", median / (double) bet) }));
            _results.Add(new List<string>(new string[] { "1st Quartile (Credits)", string.Format("{0:0}", firstQuart) }));
            _results.Add(new List<string>(new string[] { "1st Quartile (Times Bet)", string.Format("{0:0.00}", firstQuart / (double)bet) }));
            _results.Add(new List<string>(new string[] { "3rd Quartile (Credits)", string.Format("{0:0}", thirdQuart) }));
            _results.Add(new List<string>(new string[] { "3rd Quartile (Times Bet)", string.Format("{0:0.00}", thirdQuart / (double)bet) }));
            _results.Add(new List<string>(new string[] { "9th Decile (Credits)", string.Format("{0:0}", ninthDec) }));
            _results.Add(new List<string>(new string[] { "9th Decile (Times Bet)", string.Format("{0:0.00}", ninthDec / (double)bet) }));
            _results.Add(new List<string>(new string[] { "9th Decile Range (Credits)", string.Format("{0:0}x - {1:0}", ninthDec, maxPay) }));
            _results.Add(new List<string>(new string[] { "9th Decile Range (Times Bet)", string.Format("{0:0.00}x - {1:0.00}x", ninthDec / (double)bet, maxPay / (double)bet) }));
            _results.Add(new List<string>(new string[] { "Median to Mean", string.Format("{0:0.00}", median / average) }));
            _results.Add(new List<string>());
        }
        internal void getStats()
        {
            bool firstQuartFound = false;
            bool secondQuartFound = false;
            bool thirdQuartFound = false;
            bool ninthDecileFound = false;
            ulong hitsForAverage = 0;
            ulong hitsForVariance = 0;
            ulong countForDeciles = 0;
            double averageToReturn = 0;
            double varianceToReturn = 0;
            foreach(int win in m_wins.Keys)
            {
                averageToReturn += (win) * ((double)m_wins[win] / (double)currentGames);
                if (win == 0 && includeZeroWins) hitsForAverage += m_wins[win];
                else if (win > 0) hitsForAverage += m_wins[win];
                if (includeZeroWins || !m_wins.Keys.Contains(0)) countForDeciles = currentGames;
                else if (!includeZeroWins && m_wins.Keys.Contains(0)) countForDeciles = currentGames - m_wins[0];
                if(hitsForAverage >= (double)(countForDeciles / 4) && !firstQuartFound)
                {
                    firstQuart = win;
                    firstQuartFound = true;
                }
                if (hitsForAverage >= (double)(countForDeciles / 2) && !secondQuartFound)
                {
                    median = win;
                    secondQuartFound = true;
                }
                if (hitsForAverage >= ((double)(countForDeciles / 4) * 3) && !thirdQuartFound)
                {
                    thirdQuart = win;
                    thirdQuartFound = true;
                }
                if (hitsForAverage >= ((double)(countForDeciles / 10) * 9) && !ninthDecileFound)
                {
                    ninthDec = win;
                    ninthDecileFound = true;
                }
            }
            if (includeZeroWins || !m_wins.Keys.Contains(0)) hitsForVariance = currentGames;
            else if (!includeZeroWins && m_wins.Keys.Contains(0)) hitsForVariance = currentGames - m_wins[0];
            foreach(int win in m_wins.Keys)
            {
                varianceToReturn += ((win - averageToReturn) / bet * (win - averageToReturn) / bet * (((double)m_wins[win]) / (double)hitsForVariance));
            }
            average = averageToReturn;
            variance = varianceToReturn;
        }
    }
    class RecoveryMatrix
    {
        internal SortedDictionary<int, ulong> bankrollGames;
        internal List<SortedDictionary<int, ulong>> bankrollRecoveries;
        int bet;
        internal List<double> bankrollBreakdowns;
        internal RecoveryMatrix(int _bet, List<int> _startingBankRolls, List<double> _bankrollDivisions)
        {
            bet = _bet;
            bankrollGames = new SortedDictionary<int, ulong>();
            bankrollRecoveries = new List<SortedDictionary<int, ulong>>();
            bankrollBreakdowns = _bankrollDivisions;
            for (int aa = 0; aa < _bankrollDivisions.Count(); aa++)
            {
                bankrollRecoveries.Add(new SortedDictionary<int, ulong>());
            }
            for (int ab = 0; ab < _startingBankRolls.Count(); ab++)
            {
                bankrollGames.Add(_startingBankRolls[ab], 0);
                for(int ac = 0; ac < bankrollBreakdowns.Count(); ac++)
                {
                    bankrollRecoveries[ac].Add(_startingBankRolls[ab], 0);
                }
            }
        }
        internal void InputResults(List<List<string>> _results)
        {
            List<string> row = new List<string>();
            row.Add("Recovery Probabilities");
            foreach(int bankroll in bankrollGames.Keys)
            {
                row.Add(string.Format("{0}x", bankroll));
            }
            _results.Add(row);
            for(int aa = 0; aa < bankrollBreakdowns.Count(); aa++)
            {
                row = new List<string>();
                row.Add(string.Format("{0:0.00%}", bankrollBreakdowns[aa]));
                foreach(int bankroll in bankrollRecoveries[aa].Keys)
                {
                    if(bankrollRecoveries[aa][bankroll] == 0) row.Add("0.0000%");
                    else if(bankrollRecoveries[aa][bankroll] > 0) row.Add(string.Format("{0:0.0000%}", (double)(bankrollRecoveries[aa][bankroll] / bankrollGames[bankroll])));
                }
                _results.Add(row);
            }
            _results.Add(new List<string>());
        }
    }
    class SurvivabilityMatrix
    {
        internal SortedDictionary<int, ulong> spinSessions;
        internal List<SortedDictionary<int, ulong>> spinDistances;
        internal List<int> spinCounts;
        internal SurvivabilityMatrix(List<int> _spinCounts)
        {
            spinCounts = _spinCounts;
            spinSessions = new SortedDictionary<int, ulong>();
            spinDistances = new List<SortedDictionary<int, ulong>>();
            for(int aa = 0; aa < spinCounts.Count(); aa++)
            {
                spinSessions.Add(_spinCounts[aa], 0);
                spinDistances.Add(new SortedDictionary<int, ulong>());
                for(int ab = 0; ab < _spinCounts.Count(); ab++)
                {
                    spinDistances[aa].Add(_spinCounts[ab], 0);
                }
            }
        }
        internal void InputResults(List<List<string>> _results)
        {
            List<string> row = new List<string>();
            row.Add("Survivability Probabilities");
            foreach(int spinCount in spinSessions.Keys)
            {
                row.Add(spinCount.ToString());
            }
            _results.Add(row);
            for(int aa = 0; aa < spinCounts.Count(); aa++)
            {
                row = new List<string>();
                row.Add(string.Format("{0}", spinCounts[aa]));
                foreach(int spinCount in spinDistances[aa].Keys)
                {
                    if (spinSessions[spinCount] == 0) row.Add("0.0000%");
                    else if (spinSessions[spinCount] > 0) row.Add(string.Format("{0:0.0000%}", (double)spinDistances[aa][spinCount] / (double)spinSessions[spinCount]));
                }
                _results.Add(row);
            }
            _results.Add(new List<string>());
        }
    }
    class SetsStats
    {
        internal ulong totalWinThisSet, totalCashout, spinsWithWins, gamesWithin5Percent, sessionsWithBonuses, sessionsWithBonusesorProg, sessionsRecovered, sessionsBelowRecoveryLevel, sessionsPlayed;
        internal List<int> spinsThisSet;
        internal int maxPayForSet, cashouts, cashoutsW0, cashoutsW1, cashoutsW2, bet, startingBankroll;
        internal List<double> spinChancesThisSet;
        internal List<int> spinBottomRanges;
        internal double averageWinThisSet, averageSpins, medianSpins, rTP, winningSessions, cashoutsW0Bonuses, cashoutsW1Bonus, cashoutW2Bonuses, averageCashoutAmount, fivePercentFailures, 
                        sessionsWBonus, sessionsWBonusesorProg, setRecoveryChance, setSpinVolatility, setSpinMedianToMean;
        internal SetsStats(int _bet, int _startingBankroll)
        {
            totalWinThisSet = 0;
            totalCashout = 0;
            spinsWithWins = 0;
            gamesWithin5Percent = 0;
            sessionsWithBonuses = 0;
            sessionsWithBonusesorProg = 0;
            sessionsRecovered = 0;
            sessionsBelowRecoveryLevel = 0;
            spinsThisSet = new List<int>();
            maxPayForSet = 0;
            cashouts = 0;
            cashoutsW0 = 0;
            cashoutsW1 = 0;
            cashoutsW2 = 0;
            averageWinThisSet = 0;
            averageSpins = 0;
            medianSpins = 0;
            rTP = 0;
            winningSessions = 0;
            cashoutsW0Bonuses = 0;
            cashoutsW1Bonus = 0;
            cashoutW2Bonuses = 0;
            averageCashoutAmount = 0;
            fivePercentFailures = 0;
            sessionsWBonus = 0;
            sessionsWBonusesorProg = 0;
            setRecoveryChance = 0;
            setSpinVolatility = 0;
            setSpinMedianToMean = 0;
            bet = _bet;
            sessionsPlayed = 0;
            startingBankroll = _startingBankroll;
            spinChancesThisSet = new List<double>();
            spinBottomRanges = new List<int>();
            for(int aa = 0; aa < m.spinMultipliers.Count(); aa++)
            {
                spinBottomRanges.Add(m.spinMultipliers[aa] * (startingBankroll / bet));
                spinChancesThisSet.Add(0);
            }
        }
        internal void processSession(SessionStats _session)
        {
            if(_session.currentCredits >= _session.cashoutAmount)
            {
                totalCashout += (ulong)_session.currentCredits;
                cashouts++;
                if (_session.bonuses == 0) cashoutsW0++;
                if (_session.bonuses == 1) cashoutsW1++;
                if (_session.bonuses >= 2) cashoutsW2++;
                if (_session.within5Percent) _session.within5Percent = false;
            }
            totalWinThisSet += _session.totalWinThisSession;
            spinsWithWins += _session.spinsWWins;
            maxPayForSet = Math.Max(maxPayForSet, _session.maxPay);
            spinsThisSet.Add(_session.numberOfSpins);
            if (_session.within5Percent) gamesWithin5Percent++;
            if (_session.inRecovery) sessionsBelowRecoveryLevel++;
            if (_session.recoveredStartingAmount) sessionsRecovered++;
            if (_session.bonuses > 0) sessionsWithBonuses++;
            if (_session.bonusesWProg > 0) sessionsWithBonusesorProg++;
            sessionsPlayed++;
        }
        internal void processSet()
        {
            List<ulong> spinChanceCountsThisSet = new List<ulong>();
            for (int aa = 0; aa < spinBottomRanges.Count(); aa++)
            {
                spinChanceCountsThisSet.Add(0);
            }
            foreach (int spinCount in spinsThisSet)
            {
                for (int aa = 0; aa < spinBottomRanges.Count(); aa++)
                {
                    if (aa < spinBottomRanges.Count() - 1)
                    {
                        if (spinCount < spinBottomRanges[aa + 1] && spinCount >= spinBottomRanges[aa])
                        {
                            spinChanceCountsThisSet[aa]++;
                        }
                    }
                    else if (aa == spinBottomRanges.Count() - 1)
                    {
                        if (spinCount >= spinBottomRanges[aa]) spinChanceCountsThisSet[aa]++;
                    }
                }
            }
            if (spinsThisSet.Count() > 0)
            {
                averageSpins = spinsThisSet.Average();
                spinsThisSet.Sort();
                if (spinsThisSet.Count() % 2 == 0) medianSpins = (spinsThisSet[spinsThisSet.Count() / 2] + spinsThisSet[(spinsThisSet.Count() / 2) - 1]) / 2;
                else if (spinsThisSet.Count() % 2 == 1)
                {
                    double midPoint = (spinsThisSet.Count() / 2) - 0.5;
                    int actualmid = (int)midPoint;
                    medianSpins = (double)spinsThisSet[actualmid];
                }
                double spinVariance = 0;
                for(int ad = 0; ad < spinsThisSet.Count(); ad++)
                {
                    spinVariance += ((spinsThisSet[ad] - averageSpins) * (spinsThisSet[ad] - averageSpins)) / spinsThisSet.Count();
                }
                setSpinVolatility = 1.645 * Math.Sqrt(spinVariance);
            }
            averageWinThisSet = (double)totalWinThisSet / (double)spinsWithWins;
            ulong spinTotal = (ulong)spinsThisSet.Sum();
            rTP = (double)totalWinThisSet / (double)(spinTotal * (ulong)bet);
            winningSessions = (double)cashouts / (double)sessionsPlayed;
            cashoutsW0Bonuses = (double)cashoutsW0 / (double)cashouts;
            cashoutsW1Bonus = (double)cashoutsW1 / (double)cashouts;
            cashoutW2Bonuses = (double)cashoutsW2 / (double)cashouts;
            averageCashoutAmount = (double)totalCashout / (double)cashouts;
            fivePercentFailures = (double)gamesWithin5Percent / (double)sessionsPlayed;
            sessionsWBonus = (double)sessionsWithBonuses / (double)sessionsPlayed;
            sessionsWBonusesorProg = (double)sessionsWithBonusesorProg / (double)sessionsPlayed;
            setRecoveryChance = (double)sessionsRecovered / (double)sessionsBelowRecoveryLevel;
            setSpinMedianToMean = medianSpins / averageSpins;
            for(int aa = 0; aa < spinChanceCountsThisSet.Count(); aa++)
            {
                if (sessionsPlayed > 0) spinChancesThisSet[aa] = (double)(spinChanceCountsThisSet[aa] / sessionsPlayed);
                else spinChancesThisSet[aa] = 0;
            }
        }
    }
    class SessionStats
    {
        internal int startingBankroll, currentCredits, numberOfSpins, cashoutAmount, cashoutSpin, bonuses, bonusesWProg, maxPay, bet;
        internal double recoveryLevel;
        internal ulong totalWinThisSession, spinsWWins;
        internal bool within5Percent, inRecovery, recoveredStartingAmount;
        internal SessionStats(int _startingBankroll, int _bet, double _cashoutLevel = 2.5, double _recoveryLevel = 0.5)
        {
            startingBankroll = _startingBankroll;
            bet = _bet;
            currentCredits = _startingBankroll;
            numberOfSpins = 0;
            cashoutAmount = (int)(_cashoutLevel * (double)startingBankroll);
            cashoutSpin = 0;
            bonuses = 0;
            bonusesWProg = 0;
            maxPay = 0;
            within5Percent = false;
            inRecovery = false;
            recoveredStartingAmount = false;
            recoveryLevel = _recoveryLevel * (double) startingBankroll;
        }
        internal void placeBet()
        {
            currentCredits -= bet;
            numberOfSpins++;
        }
        internal void processWin(int winAmount, bool _freeGames, bool _progressive)
        {
            if(winAmount > 0)
            {
                currentCredits += winAmount;
                if(_freeGames)
                {
                    bonuses++;
                    bonusesWProg++;
                }
                if (_progressive) bonusesWProg++;
                maxPay = Math.Max(maxPay, winAmount);
                totalWinThisSession += (ulong)winAmount;
                spinsWWins++;
            }
            if (currentCredits >= cashoutAmount) cashoutSpin = numberOfSpins;
            if (currentCredits >= ((double)cashoutAmount * 0.95)) within5Percent = true;
            if (currentCredits <= recoveryLevel && !recoveredStartingAmount) inRecovery = true;
            if (currentCredits >= startingBankroll && inRecovery && !recoveredStartingAmount) recoveredStartingAmount = true;
        }
    }
    class setDistribution
    {
        internal List<SetsStats> setsRecorded;
        internal List<int> superLargeSpinCountList;
        internal int startingBankroll, setsCount, bet;
        internal bool calcsDone;
        internal ulong totalSessionsPlayed;
        internal double rTP, maxPay, averageSpins, medianSpins, averageCashoutAmount, averageWinAmount, winningSessions, cashoutsW0Bonuses, cashoutsW1Bonus, cashoutW2Bonuses, 
                        fivePercentFauilures, sessionsWBonuses, sessionsWBonusesorProgs, recoveryChance, spinVolatility, medianToMeanSpins;
        internal List<string> headers;
        internal List<int> spinBottomRanges;
        internal List<double> spinChances;
        
        internal setDistribution(int _startingBankroll, int _bet)
        {
            setsRecorded = new List<SetsStats>();
            superLargeSpinCountList = new List<int>();
            rTP = 0;
            maxPay = 0;
            averageSpins = 0;
            medianSpins = 0;
            averageCashoutAmount = 0;
            winningSessions = 0;
            cashoutsW0Bonuses = 0;
            cashoutsW1Bonus = 0;
            cashoutW2Bonuses = 0;
            fivePercentFauilures = 0;
            sessionsWBonuses = 0;
            sessionsWBonusesorProgs = 0;
            recoveryChance = 0;
            averageWinAmount = 0;
            setsCount = 0;
            spinVolatility = 0;
            medianToMeanSpins = 0;
            bet = _bet;
            startingBankroll = _startingBankroll;
            headers = new List<string> { "Set #", "Sessions Played", "Payback %", "Max Win", "Average Spins", "Median Spins", "Median to Mean Spins", "Spin Volatility", "Average Win", "Average Cashout Amount", "% of Winning Sessions", "% of Cashouts w/ 0 Bonuses",
                                         "% of Cashouts w/ 1 Bonus", "% of Cashouts w/2+ Bonuses", "% of 5% of Failures", "% of Sessions w/ 1+ Bonus", "% of Sessions w/ 1+ Bonus or Prog", "% of Sessions That Recovered"};
            spinBottomRanges = new List<int>();
            spinChances = new List<double>();
            for (int aa = 0; aa < m.spinMultipliers.Count(); aa++)
            {
                spinBottomRanges.Add(m.spinMultipliers[aa] * (startingBankroll / bet));
                spinChances.Add(0);
            }
            calcsDone = false;
        }

        internal void addSet(SetsStats _set)
        {
                setsRecorded.Add(_set);
                setsCount++;
        }

        internal void getOverallStatsForSets()
        {
            double totalRTP = 0;
            ulong totalCashoutAmount = 0;
            double winningSessionCount = 0;
            double cashoutsW0BonusesCount = 0;
            double cashoutsW1BonusesCount = 0;
            double cashoutsW2BonusesCount = 0;
            double fivePercentFailuresCount = 0;
            double sessionsWBonusesCount = 0;
            double sessionsWBonusesOrProgsCount = 0;
            double recoveryChanceCount = 0;
            double winAmountCount = 0;
            ulong aggregratedSessionCount = 0;
            List<ulong> spinChanceCounts = new List<ulong>();
            spinChances = new List<double>();
            setsCount = 0;
            superLargeSpinCountList = new List<int>();
            for (int aa = 0; aa < spinBottomRanges.Count(); aa++ )
            {
                spinChanceCounts.Add(0);
            }
            setsCount = setsRecorded.Count();
            if(setsRecorded[setsRecorded.Count() - 1].sessionsPlayed != setsRecorded[0].sessionsPlayed && setsCount != 1)
            {
                setsCount = setsRecorded.Count() - 1;
            }
            for(int ab = 0; ab < setsCount; ab++)
            {
                totalRTP += setsRecorded[ab].rTP;
                maxPay = Math.Max(maxPay, setsRecorded[ab].maxPayForSet);
                totalCashoutAmount += (ulong)setsRecorded[ab].averageCashoutAmount;
                winningSessionCount += setsRecorded[ab].winningSessions;
                cashoutsW0BonusesCount += setsRecorded[ab].cashoutsW0Bonuses;
                cashoutsW1BonusesCount += setsRecorded[ab].cashoutsW1Bonus;
                cashoutsW2BonusesCount += setsRecorded[ab].cashoutW2Bonuses;
                fivePercentFailuresCount += setsRecorded[ab].fivePercentFailures;
                sessionsWBonusesCount += setsRecorded[ab].sessionsWBonus;
                sessionsWBonusesOrProgsCount += setsRecorded[ab].sessionsWBonusesorProg;
                recoveryChanceCount += setsRecorded[ab].setRecoveryChance;
                winAmountCount += setsRecorded[ab].averageWinThisSet;
                aggregratedSessionCount += setsRecorded[ab].sessionsPlayed + 1;
                for(int ac = 0; ac < setsRecorded[ab].spinsThisSet.Count(); ac++)
                {
                    superLargeSpinCountList.Add(setsRecorded[ab].spinsThisSet[ac]);
                    bool spinCountRecorded = false;
                    for (int aa = 0; aa < spinBottomRanges.Count(); aa++)
                    {
                        if (aa < spinBottomRanges.Count() - 1)
                        {
                            if (superLargeSpinCountList[ac] < spinBottomRanges[aa] && !spinCountRecorded)
                            {
                                spinChanceCounts[aa]++;
                                spinCountRecorded = true;
                            }
                        }
                        else if (aa == spinBottomRanges.Count() - 1)
                        {
                            if (superLargeSpinCountList[ac] >= spinBottomRanges[aa] && !spinCountRecorded)
                            {
                                spinChanceCounts[aa]++;
                                spinCountRecorded = true;
                            }
                        }
                    }
                }
            }
            rTP = totalRTP / (double)setsCount;
            averageSpins = superLargeSpinCountList.Average();
            double spinVariance = 0;
            for (int ad = 0; ad < superLargeSpinCountList.Count(); ad++)
            {
                spinVariance += ((superLargeSpinCountList[ad] - averageSpins) * (superLargeSpinCountList[ad] - averageSpins)) / superLargeSpinCountList.Count();
            }
            spinVolatility = 1.645 * Math.Sqrt(spinVariance);
            superLargeSpinCountList.Sort();
            if (superLargeSpinCountList.Count() % 2 == 0) medianSpins = (superLargeSpinCountList[superLargeSpinCountList.Count() / 2] + superLargeSpinCountList[(superLargeSpinCountList.Count() / 2) - 1]) / 2;
            else if (superLargeSpinCountList.Count() % 2 == 1)
            {
                double midPoint = (superLargeSpinCountList.Count() / 2) - 0.5;
                int actualMid = (int)midPoint;
                medianSpins = (double)superLargeSpinCountList[actualMid];
            }
            averageCashoutAmount = (double)(totalCashoutAmount / (ulong)setsCount);
            winningSessions = winningSessionCount / (double)setsCount;
            cashoutsW0Bonuses = cashoutsW0BonusesCount / (double)setsCount;
            cashoutsW1Bonus = cashoutsW1BonusesCount / (double)setsCount;
            cashoutW2Bonuses = cashoutsW2BonusesCount / (double)setsCount;
            fivePercentFauilures = fivePercentFailuresCount / (double)setsCount;
            sessionsWBonuses = sessionsWBonusesCount / (double)setsCount;
            sessionsWBonusesorProgs = sessionsWBonusesOrProgsCount / (double)setsCount;
            recoveryChance = recoveryChanceCount / (double)setsCount;
            averageWinAmount = winAmountCount / (double)setsCount;
            totalSessionsPlayed = aggregratedSessionCount;
            medianToMeanSpins = medianSpins / averageSpins;
            for(int ad = 0; ad < spinBottomRanges.Count(); ad++)
            {
                spinChances.Add((double)spinChanceCounts[ad] / (double)superLargeSpinCountList.Count());
            }
            calcsDone = true;
        }
        
        internal List<List<string>> printForTOD()
        {
            getOverallStatsForSets();
            List<List<string>> stringListToReturn = new List<List<string>>();
            stringListToReturn.Add(new List<string> { "Starting Bankroll", string.Format("{0:$0.00}",(double)(startingBankroll/100)), "Spins", 
                                                      string.Format("{0:0}", (double)(startingBankroll / setsRecorded[0].bet))});
            for(int ac = 0; ac < setsCount; ac++)
            {
                List<string> setStrings = new List<string>();
                setStrings.Add(string.Format("Set {0}", ac));
                setStrings.Add(string.Format("{0:0}", setsRecorded[ac].sessionsPlayed));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].rTP));
                setStrings.Add(string.Format("{0:0}", setsRecorded[ac].maxPayForSet));
                setStrings.Add(string.Format("{0:0.00}", setsRecorded[ac].averageSpins));
                setStrings.Add(string.Format("{0:0.00}", setsRecorded[ac].medianSpins));
                setStrings.Add(string.Format("{0:0.00}", setsRecorded[ac].setSpinMedianToMean));
                setStrings.Add(string.Format("{0:0.00}", setsRecorded[ac].setSpinVolatility));
                setStrings.Add(string.Format("{0:0.00} ({1:0.00}x)", setsRecorded[ac].averageWinThisSet, setsRecorded[ac].averageWinThisSet / setsRecorded[ac].bet));
                setStrings.Add(string.Format("{0:0.00} ({1:0.00}x)", setsRecorded[ac].averageCashoutAmount, setsRecorded[ac].averageCashoutAmount / setsRecorded[ac].bet));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].winningSessions));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].cashoutsW0Bonuses));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].cashoutsW1Bonus));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].cashoutW2Bonuses));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].fivePercentFailures));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].sessionsWBonus));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].sessionsWBonusesorProg));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].setRecoveryChance));
                stringListToReturn.Add(setStrings);
            }
            List<string> summaryStrings = new List<string>();
            summaryStrings.Add(string.Format("Summary"));
            summaryStrings.Add(string.Format("{0:0.00%}", rTP));
            summaryStrings.Add(string.Format("{0:0}", totalSessionsPlayed));
            summaryStrings.Add(string.Format("{0:0}", maxPay));
            summaryStrings.Add(string.Format("{0:0.00}", averageSpins));
            summaryStrings.Add(string.Format("{0:0.00}", medianSpins));
            summaryStrings.Add(string.Format("{0:0.00}", medianToMeanSpins));
            summaryStrings.Add(string.Format("{0:0.00}", spinVolatility));
            summaryStrings.Add(string.Format("{0:0.00} ({1:0.00}x)", averageWinAmount, averageWinAmount / setsRecorded[0].bet));
            summaryStrings.Add(string.Format("{0:0.00} ({1:0.00}x)", averageCashoutAmount, averageCashoutAmount / setsRecorded[0].bet));
            summaryStrings.Add(string.Format("{0:0.00%}", winningSessions));
            summaryStrings.Add(string.Format("{0:0.00%}", cashoutsW0Bonuses));
            summaryStrings.Add(string.Format("{0:0.00%}", cashoutsW1Bonus));
            summaryStrings.Add(string.Format("{0:0.00%}", cashoutW2Bonuses));
            summaryStrings.Add(string.Format("{0:0.00%}", fivePercentFauilures));
            summaryStrings.Add(string.Format("{0:0.00%}", sessionsWBonuses));
            summaryStrings.Add(string.Format("{0:0.00%}", sessionsWBonusesorProgs));
            summaryStrings.Add(string.Format("{0:0.00%}", recoveryChance));
            stringListToReturn.Add(summaryStrings);
            List<string> row_1 = new List<string>();
            List<string> row_2 = new List<string>();
            row_1.Add("Survivability Probabilities");
            row_2.Add("");
            for (int aa = 0; aa < spinBottomRanges.Count(); aa++)
            {
                row_1.Add(spinBottomRanges[aa].ToString());
                row_2.Add(string.Format("{0:0.0000000000%}", spinChances[aa]));
            }
            stringListToReturn.Add(row_1);
            stringListToReturn.Add(row_2);
            return stringListToReturn;
        }
        
        internal void InputResults(List<List<string>> _results)
        {
            getOverallStatsForSets();
            _results.Add(new List<string> { "Starting Bankroll", string.Format("{0:$0.00}",(double)(startingBankroll/100)), "Spins", 
                                                      string.Format("{0:0}", (double)(startingBankroll / setsRecorded[0].bet))});
            _results.Add(headers);
            for (int ac = 0; ac < setsCount; ac++)
            {
                List<string> setStrings = new List<string>();
                setStrings.Add(string.Format("Set {0}", ac + 1));
                setStrings.Add(string.Format("{0:0}", setsRecorded[ac].sessionsPlayed + 1));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].rTP));
                setStrings.Add(string.Format("{0:0}", setsRecorded[ac].maxPayForSet));
                setStrings.Add(string.Format("{0:0.00}", setsRecorded[ac].averageSpins));
                setStrings.Add(string.Format("{0:0.00}", setsRecorded[ac].medianSpins));
                setStrings.Add(string.Format("{0:0.00}", setsRecorded[ac].setSpinMedianToMean));
                setStrings.Add(string.Format("{0:0.00}", setsRecorded[ac].setSpinVolatility));
                setStrings.Add(string.Format("{0:0.00} ({1:0.00}x)", setsRecorded[ac].averageWinThisSet, setsRecorded[ac].averageWinThisSet / setsRecorded[ac].bet));
                setStrings.Add(string.Format("{0:0.00} ({1:0.00}x)", setsRecorded[ac].averageCashoutAmount, setsRecorded[ac].averageCashoutAmount / setsRecorded[ac].bet));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].winningSessions));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].cashoutsW0Bonuses));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].cashoutsW1Bonus));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].cashoutW2Bonuses));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].fivePercentFailures));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].sessionsWBonus));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].sessionsWBonusesorProg));
                setStrings.Add(string.Format("{0:0.00%}", setsRecorded[ac].setRecoveryChance));
                _results.Add(setStrings);
            }
            List<string> summaryStrings = new List<string>();
            summaryStrings.Add(string.Format("Summary"));
            summaryStrings.Add(string.Format("{0:0}", totalSessionsPlayed));
            summaryStrings.Add(string.Format("{0:0.00%}", rTP));
            summaryStrings.Add(string.Format("{0:0}", maxPay));
            summaryStrings.Add(string.Format("{0:0.00}", averageSpins));
            summaryStrings.Add(string.Format("{0:0.00}", medianSpins));
            summaryStrings.Add(string.Format("{0:0.00}", medianToMeanSpins));
            summaryStrings.Add(string.Format("{0:0.00}", spinVolatility));
            summaryStrings.Add(string.Format("{0:0.00} ({1:0.00}x)", averageWinAmount, averageWinAmount / setsRecorded[0].bet));
            summaryStrings.Add(string.Format("{0:0.00} ({1:0.00}x)", averageCashoutAmount, averageCashoutAmount / setsRecorded[0].bet));
            summaryStrings.Add(string.Format("{0:0.00%}", winningSessions));
            summaryStrings.Add(string.Format("{0:0.00%}", cashoutsW0Bonuses));
            summaryStrings.Add(string.Format("{0:0.00%}", cashoutsW1Bonus));
            summaryStrings.Add(string.Format("{0:0.00%}", cashoutW2Bonuses));
            summaryStrings.Add(string.Format("{0:0.00%}", fivePercentFauilures));
            summaryStrings.Add(string.Format("{0:0.00%}", sessionsWBonuses));
            summaryStrings.Add(string.Format("{0:0.00%}", sessionsWBonusesorProgs));
            summaryStrings.Add(string.Format("{0:0.00%}", recoveryChance));
            _results.Add(summaryStrings);
            List<string> row_1 = new List<string>();
            List<string> row_2 = new List<string>();
            row_1.Add("Survivability Probabilities");
            row_2.Add("");
            for (int aa = 0; aa < spinBottomRanges.Count(); aa++)
            {
                row_1.Add(spinBottomRanges[aa].ToString());
                row_2.Add(string.Format("{0:0.0000000000%}", spinChances[aa]));
            }
            _results.Add(row_1);
            _results.Add(row_2);
            _results.Add(new List<string>());
        }
    }
    class gameStatsArg
    {
        public int costToCover;
        public double rTP, baseGame, bonusGame, hitFreq, baseGameFeatureFreq, bonusFreq, bonusMedToMean, bonusMean, bonus1stQuartile, bonus2ndQuartile, bonus3rdQuartile, bonus9thDecile, tenTimesOdds,
               hundredTimesOdds, maxWin, normalizedTOD, medianTOD, cashoutPercent, cashoutMultiple, cashoutWBonus, sessionWBonus, baseVolatility, totalVolatility;
        public ulong totalHits, totalWin, totalFGWin, totalPGWin, freeSpinsBonusPlayed, baseGameFeatures, tenTimesHits, hundredTimesHits;
        public string gameName;
        internal gameStatsArg()
        {
            costToCover = 0;
            rTP = 0;
            baseGame = 0;
            bonusGame = 0;
            hitFreq = 0;
            baseGameFeatureFreq = 0;
            bonusFreq = 0;
            bonusMedToMean = 0;
            bonusMean = 0;
            bonus1stQuartile = 0;
            bonus2ndQuartile = 0;
            bonus3rdQuartile = 0;
            bonus9thDecile = 0;
            tenTimesOdds = 0;
            hundredTimesOdds = 0;
            maxWin = 0;
            normalizedTOD = 0;
            medianTOD = 0;
            cashoutPercent = 0;
            cashoutMultiple = 0;
            cashoutWBonus = 0;
            sessionWBonus = 0;
            baseVolatility = 0;
            totalVolatility = 0;
            gameName = "";
        }
        internal void collectWinInfo(int _bet, int _totalWin, int _winInFreeGames, int _freeGamesPlayed, bool _baseGameFeature)
        {
            if (_totalWin > 0) totalHits++;
            totalWin += (ulong)_totalWin;
            totalFGWin += (ulong)_winInFreeGames;
            totalPGWin += (ulong)_totalWin - (ulong)_winInFreeGames;
            if (_freeGamesPlayed > 0) freeSpinsBonusPlayed++;
            if (_baseGameFeature) baseGameFeatures++;
            if (_totalWin >= (10 * _bet)) tenTimesHits++;
            if (_totalWin >= (100 * _bet)) hundredTimesHits++;
        }

        internal void InputResults(List<List<string>> _results, double _totalVI)
        {
            _results.Add(new List<string> {"Game Name", "Class", "Cost to Cover", "Simulation Pay %", "Base %", "Bonus %", "Hit Freq.", "Base Game Feature Freq.", "Bonus Freq.", "Bonus Med/Mean",
                         "Bonus Mean", "Bonus 1st Quartile", "Bonus 2nd Quartile", "Bonus 3rd Quartile", "Bonus 9th Decile", "10X+ Odds", "100X+ Odds", "Max Win Multiple", "Normalized TOD",
                         "Median TOD", "Cashout %", "Avg. Cashout Multiple", "% of Cashout w/Bonus", "% of Session w/Bonus", "Base Volatility", "Total Volatility"});
            List<string> statsString = new List<string>();
            statsString.Add(gameName);
            statsString.Add("3");
            statsString.Add(string.Format("{0:0}", costToCover));
            statsString.Add(string.Format("{0:0.00%}", rTP));
            statsString.Add(string.Format("{0:0.00%}", baseGame));
            statsString.Add(string.Format("{0:0.00%}", bonusGame));
            statsString.Add(string.Format("{0:0.00%}", hitFreq));
            statsString.Add(string.Format("{0:0}", baseGameFeatureFreq));
            statsString.Add(string.Format("{0:0}", bonusFreq));
            statsString.Add(string.Format("{0:0.00}", bonusMedToMean));
            statsString.Add(string.Format("{0:0.00}", bonusMean));
            statsString.Add(string.Format("{0:0.00}", bonus1stQuartile));
            statsString.Add(string.Format("{0:0.00}", bonus2ndQuartile));
            statsString.Add(string.Format("{0:0.00}", bonus3rdQuartile));
            statsString.Add(string.Format("{0:0.00}", bonus9thDecile));
            statsString.Add(string.Format("{0:0.00}", tenTimesOdds));
            statsString.Add(string.Format("{0:0.00}", hundredTimesOdds));
            statsString.Add(string.Format("{0:0.00}", maxWin));
            statsString.Add(string.Format("{0:0}", normalizedTOD));
            statsString.Add(string.Format("{0:0}", medianTOD));
            statsString.Add(string.Format("{0:0.00%}", cashoutPercent));
            statsString.Add(string.Format("{0:0.00}", cashoutMultiple));
            statsString.Add(string.Format("{0:0.00%}", cashoutWBonus));
            statsString.Add(string.Format("{0:0.00%}", sessionWBonus));
            statsString.Add(string.Format("{0:0.00}", baseVolatility));
            statsString.Add(string.Format("{0:0.00} / {1:0.00}", _totalVI, totalVolatility));
            _results.Add(statsString);
            _results.Add(new List<string>());
        }
    }
}
