using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Forms;

namespace Slot_Simulator
{
    enum RandomType { Random, SetRandom, GetRandom };
    class m
    {
        [ThreadStatic] private static RandomOps.Ran2 m_rngSeed;
        [ThreadStatic] internal static RandomOps.KISS m_rng;
        [ThreadStatic] internal static bool DisableRNGIndex = false;
        [ThreadStatic] internal static int m_rngIndex;
        [ThreadStatic] internal static RandomType m_rngType;
        [ThreadStatic] internal static List<object> m_rngList = new List<object>();
        [ThreadStatic] internal static int PsuedoRandomNumberInitialIndex;
        [ThreadStatic] private static int m_psuedoRandomNumberCurrentIndex;
        internal static string Directory;
        static m()
        {
            Multipliers1xTo100x = new List<double>(new double[] { 0, .5 });
            for (int i = 2; i <= 100; i++)
                Multipliers1xTo100x.Add(i);
            Directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }
        
        internal static ReelsPanel ReelsPanel;
        internal static List<List<string>> m_outputToExcel;
        internal static int SegmentNum = 10;
        internal static bool ZipPlay = false;
        internal static int BankRollForStatistics = 2000;
        internal static List<int> SpinsForSurvivability = new List<int>{ 5, 10, 25, 50, 100 };
        internal static List<int> bankrollsForTODStats = new List<int> { 500, 2000, 4000, 5000, 10000 };
        internal static List<double> Multipliers = new List<double>(new double[] { 0, 1, 3, 5, 10, 25, 50, 100, 150, 200, 500 });
        internal static List<double> MultipliersHigh = new List<double>(new double[] { 0, 100, 150, 200, 500, 1000, 2500, 5000, });
        internal static List<int> spinMultipliers = new List<int> { 1, 2, 5, 10, 20, 50, 100 };
        internal static List<double> Multipliers1xTo100x;
        internal static TextBox gameMessageBox, winsTB;
        //internal static NumberBox numberBoxCreditWin, numberBoxCreditTotal,numberBoxBet;
        internal static List<T> MakeNewList<T>(int _length, T _default)
        {
            List<T> list = new List<T>();
            for (int i = 0; i < _length; i++)
                list.Add(_default);
            return list;
        }

        internal static List<List<T>> MakeNewEmptyLists<T>(int _length)
        {
            List<List<T>> list = new List<List<T>>();
            for (int i = 0; i < _length; i++)
                list.Add(new List<T>());
            return list;
        }

        internal static List<List<T>> MakeNewDoubleList<T>(int _xLists, int _ofY, T _default)
        {
            List<List<T>> list = new List<List<T>>();
            for (int i = 0; i < _xLists; i++)
                list.Add(MakeNewList<T>(_ofY, _default));
            return list;
        }

        private static RandomOps.KISS RandomRNG()
        {
            if (m_rngSeed == null)
                m_rngSeed = new RandomOps.Ran2();
            RandomOps.KISS rng = new RandomOps.KISS(m_rngSeed);
            return rng;
        }

        internal static void SetRandomType(RandomType _randomType)
        {
            m_rngType = _randomType;
            switch (m_rngType)
            {
                case RandomType.GetRandom:
                    m_rngIndex = 0;
                    break;
                case RandomType.SetRandom:
                    m_rngList.Clear();
                    break;
            }
        }

        internal static void SetPsudeoRandomNumberIndex(int _indexToSet = -1)
        {
            if (_indexToSet == -1)
                PsuedoRandomNumberInitialIndex = RandomInteger(PsuedoRandomNumbers.Count);
            else PsuedoRandomNumberInitialIndex = _indexToSet;
            m_psuedoRandomNumberCurrentIndex = PsuedoRandomNumberInitialIndex;
        }

        private static int NextPsuedoRandomNumberIndex(bool _incrementIndex = true)
        {
            if (m_psuedoRandomNumberCurrentIndex >= PsuedoRandomNumbers.Count)
                m_psuedoRandomNumberCurrentIndex %= PsuedoRandomNumbers.Count;
            if (_incrementIndex)
                return m_psuedoRandomNumberCurrentIndex++;
            else
                return m_psuedoRandomNumberCurrentIndex;
        }

        internal static int RandomIntegerAlwaysRandom(int _exclusiveUpperBound, bool _psudeoRNG = false)
        {
            if (m_rng == null) m_rng = RandomRNG();
            return m_rng.Index(_exclusiveUpperBound);
        }

        internal static int RandomInteger(int _exclusiveUpperBound, bool _psudeoRNG = false, bool _incrementIndex = true)
        {
            if (m_rng == null) m_rng = RandomRNG();
            switch (m_rngType)
            {
                case RandomType.Random:
                    return _psudeoRNG ? PsuedoRandomNumbers[NextPsuedoRandomNumberIndex(_incrementIndex)] % _exclusiveUpperBound : m_rng.Index(_exclusiveUpperBound);
                case RandomType.GetRandom: 
                    return DisableRNGIndex ?
                        _psudeoRNG ? PsuedoRandomNumbers[NextPsuedoRandomNumberIndex(_incrementIndex)] % _exclusiveUpperBound : m_rng.Index(_exclusiveUpperBound) : 
                        (int)m_rngList[m_rngIndex++];
                case RandomType.SetRandom:
                    int randomInt = _psudeoRNG ? PsuedoRandomNumbers[NextPsuedoRandomNumberIndex()] % _exclusiveUpperBound : m_rng.Index(_exclusiveUpperBound);
                    m_rngList.Add(randomInt);
                    return randomInt;
            }
            throw new ArgumentException("Unknown RandomType: " + m_rngType);
        }

        internal static double RandomDouble(double _exclusiveUpperBound)
        {
            if (m_rng == null) m_rng = RandomRNG();
            switch (m_rngType)
            {
                case RandomType.Random: return m_rng.Uniform() * _exclusiveUpperBound;
                case RandomType.GetRandom: return DisableRNGIndex ? m_rng.Uniform() * _exclusiveUpperBound : (double)m_rngList[m_rngIndex++];
                case RandomType.SetRandom:
                    double randomDouble = m_rng.Uniform() * _exclusiveUpperBound;
                    m_rngList.Add(randomDouble);
                    return randomDouble;
            }
            throw new ArgumentException("Unknown RandomType: " + m_rngType);
        }

        internal static int RandomIndex(List<int> _cutoffs, bool _psudeoRNG = false, bool _incrementIndex = true)
        {
            int randomInt = m.RandomInteger(_cutoffs[_cutoffs.Count - 1], _psudeoRNG, _incrementIndex);
            for (int i = 0; i < _cutoffs.Count; i++)
                if (randomInt < _cutoffs[i])
                    return i;
            throw new ArgumentException("Random number of " + randomInt + " is out of range");
        }

        internal static T RandomObject<T>(List<T> _equallyWeightedList, bool _psudeoRNG = false)
        {
            return _equallyWeightedList[m.RandomInteger(_equallyWeightedList.Count, _psudeoRNG)];
        }

        internal static List<uint> MultipliersToCredits(List<double> _multipliers, int _bet)
        {
            List<uint> credits = new List<uint>();
            foreach (double multiplier in _multipliers)
                credits.Add((uint)(multiplier * _bet));
            return credits;
        }

        internal static List<List<int>> GetIndexesWithSymbols(List<int> _symbols, List<List<int>> _reels, List<int> _dimensions, bool _fullReel = false)
        {
            List<List<int>> indexes = new List<List<int>>();
            for (int reelNum = 0; reelNum < _dimensions.Count; reelNum++)
            {
                List<int> indexesForReel = new List<int>();
                List<int> reel = _reels[reelNum];
                int dimension = _dimensions[reelNum];
                for (int indexToCheck = 0; indexToCheck < reel.Count; indexToCheck++)
                    if (_fullReel)
                    {
                        bool isFullReel = true;
                        for (int i = 0; i < dimension; i++)
                            if (!_symbols.Contains(reel[(indexToCheck + i - 1 + reel.Count) % reel.Count]))
                            {
                                isFullReel = false;
                                break;
                            }

                        if (isFullReel)
                            indexesForReel.Add(indexToCheck);
                    }
                    else
                    {
                        for (int i = 0; i < dimension; i++)
                            if (_symbols.Contains(reel[(indexToCheck + i - 1 + reel.Count) % reel.Count]))
                            {
                                indexesForReel.Add(indexToCheck);
                                break;
                            }
                    }
                indexes.Add(indexesForReel);
            }
            return indexes;
        }

        internal static List<List<int>> GetIndexesWithSymbol(int _symbol, List<List<int>> _reels, List<int> _dimensions, bool _fullReel = false)
        {
            return GetIndexesWithSymbols(new List<int>(new int[] { _symbol }), _reels, _dimensions, _fullReel);
        }

        internal static List<List<int>> GetIndexesWithSymbolsAt(List<int> _symbols, List<List<int>> _reels, List<int> _dimensions)
        {
            List<List<int>> indexes = new List<List<int>>();
            for (int reelNum = 0; reelNum < _dimensions.Count; reelNum++)
            {
                List<int> indexesAtReel = new List<int>();
                List<int> reel = _reels[reelNum];
                for (int indexToCheck = 0; indexToCheck < reel.Count; indexToCheck++)
                    if (_symbols.Contains(reel[indexToCheck]))
                        indexesAtReel.Add(indexToCheck);
                indexes.Add(indexesAtReel);
            }
            return indexes;
        }

        internal static List<List<int>> GetIndexesWithSymbolAt(int _symbol, List<List<int>> _reels, List<int> _dimensions)
        {
            return GetIndexesWithSymbolsAt(new List<int>(new int[] { _symbol }), _reels, _dimensions);
        }

        internal static List<List<bool>> GetBoolListFromDataTable(List<List<string>> _dataTable, int _col, int _len, string _true)
        {
            List<List<bool>> values = new List<List<bool>>();
            foreach (List<string> row in _dataTable)
            {
                List<bool> config = new List<bool>();
                for (int i = _col; i < _col + _len; i++)
                    config.Add(row[i] == _true);
                values.Add(config);
            }
            return values;
        }

        internal static List<List<int>> GetIntListFromDataTable(List<List<string>> _dataTable, int _col, int _len)
        {
            List<List<int>> values = new List<List<int>>();
            foreach (List<string> row in _dataTable)
            {
                List<int> config = new List<int>();
                for (int i = _col; i < _col + _len; i++)
                {
                    int integerValue;
                    if (int.TryParse(row[i], out integerValue))
                    {
                        config.Add(integerValue);
                    }
                    else
                        throw new ArgumentException("Can't parse: " + row[i]);
                }
                values.Add(config);
            }
            return values;
        }

        internal static List<List<int>> GetIntListFromDataTable(List<List<string>> _dataTable, int _col, int _len, int _rowStart, int _rowNum)
        {
            List<List<int>> values = new List<List<int>>();
            for (int row = _rowStart; row < _rowStart + _rowNum; row++)
            {
                List<int> config = new List<int>();
                for (int i = _col; i < _col + _len; i++)
                    config.Add(int.Parse(_dataTable[row][i]));
                values.Add(config);
            }
            return values;
        }

        internal static List<List<int>> GetIntListFromDataTableZeroIndexedWhereXIsTrue(List<List<string>> _dataTable, int _col, int _len, string _trueString)
        {
            List<List<int>> values = new List<List<int>>();
            foreach (List<string> row in _dataTable)
            {
                List<int> config = new List<int>();
                for (int i = _col; i < _col + _len; i++)
                    if (row[i].ToLower() == _trueString)
                        config.Add(i - _col);
                values.Add(config);
            }
            return values;
        }

        internal static int GetBuyInOfBet(int _bet) { return 50 * _bet; }

        internal static void LoadInnerInformation(Dictionary<string, List<List<string>>> _extraData, List<string> _symbols, int _reelCount, out List<List<int>> _innerDistribution, out RandomObjectArg<List<List<int>>> _innerPatternRandomObjectPG, out RandomObjectArg<List<List<int>>> _innerPatternRandomObjectFG)
        {
            {
                List<string> patternNames = m.GetStringValuesFromDataTable(_extraData["inner chance"], 0);

                List<List<List<int>>> innerPatterns = new List<List<List<int>>>();
                foreach (string patternName in patternNames)
                {
                    List<List<int>> innerPattern = new List<List<int>>();
                    for (int reelNum = 0; reelNum < _reelCount; reelNum++)
                        innerPattern.Add(new List<int>());
                    foreach (List<string> row in _extraData["inner patterns"])
                        if (row[0] == patternName)
                        {
                            int wildNum = int.Parse(row[1]);
                            for (int reelNum = 0; reelNum < _reelCount; reelNum++)
                            {
                                int timesToAdd = int.Parse(row[2 + reelNum]);
                                for (int i = 0; i < timesToAdd; i++)
                                    innerPattern[reelNum].Add(wildNum);
                            }
                        }

                    innerPatterns.Add(innerPattern);
                }
                _innerPatternRandomObjectPG = new RandomObjectArg<List<List<int>>>(innerPatterns, m.GetIntCutoffsFromDataTable(_extraData["inner chance"], 1));
                _innerPatternRandomObjectFG = new RandomObjectArg<List<List<int>>>(innerPatterns, m.GetIntCutoffsFromDataTable(_extraData["inner chance"], 2));

                Dictionary<string, List<int>> innerDistributionByName = new Dictionary<string, List<int>>();
                List<List<string>> innerDistributionData = _extraData["inner distribution"];
                for (int col = 0; col < innerDistributionData[0].Count; col++)
                {
                    List<int> innerDistributionForPattern = new List<int>();
                    for (int row = 1; row < innerDistributionData.Count; row++)
                        if (_symbols.Contains(innerDistributionData[row][col].ToLower()))
                            innerDistributionForPattern.Add(_symbols.IndexOf(innerDistributionData[row][col]));
                    innerDistributionByName[innerDistributionData[0][col]] = innerDistributionForPattern;
                }
                _innerDistribution = new List<List<int>>();
                foreach (string patternName in patternNames)
                    _innerDistribution.Add(innerDistributionByName[patternName]);
            }
        }

        internal static void AddPayTableCount(string _name, string _prefix, int _categoryCount, List<PayArgs> _payTable, Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, Dictionary<string, PayCountArg> _payTablesTotal)
        {
            Dictionary<string, PayCountArg> payTableCounts = new Dictionary<string, PayCountArg>();
            foreach (PayArgs pay in _payTable)
                payTableCounts[_prefix + pay.Name] = new PayCountArg(pay.Pays, _categoryCount);
            _payTablesSeperated[_name] = payTableCounts;
            foreach (string key in payTableCounts.Keys)
                _payTablesTotal[key] = payTableCounts[key];
        }

        internal static void AddPayTableCount(string _name, int _categoryCount, List<string> _payNameList, Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, Dictionary<string, PayCountArg> _payTablesTotal)
        {
            Dictionary<string, PayCountArg> payTableCounts = new Dictionary<string, PayCountArg>();
            foreach (string payName in _payNameList)
                payTableCounts[payName] = new PayCountArg(m.MakeNewList<int>(1, 1000), _categoryCount);
            _payTablesSeperated[_name] = payTableCounts;
            foreach (string key in payTableCounts.Keys)
                _payTablesTotal[key] = payTableCounts[key];
        }

        internal static List<string> MultipliersToLabels(List<double> _multipliers)
        {
            List<string> labels = new List<string>();
            for (int category = 0; category < _multipliers.Count + 1; category++)
            {
                if (category == 0) labels.Add("0");
                else if (category == 1) labels.Add(string.Format("{0} < X < {1}", _multipliers[category - 1], _multipliers[category]));
                else if (category == _multipliers.Count) labels.Add(string.Format("X > {0}", _multipliers[category - 1]));
                else labels.Add(string.Format("{0} <= X < {1}", _multipliers[category - 1], _multipliers[category]));
            }
            return labels;
        }

        internal static List<int> MakeCutoffs(List<int> _weights)
        {
            List<int> cutoffs = new List<int>();
            int total = 0;
            foreach (int weight in _weights)
            {
                total += weight;
                cutoffs.Add(total);
            }
            return cutoffs;
        }

        internal static List<double> MakeCutoffs(List<double> _weights)
        {
            List<double> cutoffs = new List<double>();
            double total = 0;
            foreach (double weight in _weights)
            {
                total += weight;
                cutoffs.Add(total);
            }
            return cutoffs;
        }

        internal static List<int> GetIntValuesFromDataTable(List<List<string>> _dataTable, int _col)
        {
            List<int> values = new List<int>();
            foreach (List<string> row in _dataTable)
            {
                int integerValue;
                if (int.TryParse(row[_col], out integerValue))
                    values.Add(integerValue);
                else
                    throw new ArgumentException("Can't parse: " + row[_col]);
            }
            return values;
        }

        internal static List<int> GetIntValuesFromDataTable(List<List<string>> _dataTable, int _col, int _rowStart, int _rows)
        {
            List<int> values = new List<int>();
            for (int rowNum = _rowStart; rowNum < _rowStart + _rows; rowNum++)
                values.Add(int.Parse(_dataTable[rowNum][_col]));
            return values;
        }

        internal static List<string> GetStringValuesFromDataTable(List<List<string>> _dataTable, int _col)
        {
            List<string> values = new List<string>();
            foreach (List<string> row in _dataTable)
                values.Add(row[_col]);
            return values;
        }

        internal static List<int> GetIntCutoffsFromDataTable(List<List<string>> _dataTable, int _col)
        {
            return MakeCutoffs(GetIntValuesFromDataTable(_dataTable,_col));
        }

        internal static List<int> GetIntCutoffsFromDataTable(List<List<string>> _dataTable, int _col, int _rowStart, int _rows)
        {
            return MakeCutoffs(GetIntValuesFromDataTable(_dataTable, _col, _rowStart, _rows));
        }

        internal static Dictionary<int, int> GetIntDictonaryFromDataTable(List<List<string>> _dataTable, int _keyCol, int _valueCol)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            foreach (List<string> row in _dataTable)
                dic[int.Parse(row[_keyCol])] = int.Parse(row[_valueCol]);
            return dic;
        }

        internal static Dictionary<int, int> GetIntDictonaryFromDataTableWhereKeysAreSymbolIndexs(List<List<string>> _dataTable, int _keyCol, int _valueCol, List<string> _symbolLowerCase)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            foreach (List<string> row in _dataTable)
                dic[_symbolLowerCase.IndexOf(row[_keyCol])] = int.Parse(row[_valueCol]);
            return dic;
        }

        internal static List<double> GetDoubleValuesFromDataTable(List<List<string>> _dataTable, int _col)
        {
            List<double> values = new List<double>();
            foreach (List<string> row in _dataTable)
                values.Add(double.Parse(row[_col]));
            return values;
        }

        internal static List<double> GetDoubleCutoffsFromDataTable(List<List<string>> _dataTable, int _col)
        {
            return MakeCutoffs(GetDoubleValuesFromDataTable(_dataTable, _col));
        }

        internal static List<int> GetSymbolIndexesFromDataTable(List<List<string>> _dataTable, int _col, List<string> _symbolsLowerCase)
        {
            List<int> values = new List<int>();
            foreach (List<string> row in _dataTable)
            {
                if (_symbolsLowerCase.Contains(row[_col]))
                    values.Add(_symbolsLowerCase.IndexOf(row[_col]));
                else
                    throw new ArgumentException("Can't parse: " + row[_col]);
            }
            return values;
        }

        internal static List<List<int>> GetSymbolIndexesListFromDataTable(List<List<string>> _dataTable, int _col, string[] _seperators, List<string> _symbolsLowerCase)
        {
            List<List<int>> values = new List<List<int>>();
            foreach (List<string> row in _dataTable)
            {
                string[] dataForRow = row[_col].Split(_seperators, StringSplitOptions.RemoveEmptyEntries);
                List<int> valuesForRow = new List<int>();
                foreach (string data in dataForRow)
                {
                    if (_symbolsLowerCase.Contains(data))
                        valuesForRow.Add(_symbolsLowerCase.IndexOf(data));
                    else
                        throw new ArgumentException("Can't parse: " + row[_col]);
                }
                values.Add(valuesForRow);
            }
            return values;
        }

        internal static List<List<int>> GetIndexSuffixesForLargeImages(List<List<int>> _reels, Dictionary<int,int> _symbolToLargeSize)
        {
            List<List<int>> indexSuffixes = new List<List<int>>();
            foreach (List<int> reel in _reels)
            {
                int currentSymbol = -1;
                int currentStreak = 0;
                List<int> indexSuffixesForReel = new List<int>();
                for (int i = 0; i < reel.Count; i++)
                {
                    if (reel[i] == currentSymbol && _symbolToLargeSize.ContainsKey(currentSymbol))
                    {
                        indexSuffixesForReel.Add(++currentStreak);
                        if (currentStreak == _symbolToLargeSize[currentSymbol])
                        {
                            currentSymbol = -1;
                            currentStreak = 0;
                        }
                    }
                    else
                    {
                        currentStreak = 0;
                        indexSuffixesForReel.Add(currentStreak);
                    }
                    currentSymbol = reel[i];
                }
                indexSuffixes.Add(indexSuffixesForReel);
            }
            return indexSuffixes;
        }

        internal static int RandomIndex(List<double> _cutoffs)
        {
            double randomDouble = m.RandomDouble(_cutoffs[_cutoffs.Count - 1]);
            for (int i = 0; i < _cutoffs.Count; i++)
                if (randomDouble <= _cutoffs[i])
                    return i;
            throw new ArgumentException("Random number of " + randomDouble + " is out of range");
        }

        internal static bool RandomChance(double _chance)
        {
            return m.RandomDouble(1) < _chance;
        }

        internal static List<List<int>> GetInverseIndexes(List<List<int>> _fullList, List<List<int>> _toInverse)
        {
            List<List<int>> inverse = new List<List<int>>();
            for (int reelNum = 0; reelNum < _fullList.Count; reelNum++)
            {
                List<int> reel = new List<int>();
                for (int index = 0; index < _fullList[reelNum].Count; index++)
                    if (!_toInverse[reelNum].Contains(index))
                        reel.Add(index);
                inverse.Add(reel);
            }
            return inverse;
        }

        internal static string ToMoneyString(float _pennies)
        {
            return _pennies < 0 ?
                string.Format("-${0:0.00}", -_pennies / 100) :
                string.Format("${0:0.00}", _pennies / 100);
        }

        internal static double GetStandardDeviation(double _bet, SortedDictionary<int, long> _wins)
        {
            if (_wins.Count == 0) return 0;
            double totalWin = 0;
            double totalCount = 0;
            foreach (int pay in _wins.Keys)
            {
                totalWin += (double)pay * _wins[pay];
                totalCount += _wins[pay];
            }
            double averagePay = totalWin / totalCount;
            double variance = 0;
            foreach (int pay in _wins.Keys)
            {
                double difference = pay - averagePay;
                variance += _wins[pay] * difference * difference;
            }
            variance /= totalCount;
            variance /= _bet * _bet;
            return Math.Sqrt(variance);
        }

        internal static List<string> MakeIntHeaders(string _prefix, string _suffix, int _from, int _count)
        {
            List<string> headers = new List<string>();
            for (int i = _from; i < _from + _count; i++)
                headers.Add(_prefix + i.ToString() + _suffix);
            return headers;
        }

        internal static int MaxInt(List<int> _list)
        {
            int max = 0;
            foreach (int num in _list) max = Math.Max(num, max);
            return max;
        }

        internal static void OutputToExcel(bool _initialize)
        {
            if (_initialize)
                m_outputToExcel = new List<List<string>>();
            else
                ExcelFile.SaveExcel(m_outputToExcel, string.Format(@"{0}\z Output.xlsx", m.Directory));

        }
        internal static List<int> PsuedoRandomNumbers = new List<int>(new int[] {
            1084966650, 106194085, 33783796, 1551969057, 1160243922, 356263421, 28624529, 1241599074, 1920113011, 263599635, 182958155, 1991912941, 1436764064, 124807227, 492571931, 172240027, 867195644, 2054059249, 798721379, 1678727425, 
            891516009, 249194188, 50826154, 367219671, 2003422504, 1562356506, 1124744860, 516895351, 1795515940, 982046245, 978272194, 1036302585, 1364935020, 1744887344, 1755876452, 1415927907, 726154888, 1185267303, 2123395540, 194602728, 
            1558742883, 2044499021, 230005626, 281163553, 134414842, 320951711, 549645553, 1292172782, 969191362, 1583782922, 795294760, 989855424, 1915342461, 313847895, 1615337266, 264171874, 141281083, 534073104, 1074835008, 356994915, 
            999516515, 1595948054, 249276008, 135114559, 451550741, 1165521494, 1615352381, 92874911, 2049320125, 290449015, 2024434989, 1995064521, 996787748, 814389391, 1876480530, 277935275, 2134312728, 969745326, 353878178, 1765678362, 
            906383881, 1088884423, 1751180829, 760274947, 254865511, 555440086, 503106126, 1249552100, 1867077292, 309919312, 407197268, 1796666551, 1349749345, 363330726, 712803958, 291571676, 1021062163, 401766821, 1392859086, 1900361458, 
            431723229, 1442398790, 98047690, 948002286, 1101272088, 1967038975, 818527227, 2024772043, 138758989, 1489906110, 1389525871, 1870257560, 1230157906, 1748410105, 50172504, 113866177, 291062132, 934950109, 1004464763, 1649003095, 
            1843799268, 536774943, 1687866944, 510841904, 108003906, 832714281, 273334942, 2106074166, 1722222846, 1401348316, 622312309, 1052458156, 1672400009, 227277406, 1728221621, 142199874, 418823376, 1795454817, 1997693565, 267046095, 
            1524631548, 423500301, 861394746, 284738032, 2145072249, 1798977523, 924305844, 1726648122, 1115592458, 339661963, 467345198, 1124570931, 298997545, 1625821188, 880330937, 1060540460, 1263151962, 1490202950, 1872143051, 251313233, 
            135823800, 859999597, 965235390, 1673110378, 663232493, 1982503065, 235505375, 1196674809, 2142825636, 1517639946, 2138334098, 1384915915, 830745873, 639455949, 632412283, 1085594316, 1493767685, 1194606526, 1731298370, 1845602635, 
            1764759516, 1870069754, 1969023495, 1746319360, 1344379990, 2577855, 224061915, 1731008640, 443826682, 1746312783, 197200630, 1260590629, 508152277, 1004802545, 827959795, 146116035, 1492489801, 55572805, 1550443190, 866678910, 
            713540321, 306245850, 901525612, 1077161251, 1898737271, 191192176, 504659297, 1040930204, 513607521, 669007777, 523955140, 1868364657, 1439193374, 1660696784, 158598020, 1171800050, 1453548506, 2062749830, 753240801, 1884358200, 
            1422204320, 773408520, 889072235, 1061367521, 217548945, 857493799, 1229351239, 35767938, 191151190, 2127241163, 458794102, 206887731, 1265679273, 263382863, 1979958169, 1194693594, 1173248358, 851620803, 162104494, 929266062, 
            714510032, 492317877, 1084141493, 1210675792, 458682535, 1802449672, 427944523, 1126792165, 126190789, 1054785382, 588011936, 1424934617, 2077462211, 1920047695, 1450191270, 532353472, 779973050, 1030794433, 1148453810, 882091622, 
            1657474490, 1860738101, 1485042589, 423312006, 636772586, 361039308, 903050753, 1674412757, 1976610066, 2089254751, 1044371890, 668441794, 500659880, 502593741, 1502746375, 1372651059, 1672464835, 386010593, 1942585074, 111086718, 
            437975207, 1788510090, 429953533, 1999258988, 2022281043, 711219970, 860103642, 230299268, 350464749, 1783383552, 653516234, 1357310911, 295353751, 122306700, 142660244, 1134773870, 1021526740, 666403109, 997996233, 1740429646, 
            1233745588, 542804569, 1011762969, 1144137323, 1841053359, 364848730, 965426378, 1435206931, 450653924, 91219927, 1627921736, 339861107, 2111636654, 1030148552, 1879836678, 1921415277, 1960270074, 1839167441, 74994686, 1180711700, 
            258516425, 1808685775, 584413114, 1795443805, 1036758588, 1259513084, 1537826068, 2143730450, 1247543529, 1143333651, 795212371, 303441804, 771154415, 613362910, 381127109, 1208491338, 186510061, 389580, 77872386, 548132030, 
            1317029165, 132854308, 2009667163, 714314614, 125713991, 1841159624, 712559743, 208988723, 1579707133, 1457085259, 1186684939, 291991884, 2104396382, 584039701, 502151367, 251859777, 1859766722, 1592384627, 1395420278, 1236001767, 
            2087245491, 516241801, 1669788985, 822393385, 1761886797, 258947256, 747637508, 674592442, 669934291, 199028614, 635931840, 2100518348, 1915906374, 942563574, 859662908, 692415059, 1312364369, 1297174855, 1065940950, 2094452530, 
            732651161, 1546299819, 922873162, 1967650304, 1072300830, 597936175, 1278150071, 72413519, 1321825811, 1947919861, 1314275632, 478854335, 1621307992, 753234624, 860965550, 274234838, 1215000716, 1698105760, 1575515921, 792873302, 
            637240272, 1617188912, 1015072547, 1955573869, 1318879108, 370145300, 1654354351, 991053447, 1606096687, 929943554, 1890801292, 676249327, 194523479, 1643141701, 1766835695, 1260541665, 1450426384, 735817256, 534291221, 985222870, 
            1174272701, 1781143186, 2047746460, 942182135, 1328289772, 1177538602, 448669059, 418213259, 684682578, 1569394621, 1875696636, 1905114534, 1936321737, 1228959489, 1195882396, 24765197, 1212538824, 572480980, 343271088, 1398615195, 
            1946337899, 1541151740, 383532402, 924884005, 236967654, 1301802985, 1052826194, 1630942847, 929437255, 1451493803, 1654652994, 1045413172, 1950051047, 718497116, 102175618, 2132102069, 228569795, 520458691, 1709574727, 609087378, 
            1572051841, 460069107, 440787567, 1213574296, 1836216660, 921915124, 1210323858, 1208059754, 103521361, 1913358430, 739800395, 2081844397, 277789031, 2142155190, 1697623527, 3444170, 939679471, 655134318, 753991784, 1791814761, 
            1560600501, 955655064, 484207424, 197908671, 1798631244, 122438235, 926688295, 1407391153, 888959402, 100280312, 1194185978, 2074547902, 1449500168, 618970105, 294534451, 226203014, 1140310300, 322387048, 4381656, 1986160218, 
            2055154759, 1565298584, 688238159, 189787094, 1821318460, 1881715268, 809469068, 258476360, 1019257081, 1404106031, 1661237512, 1295257102, 1021336148, 886341785, 2092100452, 1756231603, 334329234, 810487178, 1173935396, 1625494482, 
            442906048, 398558537, 747283597, 176500136, 1314219778, 1215509854, 777607978, 1962081676, 514406062, 683819378, 545732186, 778624142, 850323428, 343676041, 1019515277, 629377343, 605574706, 882538311, 1149402612, 690536910, 
            279585019, 1167027198, 72544890, 1871940780, 2113868041, 1053972892, 1279333243, 1696544311, 956040366, 1055060785, 1126760155, 390902659, 1111289844, 1501308477, 631962407, 1909559803, 2086961454, 970963911, 1103196874, 2102012951, 
            1483587660, 100855862, 1308085736, 1014840023, 1438966633, 1354819695, 1607057091, 1008129690, 1856143819, 990457983, 1440696626, 1302117517, 412671785, 125447286, 2120850910, 1960562317, 1906391653, 1589724356, 1986664146, 1056655801, 
            1860150565, 470402491, 1100066105, 733222070, 1183094740, 1185403416, 1296633047, 1584050693, 1145423243, 1104562864, 1844222954, 399914443, 1728679007, 359673283, 1307057270, 275969079, 1690787840, 799616474, 366559080, 1568965209, 
            1436322174, 769529408, 1874829457, 1258257194, 1192610608, 1463573070, 734378276, 1381912468, 1301224940, 1782660080, 1860991334, 168058232, 570847761, 2060865796, 1126614094, 1786615646, 570736998, 1504379861, 1288662000, 411123428, 
            769623121, 1613815073, 1553966645, 398913559, 1185734661, 348536789, 519071778, 476166517, 1910354223, 1131983393, 51606999, 222735833, 50312482, 203754286, 915027431, 1395833419, 896341963, 49518615, 1467609989, 468681779, 
            1977981464, 971433633, 2124512182, 736366248, 1934981904, 325352897, 1786578006, 602158043, 742137523, 990707888, 339346605, 556088316, 1546925159, 59925814, 1134621314, 1496745093, 173039717, 800764265, 2142281360, 239683123, 
            441796758, 1749549775, 1090087087, 1808330710, 995217911, 239829957, 753310309, 1992787132, 1429367240, 1275824022, 336964711, 147704082, 1035925605, 2035475756, 710439795, 1701967596, 1035542888, 59600706, 1619258573, 317300076, 
            359867463, 980994961, 1115717126, 281209048, 719612518, 2129946375, 2087610866, 1243183483, 588791001, 1203601731, 2002737522, 1582764278, 1753214700, 2080595482, 1784651731, 990824766, 665114709, 409050861, 1334351621, 2090200257, 
            1750944038, 586049374, 1464955622, 670526667, 1155005682, 1165614936, 1048140768, 1637484027, 149107837, 514814854, 980192902, 655220444, 677176687, 1709596792, 1802186947, 505432797, 376553903, 618938237, 813001076, 1701577796, 
            100272172, 1857159958, 787400306, 2129168835, 2103301748, 1180497622, 1880156932, 841305418, 1406950466, 1602540785, 1380834657, 2106610054, 1563433669, 484398918, 1409251424, 1461188892, 1737036466, 291201583, 82591384, 2124075652, 
            477181952, 1132127193, 1508734016, 1534688340, 183891660, 2030519314, 1121630706, 2047135612, 579626182, 1852018127, 1453538807, 822167337, 914111038, 349757897, 875583856, 1834716384, 960193851, 1321235706, 1349395884, 1544926169, 
            1618960761, 1024833382, 1109232934, 1779701745, 985860324, 1417720823, 79790857, 1804000152, 1494728331, 1294263810, 1305193602, 349010606, 1119490284, 855719319, 1658799733, 32550809, 413537477, 438219864, 1368123464, 641961379, 
            720176569, 279388810, 558357920, 957332215, 378691371, 1912113214, 1875315581, 60316740, 1049876398, 1955968881, 846271977, 256799440, 1122074049, 551124877, 1032398432, 1325200956, 1751801653, 113588934, 1956944502, 1032079867, 
            351108724, 331810555, 99136120, 2077616102, 8399997, 142155748, 123263742, 1699911766, 1540862774, 1112087620, 265832661, 1542336272, 139188458, 501354502, 619783344, 2059914, 1353597452, 1771209183, 424861412, 125556535, 
            506022185, 1228021726, 18241478, 504657236, 1690602418, 206449719, 961758004, 1988101073, 1541798560, 565917327, 489613998, 2120239824, 1596325665, 1667389287, 770237510, 145677396, 1760162080, 287164226, 996678182, 541474333, 
            399373473, 1766508785, 911220058, 915383751, 771270051, 1333235401, 958717413, 658138966, 1512572662, 967593206, 989862671, 268827230, 1690140742, 1483541989, 548700637, 403126297, 72999956, 752095394, 868094659, 1742868705, 
            842352484, 1173676871, 393225619, 1677404314, 1222908623, 1117598774, 482073591, 1781191549, 101993883, 1059802665, 1626896200, 828187155, 767165868, 61840517, 1410517273, 143293055, 1192220319, 350390872, 1330279883, 611887979, 
            742081259, 1371534923, 1789184737, 741049047, 258194207, 1812943411, 1231236347, 1744371952, 585004591, 821558985, 1093918391, 1661294855, 1962040826, 1121341203, 111608711, 1013658312, 1007337855, 1571040671, 1414109507, 56072992, 
            565657876, 232633477, 1962369310, 2109945832, 61874028, 1229638303, 1562538784, 386387846, 760286781, 1716867281, 1927471750, 148527558, 348215450, 1236497250, 916673657, 291676107, 1924543858, 2132199025, 693601577, 439950604, 
            1778572614, 1797208520, 1866452480, 237905075, 1558717033, 2079003817, 2135062984, 1826202319, 645678261, 1450824116, 540685770, 1569810411, 1250496728, 241116356, 1564588449, 1181405781, 1417157415, 124406622, 1417532647, 138174450, 
            2032408074, 1242047013, 1949045097, 1185684717, 466523727, 1225968720, 2046662081, 222436713, 179006610, 1206852211, 1950124885, 2025062084, 1933692695, 1493746776, 1485102130, 158336278, 1314427219, 617572269, 2018966465, 1859765124, 
            1411215138, 612820929, 280953495, 157616372, 1745983043, 1159152095, 1701641502, 241300746, 1629785611, 1382045108, 1878673716, 1069240900, 1354238869, 677512431, 1087308756, 991241949, 1104862572, 2129787310, 2108111273, 191575238, 
            641994787, 227569978, 679901794, 223661916, 791953332, 894067442, 155455697, 1443901466, 992693228, 27008674, 36810087, 1491083092, 338577707, 626751113, 1476322462, 495001040, 1437238607, 503596674, 1924737030, 844474458, 
            633162559, 524699093, 2009536387, 994705624, 1455490788, 1426891296, 1043335342, 1151407226, 1898460526, 1524186395, 1186000420, 1988040164, 147228785, 197960072, 1907825636, 114639306, 997537185, 165004524, 1907836954, 1195412352, 
            350083547, 25468509, 758004160, 551079258, 1665066321, 613337875, 2096049672, 1997511332, 261178567, 345660721, 572767424, 1311560047, 1411535240, 1762831894, 456167132, 1200459814, 1321072800, 913178438, 292800892, 265886428, 
            1541089902, 1629429433, 1598036279, 1276611313, 878238343, 1319566314, 440459456, 1896896400, 1737053381, 1540856623, 459119527, 264603805, 1371051500, 1998774192, 1525665764, 198447305, 856503972, 822850808, 1152647845, 2059599052, 
            1375006505, 921323581, 1963165634, 1162674368, 1596724458, 1189353870, 1041528154, 939380396, 712098234, 1124801577, 808935963, 687515941, 79273593, 615037170, 1043243556, 1947909395, 1593492748, 1620756506, 2005636480, 880586838, 
            497538756, 1775899685, 409925864, 279780319, 213464171, 844480173, 1640400329, 971256281, 725370880, 1655582839, 1377643928, 912503761, 1314799920, 754451537, 63127417, 718365661, 899883171, 1017112150, 708072890, 1036535970, 
            1399768353, 981008630, 1827733884, 1092032342, 1517537166, 975155515, 1300165770, 719899546, 144322960, 445989915, 1845404955, 496776844, 2041171684, 1236028940, 1201600528, 571612425, 1416662354, 39903777, 353890171, 2043969960, 
            1766023328, 1202208077, 1855602720, 332547758, 2042878842, 1910188239, 1109383884, 1517178970, 1673643623, 1820144943, 714716596, 1536043579, 377317164, 361145585, 77854398, 412474904, 278751055, 735927771, 1156271883, 970276372, 
            785726251, 438547912, 1153805836, 1121217329, 1070028056, 1675611924, 878067736, 1716118467, 874241773, 1177813890, 60525935, 33267691, 1815926897, 857361208, 172564880, 1363978714, 453154365, 1662718395, 1368404620, 948837967, 
            1489477542, 1618601511, 1753096424, 710321929, 9691158, 1917334115, 1763110675, 1159688197, 1065277523, 1680634504, 2137517349, 1341955789, 649340171, 1290343479, 1990516615, 687901437, 315600968, 1429312982, 1173521147, 847699537, 
            1459842792, 949106568, 1714210931, 930213511, 830240788, 1682947052, 1759708622, 1226831494, 807660278, 278056634, 1457583649, 1629267876, 613752703, 801548584, 765179055, 819249865, 722987262, 2073308186, 1866027342, 346450019, 
            1603790813, 1560062094, 1692420051, 557045870, 912624999, 1888680177, 188774380, 1835324280, 832191665, 1415607799, 888916796, 1913070254, 202770570, 1295479664, 1577746034, 746683064, 22679239, 871706490, 1399450924, 778620068, 
            412237794, 155109774, 1048165047, 86269484, 1478390908, 976916334, 734281846, 1265767352, 782428352, 1863174842, 2141936510, 1014182650, 1771151303, 1757722982, 21046139, 491795734, 491960972, 1583779871, 376840286, 2018326284, 
            1105494035, 1383101327, 1418383956, 1142467717, 59081423, 1005310898, 1052585619, 1327820935, 262170968, 503794603, 1134444751, 1927535539, 471813659, 1529634320, 1213920132, 176093337, 2106162774, 820903866, 348611274, 924432554, 
            1774763099, 1184882962, 1703070023, 31035946, 2012723583, 1154694334, 1289430713, 2055925036, 1859697581, 1271363344, 1395868307, 1306748227, 838587089, 1807905184, 1025500188, 1475149339, 448742140, 356882505, 1184807831, 1628868859, 
            707584426, 746921283, 17466996, 411775420, 1111966675, 927594602, 256145104, 2055120595, 1710901384, 748111617, 52125433, 452253111, 1732259395, 845286882, 1150163562, 585912338, 945112646, 1346385450, 194351448, 1903053975, 
            1742076293, 1027463733, 1972947819, 380405522, 519338265, 754633970, 1019424693, 733462675, 257287567, 406529502, 953119050, 1711299935, 1106654099, 1317354146, 925274706, 447932431, 973685909, 800723144, 278936868, 615471201, 
            846161669, 505761921, 1593899301, 46384003, 1652325884, 1845170210, 1084815765, 62768404, 118766515, 249962089, 1722608138, 703502082, 1307278554, 1445456457, 1637789452, 1552287687, 1120554704, 242857677, 279345413, 1365753128, 
            1073579636, 1317647928, 1424192997, 925104729, 854953914, 1409593304, 1455590525, 570054827, 1122633803, 1513775098, 1784054754, 790356329, 952193720, 690766648, 432720307, 2047238510, 869812980, 756691412, 634029570, 1768509671, 
            273526718, 1033880929, 1673469965, 808227251, 1151296374, 1191159082, 107590598, 1609667476, 864379574, 2075141552, 747947241, 441374359, 1788036041, 1055744059, 1690361287, 1392736675, 1243284339, 649070921, 1982409346, 1083312857, 
            1253382167, 1772485407, 1270936280, 2120782841, 1892559438, 2108144190, 391873696, 1651683936, 1074051114, 1421637994, 1631333316, 1713548868, 111058660, 1093882681, 956726030, 64633155, 999584702, 1888764300, 75279858, 154829981, 
            314050705, 1154985817, 626140776, 1730636077, 707370778, 1658514709, 228024110, 1042762843, 1874084913, 980454641, 142155990, 578924733, 325247799, 692636451, 721992488, 347304043, 609319280, 385391575, 1451534524, 408538386, 
            401065879, 560232034, 630884508, 956518175, 1259928963, 1279398862, 1564099080, 869170523, 1344805553, 1032261300, 2039569698, 1287616458, 962988459, 9097872, 1996726499, 203669568, 1428089507, 171847085, 907821732, 1372225298, 
            448371354, 1314653803, 2105796252, 1424730931, 1469915819, 761940041, 855698807, 858604734, 2051065744, 945128227, 1045863223, 2052964683, 895708516, 1118459104, 101720094, 901375025, 393341333, 121924590, 70217229, 832543033, 
            359472177, 66950751, 1755337866, 727562045, 1060821568, 2139991730, 1065849214, 1334322734, 1249690930, 1159239177, 781917528, 843216297, 796760638, 369885511, 2048740394, 1547918707, 1650234183, 204428786, 577855925, 1954593692, 
            1470690475, 1023720819, 1474733013, 1727859061, 1845482921, 887237226, 96988311, 2031351546, 1231428041, 1346370388, 1535586878, 1429281169, 1385052939, 1813351080, 1424504225, 1303951229, 468215361, 1524227027, 119099354, 1659752810, 
            454384016, 549395905, 692655503, 1916845251, 1941144940, 106961634, 2123395194, 1193141716, 699762550, 225356384, 118104272, 2077258282, 794298220, 1821585121, 1153999662, 317490978, 2108253855, 388112188, 935983758, 1510868305, 
            1203155342, 1343582023, 845682198, 446509799, 757734843, 692995821, 1624857867, 404153384, 2091402095, 1683298611, 1061838869, 1889158753, 783407387, 992671232, 1315850247, 1089672404, 1518636536, 63481000, 1009891781, 720479976, 
            1397370430, 435363270, 523690589, 1446098406, 259717952, 1455221432, 277756539, 1940644897, 1426672060, 1120925965, 89185711, 836942189, 565746098, 472069185, 1638844278, 877011325, 567287996, 1770687822, 2065656575, 63704586, 
            920439219, 1399584157, 122014647, 1248831497, 616293156, 1505031421, 963453558, 852470055, 1582593618, 1689150392, 1927961432, 1936486466, 793722909, 1508666300, 1159164289, 1356211681, 887760566, 1276102293, 706271437, 103833406, 
            1625985646, 396069085, 1289455215, 1840383639, 1331828171, 2138376575, 1262762962, 1863269838, 1039608501, 1284558276, 1497639713, 722823695, 272827222, 262692328, 44666814, 1603801735, 2108318247, 1720163112, 1456922313, 166237185, 
            687381789, 1551619124, 1076327242, 1054202808, 500061639, 2132196313, 2102900067, 1461899550, 1755460640, 1633640931, 203818465, 954621529, 2069805941, 2099471771, 1771648357, 1127918348, 955360434, 1725434740, 1856751185, 2082544490, 
            41414977, 1524852537, 1147053837, 1510127527, 190682734, 804543628, 538198148, 1265697762, 1254783894, 1194604588, 2091630491, 1110351705, 963796766, 591687567, 445910463, 1452963008, 1097401988, 945964742, 317460019, 1928980178, 
            876225808, 2017377385, 1339362432, 1783243466, 405542724, 406167931, 983254033, 1653221643, 906514263, 273208575, 27057885, 1317482960, 275136036, 757112761, 1291873571, 1406738759, 1584115993, 17980934, 314899709, 1003772837, 
            471489218, 78617606, 488858736, 1436921487, 1226590497, 848701275, 958073732, 1465575926, 917011259, 877740225, 965778281, 881879624, 219121482, 1007155455, 409410930, 1457372498, 2118900786, 1633157335, 1107615005, 1906564538, 
            74882278, 752427830, 492915580, 1782361070, 707694982, 1164395933, 2122301804, 1342415194, 737800333, 841588902, 969371577, 1808826143, 1845862822, 1799882381, 1920432830, 210925007, 881126056, 512983771, 2056934704, 1370802819, 
            1734418477, 1449775300, 100755101, 844593255, 1054298812, 1647059243, 584230776, 496227330, 410309215, 1123125321, 1035637890, 1147057074, 1272361050, 1767520465, 53490175, 641302737, 1541416938, 1810744119, 1201419239, 551203869, 
            1572876145, 1343669096, 759396079, 481207949, 746299643, 846582303, 340730999, 603957390, 421434101, 1975626152, 920699738, 1010902387, 1840282740, 1924400422, 356432879, 2060112213, 1207410428, 401459104, 130303354, 1167628886, 
            1198391492, 306511718, 1110693035, 677150995, 528612371, 1310354672, 1737823936, 475511855, 1591362933, 1198106481, 724127222, 823047325, 1423048539, 936051230, 1617779142, 1521558496, 301450722, 680440297, 1772675949, 140404581, 
            48239341, 1803335815, 704050579, 1853133992, 2028138737, 1952531526, 671504882, 948899231, 1781104726, 551423357, 1392006733, 1810090525, 1996018662, 654475302, 435540054, 1983146749, 2025164044, 1769959051, 2112582109, 2088056294, 
            2078024632, 560923545, 69173738, 496347883, 1875328524, 657786870, 241832630, 431999756, 1433739602, 1736977316, 1275545315, 339857841, 1224117024, 1591464392, 639224658, 395325328, 450948491, 1714389786, 1898097078, 643258970, 
            964611087, 1197561225, 15391645, 2124780123, 367154532, 2036515722, 27101936, 1830856398, 784547935, 1528765738, 1353126187, 740609230, 910429226, 228560263, 859071006, 352961833, 1970482277, 608340010, 413254, 859637243, 
            1851004253, 1632381994, 45574104, 370894995, 176744375, 1299458525, 835623798, 779173677, 2053702847, 814793547, 25017092, 344743244, 508760229, 433137111, 91391385, 1012030630, 1171090832, 1088175519, 1988641850, 1241692208, 
            1369795864, 297178399, 1773676538, 57354835, 1307633048, 288851380, 1955956651, 1875562719, 1535013780, 1053014574, 621421009, 2088545946, 615545015, 1289111735, 1483556365, 1983695516, 1718886649, 1179042127, 1368909323, 677073808, 
            1451219152, 667745502, 1727585522, 1430719013, 1510921987, 381336510, 1314855015, 2135064067, 211630577, 861568577, 1016153392, 21778316, 656452146, 539429336, 1280811062, 1559097375, 751068395, 946620758, 897788046, 2093778147, 
            133160552, 249004837, 535030928, 446549328, 1176725460, 1407485946, 876814698, 1898359827, 1281465977, 1107743008, 1149943009, 1759518806, 339033247, 1520756361, 8687964, 1785476870, 1873687416, 186407260, 1569515765, 1899766440, 
            243352492, 869609556, 2061403194, 465127960, 2144472863, 501673671, 892855363, 1542304652, 797836087, 1902178908, 2035504042, 1240823232, 827260548, 2103119214, 514820036, 1360044582, 2066636134, 806590614, 1326418177, 1275328491, 
            1380304072, 1866923980, 1867340661, 16134089, 2097034726, 834043208, 1201312639, 572715879, 802213293, 742471204, 719766063, 912435901, 1366350880, 1731073974, 1784740093, 85235220, 401516708, 2103472585, 679768596, 1440062612, 
            101453219, 808588801, 338333788, 2074275243, 815207020, 419871652, 1328438634, 471799896, 715033877, 192461225, 753860736, 1338666302, 648640648, 419326837, 983079387, 861553965, 1211835476, 871770217, 1453052806, 1467300535, 
            2080567060, 355993541, 1596506332, 990079274, 1066373574, 1795673662, 1043622185, 338248481, 615239478, 2105030555, 810565408, 440970825, 428869140, 350443340, 2044706442, 1848849974, 123958614, 78781040, 2102087977, 1415267770, 
            631834741, 131429544, 1022140199, 1630360356, 1061665436, 1892368128, 395332964, 2065986212, 1613396309, 795199101, 1699717642, 1045248797, 112221387, 1061823617, 957475991, 1023811108, 2104535524, 1850611799, 975242179, 1577817144, 
            1882257193, 1769871698, 550242019, 560257116, 1120914090, 1279214243, 1156953130, 804423992, 2044048447, 1177818574, 1713005807, 1657976639, 114387663, 522486957, 1234710218, 569364167, 981580435, 1601250044, 588933320, 780207172, 
            1088141345, 1294669632, 2000340705, 2048986608, 1403044015, 969495628, 527867684, 217380832, 143180139, 1210504978, 388781906, 1566495522, 1253289479, 1187018058, 703812909, 220625276, 68147349, 1829106771, 1864894043, 1940471300, 
            945127393, 179019980, 598425321, 1630988828, 808327678, 1905081848, 202613735, 1429589537, 599830821, 365132655, 868712602, 1187702366, 772558352, 1121726866, 149996894, 2037363315, 1132860565, 1880059359, 511354439, 856488436, 
            1172997680, 1971895495, 611505262, 1703362610, 434459470, 309745460, 1472762741, 631689996, 1576547291, 351446537, 1996418711, 738688605, 1357353678, 315603896, 1841922261, 1960640988, 1631086062, 1622279535, 1534563191, 1890380843, 
            1990536278, 1118706988, 2138895584, 1885811194, 1138305023, 1347463864, 1083898178, 333197118, 1923244699, 448693531, 2071074726, 188582995, 1179099114, 1299887802, 1580491837, 1997350304, 79574875, 1342927865, 2088194514, 1639314777, 
            333415981, 627382975, 1509040102, 976503447, 760031133, 56863244, 1625789445, 589269197, 1381169037, 888867731, 27047115, 2099294992, 1268786861, 683347789, 938804566, 1612880417, 728382094, 603447868, 1456130592, 1934594749, 
            850214501, 73638034, 1885759914, 1499487409, 1537654671, 226182032, 953345962, 1229413472, 1173397189, 195406061, 1942570428, 495690931, 1492673423, 1834351392, 1729199538, 1144662745, 285556876, 1460492916, 46713013, 669575376, 
            1119077807, 1972530761, 1544539805, 2006679996, 1005051697, 414469922, 1455406688, 1124322926, 1380519572, 1645428977, 1113606672, 1587448031, 308597810, 551222687, 798752051, 583316863, 318612428, 16987861, 477060482, 1300192221, 
            173756682, 593256158, 23934267, 395206020, 639666383, 1104240867, 473414765, 900671317, 710883237, 1806576437, 2005995869, 593155676, 1973001012, 1932854278, 938656776, 68289333, 1954852134, 1391258348, 1100355470, 340208116, 
            1620765101, 92632569, 1922403505, 2052778350, 288469088, 1364693553, 1584208214, 1492716485, 1813299740, 574385590, 2140516570, 1552826384, 867525767, 411250318, 493343874, 139937925, 1802670268, 1639900303, 1908887107, 1940456061, 
            1914834794, 20320146, 1328942141, 1681304631, 354904244, 905885202, 772034494, 1292864249, 345801665, 1848134684, 173951016, 1893087, 1684270694, 1315009661, 2042325827, 2019783935, 1392571406, 1775404171, 892634181, 928219064, 
            1732750570, 447964071, 431853727, 1084912121, 1277803759, 1078075447, 799788366, 1107831664, 1914049426, 1842745660, 1469526013, 550771275, 1202106331, 294699005, 1982756210, 67600392, 1819048725, 1637361156, 526069672, 191855164, 
            509752952, 331997470, 1869653774, 2082493651, 1385980251, 1407564714, 1752644416, 613619006, 1943076592, 1506500083, 772953973, 1961333204, 1774947814, 1276878192, 370796277, 1542928065, 84094364, 1280077427, 1105960276, 342745203, 
            1058457081, 2031441501, 19960025, 1961936550, 1069865308, 267393724, 2087537718, 369742821, 2499473, 205209643, 1962150037, 1065554468, 1549072456, 1315326146, 1230956027, 2036623165, 2052910233, 1821877984, 1419844993, 864680381, 
            1937450598, 661568380, 1208897069, 461627322, 1279114564, 1096451591, 588872275, 815079096, 612925436, 1993492799, 761677209, 1097969090, 863566670, 1260501086, 1758873743, 1136660264, 1940517012, 109220474, 1744479472, 694247746, 
            2077815914, 213654132, 1633011237, 156592405, 1422190403, 1418394070, 1022090743, 1761706869, 1490570084, 491793449, 1166174182, 2045129423, 1041516933, 567480142, 1712198887, 436598601, 1961837453, 101828293, 2098192925, 453114425, 
            30269506, 1198447044, 1192406594, 217142890, 1880025560, 533530881, 1662759792, 1680411042, 852115510, 261172658, 602632142, 511713986, 448341773, 1096291892, 286307870, 863733865, 1174273611, 543396298, 265389426, 220398019, 
            302641811, 649636840, 2105471778, 94152813, 166531410, 311347441, 95716696, 776803635, 1393808341, 1804313087, 1390819205, 2057714041, 1376958824, 1577685334, 710398391, 1301277081, 127966787, 1382349968, 782413479, 2130188242, 
            953226622, 1370391903, 1859099789, 1761096152, 1288463151, 1905138981, 701364569, 1388615316, 1367110043, 1478204157, 30797573, 545417111, 886601181, 1262388078, 125343094, 449328291, 1621768331, 1797494160, 258168963, 558121862, 
            417621863, 261238982, 433996158, 535309245, 641212648, 457785452, 1424146, 1606031715, 99447892, 883877700, 1152526177, 844768598, 1661409298, 1943109185, 945971505, 1784839332, 564104905, 355775666, 1780026081, 1959799016, 
            273753832, 1824356281, 620736485, 1192721728, 1264547619, 1802346920, 2058579299, 76010101, 1155382801, 436077225, 1955939848, 782947242, 1228328901, 1376529227, 819340259, 731654716, 596679237, 1141063944, 2076370702, 2016540188, 
            661355694, 52848331, 300658670, 1547111734, 1255152112, 406304720, 1112386256, 468259671, 1773425275, 863869939, 1250421423, 848092377, 1613000743, 1158671452, 182251679, 70812606, 947848949, 1852338573, 1613060492, 235967741, 
            2103677078, 1175564203, 1581057869, 603309205, 1083390193, 440501281, 2079513741, 132295916, 551302721, 1306730130, 2132858588, 2124793813, 725893051, 456168568, 540898815, 1989059023, 290940334, 494006554, 1400182926, 1319325493, 
            2106263751, 1934094242, 1662041124, 865539931, 1779332621, 353368174, 1518577524, 386871105, 1787484526, 2007288744, 2045882392, 1100077157, 333756737, 1424768127, 363352342, 264472141, 677033274, 1329077091, 1096715742, 1697264793, 
            908363703, 1588458775, 1782118201, 2103380871, 1635884069, 1293959511, 1854138058, 1723855791, 2075764078, 1165636288, 1418598958, 1486938689, 1558481511, 1660070857, 124960431, 1329941178, 2146285089, 1440268941, 1710768082, 1281895548, 
            979363329, 1694159206, 976551936, 247422074, 481263297, 1045661000, 100888624, 354931309, 1263047365, 530301681, 914775623, 1136129672, 2124374654, 1685378560, 35655574, 2107639686, 1726112869, 1287782971, 1295661547, 769576075, 
            1204356921, 196048417, 1860526452, 280762728, 646274916, 298070969, 1024190797, 1810088515, 1875718963, 976705121, 37117259, 407362082, 890806050, 798105511, 914989488, 5326483, 2118730975, 1087302871, 2098481108, 121378697, 
            655538671, 1716277028, 121113556, 397053216, 1367867550, 847132826, 460043171, 1008345100, 630933602, 138451266, 2122243720, 1382401348, 106202646, 697331738, 1128779841, 159133412, 368688813, 1696435544, 2042947972, 242419523, 
            2026879649, 315297974, 1575341476, 2104530605, 1201241551, 955567526, 86709833, 2025058564, 1802055856, 1834265874, 1990473075, 455497653, 1732939842, 1317350685, 1887728484, 309221509, 215203162, 1859726829, 792069490, 999423776, 
            1982840445, 1649171049, 1771564720, 1541825319, 2032520428, 238284479, 566611936, 1270460349, 1781208995, 327785066, 1091347007, 576641634, 1853915651, 431544334, 1520081246, 1521923503, 163007481, 97334954, 1283808467, 693556665, 
            544690678, 2100247105, 650417095, 612550326, 494631558, 2089316947, 1641825444, 773918293, 1665327669, 834146043, 1901123053, 204524889, 1662964707, 1738660203, 1932870500, 1177096082, 45459254, 125598670, 952195522, 443422019, 
            1350918000, 2034866684, 1277544419, 2043758327, 793042638, 1079811792, 582149258, 1352954281, 611846388, 1397824516, 398849084, 1738292014, 2068247463, 656604273, 2015157502, 154802401, 2065134182, 1705679495, 1698008866, 1195465257, 
            1170445019, 1522610986, 595926674, 324784951, 141869202, 790910168, 913954410, 969134324, 363604054, 38166833, 1934827808, 615124355, 1630530686, 1205835782, 1871808891, 441896987, 79985843, 1875170866, 2113776772, 1640555526, 
            205824842, 547051425, 2076847190, 1110969944, 1450390976, 118012063, 2032081666, 1966619882, 1250741115, 1048639456, 1228554572, 1474442316, 1292290521, 115492895, 1539801546, 1843043532, 1171508017, 1649670161, 2018182320, 995146851, 
            33727214, 1997280686, 427209029, 1040308298, 306585213, 417226845, 620176431, 1819073347, 1768661413, 659364684, 1495771395, 967548982, 1758826113, 2092444101, 430723117, 1154780978, 1682886841, 1755273112, 2047069420, 1098650142, 
            401628106, 733635804, 1597851110, 1607446871, 1418513414, 370989928, 336705742, 1116769921, 1752305522, 1683076423, 1391275562, 1531008214, 1580031740, 2049498144, 1858170779, 481476130, 1218583703, 1041420580, 1642568491, 1156724508, 
            475262470, 1921290456, 498024635, 273201715, 437460168, 1261739276, 1832822411, 300716403, 1805962854, 1104121969, 361676453, 714619298, 251724843, 884542266, 544332108, 1149637064, 569565329, 1420745478, 1451949849, 1354184994, 
            249931094, 143535466, 1654173328, 518181744, 1052192661, 394608446, 19007471, 453106386, 437319705, 2052434959, 1304279050, 1728674603, 1369587853, 881868273, 2138117498, 1549705918, 2040084556, 1532546326, 1867484128, 696007198, 
            583149924, 408612995, 798132899, 961877110, 279099847, 1108157993, 978414146, 1458351488, 359696043, 672514402, 807352989, 1425772328, 1125813965, 1717152460, 255466357, 29877508, 1654055723, 24253933, 2122341048, 551364962, 
            832521123, 547505999, 1530951398, 1260210971, 1226018370, 302911714, 1731510765, 1522015288, 98203401, 667449148, 1107912843, 430732979, 1719729745, 446331357, 1058322092, 1092387034, 1795320263, 1713051604, 1278353539, 1510972438, 
            650705714, 979793057, 1683657569, 717401621, 819907015, 941352576, 1044476997, 1149691277, 510975895, 1303199598, 1809797485, 1151156964, 342360654, 694480171, 1196466073, 271431127, 1036361953, 490473474, 1381363397, 1826019054, 
            911840643, 1295829725, 1954699161, 1426660301, 1397211055, 1293195462, 1971617685, 849757829, 1912315314, 1542439937, 1580540823, 2030028538, 2098428059, 1981552569, 1549005325, 1719904280, 291649562, 227983369, 628411069, 97279255, 
            830219820, 923447148, 1959856067, 774399400, 1680252398, 1459053536, 2130660539, 1592583650, 2034801244, 1977616942, 715260490, 1652682933, 1448707746, 1577381517, 1870333972, 1718151197, 584052246, 1303720784, 1828298764, 426621690, 
            421257703, 1301995472, 787564615, 1837232459, 1944756585, 378118892, 2090006195, 1963225261, 484292336, 330827591, 759788032, 629064591, 413403892, 1991982923, 1612158759, 1151177969, 2117604299, 127417175, 1394069954, 1660260983, 
            651295829, 1334407597, 866838155, 427633994, 4459058, 1744193317, 397676424, 762001530, 741735784, 1966566869, 1506727934, 826872205, 897193623, 922885555, 381323991, 2011717535, 1220460923, 287514587, 814679648, 981481855, 
            2054194478, 1388906886, 176118411, 2002524086, 326847521, 564045833, 900352157, 16260772, 445025325, 120815192, 24067727, 945750067, 1168860632, 880181418, 667545123, 49296497, 1099402372, 328584599, 1694437732, 862694887, 
            1682465105, 442962255, 1520945282, 2096228070, 864581472, 1226287387, 2085907089, 567221402, 1735766973, 345230583, 827199373, 843051480, 1667778491, 2036103497, 384324346, 391876451, 1505545065, 728047701, 1280336570, 552024145, 
            243288183, 600049297, 1329689147, 1187706305, 210712234, 695014652, 1401711064, 835241744, 1424741765, 290929834, 1966124519, 1884599991, 438848063, 1732675181, 441947634, 1120483883, 535715551, 1597864981, 446023755, 473151431, 
            904120238, 1215134272, 2075428564, 558649679, 163186361, 40817453, 1169466971, 994898686, 990831767, 1154178393, 1040357205, 830731274, 768560632, 293886596, 1594908007, 651777922, 1113517126, 1112142374, 1772699168, 1902585983, 
            162307711, 216092515, 1153872884, 549827137, 1292814666, 200194976, 1450986828, 382924252, 1984044594, 487120907, 898843307, 2019229793, 1344891674, 1174308689, 1935276529, 471067082, 991638377, 1975136320, 1407644421, 454626424, 
            744732145, 1732181921, 643102655, 28312498, 778550184, 682023982, 1847963629, 294058528, 798949769, 1812930939, 346831982, 2082973967, 1429569688, 390763160, 1526654045, 1758516294, 1393664994, 1513362560, 1548441396, 1170126844, 
            518867036, 1693946489, 650959151, 244114715, 1429152034, 1308094322, 1263654349, 1747872332, 376965819, 179842105, 2101217302, 584347915, 274835745, 1420386917, 833821760, 293185860, 1308908390, 858186502, 720423468, 1185027143, 
            925468104, 891354110, 1682684193, 315503718, 539384178, 1161661458, 1929674630, 1986109245, 147513607, 1353513059, 1035597407, 42548185, 1388516222, 196986876, 545652977, 205375695, 1988939243, 250207441, 1442477688, 1292164422, 
            2093477759, 1527781845, 1467877419, 1298404737, 587644336, 810407368, 78941703, 693783302, 1541433777, 1700617734, 515864379, 566484387, 898417988, 548675049, 180297677, 1539605211, 1289953362, 551610711, 1661328944, 68850438, 
            1915300710, 923199991, 124607625, 557557212, 1527092981, 1259503776, 1521591506, 558024231, 1113310668, 1078671073, 1854234275, 249848100, 99650462, 1441780192, 1316166992, 543166594, 1139441088, 2118285303, 2046928254, 940771579, 
            2087166765, 1867317107, 1934376541, 172716306, 383448341, 65446645, 1794739712, 1112291879, 1842836168, 1489344594, 1318378961, 1454472888, 1444695476, 232086607, 1802187264, 1402360970, 1222469573, 909196406, 753175862, 553729559, 
            454036262, 1702268307, 141095538, 823199128, 162847502, 1765085102, 1836822889, 2097414472, 730753533, 1834339238, 1344074595, 931805808, 650603870, 537171496, 1120533368, 305234681, 1936387328, 1841897800, 515560511, 1916442548, 
            373832047, 117787563, 1233828487, 2072558797, 1544947375, 268773333, 1051339949, 94803230, 2100840906, 381550353, 1955465678, 1547709485, 443122978, 1875867078, 275996544, 1688291153, 1536777927, 1929389214, 706324802, 105464064, 
            2112003520, 533662072, 517565544, 693267689, 1590722209, 2070517954, 477280159, 691529830, 1670060723, 158986107, 988153126, 1535397130, 1335378440, 183881679, 2036349148, 1250632943, 1644950002, 1866849577, 1424568057, 56309948, 
            1324328793, 2031421960, 647616684, 934957114, 1902476903, 229507830, 2086000885, 744060708, 614957575, 1636734672, 1289186476, 95009594, 1287769104, 328277347, 1340043274, 921779807, 850901255, 806289791, 889420024, 1804104278, 
            1164624949, 2091238604, 841364331, 1600202002, 1691430435, 172497073, 204948401, 745258527, 1721753364, 331455389, 1758617923, 469317670, 1122708006, 934377675, 456489080, 859605116, 1696784132, 2126778923, 440756328, 1993767340, 
            1622652710, 1815336738, 880072836, 94241956, 1024946136, 1623970521, 686726705, 886029332, 1833364639, 883550962, 1601461882, 430451601, 1062081722, 1163971989, 1110242470, 108280116, 1926514324, 162485765, 1009904769, 1881221985, 
            1194792225, 1584620567, 706238918, 1127658666, 986037876, 1434242590, 2027042325, 1164732233, 491057139, 778647492, 2004989214, 1171490391, 62023327, 1931600765, 1430142159, 694182945, 208760499, 654622568, 372359689, 254579197, 
            1386973275, 1622477093, 1193884649, 1629853530, 1974615335, 945426875, 683447460, 438697984, 1533437890, 1469852263, 1751561529, 1033341546, 1970172086, 124235499, 1866627630, 933836344, 830459745, 1950709295, 673455596, 1773386550, 
            1606356486, 2124593194, 1636521729, 764939735, 1508013907, 1209016983, 1749242910, 1869511465, 1135257087, 1603408289, 766346160, 1568651651, 481919860, 2013977506, 46937269, 1767005115, 267029132, 809826877, 939731556, 1125400841, 
            137023698, 1908299832, 1413623610, 2125981400, 982589489, 1035115035, 896292468, 1116615316, 1712881640, 1642522323, 1560615877, 611015856, 1965988276, 800352787, 317104203, 1248306584, 1244652500, 417585272, 1504249409, 398472264, 
            1518520267, 2004263768, 176253869, 654952105, 1818195229, 1229244255, 505362101, 1362856194, 418898776, 1704191058, 1156792402, 973511192, 1176804616, 1745495295, 869583201, 321760900, 1011168346, 384425532, 1296977881, 1475134489, 
            421862517, 372106257, 320507382, 856204877, 373908853, 514944447, 533891410, 1876103643, 1515572558, 364566475, 1019754090, 2018510665, 300438931, 640883294, 1049088465, 1818718863, 1056882104, 1619383588, 1343481348, 1956250452, 
            1913138348, 398946636, 492235769, 1670118271, 180517445, 1760328175, 984881094, 928574805, 455227700, 2102727435, 1014310140, 927052662, 1344731736, 813098800, 875496128, 2006656307, 1965257545, 575450512, 213205826, 1417110902, 
            1285118547, 386854088, 1826901057, 1838708915, 421282035, 1861114127, 2037709241, 1257627038, 903378651, 537562143, 725817270, 1874219663, 606055765, 932341437, 1201472573, 402401955, 11180918, 1124426471, 1204182501, 1287529761, 
            425508479, 1730730974, 324623014, 1222687050, 1355087858, 2052913521, 1026783262, 1951026186, 533459106, 1050175965, 965042800, 27364197, 2034361602, 1196137225, 2045857376, 1463217191, 2098900906, 1167028361, 1667163497, 1494972501, 
            1816861698, 156527375, 2104408085, 2095241694, 798727366, 2065156013, 251526958, 1723857980, 1755098222, 817181109, 2107112627, 51321113, 1852614012, 1167424278, 204577478, 1369611587, 1669086899, 1418087712, 1873221936, 1167857672, 
            1237227622, 307142636, 444219285, 1861173653, 1179981851, 673341879, 1854552258, 1033250720, 1251941987, 1827305920, 1434428908, 1078808170, 1807163215, 1607211310, 1210477774, 1710111557, 719719915, 302475493, 773052067, 1998699640, 
            1025085320, 1904631786, 1314621950, 1186252747, 216101375, 1482471547, 969119530, 497195351, 429114296, 1558615191, 1387930820, 1007391808, 282914734, 1612402863, 617763990, 525001106, 1573657793, 1396042954, 719307754, 1065101776, 
            842310929, 1364885947, 1901897602, 537408307, 1715090566, 1512111984, 710890300, 1036580302, 1081614633, 1774427148, 1255607463, 1837530683, 861031986, 447360732, 932791645, 1246165457, 1552671014, 534524293, 430277730, 317585165, 
            1590828349, 1336324112, 1464897652, 1320768773, 639017143, 1505971110, 1081943661, 1494780182, 1341025179, 192233961, 465714282, 376713596, 101533524, 873020937, 1766836847, 1382278900, 2136248363, 1667801718, 883043882, 1294731048, 
            623895617, 362829568, 641240182, 883609578, 1007206764, 397511276, 391278541, 1351709926, 771580079, 195930431, 55952957, 579410477, 60781046, 252039590, 690851048, 980955696, 1682916022, 1637575627, 1775772775, 1791813562, 
            81922751, 1841259147, 1497554997, 243262846, 1124607437, 714654786, 1855385087, 1604528410, 1247755758, 285243451, 90268732, 1644744752, 996533527, 401125807, 150811789, 323141123, 1716151038, 1662934010, 352082354, 386357376, 
            2091497747, 1935945999, 2138769695, 356584087, 1609766472, 1122920685, 1827394316, 998467804, 387579097, 1970678506, 1357326098, 2016172483, 1311150858, 533969803, 493552019, 1616378479, 1127227102, 956928886, 1162277095, 198848922, 
            732616815, 1883759450, 993266761, 1823559753, 1459937854, 1249875174, 1012568548, 724772285, 1919724185, 836326104, 79875111, 929617942, 331962027, 2089948652, 89596521, 232238835, 1011910311, 2078265522, 1485553886, 680645076, 
            425843394, 17412902, 1242129932, 395062624, 700323254, 1669098479, 536819565, 1420287311, 714173266, 1113124001, 2009265716, 744642540, 498508510, 1496513339, 583072761, 943860503, 251360310, 1604275691, 515248047, 892820705, 
            697662146, 57474006, 1544989062, 321262785, 937004315, 775382846, 2025546166, 1629944512, 1697815382, 92917714, 62191244, 966924096, 1682733506, 1230268483, 982075827, 1583508432, 1541250996, 2055084731, 4353199, 631318106, 
            120408873, 875619496, 2038868957, 776175515, 866360094, 166316895, 1297562748, 769789477, 638659530, 509751408, 834432657, 1625912041, 1119406201, 1685974664, 1309252025, 1281948380, 1894109121, 1145108087, 619552801, 1828005692, 
            595060636, 1087674150, 1494858116, 125406050, 1415295057, 1561713109, 1901327029, 800863183, 1768157057, 1593330448, 1965194482, 1226571466, 1233204272, 98249906, 836902380, 836946403, 1382247457, 2136076617, 1803943971, 803271414, 
            2143817491, 1485953940, 1264119689, 458289357, 1153803367, 455674370, 1633402274, 739478270, 1789709128, 139197189, 331191644, 1432449491, 527232330, 814383502, 1771567288, 894995551, 542028079, 151401819, 412884749, 718803263, 
            2118443741, 1144756191, 1951791623, 375085217, 974774536, 850652536, 188361055, 1237646482, 688725492, 2095109901, 1294339708, 773795213, 336587270, 388163644, 891588335, 1713309205, 1659351051, 662749217, 1503605867, 415914439, 
            1016890531, 1924179922, 636567863, 1611497060, 1823154678, 57478725, 1146010348, 1183170843, 638140649, 724111205, 769859455, 1745205299, 871975942, 448443702, 373585867, 1443862806, 161665533, 751160569, 127349022, 1347651135, 
            1605393291, 712585891, 1018492354, 245506504, 1576182558, 966502699, 512365399, 811425696, 1380095284, 2086067720, 1895370692, 1845184067, 2001775450, 1352644598, 541207512, 1989762919, 1323421779, 434208037, 161182413, 653943480, 
            331625371, 882366022, 2019211841, 1875642950, 619021023, 1071164751, 1925149564, 889145380, 572371943, 1704683067, 948319941, 1033433511, 2067782192, 1284886502, 1343205133, 1068171622, 1933889745, 1652820944, 190296634, 916528317, 
            1916959212, 1214841571, 1850549184, 768940908, 1275209347, 168689147, 230296521, 450106461, 1716449258, 1659278490, 954696858, 1242667272, 1649337593, 2041536311, 88022709, 1695089955, 1194771087, 1435194399, 994006824, 1088798181, 
            1415741656, 70476470, 1812602653, 2081449595, 1483215998, 1412445328, 1209179101, 252895689, 463692615, 290485393, 796023585, 1857306495, 1636017298, 1032546680, 985482713, 946306015, 2144618616, 1050976614, 2058794501, 1558396685, 
            1949348064, 240722094, 1252910811, 1934418400, 1135239931, 786474243, 855258504, 293594245, 1456336363, 1110640633, 592429828, 177842423, 1589843145, 1326699230, 1793143119, 67797229, 1416287690, 647710765, 2046924463, 414136006, 
            291713577, 1462394367, 1110432198, 77173926, 289142310, 1543517230, 1567768803, 969450550, 1983983284, 1705851206, 1323551830, 1183858172, 2096690663, 1212777543, 950938450, 129052642, 2060453603, 578072574, 264830145, 870469869, 
            1165719754, 639499485, 140332147, 806514798, 1004540827, 620242616, 1088304341, 1847381442, 1161468362, 1157063248, 1379468560, 1610033295, 1203258606, 884726907, 467646573, 1951010744, 649804841, 1973318582, 2133732015, 858768190, 
            1086063535, 727718666, 157097700, 182038058, 227704870, 1529870157, 415076552, 571769558, 1777907362, 1103867618, 804998977, 1349279975, 222895791, 1615603520, 1091694693, 635624093, 1784020019, 1286143814, 964873378, 1114914169, 
            45179405, 130328091, 898704292, 1330018424, 384876989, 510373638, 435056127, 1720664892, 693232578, 632150261, 1097752565, 2042707145, 405381929, 1043358745, 1524616464, 1187432200, 237353054, 799735293, 891836082, 1085745784, 
            56262111, 1299161106, 1455491297, 386035559, 1029159651, 300975415, 1893951091, 1537491981, 1176786156, 1850231525, 244952017, 231416709, 569363021, 2105952447, 1264134726, 919578692, 550019215, 394139503, 1536018831, 79945999, 
            1491829705, 670480209, 1258693467, 289363378, 1147646115, 258327152, 232261246, 1956792928, 1927576300, 239633934, 2087004937, 1314946577, 1393038609, 496169058, 2071503111, 169476362, 1117574633, 1545020707, 2142673886, 857850440, 
            1311378357, 1425432585, 527176712, 1857732913, 269605987, 1641548368, 820112254, 982459929, 337682640, 91426210, 660701878, 1576115230, 702372937, 307287765, 1540492777, 1426218948, 2067353742, 1856506650, 783359866, 1077958272, 
            1670336864, 1063579622, 2077018479, 488756683, 1561928380, 643065115, 1317142260, 581787329, 283147346, 645327278, 881735400, 1723432624, 213271167, 1694013585, 1744200269, 384795964, 773278648, 655110259, 1684851491, 1049496157, 
            414186625, 581892108, 1180278416, 1419575895, 1769864234, 1498034753, 1879934426, 1157570345, 906774366, 580260241, 1849394369, 815800089, 59042376, 109058594, 771105464, 1057647473, 180174618, 353427716, 816348501, 1085480233, 
            1208033356, 1218668232, 1361055515, 1134393037, 895228866, 119440010, 677709109, 612086491, 1299402770, 1528893684, 1771373181, 666989910, 200861489, 1058638086, 823536138, 564813275, 1623023695, 1049884633, 1881838260, 2014459495, 
            417443691, 1411878661, 2042326235, 401028428, 632121938, 1646897389, 193448635, 300813981, 1288787087, 136914877, 497974284, 1850139869, 841355379, 1892933481, 1143923326, 1103633130, 868738855, 1836388530, 790213504, 891309355, 
            1751465701, 1029820766, 2081104174, 1008223786, 1589188789, 1085876542, 1980990540, 150369408, 1159069588, 1845042012, 894937199, 57996664, 304556307, 1107102629, 2076890488, 1057520221, 443710943, 1305797737, 1041066999, 1943311471, 
            1480645169, 1765054141, 1426936273, 254596628, 1156930566, 1444419485, 1458364850, 2118996027, 2095885572, 1428746309, 531318798, 1371171967, 1167009124, 702556116, 1500803030, 836675970, 511083503, 735079, 1295557295, 589377341, 
            1126339633, 325443860, 1515564878, 387261562, 532049107, 1149671232, 476898530, 1727080325, 1386336282, 1456749110, 310201180, 2016035021, 888321497, 439110385, 468486883, 1020008590, 520699516, 97958413, 1524776005, 600200473, 
            783682797, 1175752469, 988361751, 1195824782, 604182096, 339135436, 403220011, 2042101773, 1608321868, 1482530249, 1168635074, 2101555933, 1074939433, 1001361723, 671976180, 279809557, 1171395705, 1673051832, 56664698, 919742604, 
            1278584111, 707420725, 2121512540, 132237013, 863087316, 1509451624, 697610824, 1569200565, 1473136117, 690087676, 718530996, 1234086565, 232026323, 444596951, 639686546, 694261173, 694400204, 1997609692, 35034530, 27437864, 
            1084915009, 1614661711, 149907280, 1521005668, 544413366, 1126394137, 337025791, 1666371648, 1619785581, 192514269, 180039841, 408792743, 8358640, 354954686, 492254636, 519374722, 1745631402, 1229301426, 1036686268, 1664496308, 
            1561697232, 1244500147, 126235469, 1233500378, 1952695197, 1932360146, 2047582109, 700073942, 92983090, 1012850970, 2105253892, 1405417002, 1447568713, 2024764326, 1779088678, 771136129, 2004605401, 1031852982, 1701319738, 395983028, 
            1983464290, 303741587, 2115811120, 1637646955, 1757822958, 2110022562, 42043830, 637157844, 359842364, 1868841271, 263329905, 419230643, 1132394656, 1731016002, 531742755, 359073720, 391736267, 1415307423, 1143449969, 12545125, 
            1794590988, 398864725, 464823007, 78538649, 1696320274, 813587692, 1712995033, 1653264848, 1353301826, 797797746, 980948972, 1999587918, 248730033, 405779953, 1065636127, 1351719236, 1217201887, 343773229, 1043068506, 453513870, 
            159935363, 613484517, 1397603965, 1570882634, 1748822585, 804853315, 1805372555, 1509080582, 1182378059, 2027001600, 590833150, 585810340, 1317718990, 1450153765, 1939209846, 2002538350, 1247935751, 465985487, 1379810707, 874996046, 
            1535059602, 432391358, 303321038, 1680284952, 1749132348, 1464698298, 573142436, 561310998, 611394207, 1892124340, 1079228587, 2120341243, 991856675, 1839745967, 279743221, 762719996, 213738979, 971057203, 1746281021, 893075772, 
            361061599, 744897023, 1238542230, 1346017501, 1067760145, 20559191, 890488311, 1234494144, 309631674, 1094395581, 586232474, 410860845, 1534012758, 605124555, 429542598, 1993898097, 1300015343, 736020190, 1734495536, 812379886, 
            1940818791, 1508442431, 980065954, 1191094176, 235826901, 205716365, 816877227, 1043403683, 2080095741, 939259165, 213207467, 1445531167, 1776407095, 1760416287, 1353482404, 413589829, 1912569715, 838660896, 805519150, 274106253, 
            1039812377, 2145267789, 1476453251, 8044614, 1437872740, 1815870503, 809566828, 650963580, 1228255942, 1424275626, 110136037, 768918995, 1543265753, 1483375742, 1122488775, 1463561435, 445040071, 1911366868, 616910171, 954590837, 
            1092062164, 1141265896, 1530311290, 480500393, 481035415, 46247829, 2062128968, 1134429818, 1475246171, 1864716096, 1012145120, 765580165, 1687811003, 1398377257, 1550286116, 469732238, 1592765453, 158259282, 1478318291, 2041491512, 
            466836402, 179089834, 1717685628, 596205258, 536682614, 189077337, 80973510, 1518091244, 1577955412, 402512487, 503205155, 1595977015, 67872260, 530167394, 21327285, 1206398596, 1562624616, 1229056403, 300822913, 1635754291, 
            2037496011, 417915648, 782335881, 1969060341, 1228865281, 1970104745, 357553401, 738478989, 1688540513, 540578369, 1286128747, 439872158, 1810275623, 1278032756, 2076485477, 1104305506, 1387935276, 1758280304, 1992836593, 1755094427, 
            1241219968, 941451770, 14716127, 621568991, 1138090086, 427912844, 1294103569, 1209740072, 392870931, 1263560485, 727714528, 2024731895, 1951744450, 1443626932, 1071793816, 300922606, 1566629863, 1807418469, 2083112398, 1794219924, 
            977166368, 644341009, 520233347, 886050724, 990849022, 1655980905, 1304016826, 1153621113, 13574761, 208050081, 270138508, 853469133, 1652032902, 1240491186, 1977152322, 1576092886, 115037706, 1591818443, 835066933, 266394619, 
            1323182681, 632346496, 984475873, 757199192, 2083235274, 602030475, 54031786, 1313280320, 747173759, 666097427, 206356307, 1058615653, 286109179, 1473941346, 2097264376, 489405264, 927528765, 1583304001, 63948230, 1072739715, 
            1408948623, 257361291, 1961576162, 1041708055, 1589229002, 1698373377, 1183032073, 1692326452, 283582705, 1915252328, 1629197026, 1462152853, 168304900, 480931270, 890800987, 1765815384, 2004014360, 828439275, 1051587492, 863818225, 
            863763362, 1575294073, 42600123, 1055412177, 1902463188, 1205065821, 774884396, 1014108257, 300395754, 213074200, 387639396, 575966748, 339418990, 383948391, 1694412798, 700769842, 58824934, 2117359636, 269249250, 247217565, 
            1084740305, 70561274, 1083430869, 1822305245, 1869874597, 1734547330, 1608334747, 607119292, 800630170, 1577120102, 962390888, 1867977435, 1220322522, 395309262, 1984031490, 1066097320, 377476754, 328450998, 996610297, 1963840879, 
            1700647464, 1201990255, 1590145647, 1677140493, 1543569966, 2142704988, 1123901508, 619317366, 944879515, 2025759353, 1401490724, 380273081, 1509488683, 1162067480, 1927206728, 1223036801, 531549891, 1263669287, 1525855191, 1842134314, 
            284944293, 492936568, 710466808, 1137776402, 1592330062, 936722319, 1535811110, 94023058, 1338758166, 1130446756, 932728036, 1549372640, 1166861803, 336665970, 2016960313, 40726896, 1843019091, 709171005, 407987557, 883808760, 
            65063739, 2111398355, 721758642, 1014003913, 1818329552, 1498000597, 132911329, 2027438361, 1670145677, 1746583009, 1070078401, 2136779588, 917835993, 1152267930, 693410512, 1951490713, 14454554, 665172297, 196852588, 1052609836, 
            1942144307, 1428802742, 1568892869, 1694894795, 2026757281, 1150018379, 1106241427, 1274472420, 514253487, 217281867, 999420211, 1817306325, 1791234827, 2082222531, 2023705813, 31029044, 36393979, 501194887, 607141480, 1087775440, 
            1621258726, 312455359, 145492456, 775623745, 447765870, 1414257687, 157220778, 173785643, 1286684983, 1522739619, 1402094573, 1714057101, 531667174, 1077240205, 38836525, 836157015, 158555990, 58899702, 1764616744, 781899751, 
            1120268534, 378771586, 1609384594, 1805573267, 345022152, 1686521326, 1507857578, 867213228, 1685305798, 1743620672, 752445978, 382367308, 23328618, 1434794327, 696206195, 1694403189, 759852050, 372612727, 366025924, 1803149963, 
            462688727, 155519192, 209882901, 1017028625, 509655695, 187492945, 1100850461, 848754201, 442096172, 1541474015, 519464942, 2049094852, 1389883952, 1535889963, 413809012, 91932689, 1760042586, 1576148664, 346198813, 368655481, 
            889396703, 380474481, 791808904, 1402461002, 543618377, 1263302254, 302598696, 514402994, 225684852, 1412426779, 150406604, 319245633, 1677874159, 133364626, 1073025437, 996385376, 2137566048, 2137695695, 2113705272, 1724010423, 
            2089502066, 1229991475, 681292070, 216166723, 898382653, 705545264, 1106679548, 299567100, 187237845, 54086359, 1349277400, 1128003878, 724381996, 2033569708, 979772465, 2038213844, 897145955, 222255269, 1392798701, 1514498304, 
            1058907068, 861301945, 240242973, 1758178000, 395001191, 195699253, 654540074, 1269826877, 1197253392, 914586740, 2124437079, 1475063865, 87083301, 1427442672, 372492666, 1091332897, 797413794, 572789216, 588647272, 1080312359, 
            688391469, 1099908030, 62662447, 1559757234, 1892843303, 653491373, 2116517792, 1124685931, 567143232, 1882516260, 839443997, 326757013, 173446663, 1341231406, 1059425771, 1379897932, 1544236224, 1530871098, 1670000916, 2134711556, 
            607756112, 198701983, 1656841828, 814412434, 1000326908, 757166073, 692514951, 1651635964, 142428031, 632087103, 1280778649, 728057262, 1930571440, 160478432, 80377010, 395351621, 2030742616, 1881662794, 1475353891, 771291389, 
            1372730014, 684371863, 58833642, 145597491, 759704630, 1570292279, 1662831202, 1960622370, 856873430, 1606036218, 645761274, 1342761209, 1283430888, 811074877, 787794301, 1685139151, 1149403329, 1879532514, 1989510124, 1872023426, 
            1476048373, 967652903, 1065786310, 1570005954, 279421601, 381099618, 995782947, 605397585, 882211073, 594028826, 451275954, 1901385912, 1157984438, 393775003, 1186900647, 2130897187, 10529095, 1912566088, 939268282, 1920567368, 
            1006780914, 1399200779, 1182437203, 765300853, 1563737659, 1207209173, 2107092698, 2094120016, 1759589032, 1833073930, 825477836, 620060787, 1932419179, 942766319, 428944127, 85139964, 1987603581, 253792065, 956808026, 151575403, 
            144794790, 1993820889, 1905650212, 529275182, 1725702369, 1758541128, 2056741861, 2047931493, 187125290, 1150630178, 1007095347, 767235308, 1649692965, 543264720, 1619973871, 364601798, 2051085122, 1231113743, 1742374333, 894217453, 
            1244415368, 2122962542, 62191119, 615342093, 1439365184, 1482431935, 88639419, 189801639, 1749463181, 931052606, 2074293403, 243745921, 2038161633, 1674684687, 1283252639, 1024242867, 418475598, 84445152, 326165138, 2134179286, 
            1617463258, 347585847, 583586855, 1030199746, 712912045, 941327461, 1781024105, 69678294, 1409022311, 1247854298, 1297538235, 169244387, 262165533, 1805526724, 1924420947, 1462467983, 1830606378, 1080628835, 1447983660, 648899217, 
            1295912151, 1461193031, 625232272, 45064911, 384900333, 1696964620, 786814638, 1126953968, 1676645495, 601587933, 1605763581, 1046376279, 844541972, 119014987, 528876741, 1461801547, 843730698, 1308140965, 1378096744, 350409719, 
            947890745, 1170517036, 568276320, 1178372068, 307436727, 32986939, 1376298395, 1892926416, 192532088, 2064008706, 1703730186, 21098778, 1197401294, 1250168128, 855637894, 2032215486, 135279652, 2130809111, 647950599, 1848511621, 
            294416792, 1545161020, 1130205884, 1261117516, 1116998664, 508478455, 1663338924, 1217661469, 1451467451, 1939617570, 650868616, 1477254100, 1522067350, 713175654, 2119108734, 861775750, 36375390, 73610702, 1071719812, 1706848223, 
            1612609888, 776299840, 1407806414, 326312126, 1775629740, 1515677443, 1372175758, 742181654, 2062839135, 1001351375, 216179603, 431643440, 1762481441, 804278741, 917331991, 449461994, 506544314, 497009722, 1088507151, 1078918239, 
            1566771257, 1763815960, 1369583159, 377375195, 1395188266, 94670505, 102018267, 932107738, 1094147392, 836486956, 220460267, 749512282, 621414437, 774945227, 1545818921, 1338271298, 2039570607, 994904190, 385395877, 1635136818, 
            108797074, 801494783, 89526571, 1894573167, 1792575888, 1569324810, 1894668563, 2059072533, 1394646154, 28589323, 1851221615, 844629335, 409725823, 796524935, 635240636, 303138200, 781591522, 1956104048, 1823526191, 435804357, 
            2125074240, 1097236464, 1047599192, 774698158, 1039568203, 1836744915, 740364123, 2108333508, 1631717761, 1049178977, 1688916432, 1365352316, 18227094, 1915655406, 1433267752, 609386557, 818348861, 1511275962, 1467999231, 1270405786, 
            1044705372, 57264399, 184217162, 577282233, 872543770, 1090204511, 1492355781, 637359640, 684079524, 1644148587, 308275882, 1402486126, 1508405192, 1412993038, 1945804434, 801742929, 1784588510, 81561412, 1760423395, 1596398371, 
            137727409, 1500283372, 1965290414, 2130610111, 53125240, 1363878188, 1440137449, 2038209689, 137334625, 2041268239, 170791942, 28258063, 2094682065, 1017293433, 1363184239, 1492948713, 1669292695, 219115629, 568779349, 1419511595, 
            1171510470, 1662062753, 1929104614, 432148106, 366325982, 86092503, 1925792338, 811235516, 2088029912, 692165458, 1101448893, 1803524987, 2054997700, 594635049, 1644688353, 1063620111, 265690170, 2007776308, 1324464853, 764006510, 
            57205715, 441101179, 1556797600, 1695230719, 1646712982, 1139528959, 1966330566, 1961154187, 1667179954, 517519053, 607474407, 794344681, 1116619547, 383077354, 288503005, 2045463906, 258145469, 1468359269, 1865797539, 1896233198, 
            1839722185, 587850292, 930073258, 1457537276, 1132301275, 1037297479, 166034165, 1846298688, 1781414252, 1388642877, 1133548194, 409519352, 1287925824, 1722088189, 877928486, 650246421, 1428553075, 1656235245, 156268804, 1805713734, 
            1258291465, 962471505, 370414725, 1832048969, 1459095795, 1016383953, 233334741, 4164123, 1251852068, 280356578, 1003084400, 1194725898, 1718158065, 131193692, 111913209, 1277006813, 1783762259, 246464428, 1125574734, 310976530, 
            1754940545, 1146431310, 616649358, 596893354, 1630029765, 779341175, 838968419, 543451969, 433793356, 395457692, 897699659, 900208763, 1083462776, 664104466, 1942387495, 1210257178, 843375549, 1407855454, 1358843521, 1449621386, 
            660328510, 523732202, 2139637698, 1283426586, 286284888, 1578818122, 1602849496, 632512004, 1267298278, 1907843884, 1391894295, 485975914, 74272022, 1892006333, 1768187548, 917938385, 564327844, 1963437216, 1591774299, 274614861, 
            1042879154, 1049773844, 446904286, 642857210, 1215875597, 389513808, 1581667476, 749337982, 2052217300, 946312859, 1593475245, 1740293067, 619799050, 1833395856, 439192929, 1961269898, 945947322, 611201838, 1908344516, 1720093363, 
            1585522978, 1976863213, 151614619, 576631520, 893087399, 940030052, 1791244293, 1892172031, 536913698, 41810529, 640685019, 1175084924, 750006322, 253467521, 464436254, 1520484373, 1940823957, 129614177, 1102033970, 1438904041, 
            558007297, 982925472, 653781671, 1683685780, 1802095655, 922312041, 2032206638, 381744363, 103857791, 666864335, 323196921, 1035328620, 991906564, 1671186960, 1047070156, 556017040, 1428256251, 958271544, 692692438, 867975293, 
            115855410, 41807730, 1081455674, 1478358181, 877695035, 994360075, 335689644, 1199777969, 1501635996, 752876983, 593764637, 1963641218, 1850425155, 237751817, 1865306833, 691912658, 1702892084, 727566248, 1808039132, 1282936512, 
            101275396, 308515662, 1427927524, 797879771, 1557326755, 1964183737, 2044188183, 888604730, 1255478242, 1323208055, 1652229588, 389678332, 1645097760, 39745806, 866590273, 1041865330, 226172941, 670357098, 402232685, 2066348030, 
            151091756, 1102992233, 850959439, 304000430, 933654340, 964699818, 2005366322, 875759568, 195615466, 609281314, 1296128898, 863077330, 1245717764, 487750400, 2109870663, 2096274932, 970484623, 691773416, 906830711, 1039402653, 
            764748399, 731421881, 144377609, 665867808, 18775905, 140122363, 1979157220, 1134436036, 1794891701, 2044591718, 1155460952, 547986590, 681900743, 663887732, 2068043432, 1281848284, 537496372, 1910828408, 1330015994, 1992378385, 
            795931963, 418609375, 71441554, 371410199, 340571161, 450758863, 243203547, 1975856536, 434362337, 259479239, 1615761631, 915994908, 1944039883, 815542154, 1759204392, 1062606503, 628011328, 487958497, 1099081303, 1597282968, 
            635879027, 1253951432, 1044139417, 58798547, 332927728, 2033878732, 1043295793, 1099205244, 1345302738, 563703425, 1516848508, 783195015, 606307624, 871400279, 1533175819, 561910996, 800935412, 1778315874, 1489878084, 88940427, 
            569415004, 1270695372, 731848782, 1558171239, 1274214974, 1236770152, 1408181154, 736376514, 1293259956, 571078280, 2013806895, 903014475, 270306875, 749599320, 1238298750, 2036023243, 2129307353, 676272496, 1555993512, 1820144044, 
            1895122410, 970281543, 1855522595, 1877984753, 152029829, 983713795, 2024493998, 1444110284, 1970973576, 1541307894, 1336612628, 1747313933, 1896818714, 1956400036, 1784973860, 589524607, 1974338901, 1833823843, 1203719987, 618822855, 
            1434852451, 1278271989, 1910881437, 85148416, 1213151107, 775744924, 1281338483, 1113184457, 1350307716, 856233089, 1976699851, 1981184295, 1066064055, 900777802, 1308845948, 438722741, 1257401465, 2147268730, 631142538, 947033702, 
            757861483, 1823737142, 158198945, 1858489651, 1570497380, 805725030, 960415582, 1501244556, 1374132486, 1274918697, 1020624091, 1934616361, 2072658739, 759805361, 910512003, 2071794976, 1120750107, 1856700580, 829619510, 980064154, 
            244900603, 1642268654, 992462509, 1011737926, 682494069, 786174142, 590393065, 1539105939, 347447158, 111772938, 766872198, 532700508, 2085747821, 881650463, 841384712, 2098891500, 2097598653, 232323065, 362263245, 225045863, 
            119703620, 4944603, 1968437616, 1380980973, 2073825567, 605051905, 2065176661, 1160946968, 1883170673, 232040489, 444605651, 399778364, 33951258, 1193752948, 747607449, 936684456, 124878512, 1307397981, 2056577799, 1351070248, 
            839071316, 1680536095, 1798492122, 1578326410, 1992133063, 381614024, 526719569, 883416321, 2023742031, 1663913468, 845207412, 1406606153, 136340897, 1799397003, 1742741459, 1496358434, 1909818424, 74606998, 1684588169, 53290601, 
            989897523, 539730571, 1743375293, 130816144, 1263085470, 1546548753, 474430824, 330754551, 736692251, 1273417050, 1806941700, 689948298, 1860010083, 454583971, 107393890, 1247140863, 189303263, 1037577936, 1117366520, 844141564, 
            1071584908, 1617600769, 571656980, 1929354158, 854352138, 1795469341, 1976141081, 913405598, 1155145074, 729915664, 737647900, 1273203353, 1824587521, 1825802399, 297378390, 337845461, 296765279, 163801990, 2044760855, 1041174853, 
            1781490763, 608508215, 115147929, 1497559537, 427880483, 225728883, 379845088, 1968369386, 370457649, 1341288386, 460765325, 2010845465, 429488011, 112122662, 1025297501, 878145344, 1512794408, 2139086676, 1091797585, 323093600, 
            517908626, 813523043, 338305957, 1439848218, 1840783608, 250020894, 792576556, 304502957, 1147964243, 1628007187, 269153072, 294583400, 2097170612, 1880506944, 1419875068, 561624721, 1744482475, 1935278020, 396677397, 1654725245, 
            1873926708, 1476416568, 551311107, 1613572202, 810964000, 868424170, 1012648865, 974566942, 927018051, 1618336356, 758345311, 806513917, 86443659, 1965655847, 572649981, 1948640173, 928784965, 1383487837, 1530731508, 141112687, 
            1353116492, 930571533, 568899588, 999568932, 1958725796, 762003277, 858981818, 3192958, 933818498, 1614950451, 1623032810, 1982769809, 1889969506, 1851883162, 171424874, 1936467436, 340264004, 1005915100, 1881924809, 80106924, 
            1447215230, 1043748875, 1194996032, 28687706, 256607158, 686001100, 1845504156, 1554601934, 587680366, 517171243, 138505314, 603116709, 1148832752, 1575712890, 683512320, 386996570, 1717982770, 99214381, 1536138523, 527633489, 
            1647535818, 1408384806, 1825844554, 2140062249, 449789442, 488668249, 6496676, 513823889, 725874330, 879487606, 1873524034, 291099831, 1260011416, 2003913669, 1649426136, 1006477366, 1283070246, 1831638997, 1069757625, 886345705, 
            480407707, 1211852125, 926419939, 1081053878, 1526964228, 305657599, 1902527835, 1050796628, 624549377, 91513800, 1282693482, 773665561, 1824953235, 621891487, 1241421411, 246248065, 1096486460, 28752748, 1895723852, 733630685, 
            1511822808, 1654120552, 778985584, 1845594966, 536379516, 1424889518, 352849829, 2037246263, 1115024085, 718974366, 544481466, 1192812116, 1305265855, 1147806639, 96644389, 1562905401, 1365594150, 1643043174, 817306574, 1900759768, 
            1132870473, 257144494, 1487437387, 2086937014, 1801960978, 108331548, 683419359, 320561241, 856453421, 557555639, 83340601, 1522855995, 760073935, 711710318, 637105506, 538354270, 274633114, 1732368350, 2069531085, 602835872, 
            226208336, 501645486, 15946549, 665383669, 1897343002, 1189568512, 1297220658, 1563833920, 764532987, 1026764661, 852296354, 1767248325, 1184559234, 222549505, 152413431, 1847467885, 1485909366, 384073379, 757181128, 1854721155, 
            1357053860, 62284087, 1836074138, 764442183, 2055643972, 530607119, 525733655, 911397401, 1912253464, 811676986, 1752308749, 1828265910, 1080064075, 1462542428, 1941323147, 1669727622, 618065329, 649956816, 1890037994, 1116028381, 
            1553349386, 1152457139, 1554851571, 1266464660, 1252311093, 176706937, 192998378, 1165202148, 2007164879, 1000489915, 1072458588, 1489000495, 77582300, 1293589232, 74633353, 209808005, 1117394116, 395826406, 265898560, 933582071, 
            653047805, 1456859677, 623721963, 747702126, 538462299, 138862733, 522234213, 1050596354, 1234738493, 1111127141, 367675751, 1684410099, 693757568, 1036090218, 1997481234, 93504814, 1406863248, 2107881682, 984326897, 558248424, 
            1467196133, 2057337794, 361094303, 761979633, 1938151328, 1218824477, 323175692, 325847967, 1364862202, 363327244, 1247198025, 27279003, 580605499, 2104482615, 361878831, 75717440, 1395491440, 1196744515, 1695230712, 696254859, 
            1198172066, 46404481, 1965448771, 266967384, 1988579995, 52994977, 1037640246, 972660725, 377848923, 656355396, 2085007325, 2140434502, 205768754, 1222077566, 2010419882, 525822505, 621305976, 1293768982, 1447470221, 1040648166, 
            925213513, 1920510008, 37029274, 479497018, 1299819826, 657448939, 540860340, 1998126947, 1692038791, 1872607241, 1664426456, 984489216, 983789525, 37588810, 1892139708, 1178412012, 1948138628, 816537037, 16101806, 1610408201, 
            1628314326, 321029293, 1145577740, 1422406312, 1692330920, 887641043, 1089829916, 1583867727, 1354842937, 1509628065, 8959462, 1946615367, 1407685059, 495169411, 1489724183, 1363437332, 294130339, 1684218813, 1831500779, 750264621, 
            816050153, 1939240682, 1831603133, 261590554, 214850543, 69726095, 139083311, 840012154, 312933479, 1698428577, 87271769, 953018415, 1329846712, 2057105431, 385158137, 673084903, 2120065328, 1990182203, 508960815, 1437600228, 
            257626870, 112636658, 146333610, 1821947094, 1773033926, 1674402510, 1617479801, 1258160119, 920594705, 1389660665, 752560206, 1173876103, 419059588, 355309791, 78142213, 1462706287, 488201490, 628571823, 353499747, 912951187, 
            492853048, 1427871439, 600588320, 235002207, 384644793, 1977265806, 1680198543, 377487653, 19850189, 1934579787, 1433674370, 1889864313, 96518036, 1025367751, 313264254, 831999647, 359456832, 2042553351, 34536650, 918064135, 
            741017462, 1490014019, 159522696, 1245671979, 731158667, 2010068931, 571948510, 1691386421, 1122618524, 602744738, 662026636, 69785727, 2112179838, 31723251, 1444170650, 1783026950, 1544180557, 433114312, 794294829, 1624411782, 
            1822395152, 1147400733, 833937188, 1928999814, 46399609, 1047409463, 1349058579, 1064343431, 1524374957, 1706449450, 756630733, 576739818, 1399688087, 1308440805, 1370808054, 1698881849, 212015539, 1844190470, 229843409, 128830267, 
            1835680173, 1294082914, 1233470860, 1064875151, 1345781499, 906036935, 2086034417, 1783368457, 2091741163, 639437385, 2146183921, 1809846350, 1891523637, 611230982, 583155725, 1015833252, 239830219, 253302416, 1352162449, 586332579, 
            1963585709, 804215230, 636652859, 2105867221, 907262025, 656771464, 2002278482, 1016574159, 1149895317, 396460166, 671737506, 1523977516, 1565800567, 714391619, 2089095398, 834804369, 219572928, 1389682524, 355021389, 2074130277, 
            611138927, 802312438, 984941049, 2116262633, 431736535, 1817081348, 473113669, 1069211197, 1494893467, 1534501395, 1467344032, 1874946640, 1649109343, 1498947066, 811101305, 119600871, 1137089135, 1180265348, 1261488639, 2072293102, 
            1293728499, 1615509886, 1269011406, 82141442, 403949408, 1187850639, 1065026746, 495304159, 1542305786, 1586476984, 1513397291, 1613349208, 269957671, 1019798534, 62516466, 399821975, 1679046866, 1133437871, 639946835, 28469681, 
            1215417867, 107248503, 1205690598, 680880591, 1411850483, 47358607, 1841457496, 432192330, 405964193, 1001569013, 1172634432, 384713024, 1124820147, 1015299347, 859191075, 2139051823, 1754817473, 212883953, 864783010, 1154103767, 
            1973394023, 1039338787, 1166444130, 2072376512, 1084655027, 1721974705, 358147196, 24968049, 1846644387, 539051378, 267338618, 416044407, 1339725521, 1088957221, 356128391, 1721331500, 1769771978, 20832415, 237898654, 1931492083, 
            1149986383, 2117892446, 779189807, 300299295, 332476857, 513892348, 2012378844, 1048709404, 47781212, 1003802789, 1698269155, 1217764191, 1279805412, 858267502, 355523253, 135569010, 616287266, 1793602584, 1506890635, 297369532, 
            1078550845, 509570694, 566974299, 1588905804, 565278892, 2013117301, 707087029, 782777091, 1617738498, 624171081, 577168558, 1911836950, 1941011019, 851165934, 882591835, 1422008910, 1049545279, 1830700154, 497588958, 2114742100, 
            878564517, 935547330, 1487055669, 222938654, 1914900208, 596772263, 369371731, 246130733, 650959392, 252943003, 1482278759, 1693494508, 904232008, 1358041643, 1079238129, 542212600, 1957970033, 1622668356, 1040140215, 2110803558, 
            1127445482, 1040762996, 1143714605, 908414727, 960196305, 559019879, 182676633, 1390293685, 965592982, 190768808, 1086742521, 1777379277, 74370638, 264422590, 1914328769, 1274193507, 179462691, 82389332, 54907168, 1921258665, 
            1937153602, 1314918993, 433685901, 846892860, 995691417, 1555822569, 1769777462, 557098541, 121004331, 544192142, 1254631848, 238558886, 1637821697, 1933816181, 599188128, 1479578616, 1296601027, 1925449143, 198598774, 819034128, 
            1190247465, 1929775526, 993348905, 1416197897, 1113511642, 1869228435, 752402406, 1987818054, 210362788, 2043944609, 257753131, 1374658401, 1485291730, 44433562, 1980657132, 605682673, 2131297920, 613231336, 1942520378, 737376838, 
            1321646048, 2119076553, 424093482, 1431451478, 320835601, 2043604676, 2100291519, 929447184, 22730971, 1559139099, 1941669203, 171362363, 2024018377, 916402568, 1281538407, 221138113, 1042329461, 1260718026, 1815555183, 884292038, 
            451261374, 1821904540, 485735626, 1202757137, 1408769174, 898390449, 575436943, 1756414344, 1548760324, 1080356875, 1580514988, 114215561, 1001071303, 2072743239, 1737092097, 375690370, 452771191, 1862706226, 233334070, 686836116, 
            690734144, 387664122, 1496596369, 374836200, 321390830, 1414512355, 1182847195, 428775068, 301456521, 1273594336, 1675921049, 579028042, 891153477, 1079880486, 1146564236, 254684947, 1594582041, 1842879028, 598469586, 2119492751, 
            641545292, 1769367720, 496558116, 1915646157, 1364431174, 1196207659, 1072985249, 524991338, 38120035, 1156618400, 462416794, 377292613, 1030245003, 526869234, 407137791, 676789293, 1695843872, 1308063628, 835966359, 2088064195, 
            300166034, 1854351116, 1076030558, 1294832764, 497454572, 1133535925, 1492911493, 1334184720, 1600544535, 737720210, 1418901975, 373021992, 798340193, 1827221939, 1351495163, 2027661179, 509347052, 1349577653, 2032790724, 1484040113, 
            953713012, 927321720, 1387448522, 1119185945, 1923556803, 2099102698, 1304029642, 2019701069, 367772983, 767215977, 470066176, 1736153324, 1213855968, 355978559, 1056815092, 206848736, 676629926, 324557746, 1674788575, 1522723585, 
            2071411779, 2027624644, 543759388, 108270340, 86331615, 2006918245, 1663954426, 863025344, 1093850478, 834317115, 352930642, 1064540068, 518161906, 1835620489, 435732976, 386565956, 501933030, 1389075590, 745407965, 1584158878, 
            101391835, 75587993, 460576823, 693314978, 23874002, 1625196836, 936843895, 1488745383, 332123424, 1470687135, 1495738901, 2141429708, 235538443, 2013427998, 200941704, 1120047961, 1941458460, 137431670, 622028291, 566311965, 
            704797738, 1353876059, 168653254, 889765971, 650758220, 1436019093, 364904986, 2070162728, 572123672, 1507766706, 1505572694, 1804691794, 859620206, 611016274, 296460347, 1617168134, 1224025906, 381755485, 1304712258, 82494413, 
            397587665, 739007721, 282889989, 1373938065, 442653039, 1565497576, 1560962908, 633786909, 626290151, 1920786392, 1752153772, 857997831, 874355687, 1951114675, 668110348, 1967477405, 1682832601, 4757830, 1519711391, 1442021779, 
            373647611, 1287403807, 1442618625, 853321902, 1032342548, 1925270743, 539696950, 1578366306, 1676487427, 848073777, 1893426026, 1603655473, 1740932505, 116186264, 1764577295, 1276331307, 100960269, 1317939866, 1052713350, 1142042629, 
            1646271237, 872436139, 1578685697, 1371844521, 850044405, 1875033103, 806198994, 471801533, 723881627, 16140023, 1797705903, 878872891, 2117838035, 909306711, 1388225094, 2110250020, 513487689, 1694584388, 275676033, 1538767177, 
            1418126327, 1264383700, 187979566, 1355382951, 1039946820, 1557778235, 467892512, 71652383, 297035968, 1279221326, 1864814836, 812489018, 152244706, 812800248, 455989289, 1321992678, 625200541, 1668736112, 1859701245, 1315922180, 
            509570380, 1290430115, 1937019955, 195678041, 1069043242, 1350725398, 173507046, 618211163, 988199857, 73507659, 909827663, 1387552117, 816898762, 1850450706, 1878872547, 1654142978, 288705569, 1473561850, 2064748531, 1125514911, 
            756384091, 1163840250, 1143340829, 274444770, 1716584343, 689245219, 603773476, 940800306, 1981552165, 1384102193, 1143108546, 1441694160, 1271736029, 999701928, 1362583600, 484260984, 447486499, 1864346573, 1519432001, 854689406, 
            1372993394, 1163481814, 1637002814, 1421973906, 225787650, 2108352491, 1870916894, 1980158405, 1062953465, 606266879, 1031931486, 257421260, 2072324496, 1613636977, 1808348839, 1223553984, 23301963, 1141909299, 687390375, 1369651380, 
            14685723, 1942996637, 1091132198, 420090881, 1731312048, 738553665, 1416442123, 1331892319, 859061167, 1466093540, 1782953642, 790570190, 407434599, 1585743144, 579337705, 466712684, 1487289168, 17094560, 604369986, 770093641, 
            1580357227, 1408547824, 223062891, 302872947, 1780372949, 1230373690, 1097104562, 1478853662, 831787744, 294744044, 522045154, 1853005889, 400362102, 1062591982, 217987108, 1684081394, 1229869865, 393739233, 2118851411, 1929749792, 
            48351270, 1277146824, 1569111499, 1747688835, 261499370, 1936793134, 244913249, 245486432, 832323609, 1936781265, 1734604281, 1753989925, 632805461, 97864292, 782535793, 31513723, 1043895463, 845231049, 1158060702, 2038486089, 
            661338378, 943781815, 1696685956, 165892448, 613986615, 664127896, 1595558341, 1370480959, 417333765, 1119532574, 802050540, 116589774, 519474522, 445028098, 1020175980, 938009154, 303282260, 1535246353, 513187764, 1939233607, 
            1191756354, 2017373605, 762092329, 160362669, 1334669604, 446887937, 1105835791, 2115813959, 1082407519, 1693389779, 1170107901, 373914114, 371318256, 877744817, 1512891069, 1748960159, 19226779, 1245231588, 1090706546, 897671805, 
            1393770808, 2009760292, 327701133, 2034336226, 1839158583, 1391450963, 1778517420, 2138028658, 769258122, 812905366, 762439605, 348771658, 1276423146, 1149503380, 345461423, 752867737, 2056965994, 2023221730, 2129737402, 1701474296, 
            233963962, 1047055997, 274841876, 1626299763, 264494266, 1225947060, 1996386351, 1987486929, 708502307, 265061341, 272599670, 966122791, 1636208392, 47211242, 1312041294, 1093265280, 1244566190, 1964371505, 717082770, 1343498455, 
            10054549, 229022680, 1658573946, 1270979062, 707975727, 850675079, 2065636590, 1030317921, 493744317, 703092897, 2123825136, 1923982309, 2663370, 201334900, 1774588792, 1891778696, 1338132462, 1558299773, 2097630716, 2089739553, 
            1248410390, 57724557, 1854260704, 1620310138, 1238047995, 1857475127, 1941980337, 1231148484, 957527651, 522925426, 1105286543, 425625785, 1897473272, 925191862, 590432631, 534541389, 302272277, 1575459092, 2009794021, 1175822804, 
            1130126997, 1096043782, 557857100, 1661640861, 1770978575, 1762521666, 1471401207, 2115698840, 856106573, 77568050, 526878287, 1087576383, 1330297327, 628692820, 446514029, 1876066099, 1536408001, 538281279, 1109203173, 1374392960, 
            1583616735, 886690584, 533309346, 2038439606, 535334589, 357600359, 74730483, 1543831472, 360955230, 1571009515, 701099371, 1995507515, 374883607, 52414616, 975355083, 2109335096, 1346220075, 2059510991, 1627280195, 798640816, 
            658146615, 1820785065, 14870791, 2023320014, 1773373864, 389985153, 251714681, 1977813550, 1334454039, 1016850188, 997420991, 2092427230, 407321708, 568064309, 399037179, 314304302, 1660562662, 638869484, 1608895184, 1378278411, 
            233853938, 397758168, 1151294343, 597473267, 607760112, 1415604380, 221387164, 1440329110, 81357466, 482487353, 499827936, 1455517917, 711997874, 1261699570, 614122525, 1223427844, 143592415, 108499072, 737390176, 472274343, 
            1044773265, 724577287, 1134241504, 1202839872, 2133480721, 2002202149, 1425021312, 849506095, 1474628489, 1654459172, 1531199507, 2040846098, 1054861495, 1692412539, 797116354, 1573070734, 218340219, 1119804794, 1065366723, 422660982, 
            362583090, 1940189900, 1880082926, 1948743071, 1432393567, 784957660, 513198625, 1903595544, 1789237691, 617259034, 1589121305, 2132040200, 1129502649, 699643863, 1875085972, 819720525, 1941634522, 674710970, 1008858938, 1596454098, 
            1315142555, 556196921, 319681478, 1254984710, 377977411, 2034468872, 379955737, 1292747423, 1809900434, 1382716080, 44844199, 583435846, 9638220, 1446932345, 737247036, 1931662395, 29347553, 482293710, 576151131, 1226192882, 
            1680985285, 1403103506, 1207526936, 312399229, 911431775, 1045963479, 225791863, 1711947886, 1453051577, 1129745776, 286255881, 1131506435, 1471650678, 581793839, 1671087266, 723377730, 2103176264, 126990954, 784442848, 1609562001, 
            1443880939, 173817196, 159014373, 58944996, 1291554452, 786406857, 1579038708, 558815400, 1715550267, 1611473624, 1143923814, 253428779, 448445433, 875437305, 348208441, 1740298976, 968197392, 954952412, 610119862, 1792837812, 
            1361957455, 420621167, 1505613932, 1837258976, 1232250501, 996774396, 699417681, 1289540934, 2012357056, 467420976, 1815518750, 1257309675, 1859690443, 2047550249, 1227687930, 1763520877, 842114388, 255206186, 1806276356, 1665391747, 
            1290521235, 481980758, 1009537273, 1653467729, 924176352, 1363028930, 215617454, 1224174416, 383583701, 1260233809, 713566604, 1739270790, 603908989, 747876812, 202121383, 238833230, 1357845774, 1267520458, 1172513954, 1404798343, 
            1168613058, 499063184, 1312002257, 882871051, 1787247091, 144562588, 332944113, 2024185251, 497213919, 617293688, 2085727185, 1200340602, 393171875, 1506523989, 925912506, 573750533, 1462546209, 1432047230, 1055029820, 940840471, 
            2095740832, 1525996726, 466463886, 282737421, 154315087, 106548737, 885487785, 96776126, 844430273, 351146969, 697332121, 1648290162, 532232391, 498903408, 1085071510, 2011610349, 577114347, 735585285, 1427769805, 224576003, 
            1471738704, 330163582, 34409304, 1902786019, 205401499, 91879389, 1778240045, 1482234079, 2125755283, 1947575345, 1694534471, 2030481661, 601564553, 1885048212, 1131059965, 232288275, 134603105, 1576701196, 1200251438, 435634929, 
            992069887, 2012969785, 1242061045, 54579595, 231351256, 1309345992, 1260305555, 341565117, 452801722, 994381793, 1416522761, 1445435145, 602387995, 147644850, 450175639, 788471917, 1529604690, 1173003338, 316011101, 28429800, 
            344878840, 1841376929, 1531333994, 2058152821, 1067501068, 1525847367, 1512255616, 7875154, 423775111, 359749233, 380359451, 1327687438, 1057913490, 932650420, 1423756904, 1796802177, 98189896, 503563014, 636393332, 30812308, 
            1646376834, 157443109, 954577495, 1464777772, 1706630096, 184029527, 1273113610, 146588582, 591866760, 923312313, 1690832057, 269638865, 1350112683, 1646984860, 379214379, 409598001, 957692373, 668892942, 1305114008, 698803981, 
            428475360, 1805795331, 351639570, 140224305, 604403604, 1862540904, 1235510180, 18999971, 1900485977, 98725949, 1705315762, 907933931, 114154, 1782942327, 1735902332, 1576354562, 508109969, 935846948, 1252946559, 99358576, 
            38330268, 585899674, 1462967653, 1152546135, 1311186569, 272582860, 1044798731, 118927517, 1186004108, 356251207, 1518143566, 1573005613, 1175116159, 2065621349, 341197056, 12040035, 55071170, 1912174141, 104472937, 990143217, 
            472897091, 2145410845, 639781120, 1884219306, 1071841517, 1748280703, 90682951, 1583695450, 247888131, 1343753814, 1050796411, 1199235356, 1648062701, 2020927475, 6036120, 1897344541, 92570223, 1583170861, 1494591891, 1111942008, 
            503197759, 1698025372, 1102609386, 403049, 1653946734, 1322375680, 1598840335, 299274172, 1640297729, 1821501997, 2058338577, 263246347, 1054014066, 483807613, 1853689513, 1979477339, 623139718, 489704726, 1247986058, 347030244, 
            326798762, 978091617, 162742279, 1030822254, 1040498441, 1069263549, 1294142958, 105861817, 2050771387, 116942907, 527576977, 1617347738, 412224372, 914122771, 1539414692, 1941323184, 1563649302, 270325646, 1948471199, 11974021, 
            516552402, 1726691673, 1744360772, 364269583, 1099685081, 655617984, 1693240355, 1853291058, 588599207, 1361041080, 1398981688, 2121893657, 320905248, 1609164286, 2139406555, 993525043, 1195416238, 156790961, 1727255541, 824294690, 
            1024973276, 873488146, 1113306022, 1461318110, 1142652799, 402806422, 1882343815, 1016291497, 455470415, 1023614390, 1816402294, 1000712003, 1911833587, 388233281, 1146350866, 949597361, 1473354574, 22123411, 441369623, 1575636044, 
            1572908758, 140510222, 30824821, 372133810, 2127137357, 1512817392, 2070266744, 1434682755, 1821382200, 2075029156, 1275054293, 1455884241, 1958137650, 245980366, 580431664, 2074027150, 617083571, 1803524988, 111253393, 1690196043, 
            723281435, 1446697411, 462802141, 611978709, 1740460960, 1795623337, 1542411448, 1982452189, 123894142, 65333543, 45977618, 1597339263, 2146439960, 1246516525, 524264837, 655408969, 1043860587, 185112402, 1452485202, 1237835260, 
            2133376537, 511602503, 863865989, 1593983201, 1450869429, 1539632136, 1670106776, 1776384308, 1844543736, 1927614534, 971719268, 1686541926, 170433264, 121292601, 2146176713, 24799999, 1019578842, 1395302059, 153514351, 1325706509, 
            467018596, 1859600486, 1414905043, 1790105246, 1222134663, 854152997, 655088252, 1725198465, 92223434, 701714803, 973018938, 200826163, 1435428926, 989175264, 961031285, 1610857712, 6924524, 1792068333, 1414382001, 783333313, 
            423981768, 1487058270, 570274953, 286794332, 302993719, 1137965259, 989460873, 935652933, 333597218, 1964148336, 1413256352, 1527471296, 1683707152, 1031787395, 1409763231, 2050369195, 662981271, 1137004835, 1018310264, 1348917100, 
            1346282378, 131921910, 145062501, 930984262, 1679485775, 1152472880, 520906033, 1968993315, 121569048, 1312915486, 909990104, 1463463239, 540503977, 332105148, 212147380, 40710186, 1583785004, 839056847, 1454194776, 1664242220, 
            751642513, 1150913643, 749683404, 1835315465, 591855786, 470815220, 238779199, 1665169193, 2044955295, 1229617473, 856609261, 1834501358, 2059614519, 824703667, 744936993, 1391090262, 61597006, 1966144407, 1789774829, 1075398772, 
            1546381036, 287764662, 1305275372, 918251412, 122559825, 2067755830, 734157422, 2023529744, 71480166, 1043979570, 896549220, 1846247857, 266771691, 551605296, 204535228, 1222606639, 203126909, 1110194497, 1758688505, 929861052, 
            1086369585, 2076422090, 1202143989, 1775868438, 383872315, 1051483490, 229783076, 609902337, 1358171910, 1180264405, 1116296053, 297307508, 1990796391, 401799955, 1821994938, 653507532, 1456551751, 657283640, 1981434793, 218530815, 
            405018555, 106018435, 827897781, 915941979, 476420046, 1632229656, 1684119114, 262056359, 2118502472, 1998702899, 1752570379, 958310638, 1904811827, 382395470, 1149051554, 474072304, 507275989, 1093557062, 1187581597, 1341462565, 
            612940640, 342707807, 746289420, 1696883868, 1540923594, 145889575, 315056648, 1158171630, 13896949, 1134509507, 678087985, 868037368, 101725589, 1898945656, 809295396, 390769174, 1603533570, 942585432, 1777608948, 1154445670, 
            694897515, 1246682589, 2136903569, 1351414221, 1760982161, 1628195310, 1033976230, 2116959420, 311567230, 229454462, 811185044, 1844222570, 1263899043, 818887240, 2003164405, 1275574587, 1286455310, 174203710, 1410381918, 1951806931, 
            1255593528, 95785077, 253372004, 1052155746, 6188767, 1642782989, 867123688, 1443352205, 830185141, 1949042284, 373523560, 126073520, 1712095443, 226144398, 395141188, 253598535, 1669419920, 542066710, 861216682, 1209853528, 
            1924058104, 1592658844, 1909431674, 1515736011, 2025638988, 190635530, 1681635436, 46727749, 1732768249, 334906113, 815912565, 1616039511, 339203129, 359746599, 1350318246, 525295750, 1728297387, 2006787959, 359277119, 1396767995, 
            1040209075, 1329986243, 1761294462, 1717754040, 1299202831, 1150203512, 1035809200, 324579035, 231777585, 2089321370, 794679337, 1880008063, 198335814, 1134887144, 856158717, 831647042, 935825250, 91055815, 1957720803, 1152106976, 
            641897780, 455855976, 511647880, 1728664227, 1007227820, 1078766141, 934904297, 350652762, 487715755, 500148990, 272804656, 89093508, 43051270, 422920635, 1446395652, 863181640, 1145119406, 1141383901, 36554318, 1112904204, 
            1548184714, 1051510817, 1444089733, 66950585, 1578905834, 1755515726, 1169554358, 2117950695, 713635234, 519279290, 1344248099, 1047909332, 908602692, 567431479, 1625068113, 321640178, 187021724, 263645591, 1493681516, 234614582, 
            811986396, 1195155474, 69512712, 33553112, 294994465, 557082417, 1087206219, 2021832266, 304494798, 71622111, 2089319110, 437039136, 1224153625, 818925759, 995093641, 1005503582, 847273215, 1187448645, 208989938, 47109405, 
            1609281793, 1232741094, 1242981265, 887414174, 1673676329, 186102471, 1863500393, 445723846, 2009883498, 534190346, 2063822942, 1903212507, 953231871, 1703352357, 341328466, 919980822, 205281798, 1004947292, 1631849997, 1616641753, 
            2006123788, 1564719985, 845786339, 310101476, 989388924, 604964188, 1442304660, 1179436368, 449531490, 1149306505, 900145853, 1132848374, 83142752, 2112264944, 1251228402, 906650476, 1967456050, 722464453, 1253484708, 1838957017, 
            1531616400, 1626834198, 92621529, 407861528, 648094964, 595792429, 1918206561, 1960195910, 1602290873, 1139090592, 1748523316, 1260093057, 1667288041, 188124099, 1337997214, 599322830, 35408262, 1284822823, 1952008213, 2072195224, 
            60862724, 1123821169, 1915081085, 611083846, 463569912, 1471218829, 1692342902, 781010552, 986801661, 214237043, 996030133, 906537374, 505722241, 1784357117, 1708281894, 402993702, 509490642, 1567346302, 1551141722, 2034979818, 
            524397880, 1003340468, 1525340146, 1141119880, 812699313, 513965093, 412624390, 1427536491, 650745414, 1965173615, 716130312, 147764271, 870058202, 1157233888, 1178403720, 903676271, 633339935, 1350113541, 1423762528, 708563806, 
            1270988824, 1908056588, 1616122897, 807015296, 648634795, 557443704, 1045563677, 1476070214, 1186228918, 1168128813, 1703511796, 934259286, 135492104, 495849040, 765794585, 1718795910, 644967103, 112951293, 1421528992, 1621765931, 
            1597818793, 658702315, 119314185, 629274845, 1488654997, 77233410, 410957589, 966469645, 644154391, 2129251187, 1030955122, 500629018, 722240829, 329383878, 2140620269, 1759722732, 827059994, 972998836, 808261396, 1321598674, 
            1961509308, 562185718, 1864314155, 1040769330, 336074445, 724281722, 955688221, 1939586662, 1156339268, 1101483473, 1831793635, 2069297665, 396051358, 1928048795, 1101300113, 1183837441, 633959349, 1002353530, 1082168256, 1180706438, 
            1209661272, 1406821313, 1838480160, 164962644, 1295592143, 2105962382, 578722428, 736698697, 783206200, 362539540, 1853195696, 801302261, 872317548, 1272415421, 1492180690, 1171768542, 164111644, 542006657, 1941931773, 1578814453, 
            265761240, 1588753791, 251377605, 1281534105, 1664599688, 1758000677, 2121926546, 1695337718, 525115061, 751491417, 787854727, 2113222773, 1491906814, 1901802143, 1956350448, 1488471749, 53896393, 1778153931, 590656921, 588296760, 
            1042351740, 403150220, 1010285947, 2123452294, 959657288, 1689931604, 2147218393, 895808656, 1623445407, 1310773317, 86439648, 1485782331, 1378695562, 1117417472, 1074627976, 1361727405, 606089976, 620565750, 63470060, 543326021, 
            1986675373, 1745730315, 964207563, 942935412, 880628657, 1487549736, 136196396, 632275313, 1978691148, 985041775, 791745872, 958154242, 1680108963, 440122945, 2042706696, 1802607279, 990536295, 1866657760, 1686405515, 604699755, 
            330551372, 582167471, 36107109, 1705873791, 1419127599, 840052150, 1935500133, 135899906, 197009792, 750505984, 255230620, 464371154, 1971213488, 569609496, 1402629776, 642503589, 145294732, 1807329021, 1468365425, 1336134530, 
            699042955, 1680771258, 1385139209, 1451206694, 205345351, 819265705, 847221229, 1326145115, 1653305843, 1306247418, 1003327127, 1228031389, 939625856, 73310399, 1873657611, 231049466, 1748215526, 1121056109, 1263389381, 271535404, 
            165132378, 968335229, 643432027, 2006279673, 259012085, 372231299, 1342621318, 97373939, 936763825, 323323558, 1273569392, 1959123767, 1246060871, 2069248064, 326117575, 2000486672, 1778767097, 223107132, 1962110744, 1125713496, 
            1000292510, 874483279, 796736503, 1633196563, 1458277862, 1075198207, 847179964, 1540169468, 2055555142, 447864055, 129380566, 1116818801, 471205157, 420581863, 599930965, 809761290, 202621155, 1993103324, 1863609871, 76326875, 
            1093678545, 1123008755, 869015176, 434527410, 1399888768, 32487281, 1576677335, 353087738, 464135327, 217491316, 2041248, 1245096934, 155198029, 1784355970, 780787107, 1418818801, 724606430, 1391388763, 1047181087, 107961246, 
            886363981, 319366824, 1154727349, 1416015684, 296790656, 735722579, 170011191, 17135652, 189132930, 1803066399, 477612941, 722762815, 336671597, 397654378, 349487922, 1293162921, 1482998148, 8967630, 1584603972, 1199501258, 
            1243955360, 646896025, 1674743118, 1429016944, 1728414753, 635326262, 2052032735, 335386267, 1066187844, 2068425265, 535792105, 1630301654, 1288878577, 2126103452, 532654737, 2098848133, 1123376437, 247628632, 1778319959, 1415094903, 
            2039721956, 813102443, 1248179430, 939054761, 1940316591, 451211436, 351216819, 724471408, 1771952296, 1307488696, 1496527569, 1417467607, 1665113598, 1786306621, 709715424, 618619840, 1132241737, 1288112813, 1085752088, 1873589973, 
            1847390199, 1577969148, 169667444, 367146627, 1122883580, 1285892649, 1567344390, 1937143012, 1834905215, 1769046265, 1418115103, 153618555, 371027755, 1616315293, 883012805, 2070210700, 517010780, 1292686198, 2075362564, 1688526088, 
            1485209733, 1195209050, 1015154000, 1600075090, 1202980845, 947175970, 514929829, 2020717947, 1328038944, 672519361, 2101634725, 1040595846, 975002552, 920699061, 475093937, 1268672178, 1344609149, 1319658921, 1967260578, 1690014631, 
            648212701, 805895894, 317413956, 1114470235, 569998561, 1836551664, 1315240700, 1213248083, 1771598834, 96264726, 1670752878, 2068608361, 946243196, 620176936, 1218097701, 2083437815, 1579285427, 1853026617, 940162798, 1147356020, 
            1740762531, 1873873875, 1909938153, 1702545571, 1356482069, 880574946, 2099877574, 1757026583, 1131496506, 1222821719, 621982579, 1029692375, 524048394, 388331478, 446019989, 2099806245, 723504415, 419333180, 1236880090, 485403483, 
            1290392708, 136152228, 1683736023, 24073798, 926947960, 1292607319, 1145603125, 1663771615, 247343833, 1697006999, 1620042957, 426049379, 136891436, 1390565799, 869418111, 1286840503, 520242795, 16674322, 1030409845, 2139301478, 
            1148152559, 1357244096, 658845388, 561754073, 1711036967, 1638212442, 1693419998, 380274848, 1835899178, 484035936, 321560019, 1908733424, 1837220182, 1097917072, 1900853126, 1888054277, 536062088, 491742869, 475873260, 850307176, 
            915610320, 293424408, 441822828, 822082218, 1847410657, 164526347, 2036689143, 260709679, 2073576281, 120735971, 75365995, 472437230, 185619848, 270343163, 476258272, 1070247047, 1331635483, 2097061324, 1021344758, 1643943056, 
            5134117, 235004298, 1886900117, 1269817206, 1337787548, 1371267729, 286842644, 1439995321, 185574937, 1366314435, 1583710910, 1213010106, 1974106254, 1664343381, 1523342292, 515689678, 909146181, 2117733206, 1657308114, 2144964410, 
            1855526786, 1363948086, 2089513735, 1724161024, 1846580812, 241811007, 1470937129, 732140478, 1657529997, 1236702010, 2069846118, 603081493, 657488826, 1018485982, 1547795271, 1350837666, 1650114337, 427645892, 998965540, 390587298, 
            793738507, 973401213, 821776115, 1123558340, 1811661265, 203559183, 617730016, 887439523, 256834904, 681357995, 159606266, 508469507, 201890701, 1038916417, 1718192226, 490799408, 79597030, 840451504, 1931211168, 278327575, 
            1552907160, 115617168, 2038081324, 436409121, 397244716, 1711371753, 1534627867, 1038068090, 1689895085, 986401232, 1231851859, 2088094577, 1298913848, 306312979, 511064738, 526950536, 1125103888, 811443196, 1172503714, 1084152388, 
            1161970043, 1108182710, 345327551, 54300212, 1195071292, 534411147, 1372535822, 1612262866, 1177090006, 2144828324, 991382933, 1138082781, 2101158316, 1298461229, 937314714, 1824747601, 343247996, 435805395, 1434860033, 1511659960, 
            1216044020, 718604149, 1075357597, 114967018, 1151051638, 892639647, 1871498543, 1925229824, 1448135294, 1942122537, 565209193, 848526807, 2009338621, 1133509266, 1352623899, 1369406320, 372815718, 1719458159, 1929368390, 857723266, 
            117170003, 1234654399, 2043365994, 884779618, 1208536304, 1456476617, 778825989, 2057063285, 1917220581, 1715399569, 580616484, 90768196, 846535075, 1990604795, 479049815, 175552286, 319210600, 1728157868, 1540568492, 1940919677, 
            2064265773, 157923572, 1113745698, 2058406089, 1072114825, 306822534, 489688241, 1394199820, 2124727940, 495147659, 1877811044, 1592951886, 1258717891, 1834117742, 916693054, 1877370698, 1570652941, 1803793249, 1784251965, 563891812, 
            1694548376, 462040696, 508987584, 834095641, 1162643039, 238376384, 754704890, 490705284, 292238939, 132586033, 297245769, 861157696, 46610016, 130504425, 154404949, 1730411107, 610013478, 88706954, 163330340, 898757326, 
            1678301685, 219833525, 712591727, 1998917159, 2046377905, 1471728030, 1006169662, 99103289, 896025976, 864666769, 1671515402, 1926289696, 116043220, 1155692706, 422680957, 546264970, 843630769, 1808819910, 886436046, 1356941662, 
            213819435, 935165598, 889161053, 153363610, 576359676, 1515258555, 345877754, 1694669672, 611607361, 1295248389, 880370358, 435684283, 1760909248, 1924897271, 1430232630, 1323809096, 1692625780, 1119438960, 557485349, 1539091093, 
            2129466491, 592588451, 1664543716, 795021852, 1047333314, 885477711, 68856340, 78438382, 499155296, 327615961, 1891521529, 1131945050, 2147205521, 2009236992, 1169046332, 1885056742, 1192696655, 1227809512, 1010744453, 2128913410, 
            1262914081, 1130263226, 1481219560, 485922535, 1172431324, 2005483427, 412631652, 67296342, 68191803, 1180470671, 2128808975, 1885831871, 191618370, 1878122763, 1401800982, 1057576607, 646462698, 215155036, 46595866, 1336998174, 
            1438306397, 765957995, 1640041905, 1350916123, 1485570233, 660007674, 1431655513, 2110179630, 1190721088, 1427859771, 280464779, 655388662, 1202708971, 905864120, 636574917, 2007186428, 1390633546, 815308740, 1786028543, 1213012459, 
            332602367, 1752013800, 309130887, 1880200195, 1972945862, 714932733, 1447250026, 2097784745, 501832693, 380702073, 564184690, 207562016, 479769199, 1272609630, 1583539930, 1295277834, 1056534757, 105809172, 1818815737, 1888039115, 
            1519295797, 46324655, 189482617, 2000949639, 1536114560, 489527342, 1579434332, 1065717337, 1458841729, 239912914, 1954989715, 77515564, 1736898603, 812355952, 1597889896, 1439089606, 1361246367, 126377309, 302037114, 55888414, 
            1231341291, 1099466899, 2020362913, 75259045, 156530417, 2010915492, 933473866, 783333542, 1997704841, 1722736567, 1304165189, 1855625160, 1844349713, 1080577762, 853623748, 248168980, 105697397, 1597461431, 1863341148, 1676336998, 
            1125394096, 660605781, 1105228632, 1844905582, 1887701926, 1254726766, 891912389, 720795668, 1541827772, 2075575416, 1328206367, 1080513508, 751871357, 421783960, 781914954, 1633544260, 1769366821, 402827662, 364508745, 171501970, 
            1643367345, 1214218353, 1949154599, 876093691, 2138427033, 524304762, 1832064960, 1779988810, 201375414, 1217300074, 1858099628, 893006479, 474316120, 60887654, 351601817, 552152610, 1172438516, 488336959, 1004358187, 1713714497, 
            1354609697, 675890900, 1762717906, 1992492723, 360772356, 1844160501, 1977224487, 1568686379, 775821392, 1732810785, 168227669, 1253581570, 1532969821, 708215965, 2135117673, 1109660738, 23411920, 510870825, 745784443, 191789483, 
            276105216, 63068927, 369897587, 208463707, 1900921678, 161399125, 864437, 1997118510, 429046958, 538843392, 143071450, 641299383, 2086904346, 1599440618, 1084950958, 995853213, 932948705, 318758189, 1402379959, 1877548663, 
            834663411, 1952154035, 813486065, 1352897810, 395920148, 1818202245, 940757494, 562788625, 806678152, 1119353732, 1925982432, 1907654827, 1221333960, 1059331657, 123182355, 1268556456, 1895833009, 792610308, 571882550, 1967814905, 
            1240064354, 570092550, 1745545414, 2072815584, 669131658, 1560594979, 467947398, 659795284, 746134531, 303478779, 185872426, 428125621, 1295050852, 1496223516, 475838157, 1900019569, 2056379099, 170421204, 1099921547, 2094839351, 
            2002850885, 1602743715, 907892252, 1047683210, 1969372624, 1237859833, 1701362933, 1171437006, 1075212786, 1361085826, 455237392, 1388829811, 825177170, 164676600, 1824122547, 1931033536, 1537980610, 1775171327, 351359174, 709079929, 
            957737677, 1832085756, 770117117, 797506538, 122837767, 124085162, 703860056, 1762777788, 1160597461, 17643357, 1195654054, 800462246, 485913001, 1695421203, 2058639440, 18809129, 1452283519, 671197564, 633573728, 2076091678, 
            329537729, 334627814, 1523739240, 1621644405, 1347883175, 1449117760, 553399288, 2090305599, 422829318, 1190253882, 23531011, 552690600, 212172832, 1045460065, 487627134, 1959700964, 1614259965, 2112276363, 182529884, 1375542956, 
            1948350907, 1795055723, 331427203, 2063504991, 1021168829, 1399729878, 2279914, 651701671, 956114836, 811969788, 98035275, 306593772, 1728553636, 604264779, 1954059215, 215486774, 637633048, 1117127448, 2023206445, 2070361265, 
            1727231523, 891683943, 1527886277, 916440705, 47692910, 1609397781, 757242342, 1419859145, 1994448129, 828240268, 1477142639, 1887899042, 642531628, 1229213830, 1592946899, 903290960, 601035794, 624771810, 2131960064, 2067846431, 
            1939231553, 459567848, 582616438, 1599860146, 2007388127, 80555382, 655948023, 1597931399, 1824370443, 1653620079, 227793771, 1296750493, 323636012, 917101710, 898749186, 1656743714, 929991965, 1639234417, 1693104190, 2141318516, 
            692201002, 2034769183, 1019405053, 433493364, 960373452, 1759154378, 1176423087, 122553819, 1189983539, 2039843205, 1935688130, 1264962123, 2051833302, 1747089162, 1985718756, 501676871, 1490841524, 1441684418, 107066648, 1145905198, 
            1093847933, 1230665652, 2005510467, 576464497, 1029102329, 872899408, 852487158, 735471320, 601471752, 1997135965, 1627683421, 1286272050, 777328702, 569623463, 517189927, 894584551, 1046893825, 1887748375, 15211809, 591832146, 
            1931661921, 2093773698, 1451567220, 1646459170, 773182740, 31252965, 1028787501, 1586521292, 1647104281, 979421714, 1522015832, 473538787, 115015539, 1824694565, 2141139814, 268338401, 2094886543, 1567640439, 110426711, 1126864578, 
            381378929, 1361843578, 977053177, 2043699611, 879520520, 3417748, 1991271565, 1296993238, 1246861290, 1771532465, 960387481, 448342512, 1453474924, 892450357, 906514093, 2054481410, 355431740, 1608922808, 1055567828, 635959570, 
            134697424, 732657551, 495357978, 279731916, 1745093578, 139752481, 557346805, 1579458561, 816377464, 1575232467, 735751375, 1863499639, 1795323875, 1581018096, 1007276433, 2086205117, 1001029309, 2010470214, 24412780, 96527939, 
            1667514514, 662750911, 1008710945, 1414330376, 2147429147, 1120218949, 85316093, 1280420091, 2049985594, 1909675185, 969144283, 1814796038, 1839487010, 1811923187, 1125126733, 1229254848, 341131309, 932869774, 268358899, 988280563, 
            734959341, 1175357729, 1053528575, 354361976, 1796561986, 294904741, 1576827180, 1858459551, 1794315048, 455075651, 394874856, 182350089, 1896246681, 1608460885, 1405485988, 27610064, 1991506957, 888136414, 1403981645, 378437298, 
            1204111890, 2070379837, 1254880406, 1953016616, 927429045, 309728187, 906426401, 1594443750, 137540402, 475192910, 729668437, 1098659812, 4084451, 1961867084, 917699988, 1159045864, 1908253675, 1082823580, 283052559, 278073185, 
            338736082, 862890149, 1964411786, 29222717, 1296504342, 2093510434, 1118770262, 1111862285, 1083572056, 998485999, 439359082, 1801941742, 25351263, 1831174360, 744427059, 1557303526, 1226406435, 1364593708, 1749494630, 1814342005, 
            1997603715, 1161492237, 1207208504, 93965233, 650819642, 1929716704, 331342280, 105262399, 414437197, 1545788736, 1104728316, 1035891803, 2117244753, 1851093375, 1558947587, 179517591, 800430923, 740206167, 1206317025, 1586596900, 
            888571861, 1676018247, 361607804, 532066370, 278868705, 877751246, 449930816, 1721559879, 82759186, 447722519, 690850253, 1651513702, 2058641139, 39535702, 1913953421, 661941168, 1111117783, 960484589, 1395469826, 1168652351, 
            1774734633, 1852465002, 1194484557, 1747746135, 1294821271, 227002526, 1289177003, 1577201861, 186013172, 2099912781, 1691396776, 1157386587, 768511670, 218605548, 920858795, 629505207, 673172363, 418854212, 542062883, 1543430618, 
            1264230048, 962963125, 2113114105, 1414738049, 1916192656, 1743800828, 1474357108, 241494178, 1813424781, 1697143860, 1104441619, 451491550, 1430861846, 1190180125, 750211688, 1403649159, 566941435, 1225045413, 362251477, 1585500348, 
            490909808, 113948718, 81319132, 1805783217, 1715002557, 516127697, 1708883405, 1366406214, 1875533762, 768921034, 2143586194, 1814373534, 1994351490, 1735853606, 341602906, 2073569826, 1613565436, 1725471836, 1825322596, 1142082105, 
            619417043, 1602825196, 971333527, 759889337, 1069711815, 1620622423, 834119035, 1275182821, 1948531423, 660207610, 1263774592, 970965237, 1642643860, 1887001337, 969813654, 432125862, 887484636, 1156832845, 1421727289, 267142148, 
            692167321, 663727444, 785652357, 1677758315, 512379754, 1573289704, 123908557, 1323146129, 53483935, 1793970056, 1987931608, 713906142, 76068543, 1316705506, 1852716785, 272983182, 1347462875, 1712088613, 196164696, 1364630455, 
            896437244, 1975940236, 1418545100, 1232806668, 1749392726, 335247526, 1472700532, 1549202438, 73582266, 897723394, 1888504670, 1100395693, 1755836411, 98071766, 1981643099, 1882367843, 1098914865, 1127021807, 619851450, 1363234949, 
            1385757587, 1553376200, 1844696326, 1039834369, 32791509, 471605087, 1496880029, 560697992, 1166012440, 1555847272, 344486135, 1710486529, 1945621419, 1061279381, 1463957004, 1820152577, 1355091072, 1225560342, 481521859, 665033081, 
            194832733, 1013848031, 58987933, 1094814937, 2141363637, 624352310, 1549208385, 1514159106, 517854592, 1677331834, 1562050363, 1392558704, 1300397057, 845881990, 1324024413, 423629157, 1630844181, 2097171647, 1042033231, 771598532, 
            1812933842, 665319550, 1959494844, 384642612, 1314306674, 88517024, 363079290, 1983114058, 2097671713, 204611870, 1399860080, 493041377, 1414740043, 2092026993, 90211903, 808328524, 635735892, 515564508, 1175167816, 585835783, 
            301047848, 1824054053, 455615920, 349192995, 1868502400, 2083284796, 1679336711, 1031666622, 693390054, 705087102, 1464990002, 61184328, 69550449, 1154436884, 1861478250, 821886377, 562427160, 1782917112, 606525281, 1964439054, 
            438119244, 757186186, 231275434, 1077816099, 840845036, 919479342, 387260453, 1224229028, 687437504, 1628623786, 1934059196, 742284857, 552881425, 1019503392, 721977335, 1157149628, 1819061673, 1092288910, 1320975382, 1777065763, 
            1502897735, 1100959569, 660348996, 431868318, 636762374, 492458393, 428728503, 569300867, 375287883, 89064887, 767166138, 52632331, 1889684014, 1515247343, 1955082838, 1866515452, 166464740, 1632886162, 1042297331, 263875777, 
            363109316, 1424252809, 1944756241, 1803851731, 660006867, 700109830, 1844001421, 3371524, 389227727, 2112111775, 667285559, 1317499644, 1614404149, 1498508932, 1423045305, 1262024324, 1047629904, 407240603, 1767565584, 1724874780, 
            520031813, 392450231, 611175183, 1730815605, 676776931, 1349832011, 1504046900, 1615302513, 1567552467, 884779727, 1971920197, 1999194542, 2136207774, 2021007538, 1289929380, 2096168688, 2058037511, 510942743, 1572381626, 1523880474, 
            1441518790, 1642909821, 65742983, 1724703935, 266664572, 1593853607, 843180336, 1771947823, 847665618, 515372074, 351124845, 1981641706, 1386008708, 1561836017, 1937625199, 1008887047, 798422062, 670154770, 1209082768, 547593113, 
            412910493, 136585114, 807047263, 378846234, 1632504057, 1610310526, 2136657792, 623189914, 1837153961, 422672190, 1577771577, 963150790, 198631018, 1328095801, 257435062, 1070473109, 1659128824, 381637531, 2045153671, 864529820, 
            1339621831, 40686971, 450501992, 1835465152, 1485294789, 1273212709, 1990196012, 2084672709, 2137555482, 1631542867, 342412638, 1283344851, 653226434, 261196481, 1102731545, 1026473104, 1125385138, 1916890545, 1235145203, 2076202429, 
            862411691, 1198551861, 2053728898, 1562227462, 1878614517, 2098480530, 1413553837, 1524975623, 343869097, 66441750, 751005514, 1440371657, 2063080526, 935763812, 1022120007, 1121297406, 2087233269, 1478567856, 1001395295, 523307729, 
            2092875746, 1525554100, 40425842, 452493832, 1985455219, 1367197322, 96160645, 1028791506, 1186619667, 1668452220, 327750335, 81480156, 1953129694, 2003985054, 1094575644, 1949363270, 1398729447, 1514049666, 974575107, 1309372446, 
            425293912, 388978828, 1670585361, 1088042609, 1119929485, 1552380516, 1915981547, 1746836568, 1743594031, 923067637, 193640497, 1094311977, 1870005825, 1649332713, 1955347821, 1203651954, 1680677048, 798714007, 24058180, 1852129231, 
            335599067, 1033972312, 2032200511, 1496135945, 2122055869, 1727156021, 599974792, 1876482354, 596141594, 1149393421, 1391468262, 409885538, 444442007, 663817139, 1922788235, 220652679, 366241781, 1673053852, 68303073, 1210598197, 
            1909444421, 1266502649, 820582400, 524322326, 1439363848, 1134200126, 1037383713, 2021659199, 1171533623, 1577770053, 32680114, 1167987273, 1176573807, 1695381858, 1824863213, 18257930, 1181621815, 1116632719, 1440676119, 1402013464, 
            2055643699, 848862148, 1133145993, 1295757556, 1337430829, 1444873706, 1454688823, 150731327, 18107140, 540288843, 922669218, 1116409353, 2052285965, 1479938727, 143437154, 2031794332, 1186282997, 695641691, 483022710, 160175310, 
            810725329, 530443207, 2073705948, 128983938, 798921, 163843741, 661704838, 1599382112, 343284416, 2049894246, 504705166, 665799327, 1926086205, 1444165075, 1652233107, 359664490, 816653607, 1549244669, 612427485, 1076995529, 
            919860706, 41110485, 1787509482, 913088775, 498374469, 1296768789, 727973972, 1281090718, 584489017, 2120395603, 1857588159, 391278592, 1006473006, 2118393731, 2128846133, 514336377, 1121900765, 818732676, 383399288, 379187457, 
            2105280246, 626915670, 862050333, 319249030, 754364776, 1426085978, 1404019795, 1252455995, 1218245223, 537910126, 2006136309, 825096949, 1966341991, 351659181, 229005563, 1046575549, 1885054765, 106235383, 1363295963, 1669807086, 
            1613264803, 709446443, 545215698, 1348316962, 2022000077, 1427656923, 642229543, 799169741, 1841128143, 1324852961, 1219142342, 853472958, 774231410, 1586202948, 1302149974, 1322227067, 1970778781, 1675668093, 1955529999, 1280142221, 
            1013935766, 112579387, 1209372099, 1093394584, 992434737, 1080817566, 452936647, 2124849021, 192995704, 1288203896, 610086171, 1135085403, 75762493, 631062511, 536461786, 96601478, 1699195136, 1306434506, 1934361759, 2017325873, 
            438637752, 2019068666, 432739172, 1314330988, 1982281077, 1692390418, 202252538, 299410761, 501096365, 243003037, 907467769, 1247371828, 1659952119, 1994687509, 25499809, 31140664, 212473990, 873437456, 1212633171, 250374661, 
            2009930785, 1232793264, 670947635, 1743421522, 35096460, 986703348, 1710896058, 421042604, 1636736278, 204500781, 2103656713, 1264905304, 410407104, 1388797781, 1290655414, 1862953379, 358156381, 835822661, 1767874668, 1371333903, 
            1993432782, 866101822, 306868741, 1658186978, 1395710140, 539802800, 274981715, 1500672254, 867579083, 3078787, 673595289, 1230970590, 955034626, 284149111, 1745008609, 1603916574, 947344998, 1711323453, 317775822, 1078436565, 
            875445747, 817607511, 650562768, 1660746324, 302711399, 525971737, 1774129231, 778602945, 1958544729, 1988054340, 2104516118, 1324854928, 1663610080, 365761247, 597857399, 1121125779, 303807981, 1999898763, 1959658694, 1985579818, 
            1224496164, 1686019277, 1113207009, 279144586, 489754482, 850110249, 1126633820, 967895294, 149202578, 1581750331, 52486373, 599706781, 1587282931, 2051300039, 1546444894, 631088141, 408416676, 1044048747, 652422592, 1833480938, 
            848603592, 1965479479, 1676753905, 686025983, 1919204185, 1485571393, 230401929, 2001336729, 1835714379, 2090269872, 177898379, 1903480051, 937488249, 1496556911, 1245244624, 1432334035, 1383638961, 851230225, 1981225208, 747870631, 
            143367805, 2075862567, 2079636875, 241035495, 1694413926, 1278236984, 1461737322, 615488549, 869775660, 952142045, 1209147550, 2004163534, 1876860387, 575475594, 1042334545, 1474935415, 1157805302, 1040970062, 1623611406, 1821231183, 
            1696360172, 1897561551, 1707940321, 48525777, 2060022250, 218415598, 1182205979, 1656618810, 288194778, 1795920035, 1548464795, 1906279382, 1523124297, 1955894384, 596574797, 199083323, 1055663837, 1094910191, 1084425772, 1638380466, 
            365826400, 112787970, 231274876, 1108651583, 304473694, 1558159340, 1246217690, 45144562, 773030155, 1334482427, 698517892, 534104028, 122735656, 1859286578, 1836852273, 1829595757, 1453105926, 1937928167, 336223900, 746587462, 
            1278972500, 1799363694, 343014174, 218633639, 285170602, 955401072, 352116287, 1547953315, 5770033, 801567278, 1130138734, 1288743728, 689335458, 182915437, 2018129530, 1548443314, 1468573518, 1020746291, 684781431, 2118327522, 
            638148887, 149848960, 924542965, 144158377, 1546526602, 1313877456, 59154639, 1403278217, 1440786260, 899288281, 1578105591, 2012695261, 512900139, 499092834, 399222499, 723269321, 2137241291, 31536954, 278815694, 69663780, 
            871263760, 425809041, 1912149378, 1343031216, 742987957, 1169645478, 2145639976, 671235366, 122054541, 2057183110, 278966839, 1788550235, 1014508804, 1846547214, 1665806170, 410080929, 1630612460, 1455508128, 1668453863, 1861069624, 
            122325558, 1104645369, 1704038077, 1451164546, 2118954457, 1170982376, 337933797, 201828950, 794404606, 2055831114, 563223553, 321671005, 1417870835, 556649291, 807817247, 484665284, 941830824, 1672825778, 2139182157, 1820607204, 
            1311756189, 811500353, 2072142467, 1006278467, 1576906910, 990052150, 601504474, 1984210962, 2094141296, 2096072291, 349077407, 1354697105, 1044531552, 1740127777, 448936388, 229221640, 2108549245, 1712860893, 1679306686, 2087397899, 
            766178766, 1716749509, 227320481, 1499227577, 1141009188, 761855704, 830406056, 260279863, 499883875, 1701406431, 2061444820, 12065760, 1935681122, 1282629478, 791715603, 1623151372, 1974629494, 178362103, 1801106971, 2010631082, 
            978726325, 1612409187, 1792076917, 1103798231, 1080628278, 495929116, 573688191, 677355600, 888698005, 328727320, 1556832303, 1480779025, 57686711, 1494410603, 1088387016, 1911186551, 938057528, 1778968723, 296879997, 644747382, 
            1289190769, 926964029, 1059969279, 864299175, 650209089, 1200032883, 963730899, 846685411, 858423280, 1544272867, 628108775, 179078100, 1726081318, 472849937, 888545858, 951361741, 1917556639, 1337129599, 797044367, 18722348, 
            436977123, 1686763804, 1919206439, 1409080018, 1359188801, 331543729, 18944340, 850623964, 333992148, 1624559640, 983684285, 69185474, 723503256, 1465513477, 1550104088, 278301308, 1709761661, 1909125868, 2001845028, 173953763, 
            1553222901, 2041153684, 167593664, 1968397433, 946513873, 2137833079, 543703490, 470844605, 1709543160, 49337858, 918160964, 1364853706, 267548077, 1402069148, 1581667983, 879962243, 1296422318, 1301811604, 917215410, 1883888210, 
            1489247886, 1840697081, 428014474, 123014460, 2025010219, 537817106, 1671923173, 1154118011, 19852201, 561904843, 288847059, 490259110, 1519839694, 1387767351, 2069793750, 1982590806, 670827062, 535738658, 2023577724, 270783751, 
            1502018408, 939950711, 1501412539, 354285439, 1642720702, 1544718325, 1228119123, 1597930595, 2033585568, 648497888, 1117619354, 605954542, 685044159, 504724231, 740387042, 458058046, 1012364917, 849443070, 1008870185, 720155934, 
            1437925678, 1839197174, 1569187808, 1530886153, 661778954, 1795078400, 1851051783, 600391158, 1296956320, 307596389, 507151067, 1193007098, 1649399040, 606082371, 156812405, 375955386, 299097224, 435209089, 1277012199, 1628547993, 
            358028819, 1333451342, 396334877, 1610434429, 2058866862, 609588611, 1286588501, 1331347764, 2031141707, 2125502416, 1632277165, 773300790, 1751477329, 336904842, 632032125, 613545077, 441212874, 438987626, 1616482225, 1223235279, 
            1986871197, 1004636758, 1800725759, 1678849358, 1437734091, 751896194, 1728142839, 1200949677, 225663754, 794784565, 2140546325, 1126102869, 1329529759, 1778614323, 4953501, 1484552054, 12518153, 1676360925, 526313402, 203142249, 
            848270627, 645540802, 1522795502, 2036146071, 1164688680, 1805890639, 1625707585, 1127137549, 348323374, 1688294438, 946460887, 2019774111, 544116518, 1326218175, 1655339004, 939078792, 379492091, 355221103, 571783112, 748102326, 
            297423372, 786276863, 532796526, 818580574, 148087628, 952259963, 670281944, 650487290, 316298600, 1951492642, 1489173392, 2056736528, 527136865, 1264974945, 71090565, 1410446867, 1153779088, 746809974, 155839573, 1536948539, 
            10388501, 1184121771, 847965055, 1556036414, 1153334377, 1215725715, 1017031230, 2082522640, 760147336, 690687393, 1141940153, 197029335, 934655386, 281031041, 388510829, 803364530, 817193123, 1832289689, 953391436, 1456605802, 
        });
    }
}
