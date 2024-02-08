using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Slot_Simulator
{
    class Reels
    {
        internal List<string> Symbols, SymbolsOriginal;
        internal List<List<int>> ReelsPG, ReelsPG1, ReelsPG2, ReelsFG, ReelsFG5, ReelsFG6, ReelsFG7, ReelsFG8, ReelsSymReplacePG, ReelsSymReplaceFG;
        internal List<List<int>> ReelsPGCutoffs, ReelsPG1Cutoffs, ReelsPG2Cutoffs, ReelsFGCutoffs, ReelsFG5Cutoffs, ReelsFG6Cutoffs, ReelsFG7Cutoffs, ReelsFG8Cutoffs;
        internal List<List<List<int>>> ReelsAlt;
        internal List<List<List<int>>> ReelsAltCutoffs;
        internal List<List<int>> SSSymbolsBG;
        internal List<List<int>> SSSymbolsFG;
        internal List<List<List<int>>> SSSymbolsAlt;
        internal List<List<int>> SSInsertionInfoIndicesBG;
        internal List<List<int>> SSInsertionInfoIndicesFG;
        internal List<List<List<int>>> SSInsertionInfoIndicesAlt;
        internal List<int> SSInsertionIndexBG;
        internal List<int> SSInsertionIndexFG;
        internal List<List<int>> SSInsertionIndexAlt;
        internal List<int> SSInsertionLengthBG;
        internal List<int> SSInsertionLengthFG;
        internal List<List<int>> SSInsertionLengthAlt;
        internal List<List<int>> SSPositionsToChangeBG;
        internal List<List<int>> SSPositionsToChangeFG;
        internal List<List<List<int>>> SSPositionsToChangeAlt;
        internal List<List<int>> SSPositionWeightChangesBG;
        internal List<List<int>> SSPositionWeightChangesFG;
        internal List<List<List<int>>> SSPositionWeightChangesAlt;
        internal List<int> SSCutoffsBG;
        internal List<int> SSCutoffsFG;
        internal List<List<int>> SSCutoffsAlt;
        internal List<List<List<int>>> ActualReelsPG;
        internal List<List<List<int>>> ActualReelsPG1;
        internal List<List<List<int>>> ActualReelsPG2;
        internal List<List<List<int>>> ActualReelsFG;
        internal List<List<List<int>>> ActualReelsFG5;
        internal List<List<List<int>>> ActualReelsFG6;
        internal List<List<List<int>>> ActualReelsFG7;
        internal List<List<List<int>>> ActualReelsFG8;
        internal List<List<List<List<int>>>> ActualReelsAlt;
        internal List<List<List<int>>> ActualReelsPGCutoffs;
        internal List<List<List<int>>> ActualReelsPG1Cutoffs;
        internal List<List<List<int>>> ActualReelsPG2Cutoffs;
        internal List<List<List<int>>> ActualReelsFGCutoffs;
        internal List<List<List<int>>> ActualReelsFG5Cutoffs;
        internal List<List<List<int>>> ActualReelsFG6Cutoffs;
        internal List<List<List<int>>> ActualReelsFG7Cutoffs;
        internal List<List<List<int>>> ActualReelsFG8Cutoffs;
        internal List<List<List<List<int>>>> ActualReelsAltCutoffs;
        internal List<List<int>> SSWeightsBG;
        internal List<List<int>> SSWeightsFG;
        internal List<List<List<int>>> SSWeightsAlt;
        internal bool UseReelWeights { set { m_useReelWeights = value; } }
        public bool m_useReelWeights;

        internal Reels()
        {
            ReelsAlt = new List<List<List<int>>>();
            ReelsAltCutoffs = new List<List<List<int>>>();
        }

        internal List<List<int>> GetReels(ReelType _reelType = ReelType.PG, int _reelAltIndex = 0)
        {
            switch (_reelType)
            {
                case ReelType.PG: return ReelsPG;
                case ReelType.PG1: return ReelsPG1;
                case ReelType.PG2: return ReelsPG2;
                case ReelType.FG: return ReelsFG;
                case ReelType.FG5: return ReelsFG5;
                case ReelType.FG6: return ReelsFG6;
                case ReelType.FG7: return ReelsFG7;
                case ReelType.FG8: return ReelsFG8;
                case ReelType.Alt: return ReelsAlt[_reelAltIndex];
            }
            return null;
        }

        internal List<List<int>> GetReelCutoffs(ReelType _reelType = ReelType.PG, int _reelAltIndex = 0)
        {
            switch (_reelType)
            {
                case ReelType.PG: return ReelsPGCutoffs;
                case ReelType.PG1: return ReelsPG1Cutoffs;
                case ReelType.PG2: return ReelsPG2Cutoffs;
                case ReelType.FG: return ReelsFGCutoffs;
                case ReelType.FG5: return ReelsFG5Cutoffs;
                case ReelType.FG6: return ReelsFG6Cutoffs;
                case ReelType.FG7: return ReelsFG7Cutoffs;
                case ReelType.FG8: return ReelsFG8Cutoffs;
                case ReelType.Alt: return ReelsAltCutoffs[_reelAltIndex];
            }
            return null;
        }

        internal List<int> GetRandomIndexes(List<List<int>> _reelSet, List<List<int>> _reelWeights)
        {
            List<int> indexes = new List<int>();
            if (m_useReelWeights)
            {
                foreach (List<int> reelCutoffs in _reelWeights)
                    indexes.Add(m.RandomIndex(reelCutoffs));
            }
            else
            {
                foreach (List<int> reel in _reelSet)
                    indexes.Add(m.RandomInteger(reel.Count));
            }
            return indexes;
        }

        internal int GetRandomIndexesGivenReel(List<List<int>> _reelSet, List<List<int>> _reelWeights, int _reel)
        {
            int index = 0;
            if (m_useReelWeights)
            {
                index = m.RandomIndex(_reelWeights[_reel]);
            }
            else
            {
                index = m.RandomInteger(_reelSet[_reel].Count);
            }
            return index;
        }

        internal void ProcessReelStrips(object[,] _values, int _col, ReelType _reelType, int _reelCount)
        {
            List<List<int>> reels = new List<List<int>>();
            if (m_useReelWeights)
            {
                List<List<int>> cutoffs = new List<List<int>>();
                for (int col = _col; col < _col + _reelCount * 2; col+=2)
                {
                    List<int> reel = new List<int>();
                    List<int> weights = new List<int>();
                    
                    for (int row = 3; row <= _values.GetLength(0); row++)
                    {
                        string symbol = _values[row, col] == null ? "" : _values[row, col].ToString().ToLower();
                        if (symbol == "") break;
                        reel.Add(Symbols.IndexOf(symbol));
                        weights.Add(int.Parse(_values[row, col + 1].ToString()));
                    }
                    reels.Add(reel);
                    cutoffs.Add(m.MakeCutoffs(weights));
                }
                switch (_reelType)
                {
                    case ReelType.PG:
                        ReelsPG = reels;
                        ReelsPGCutoffs = cutoffs;
                        if (ReelsFG == null)
                        {
                            ReelsFG = reels;
                            ReelsFGCutoffs = cutoffs;
                        }
                        break;
                    case ReelType.PG1:
                        ReelsPG1 = reels;
                        ReelsPG1Cutoffs = cutoffs;
                        break;
                    case ReelType.PG2:
                        ReelsPG2 = reels;
                        ReelsPG2Cutoffs = cutoffs;
                        break;
                    case ReelType.FG:
                        ReelsFG = reels;
                        ReelsFGCutoffs = cutoffs;
                        break;
                    case ReelType.FG5:
                        ReelsFG5 = reels;
                        ReelsFG5Cutoffs = cutoffs;
                        break;
                    case ReelType.FG6:
                        ReelsFG6 = reels;
                        ReelsFG6Cutoffs = cutoffs;
                        break;
                    case ReelType.FG7:
                        ReelsFG7 = reels;
                        ReelsFG7Cutoffs = cutoffs;
                        break;
                    case ReelType.FG8:
                        ReelsFG8 = reels;
                        ReelsFG8Cutoffs = cutoffs;
                        break;
                    case ReelType.Alt:
                        ReelsAlt.Add(reels);
                        ReelsAltCutoffs.Add(cutoffs);
                        break;
                }

            }
            else
            {
                for (int col = _col; col < _col + _reelCount; col++)
                {
                    List<int> reel = new List<int>();
                    for (int row = 3; row <= _values.GetLength(0); row++)
                    {                        
                        string symbol = _values[row, col] == null ? "" : _values[row, col].ToString().ToLower();
                        if (symbol == "") break;
                        reel.Add(Symbols.IndexOf(symbol));
                    }
                    reels.Add(reel);
                }
                switch (_reelType)
                {
                    case ReelType.PG:
                        ReelsPG = reels;
                        if (ReelsFG == null) ReelsFG = reels;
                        break;
                    case ReelType.PG1: ReelsPG1 = reels; break;
                    case ReelType.PG2: ReelsPG2 = reels; break;
                    case ReelType.FG: ReelsFG = reels; break;
                    case ReelType.FG5: ReelsFG5 = reels; break;
                    case ReelType.FG6: ReelsFG6 = reels; break;
                    case ReelType.FG7: ReelsFG7 = reels; break;
                    case ReelType.FG8: ReelsFG8 = reels; break;
                    case ReelType.Alt: ReelsAlt.Add(reels); break;
                }
            }
        }

        internal void GetSSSelection(object[,] _values, int _col, int _reelCount, ReelType _reelType)
        {
            List<int> symbols = new List<int>();
            List<int> weights = new List<int>();
            List<int> indices = new List<int>();
            List<List<int>> insertionPatterns = new List<List<int>>();
            List<List<int>> SSSymbols = new List<List<int>>();
            SSSymbolsAlt = new List<List<List<int>>>();
            SSInsertionInfoIndicesAlt = new List<List<List<int>>>();
            SSCutoffsAlt = new List<List<int>>();
            for(int row = 3; row <= _values.GetLength(0); row++)
            {
                symbols = new List<int>();
                indices = new List<int>();
                for(int col = _col; col < _col +_reelCount; col++)
                {
                    if (_values[row, col] != null) symbols.Add(Symbols.IndexOf(_values[row, col].ToString().ToLower()));
                }
                for(int col = _col + _reelCount; col < _col + (_reelCount * 2); col++)
                {
                    if (_values[row, col] != null && _values[row, col].ToString() != "") indices.Add(int.Parse(_values[row, col].ToString()));
                }
                if (_values[row, _col + (_reelCount * 2)] != null) weights.Add(int.Parse(_values[row, _col + (_reelCount * 2)].ToString()));
                if (symbols.Count != 0 && symbols != null) SSSymbols.Add(symbols);
                if (indices.Count != 0) insertionPatterns.Add(indices);
            }
            switch (_reelType)
            {
                case ReelType.PG:
                    SSCutoffsBG = m.MakeCutoffs(weights);
                    SSSymbolsBG = SSSymbols;
                    SSInsertionInfoIndicesBG = insertionPatterns;
                    if (SSSymbolsFG == null) SSSymbolsFG = SSSymbols;
                    if (SSCutoffsFG == null) SSCutoffsFG = SSCutoffsBG;
                    if (SSInsertionInfoIndicesFG == null) SSInsertionInfoIndicesFG = SSInsertionInfoIndicesBG;
                    break;
                case ReelType.FG:
                    SSCutoffsFG = m.MakeCutoffs(weights);
                    SSSymbolsFG = SSSymbols;
                    SSInsertionInfoIndicesFG = insertionPatterns;
                    break;
                case ReelType.Alt:
                    SSCutoffsAlt.Add(m.MakeCutoffs(weights));
                    SSSymbolsAlt.Add(SSSymbols);
                    SSInsertionInfoIndicesAlt.Add(insertionPatterns);
                    break;
            }
        }

        internal void GetSSInsertionInformation(object[,] _values, int _col, int _reelCount, ReelType _reelType)
        {
            List<int> whereToInsert = new List<int>();
            List<int> lengthOfStack = new List<int>();
            List<List<int>> positionsThatChange = new List<List<int>>();
            List<List<int>> weightsThatChange = new List<List<int>>();
            SSInsertionIndexAlt = new List<List<int>>();
            SSInsertionLengthAlt = new List<List<int>>();
            SSPositionsToChangeAlt = new List<List<List<int>>>();
            SSPositionWeightChangesAlt = new List<List<List<int>>>();
            SSWeightsAlt = new List<List<List<int>>>();
            ActualReelsAlt = new List<List<List<List<int>>>>();
            ActualReelsAltCutoffs = new List<List<List<List<int>>>>();
            for (int row = 3; row < _values.GetLength(0); row++ )
            {
                List<int> positionChangesForThisOne = new List<int>();
                List<int> weightChangesForThisOne = new List<int>();
                int positionChangesToMake = 0;
                if (_values[row, _col + 1] != null) whereToInsert.Add(int.Parse(_values[row, _col + 1].ToString()));
                if (_values[row, _col + 2] != null) lengthOfStack.Add(int.Parse(_values[row, _col + 2].ToString()));
                if (_values[row, _col + 3] != null) positionChangesToMake = int.Parse(_values[row, _col + 3].ToString());
                if(positionChangesToMake > 0)
                {
                    for(int aa = 0; aa < positionChangesToMake; aa++)
                    {
                        positionChangesForThisOne.Add(int.Parse(_values[row, _col + 4 + (aa * 2)].ToString()));
                        weightChangesForThisOne.Add(int.Parse(_values[row, _col + 4 + (aa * 2) + 1].ToString()));
                    }
                    positionsThatChange.Add(positionChangesForThisOne);
                    weightsThatChange.Add(weightChangesForThisOne);
                }
            }
            switch (_reelType)
            {
                case ReelType.PG:
                    SSInsertionIndexBG = whereToInsert;
                    SSInsertionLengthBG = lengthOfStack;
                    SSPositionsToChangeBG = positionsThatChange;
                    SSPositionWeightChangesBG = weightsThatChange;
                    if (SSInsertionIndexFG == null) SSInsertionIndexFG = SSInsertionIndexBG;
                    if (SSInsertionLengthFG == null) SSInsertionLengthFG = SSInsertionLengthBG;
                    if (SSPositionsToChangeFG == null) SSPositionsToChangeFG = SSPositionsToChangeBG;
                    if (SSPositionWeightChangesFG == null) SSPositionWeightChangesFG = SSPositionWeightChangesBG;
                    break;
                case ReelType.FG:
                    SSInsertionIndexFG = whereToInsert;
                    SSInsertionLengthFG = lengthOfStack;
                    SSPositionsToChangeFG = positionsThatChange;
                    SSPositionWeightChangesFG = weightsThatChange;
                    break;
                case ReelType.Alt:
                    SSInsertionIndexAlt.Add(whereToInsert);
                    SSInsertionLengthAlt.Add(lengthOfStack);
                    SSPositionsToChangeAlt.Add(positionsThatChange);
                    SSPositionWeightChangesAlt.Add(weightsThatChange);
                    break;
            }
        }

        internal void ProcessSSWeights(object[,] _values, int _col, ReelType _reelType, int _reelAltIndex = 0)
        {
            if(_reelType == ReelType.PG)
            {
                if (SSInsertionLengthBG.Count() > 0)
                {
                    List<List<int>> allSSWeights = new List<List<int>>();
                    for (int col = _col; col < SSInsertionLengthBG.Count() + _col; col++)
                    {
                        List<int> SSWeights = new List<int>();
                        for (int row = 3; row < SSInsertionLengthBG[col - _col] + 3; row++)
                        {
                            SSWeights.Add(int.Parse(_values[row, col].ToString()));
                        }
                        allSSWeights.Add(SSWeights);
                    }
                    SSWeightsBG = allSSWeights;
                    if (SSWeightsFG == null) SSWeightsFG = allSSWeights;
                }
            }
            if (_reelType == ReelType.FG)
            {
                if (SSInsertionLengthFG.Count() > 0)
                {
                    List<List<int>> allSSWeights = new List<List<int>>();
                    for (int col = _col; col < SSInsertionLengthFG.Count() + _col; col++)
                    {
                        List<int> SSWeights = new List<int>();
                        for (int row = 3; row < SSInsertionLengthFG[col - _col] + 3; row++)
                        {
                            SSWeights.Add(int.Parse(_values[row, col].ToString()));
                        }
                        allSSWeights.Add(SSWeights);
                    }
                    SSWeightsFG = allSSWeights;
                }
            }
            if(_reelType == ReelType.Alt)
            {
                if (SSInsertionLengthAlt[_reelAltIndex].Count() > 0)
                {
                    List<List<int>> allSSWeights = new List<List<int>>();
                    for (int col = _col; col < SSInsertionLengthAlt[_reelAltIndex].Count() + _col; col++)
                    {
                        List<int> SSWeights = new List<int>();
                        for (int row = 3; row < SSInsertionLengthAlt[_reelAltIndex][col - _col] + 3; row++)
                        {
                            SSWeights.Add(int.Parse(_values[row, col].ToString()));
                        }
                        allSSWeights.Add(SSWeights);
                    }
                    SSWeightsAlt.Add(allSSWeights);
                }
            }
        }

        internal void buildReelStrips(int _reelCount, ReelType _reelType)
        {
            if(SSInsertionIndexBG == null && _reelType == ReelType.PG)
            {
                if (ActualReelsPG == null) ActualReelsPG = new List<List<List<int>>>();
                if (ActualReelsPGCutoffs == null) ActualReelsPGCutoffs = new List<List<List<int>>>();
                for(int af = 0; af < _reelCount; af++)
                {
                    List<List<int>> newReels = new List<List<int>>();
                    List<List<int>> newCutoffs = new List<List<int>>();
                    newReels.Add(ReelsPG[af]);
                    newCutoffs.Add(ReelsPGCutoffs[af]);
                    ActualReelsPG.Add(newReels);
                    ActualReelsPGCutoffs.Add(newCutoffs);
                }
            }
            else if(SSInsertionIndexBG != null && SSInsertionIndexBG.Count > 0 && _reelType == ReelType.PG)
            {
                if (ActualReelsPG == null) ActualReelsPG = new List<List<List<int>>>();
                if (ActualReelsPGCutoffs == null) ActualReelsPGCutoffs = new List<List<List<int>>>();
                int newSymbol = -1;
                if (Symbols.Contains("ss")) newSymbol = Symbols.IndexOf("ss");
                else if (Symbols.Contains("re")) newSymbol = Symbols.IndexOf("re");
                for(int aa = 0; aa < _reelCount; aa++)
                {
                    List<List<int>> newReels = new List<List<int>>();
                    List<List<int>> newCutoffs = new List<List<int>>();
                    newReels.Add(ReelsPG[aa]);
                    if(m_useReelWeights) newCutoffs.Add(ReelsPGCutoffs[aa]);
                    for(int ab = 0; ab < SSInsertionIndexBG.Count; ab++)
                    {
                        List<int> newReel = new List<int>();
                        List<int> newWeights = new List<int>();
                        for(int ac = 0; ac < ReelsPG[aa].Count(); ac++)
                        {
                            if(ac == SSInsertionIndexBG[ab])
                            {
                                for (int ad = 0; ad < SSInsertionLengthBG[ab]; ad++)
                                {
                                    newReel.Add(newSymbol);
                                    if(m_useReelWeights) newWeights.Add(SSWeightsBG[ab][ad]);
                                }
                                newReel.Add(ReelsPG[aa][ac]);
                                if (m_useReelWeights)
                                {
                                    if (SSPositionsToChangeBG.Count() > 0)
                                    {
                                        if (SSPositionsToChangeBG[ab].Contains(ac)) newWeights.Add(SSPositionWeightChangesBG[ab][SSPositionsToChangeBG[ab].IndexOf(ac)]);
                                        else if (ac == 0 && !SSPositionsToChangeBG[ab].Contains(ac)) newWeights.Add(ReelsPGCutoffs[aa][ac]);
                                        else if (ac != 0 && !SSPositionsToChangeBG[ab].Contains(ac)) newWeights.Add(ReelsPGCutoffs[aa][ac] - ReelsPGCutoffs[aa][ac - 1]);
                                    }
                                    else if (SSPositionsToChangeBG.Count() == 0 || SSPositionsToChangeBG == null)
                                    {
                                        if (ac == 0) newWeights.Add(ReelsPGCutoffs[aa][ac]);
                                        else if (ac != 0) newWeights.Add(ReelsPGCutoffs[aa][ac] - ReelsPGCutoffs[aa][ac - 1]);
                                    }
                                }
                            }
                            else if (ac != SSInsertionIndexBG[ab])
                            {
                                newReel.Add(ReelsPG[aa][ac]);
                                if (m_useReelWeights)
                                {
                                    if (SSPositionsToChangeBG.Count() > 0)
                                    {
                                        if (SSPositionsToChangeBG[ab].Contains(ac)) newWeights.Add(SSPositionWeightChangesBG[ab][SSPositionsToChangeBG[ab].IndexOf(ac)]);
                                        else if (ac == 0 && !SSPositionsToChangeBG[ab].Contains(ac)) newWeights.Add(ReelsPGCutoffs[aa][ac]);
                                        else if (ac != 0 && !SSPositionsToChangeBG[ab].Contains(ac)) newWeights.Add(ReelsPGCutoffs[aa][ac] - ReelsPGCutoffs[aa][ac - 1]);
                                    }
                                    else if (SSPositionsToChangeBG.Count() == 0 || SSPositionsToChangeBG == null)
                                    {
                                        if (ac == 0) newWeights.Add(ReelsPGCutoffs[aa][ac]);
                                        else if (ac != 0) newWeights.Add(ReelsPGCutoffs[aa][ac] - ReelsPGCutoffs[aa][ac - 1]);
                                    }
                                }
                            }
                        }
                        newReels.Add(newReel);
                        if(m_useReelWeights) newCutoffs.Add(m.MakeCutoffs(newWeights));
                    }
                    ActualReelsPG.Add(newReels);
                    if(m_useReelWeights) ActualReelsPGCutoffs.Add(newCutoffs);
                }
                if (ActualReelsFG == null) ActualReelsFG = ActualReelsPG;
                if (ActualReelsFGCutoffs == null) ActualReelsFGCutoffs = ActualReelsPGCutoffs;
            }
            if (SSInsertionIndexFG != null && SSInsertionIndexFG.Count > 0 && _reelType == ReelType.FG)
            {
                ActualReelsFG = new List<List<List<int>>>();
                ActualReelsFGCutoffs = new List<List<List<int>>>();
                int newSymbol = -1;
                if (Symbols.Contains("ss")) newSymbol = Symbols.IndexOf("ss");
                else if (Symbols.Contains("re")) newSymbol = Symbols.IndexOf("re");
                for (int aa = 0; aa < _reelCount; aa++)
                {
                    List<List<int>> newReels = new List<List<int>>();
                    List<List<int>> newCutoffs = new List<List<int>>();
                    newReels.Add(ReelsFG[aa]);
                    if(m_useReelWeights) newCutoffs.Add(ReelsFGCutoffs[aa]);
                    for (int ab = 0; ab < SSInsertionIndexFG.Count; ab++)
                    {
                        List<int> newReel = new List<int>();
                        List<int> newWeights = new List<int>();
                        for (int ac = 0; ac < ReelsFG[aa].Count(); ac++)
                        {
                            if (ac == SSInsertionIndexFG[ab])
                            {
                                for (int ad = 0; ad < SSInsertionLengthFG[ab]; ad++)
                                {
                                    newReel.Add(newSymbol);
                                    if(m_useReelWeights) newWeights.Add(SSWeightsFG[ab][ad]);
                                }
                                newReel.Add(ReelsFG[aa][ac]);
                                if (m_useReelWeights)
                                {
                                    if (SSPositionsToChangeFG.Count() > 0)
                                    {
                                        if (SSPositionsToChangeFG[ab].Contains(ac)) newWeights.Add(SSPositionWeightChangesFG[ab][SSPositionsToChangeFG[ab].IndexOf(ac)]);
                                        else if (ac == 0 && !SSPositionsToChangeFG[ab].Contains(ac)) newWeights.Add(ReelsFGCutoffs[aa][ac]);
                                        else if (ac != 0 && !SSPositionsToChangeFG[ab].Contains(ac)) newWeights.Add(ReelsFGCutoffs[aa][ac] - ReelsFGCutoffs[aa][ac - 1]);
                                    }
                                    else if (SSPositionsToChangeFG.Count() == 0 || SSPositionsToChangeFG == null)
                                    {
                                        if (ac == 0) newWeights.Add(ReelsFGCutoffs[aa][ac]);
                                        else if (ac != 0) newWeights.Add(ReelsFGCutoffs[aa][ac] - ReelsFGCutoffs[aa][ac - 1]);
                                    }
                                }
                            }
                            else if (ac != SSInsertionIndexFG[ab])
                            {
                                newReel.Add(ReelsFG[aa][ac]);
                                if (m_useReelWeights)
                                {
                                    if (SSPositionsToChangeFG.Count() > 0)
                                    {
                                        if (SSPositionsToChangeFG[ab].Contains(ac)) newWeights.Add(SSPositionWeightChangesFG[ab][SSPositionsToChangeFG[ab].IndexOf(ac)]);
                                        else if (ac == 0 && !SSPositionsToChangeFG[ab].Contains(ac)) newWeights.Add(ReelsFGCutoffs[aa][ac]);
                                        else if (ac != 0 && !SSPositionsToChangeFG[ab].Contains(ac)) newWeights.Add(ReelsFGCutoffs[aa][ac] - ReelsFGCutoffs[aa][ac - 1]);
                                    }
                                    else if (SSPositionsToChangeFG.Count() == 0 || SSPositionsToChangeFG == null)
                                    {
                                        if (ac == 0) newWeights.Add(ReelsFGCutoffs[aa][ac]);
                                        else if (ac != 0) newWeights.Add(ReelsFGCutoffs[aa][ac] - ReelsFGCutoffs[aa][ac - 1]);
                                    }
                                }
                            }
                        }
                        newReels.Add(newReel);
                        if(m_useReelWeights) newCutoffs.Add(m.MakeCutoffs(newWeights));
                    }
                    ActualReelsFG.Add(newReels);
                    if(m_useReelWeights) ActualReelsFGCutoffs.Add(newCutoffs);
                }
            }
            if (SSInsertionIndexAlt != null && SSInsertionIndexAlt.Count > 0 && _reelType == ReelType.Alt)
            {
                if (ActualReelsAlt == null) ActualReelsAlt = new List<List<List<List<int>>>>();
                if (ActualReelsAltCutoffs == null) ActualReelsAltCutoffs = new List<List<List<List<int>>>>();
                for(int aa = 0; aa < ReelsAlt.Count(); aa++)
                {
                    List<List<List<int>>> newAltReelSet = new List<List<List<int>>>();
                    List<List<List<int>>> newAltReelCutoffs = new List<List<List<int>>>();
                    if(SSInsertionIndexAlt[aa].Count() > 0)
                    {
                        int newSymbol = -1;
                        if (Symbols.Contains("ss")) newSymbol = Symbols.IndexOf("ss");
                        else if (Symbols.Contains("re")) newSymbol = Symbols.IndexOf("re");
                        for(int ab = 0; ab < _reelCount; ab++)
                        {
                            List<List<int>> newReels = new List<List<int>>();
                            List<List<int>> newCutoffs = new List<List<int>>();
                            newReels.Add(ReelsAlt[aa][ab]);
                            if(m_useReelWeights) newCutoffs.Add(ReelsAltCutoffs[aa][ab]);
                            for(int ac = 0; ac < SSInsertionIndexAlt[aa].Count(); ac++)
                            {
                                List<int> newReel = new List<int>();
                                List<int> newWeights = new List<int>();
                                for(int ad = 0; ad < ReelsAlt[aa][ab].Count(); ad++)
                                {
                                    if(ad == SSInsertionIndexAlt[aa][ac])
                                    {
                                        for(int ae = 0; ae < SSInsertionLengthAlt[aa][ac]; ae++)
                                        {
                                            newReel.Add(newSymbol);
                                            if(m_useReelWeights) newWeights.Add(SSWeightsAlt[aa][ac][ae]);
                                        }
                                        newReel.Add(ReelsAlt[aa][ab][ad]);
                                        if (m_useReelWeights)
                                        {
                                            if (SSPositionsToChangeAlt[aa][ac].Count() > 0)
                                            {
                                                if (SSPositionsToChangeAlt[aa][ac].Contains(ad)) newWeights.Add(SSPositionWeightChangesAlt[aa][ac][SSPositionWeightChangesAlt[aa][ac].IndexOf(ad)]);
                                                else if (ad == 0 && !SSPositionsToChangeAlt[aa][ac].Contains(ad)) newWeights.Add(ReelsAltCutoffs[aa][ab][ad]);
                                                else if (ad != 0 && !SSPositionsToChangeAlt[aa][ac].Contains(ad)) newWeights.Add(ReelsAltCutoffs[aa][ab][ad] - ReelsAltCutoffs[aa][ab][ad - 1]);
                                            }
                                            else if (SSPositionsToChangeAlt[aa][ab].Count() == 0 || SSPositionsToChangeAlt[aa][ab] == null)
                                            {
                                                if (ad == 0) newWeights.Add(ReelsAltCutoffs[aa][ab][ad]);
                                                else if (ad != 0) newWeights.Add(ReelsAltCutoffs[aa][ab][ad] - ReelsAltCutoffs[aa][ab][ad - 1]);
                                            }
                                        }
                                    }
                                    else if (ad != SSInsertionIndexAlt[aa][ac])
                                    {
                                        newReel.Add(ReelsAlt[aa][ab][ad]);
                                        if (m_useReelWeights)
                                        {
                                            if (SSPositionsToChangeAlt[aa][ac].Count() > 0)
                                            {
                                                if (SSPositionsToChangeAlt[aa][ac].Contains(ad)) newWeights.Add(SSPositionWeightChangesAlt[aa][ac][SSPositionWeightChangesAlt[aa][ac].IndexOf(ad)]);
                                                else if (ad == 0 && !SSPositionsToChangeAlt[aa][ac].Contains(ad)) newWeights.Add(ReelsAltCutoffs[aa][ab][ad]);
                                                else if (ad != 0 && !SSPositionsToChangeAlt[aa][ac].Contains(ad)) newWeights.Add(ReelsAltCutoffs[aa][ab][ad] - ReelsAltCutoffs[aa][ab][ad - 1]);
                                            }
                                            else if (SSPositionsToChangeAlt[aa][ab].Count() == 0 || SSPositionsToChangeAlt[aa][ab] == null)
                                            {
                                                if (ad == 0) newWeights.Add(ReelsAltCutoffs[aa][ab][ad]);
                                                else if (ad != 0) newWeights.Add(ReelsAltCutoffs[aa][ab][ad] - ReelsAltCutoffs[aa][ab][ad - 1]);
                                            }
                                        }
                                    }
                                }
                                newReels.Add(newReel);
                                if(m_useReelWeights) newCutoffs.Add(m.MakeCutoffs(newWeights));
                            }
                            newAltReelSet.Add(newReels);
                            if(m_useReelWeights) newAltReelCutoffs.Add(newCutoffs);
                        }
                        ActualReelsAlt.Add(newAltReelSet);
                        if(m_useReelWeights) ActualReelsAltCutoffs.Add(newAltReelCutoffs);
                    }
                }
            }
        }
    }
}
