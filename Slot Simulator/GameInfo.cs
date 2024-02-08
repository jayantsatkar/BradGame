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
    enum DefaultGameStates { PGPostSpin, FGPreSpin, FGPostSpin, ReSpinPreSpin, ReSpinPostSpin, BankGameState, SuperFGPreSpin, SuperFGPostSpin}
    class GameInfo
    {
        internal static GameInfo CreateGameInfo(string _gameInfoFilePath)
        {
            using (ExcelFile excelFile = new ExcelFile(_gameInfoFilePath))
            {
                string gameType = excelFile.Range[2][2].Value.ToString().ToLower();
                switch (gameType)
                {
                    case "buffalo": return new Buffalo(excelFile);
                    case "dragon's law": return new DragonsLaw(excelFile);
                    case "jumpin jalapenos": return new JumpinJalapenos(excelFile);
                    case "kitty glitter": return new KittyGlitter(excelFile);
                    case "solstice celebration": return new SolsticeCelebration(excelFile);
                    case "fu panda": return new Fu_Panda(excelFile);
                    case "template": return new Template(excelFile);
                    case "queen of egypt": return new Queen_of_Egypt(excelFile);
                    case "phoenix wins": return new Phoenix_Wins(excelFile);
                    case "sapphire wins": return new Sapphire_Wins(excelFile);
                    case "jaguar jewels": return new Jaguar_Jewels(excelFile);
                    case "fu cat": return new Fu_Cat(excelFile);
                    case "burning rose": return new Burning_Rose(excelFile);
                    case "fu cat pxs": return new Fu_Cat_PXS(excelFile);
                    case "ruby wins": return new Ruby_Wins(excelFile);
                    case "bone garden": return new Bone_Garden(excelFile);
                    case "quasi shot": return new Quasi_Shot(excelFile);
                    case "lucky genie": return new Lucky_Genie(excelFile);
                    case "fire wolf": return new Fire_Wolf(excelFile);
                    case "golden wins lucky scroll": return new Golden_Wins_Lucky_Scroll(excelFile);
                    case "butterfly game": return new Butterfly_Game(excelFile);
                    case "emerald fairy": return new Emerald_Fairy(excelFile);
                    case "fu babies": return new Fu_Babies(excelFile);
                    case "fire wolf2": return new Fire_Wolf2(excelFile);
                    case "temp": return new Temp(excelFile);
                    //case "markov chains": return new Markov_Chains(excelFile);
                    //case "fu pinata": return new Fu_Pinata(excelFile);
                    //case "dynamite dinos": return new Dynamite_Dinos(excelFile);
                    default: throw new ArgumentException("Unknown file type: " + gameType);
                }
            }
        }
        internal GameInfo(ExcelFile _excelFile)
        {
            FileNamePath = _excelFile.FilePath;
            FileName = _excelFile.FileName;
            object[,] values = (object[,])_excelFile.Range.Value2;
            Reels = new Reels();
            m_scatterToFreeGames = new Dictionary<string, Dictionary<int, int>>();
            m_scatterToFreeGamesFG = new Dictionary<string, Dictionary<int, int>>();
            m_freeGamesToScatter = new Dictionary<string, Dictionary<int, int>>();
            m_freeGamesToScatterFG = new Dictionary<string, Dictionary<int, int>>();
            OversizedOffsets = new Dictionary<string, Point>();
            m_extraData = new Dictionary<string, List<List<string>>>();
            m_extraGeneralData = new Dictionary<string, string>();
            for (int col = 1; col <= values.GetLength(1); col++)
            {
                string header = values[1, col] == null ? "" : values[1, col].ToString().ToLower();
                int altIndex = 0;
                switch (header)
                {
                    case "general": ProcessGeneral(values, col); break;
                    case "symbols": ProcessSymbols(values, col); break;
                    case "wild info": ProcessWilds(values, col); break;
                    case "scatter info": ProcessScatters(values, col); break;
                    case "respin": ProcessRespin(values, col); break;
                    case "pg reel strips": Reels.ProcessReelStrips(values, col, ReelType.PG, ReelCount); break;
                    case "pg1 reel strips": Reels.ProcessReelStrips(values, col, ReelType.PG1, ReelCount); break;
                    case "pg2 reel strips": Reels.ProcessReelStrips(values, col, ReelType.PG2, ReelCount); break;
                    case "fg reel strips": Reels.ProcessReelStrips(values, col, ReelType.FG, ReelCount); break;
                    case "fg5 reel strips": Reels.ProcessReelStrips(values, col, ReelType.FG5, ReelCount); break;
                    case "fg6 reel strips": Reels.ProcessReelStrips(values, col, ReelType.FG6, ReelCount); break;
                    case "fg7 reel strips": Reels.ProcessReelStrips(values, col, ReelType.FG7, ReelCount); break;
                    case "fg8 reel strips": Reels.ProcessReelStrips(values, col, ReelType.FG8, ReelCount); break;
                    case "alt reel strips": Reels.ProcessReelStrips(values, col, ReelType.Alt, ReelCount); break;
                    case "pg ss selection": Reels.GetSSSelection(values, col, ReelCount, ReelType.PG); break;
                    case "fg ss selection": Reels.GetSSSelection(values, col, ReelCount, ReelType.FG); break;
                    case "alt ss selection": Reels.GetSSSelection(values, col, ReelCount, ReelType.Alt); break;
                    case "pg sym replacement": ProcessSymReplacementFeature(values, col, ReelType.PG, ReelCount); break;
                    case "fg sym replacement": ProcessSymReplacementFeature(values, col, ReelType.FG, ReelCount); break;
                    case "pg ss insertion information": Reels.GetSSInsertionInformation(values, col, ReelCount, ReelType.PG); break;
                    case "fg ss insertion information": Reels.GetSSInsertionInformation(values, col, ReelCount, ReelType.FG); break;
                    case "alt ss insertion information": Reels.GetSSInsertionInformation(values, col, ReelCount, ReelType.Alt); break;
                    case "pg ss weights": Reels.ProcessSSWeights(values, col, ReelType.PG); Reels.buildReelStrips(ReelCount, ReelType.PG); break;
                    case "fg ss weights": Reels.ProcessSSWeights(values, col, ReelType.FG); Reels.buildReelStrips(ReelCount, ReelType.FG); break;
                    case "alt ss weights": Reels.ProcessSSWeights(values, col, ReelType.Alt, altIndex); altIndex++; break;
                    case "pay table": ProcessPayTable(values, col); break;
                    case "fg pay table": ProcessPayTable(values, col, true); break;
                    case "lines": ProcessLines(values, col); break;
                    case "progressive information": if(m_progressiveType == "custom" || m_progressiveType == "dajidali") ProcessProgressives(values, col); break;
                    case "progressive chance": ProcessProgressiveChance(values, col); break;
                    case "progressive weights": ProcessProgressiveWeights(values, col); break;
                    case "jackpot scripts": ProcessJackpotScripts(values, col); break;
                    case "surge feature": ProcessSurgeFeature(values, col); break;
                    case "": break;
                    default: ProcessExtraInformation(values, col); break;
                }
            }
            if (Reels.ReelsAlt.Count() > 0) Reels.buildReelStrips(ReelCount, ReelType.Alt);
            if(Reels.ReelsPG != null)
                currentReelSet = Reels.ReelsPG;
            else                                           //this is for when we have 2 base game reel strip sets and therefore ReelsPG hasn't been set
                currentReelSet = Reels.ReelsPG1;
            if (Reels.m_useReelWeights) currentCutoffSet = Reels.ReelsPGCutoffs;
            ProcessOversized();
            ReelIndexes = m.MakeNewList<int>(ReelCount, 0);
            m_cycleSizeForCalc = 1;
            m_cycleSize = 1;
            for (int aa = 0; aa < ReelCount; aa++)
            {
                if (!Reels.m_useReelWeights)
                {
                    if (Reels.ReelsPG != null)
                    {
                        m_cycleSize *= (ulong)Reels.ReelsPG[aa].Count();
                        m_cycleSizeForCalc *= (ulong)Reels.ReelsPG[aa].Count();
                    }
                    else                                  //this is for when we have 2 base game reel strips that are both equally likely to be chosen and have the same cycle
                    {
                        m_cycleSize *= (ulong)Reels.ReelsPG1[aa].Count();
                        m_cycleSizeForCalc *= (ulong)Reels.ReelsPG1[aa].Count();
                    }
                }
                else if (Reels.m_useReelWeights)
                {
                    if (Reels.ReelsPG != null)
                    {
                        m_cycleSize *= (ulong)Reels.ReelsPG[aa].Count();
                        m_cycleSizeForCalc *= (ulong)Reels.ReelsPGCutoffs[aa][Reels.ReelsPG[aa].Count() - 1];
                    }
                    else                                  //this is for when we have 2 base game reel strips that are both equally likely to be chosen and have the same cycle
                    {
                        m_cycleSize *= (ulong)Reels.ReelsPG1[aa].Count();
                        m_cycleSizeForCalc *= (ulong)Reels.ReelsPG1Cutoffs[aa][Reels.ReelsPG1[aa].Count() - 1];
                    }
                }
            }
            Sounds = m.MakeNewList<SoundTypes>(ReelCount, SoundTypes.ReelLock);
            Sounds[0] = SoundTypes.Reel1;
            Sounds[1] = SoundTypes.Reel2;
            Sounds[2] = SoundTypes.Reel3;
            Sounds[3] = SoundTypes.Reel4;
            Sounds[4] = SoundTypes.Reel5;
            Sounds.Add(SoundTypes.Empty);
            InFreeGames = false;
            m_symbolToPayArgPG = new Dictionary<int, PayArgs>();
            foreach (PayArgs payArg in PayTables)
                m_symbolToPayArgPG[payArg.CanBeWithoutMultipliers[0]] = payArg;
            m_symbolToPayArgFG = new Dictionary<int, PayArgs>();
            foreach (PayArgs payArg in PayTablesFG)
                m_symbolToPayArgFG[payArg.CanBeWithoutMultipliers[0]] = payArg;
        }
        //Variables//////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Misc
        internal string GameName, FileName, FileNamePath;
        internal string FileNameWithoutGameData { get { return FileName.Replace("Game Data ", ""); } }
        internal Reels Reels;
        internal List<WildCanBeArg> WildCanBes;
        internal Dictionary<string, List<List<string>>> m_extraData;
        internal Dictionary<string, string> m_extraGeneralData;
        internal Dictionary<string, Dictionary<int, int>> m_scatterToFreeGames, m_scatterToFreeGamesFG;
        internal Dictionary<string, Dictionary<int, int>> m_freeGamesToScatter, m_freeGamesToScatterFG;
        //For Display
        internal System.Windows.Forms.Keys CheatKey;
        internal Dictionary<string, Point> OversizedOffsets;
        internal int[] SpinOrder;
        internal int ReelsStartX = 0;
        internal int ReelsStartY = 0;
        internal int BackgroundWidth = 1240;
        internal int BackgroundHeight = 800;
        internal int ReelSpacing = 20;
        internal int SymbolWidth = 200;
        internal int SymbolHeight = 174;
        internal int ActionPauseInMilliSeconds = 0;
        internal int ShowWinDelayInFrames = 30;
        public int m_frmProgLeftStart = 0;
        public double conversionRatio = 0;
        public int originalBasicImageSize = 0;
        public string m_lineConfiguration = "";
        public frmProgressives frmProg;
        public bool m_soundMute = true;
        public int m_bankSize = 1;
        public int m_shamrockBoosts = 0;
        public int m_currentBankedGame = 1;
        public frmChooseVolatility frmChoose;
        public frmPickGame frmPickGame;
        //internal int TopMessagesOffsetAdjustment = 0;
        internal int BottomMessagesOffsetAdjustment = 0;
        internal int BottomMetersOffsetAdjustment = 0;
        internal int BottomMetersXOffset = -1;
        internal int MaximizedX = 0;
        internal int MaximizedY = 0;
        internal int CustomOffsetXNumberBoxBet = -1;
        internal int CustomOffsetYNumberBoxBet = -1;
        internal int CustomOffsetXNumberBoxWin = -1;
        internal int CustomOffsetYNumberBoxWin = -1;
        internal int CustomOffsetXNumberBoxTotal = -1;
        internal int CustomOffsetYNumberBoxTotal = -1;
        internal string CustomPrefixNumberBoxBet = "betfont";
        internal string CustomPrefixNumberBoxWin = "betfont";
        internal string CustomPrefixNumberBoxTotal = "betfont";
        internal bool TopMessageShow = true;
        internal bool BottomMessageShow = true;
        internal bool BottomLeftMessageShow = true;
        internal bool BottomRightMessageShow = true;
        internal int ShowWinIterationDelay = 33;
        protected int[] cDefaultSpinOrder = { 1, 2, 3, 4, 5 };
        protected int[] cNudgeSpinOrder = { -1, -1, -1, -1, -1 };
        internal List<SoundTypes> Sounds;
        internal Dictionary<int, string> BonusCodesToColors = new Dictionary<int, string>();
        internal bool Animating;
        //For Calc
        internal bool InFreeGames;
        internal int Bet { get { return MinBet * BetLevel; } }
        internal float ExpectedRTP = .90F;
        internal int MinBet, BetLevel, Lines, ReelCount, BonusCode;
        internal List<int> Dimensions;
        internal List<int> ReelIndexes;
        internal List<int> PossibleBetLevels;
        internal List<List<int>> m_lines;
        internal List<PayArgs> PayTables, PayTablesFG;
        internal List<WinArgs> Wins, WinsToShow;
        public int[] hold_or_spin_reels = new int[5] {0, 0, 0, 0, 0};
        protected bool m_noWildsOnFirstReel = false;
        public int m_freeGamesLeft, m_freeGamesPlayed;
        public int ReplaceIndex = 0;
        public int num_respins = 0;
        protected Dictionary<int, PayArgs> m_symbolToPayArgPG, m_symbolToPayArgFG;
        internal int m_insertionSymbol = -1;
        internal int m_insertionIndex = 0;
        internal string m_normalEvaluationType = "";
        internal string m_scatterEvaluationType = "";
        internal bool m_stackShow = true;
        internal List<List<int>> currentReelSet;
        internal List<List<int>> currentCutoffSet;
        internal List<List<int>> m_freeGameReels;
        internal List<List<int>> m_freeGameCutoffs;
        internal List<double> m_currentProgressiveAwards;
        internal bool trigger_surge_feature = false;
        //For Stats
        internal const string cFreeGamePrefix = "Free Game: ";
        internal int WinsThisGame, WinsThisSpin;
        internal bool DoDefaultWinDistributions = false;
        internal bool DoCustomStats = false;
        internal bool InSimulation = false;
        internal bool SecondFeature = false;
        internal bool StepThrough = false;
        public int m_statsWinsThisPG;
        public WinDistributionChart m_statsWinDistributionPG, m_statsWinDistributionFG, m_statsWinDistributionPaidGame, m_statsWinDistributionPaidGameHigh;
        internal double m_gamePayback = 0.90;
        //protected RecoveryMatrix m_standardRecoveryCalc;
        //protected SurvivabilityMatrix m_standardSurvivabilityCalc;
        public List<setDistribution> m_setsForSimulation;
        protected List<SessionStats> m_currentSessionsInProgress;
        protected bool fiftyTimesBetAlreadyIncluded = false;
        protected List<int> currentSetIndex;
        public SortedDictionary<int, long[]> m_winsByType;
        public long m_numberOfFreeGameSessions;
        public string m_progressiveType;
        public int m_progressiveWinPG = 0;
        public int m_progressiveWinFG = 0;
        public List<ProgressiveData> m_progressives;
        public int m_totalProgressiveWinFG = 0;
        public int m_maxProgressiveWinThisFG = 0;
        public int m_fgWithProgressives = 0;
        public int m_progressiveGames = 0;
        public int m_pgWithProgressives = 0;
        public double m_progressiveChance = 0;
        public List<int> m_progressiveCutoffs;
        public List<int> m_surgeCutoffs;
        public List<int> m_SymReplacePGCutoffs;
        public List<int> m_SymReplaceFGCutoffs;
        public bool m_dajidaliCheat = false;
        public List<pickSciptData> m_jackpotScripting;
        public List<int> m_currentStepThroughStops;
        internal ulong m_cycleSize;
        internal List<int> m_weightsForStepThrough;
        internal ulong m_cycleSizeForCalc;
        internal List<string> m_stringsForPick;
        internal bool doTODStats = false;
        //Virtual//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal virtual void PreSpin(bool _showStacks)
        {
            InFreeGames = false;
            m_currentBankedGame = 1;
            m_shamrockBoosts = 0;
            if (StepThrough) _showStacks = false;
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            if (_showStacks)
            {
                if (!StepThrough) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsBG);
                else if (StepThrough) m_insertionIndex = 0;
                for (int ae = 0; ae < ReelCount; ae++)
                {
                    if (Reels.SSSymbolsBG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("ss") && Reels.SSSymbolsBG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsPG[ae][Reels.SSInsertionInfoIndicesBG[m_insertionIndex][ae] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsPGCutoffs[ae][Reels.SSInsertionInfoIndicesBG[m_insertionIndex][ae] + 1]);
                    }
                    else if (Reels.SSSymbolsBG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("ss") || Reels.SSSymbolsBG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsPG[ae]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsPGCutoffs[ae]);
                    }
                }
            }
            else if (!_showStacks)
            {
                for(int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsPG[af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsPGCutoffs[af]);
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            if(!StepThrough) ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
            else if (StepThrough)
            {
                List<bool> reelStopCheck = new List<bool>();
                m_weightsForStepThrough = new List<int>();
                for(int aa = 0; aa < ReelCount; aa++)
                {
                    if (m_currentStepThroughStops[aa] == Reels.ReelsPG[aa].Count() - 1) reelStopCheck.Add(true);
                    else if (m_currentStepThroughStops[aa] != Reels.ReelsPG[aa].Count() - 1) reelStopCheck.Add(false);
                }
                if (reelStopCheck[1] && reelStopCheck[2] && reelStopCheck[3] && reelStopCheck[4])
                {
                    if (reelStopCheck[0]) m_currentStepThroughStops[0] = 0;
                    else if (!reelStopCheck[0]) m_currentStepThroughStops[0]++;
                }
                if (reelStopCheck[2] && reelStopCheck[3] && reelStopCheck[4])
                {
                    if (reelStopCheck[1]) m_currentStepThroughStops[1] = 0;
                    else if (!reelStopCheck[1]) m_currentStepThroughStops[1]++;
                }
                if (reelStopCheck[3] && reelStopCheck[4])
                {
                    if (reelStopCheck[2]) m_currentStepThroughStops[2] = 0;
                    else if (!reelStopCheck[2]) m_currentStepThroughStops[2]++;
                }
                if (reelStopCheck[4])
                {
                    if (reelStopCheck[3]) m_currentStepThroughStops[3] = 0;
                    else if (!reelStopCheck[3]) m_currentStepThroughStops[3]++;
                    m_currentStepThroughStops[4] = 0;
                }
                else if (!reelStopCheck[4]) m_currentStepThroughStops[4]++;
                for (int ab = 0; ab < ReelCount; ab++)
                {
                    ReelIndexes[ab] = m_currentStepThroughStops[ab];
                    if (Reels.m_useReelWeights && ReelIndexes[ab] != 0) m_weightsForStepThrough.Add(currentCutoffSet[ab][ReelIndexes[ab]]-currentCutoffSet[ab][ReelIndexes[ab]-1]);
                    else if (Reels.m_useReelWeights && ReelIndexes[ab] == 0) m_weightsForStepThrough.Add(currentCutoffSet[ab][ReelIndexes[ab]]);
                    else if (!Reels.m_useReelWeights) m_weightsForStepThrough.Add(1);
                }
            }
            m_freeGamesLeft = 0;
            m_freeGamesPlayed = 0;
            m_progressiveWinPG = 0;
            m_progressiveWinFG = 0;
            m_totalProgressiveWinFG = 0;
            m_maxProgressiveWinThisFG = 0;
            Wins = new List<WinArgs>();
            WinsToShow = Wins;
            ActionPauseInMilliSeconds = 0;
            BonusCode = 0;
            SpinOrder = cDefaultSpinOrder;
            Animating = false;
            if (DoDefaultWinDistributions)
            {
                m_statsWinDistributionPaidGame.IncGames();
                m_statsWinDistributionPaidGameHigh.IncGames();
                m_statsWinDistributionPG.IncGames();
            }
            if (!InSimulation)
            {
                m.gameMessageBox.Text = "";
            }
        }
        internal virtual void PreSpinRandomBase(int cycle)
        {
            InFreeGames = false;
            m_currentBankedGame = 1;
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();

            if (cycle == 0)
            {
                for (int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsPG1[af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsPG1Cutoffs[af]);
                }
            }
            else
            {
                for (int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsPG2[af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsPG2Cutoffs[af]);
                }
            }
            
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
            
            m_freeGamesLeft = 0;
            m_freeGamesPlayed = 0;
            m_progressiveWinPG = 0;
            m_progressiveWinFG = 0;
            m_totalProgressiveWinFG = 0;
            m_maxProgressiveWinThisFG = 0;
            Wins = new List<WinArgs>();
            WinsToShow = Wins;
            ActionPauseInMilliSeconds = 0;
            BonusCode = 0;
            SpinOrder = cDefaultSpinOrder;
            Animating = false;
            if (DoDefaultWinDistributions)
            {
                m_statsWinDistributionPaidGame.IncGames();
                m_statsWinDistributionPaidGameHigh.IncGames();
                m_statsWinDistributionPG.IncGames();
            }
            if (!InSimulation)
            {
                m.gameMessageBox.Text = "";
            }
        }
        internal virtual GameAction PostSpin() { throw new ArgumentException("Must Override Post Spin"); }
        internal virtual void SaveStateBeforeSpin() { }
        internal virtual void ReloadStateToBeforeSpin() { }
        internal virtual void ResetAnyAnimationsIfFastPlay() { Animating = false; }
        internal virtual void GetFGReelStrips(bool _showStacks)
        {
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            if (_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int ae = 0; ae < ReelCount; ae++)
                {
                    if (Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("ss") && Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsFG[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsFGCutoffs[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                    }
                    else if (Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("ss") || Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsFG[ae]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFGCutoffs[ae]);
                    }
                }
            }
            else if (!_showStacks)
            {
                if(Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsFG[af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFGCutoffs[af]);
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
        }
        internal virtual void GetFG5ReelStrips(bool _showStacks)
        {
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            if (_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int ae = 0; ae < ReelCount; ae++)
                {
                    if (Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("ss") && Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsFG5[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsFG5Cutoffs[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                    }
                    else if (Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("ss") || Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsFG5[ae]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFG5Cutoffs[ae]);
                    }
                }
            }
            else if (!_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsFG5[af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFG5Cutoffs[af]);
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
        }
        internal virtual void GetFG6ReelStrips(bool _showStacks)
        {
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            if (_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int ae = 0; ae < ReelCount; ae++)
                {
                    if (Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("ss") && Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsFG6[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsFG6Cutoffs[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                    }
                    else if (Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("ss") || Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsFG6[ae]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFG6Cutoffs[ae]);
                    }
                }
            }
            else if (!_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsFG6[af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFG6Cutoffs[af]);
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
        }
        internal virtual void GetFG7ReelStrips(bool _showStacks)
        {
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            if (_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int ae = 0; ae < ReelCount; ae++)
                {
                    if (Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("ss") && Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsFG7[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsFG7Cutoffs[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                    }
                    else if (Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("ss") || Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsFG7[ae]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFG7Cutoffs[ae]);
                    }
                }
            }
            else if (!_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsFG7[af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFG7Cutoffs[af]);
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
        }
        internal virtual void GetFG8ReelStrips(bool _showStacks)
        {
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            if (_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int ae = 0; ae < ReelCount; ae++)
                {
                    if (Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("ss") && Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsFG8[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsFG8Cutoffs[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                    }
                    else if (Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("ss") || Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsFG8[ae]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFG8Cutoffs[ae]);
                    }
                }
            }
            else if (!_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsFG8[af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFG8Cutoffs[af]);
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
        }
        internal virtual void GetAltReelStrips(int altIndex, bool _showStacks)
        {
            List<List<int>> newReelSet = new List<List<int>>();
            List<List<int>> newReelCutoffs = new List<List<int>>();
            if (_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int ae = 0; ae < ReelCount; ae++)
                {
                    if (Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("ss") && Reels.SSSymbolsFG[m_insertionIndex][ae] != Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ActualReelsFG[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ActualReelsFGCutoffs[ae][Reels.SSInsertionInfoIndicesFG[m_insertionIndex][ae] + 1]);
                    }
                    else if (Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("ss") || Reels.SSSymbolsFG[m_insertionIndex][ae] == Reels.Symbols.IndexOf("re"))
                    {
                        newReelSet.Add(Reels.ReelsFG[ae]);
                        if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFGCutoffs[ae]);
                    }
                }
            }
            else if (!_showStacks)
            {
                if (Reels.SSCutoffsFG != null) m_insertionIndex = getInsertionIndex(Reels.SSCutoffsFG);
                for (int af = 0; af < ReelCount; af++)
                {
                    newReelSet.Add(Reels.ReelsAlt[altIndex][af]);
                    if (Reels.m_useReelWeights) newReelCutoffs.Add(Reels.ReelsFGCutoffs[af]);
                }
            }
            currentReelSet = newReelSet;
            currentCutoffSet = newReelCutoffs;
            ReelIndexes = Reels.GetRandomIndexes(newReelSet, newReelCutoffs);
        }
        //Calculate
        internal virtual int getInsertionIndex(List<int> SSSelection)
        {
            int index = -1;
            if(SSSelection.Count > 0 && SSSelection != null)
            {
                index = m.RandomIndex(SSSelection);
            }
            return index;
        }
        internal virtual List<List<int>> GetScreenSymbols(List<List<int>> ReelSet)
        {
            List<List<int>> reels = ReelSet;
            List<List<int>> screenSymbols = new List<List<int>>();
            List<List<int>> currentInsertionSet = InFreeGames ? Reels.SSSymbolsFG : Reels.SSSymbolsBG;
            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
            {
                List<int> reel = reels[reelNum];
                List<int> reelSymbols = new List<int>();
                for (int i = 0; i < Dimensions[reelNum]; i++)
                {
                    int index = (ReelIndexes[reelNum] + i + reel.Count) % reel.Count;
                    int symbolIndex = reel[index];
                    if (InFreeGames == true && m_SymReplaceFGCutoffs != null)
                        reelSymbols.Add(symbolIndex == m_insertionSymbol ? Reels.Symbols.IndexOf(Reels.Symbols[Reels.ReelsSymReplaceFG[reelNum][ReplaceIndex]]) : symbolIndex);
                    else if (InFreeGames == false && m_SymReplacePGCutoffs != null)
                        reelSymbols.Add(symbolIndex == m_insertionSymbol ? Reels.Symbols.IndexOf(Reels.Symbols[Reels.ReelsSymReplacePG[reelNum][ReplaceIndex]]) : symbolIndex);
                    else
                        reelSymbols.Add(symbolIndex == m_insertionSymbol ? currentInsertionSet[m_insertionIndex][reelNum] : symbolIndex);
                }
                screenSymbols.Add(reelSymbols);
            }
            return screenSymbols;
        }
        internal virtual List<List<int>> GetScreenSymbolsFromReelCombos(List<List<List<int>>> _reelCombos)
        {
            List<List<int>> screenSymbols = new List<List<int>>();
            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                screenSymbols.Add(_reelCombos[reelNum][ReelIndexes[reelNum]]);
            return screenSymbols;
        }
        internal virtual bool CalculateWins(List<WinArgs> _wins, List<List<int>>_screenSymbols, string _type, ref int _freeGames, int _multiplier = 1)
        {
            if(_type == "normal")
            {
                if (m_normalEvaluationType == "line") CalculateLineWins(_wins, _screenSymbols, _multiplier);
                else if (m_normalEvaluationType == "powerxstream") CalculateAnyWayWins(_wins, _screenSymbols, _multiplier);
                else if (m_normalEvaluationType == "powerxstream2") CalculateAnyWayLtoRRtoL(_wins, _screenSymbols, _multiplier);
            }
            else if (_type == "scatter")
            {
                if (m_scatterEvaluationType == "standard") CalculateScatterWins(_wins, _screenSymbols, ref _freeGames, _multiplier);
                else if (m_scatterEvaluationType == "lefttoright") CalculateScatterWinsLeftToRight(_wins, _screenSymbols, ref _freeGames, _multiplier);
                else if (m_scatterEvaluationType == "multiway") CalculateScatterWayWins(_wins, _screenSymbols, ref _freeGames, _multiplier);
            }
            return _wins.Count > 0 ? true : false;
        }
        internal virtual bool CalculateLineWins(List<WinArgs> _wins, List<List<int>> _screenSymbols, int _multiplier = 1)
        {
            bool anyWin = false;
            string _prefix = InFreeGames ? cFreeGamePrefix : "";
            List<PayArgs> payTable = InFreeGames ? PayTablesFG : PayTables;
            Dictionary<int, PayArgs> symbolToPayArg = InFreeGames ? m_symbolToPayArgFG : m_symbolToPayArgPG;
            List<int> lineSymbols = m.MakeNewList<int>(ReelCount, 0);
            for (int lineIndex = 0; lineIndex < Lines; lineIndex++)
            {
                for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                    lineSymbols[reelNum] = _screenSymbols[reelNum][m_lines[lineIndex][reelNum]];
                if (m_noWildsOnFirstReel)
                {
                    if (symbolToPayArg.ContainsKey(lineSymbols[0]) && !symbolToPayArg[lineSymbols[0]].IsScatter)
                    {
                        PayArgs payArg = symbolToPayArg[lineSymbols[0]];
                        int match = 0;
                        for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                        {
                            if (payArg.CanBe.ContainsKey(lineSymbols[reelNum]))
                            {
                                match++;
                                _multiplier *= payArg.CanBe[lineSymbols[reelNum]];
                            }
                            else break;
                        }
                        if (match > 0 && payArg.Pays[match - 1] > 0)
                        {
                            List<List<int>> cellNums = null;
                            if (!InSimulation)
                            {
                                cellNums = new List<List<int>>();
                                foreach (int cellNum in m_lines[lineIndex])
                                    cellNums.Add(new List<int>(new int[] { cellNum }));
                            }
                            _wins.Add(new WinArgs(_prefix == "" ? payArg.Name : _prefix + payArg.Name, payArg.CanBeWithoutMultipliers[0], payArg.Pays[match - 1] * _multiplier * BetLevel, match, _multiplier, cellNums));
                            anyWin = true;
                        }
                    }
                }
                else
                {
                    int topMatch = 0;
                    int topPay = 0;
                    int topMultiplier = 0;
                    PayArgs topPayArg = null;
                    foreach (PayArgs payArg in payTable)
                    {
                        int currentMultiplier = _multiplier;
                        if (!payArg.IsScatter)
                        {
                            int match = 0;
                            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                            {
                                if (payArg.CanBe.ContainsKey(lineSymbols[reelNum]))
                                {
                                    match++;
                                    if(currentMultiplier == 1)
                                    {
                                        currentMultiplier *= payArg.CanBe[lineSymbols[reelNum]];
                                    }
                                }
                                else break;
                            }
                            if (match > 0 && payArg.Pays[match - 1] > 0)
                            {
                                int pay = payArg.Pays[match - 1] * currentMultiplier * BetLevel;
                                if (topPay < pay)
                                {
                                    topPay = pay;
                                    topMatch = match;
                                    topMultiplier = currentMultiplier;
                                    topPayArg = payArg;
                                }
                            }
                        }
                    }
                    if (topPayArg != null)
                    {
                        List<List<int>> cellNums = null;
                        //if (!InSimulation)
                        //{
                            cellNums = new List<List<int>>();
                            foreach (int cellNum in m_lines[lineIndex])
                                cellNums.Add(new List<int>(new int[] { cellNum }));
                        //}
                        _wins.Add(new WinArgs(_prefix == "" ? topPayArg.Name : _prefix + topPayArg.Name, topPayArg.CanBeWithoutMultipliers[0], topPay, topMatch, topMultiplier, cellNums));
                        anyWin = true;
                    }
                }
            }
            //TODO
            return anyWin;
        }
        internal virtual bool CalculateAnyWayWins(List<WinArgs> _wins, List<List<int>> _screenSymbols, int _multiplier = 1)
        {
            bool anyWin = false;
            //List<PayArgs> payTable = InFreeGames ? PayTablesFG : PayTables;
            string _prefix = InFreeGames ? cFreeGamePrefix : "";
            Dictionary<int, PayArgs> symbolToPayArg = InFreeGames ? m_symbolToPayArgFG : m_symbolToPayArgPG;
            List<int> done = new List<int>();
            foreach (int symbolOn1 in _screenSymbols[0])
                if (!done.Contains(symbolOn1) && symbolToPayArg.ContainsKey(symbolOn1) && !symbolToPayArg[symbolOn1].IsScatter)
                {
                    done.Add(symbolOn1);
                    PayArgs payArg = symbolToPayArg[symbolOn1];
                    int match = 0;
                    int count = 1;
                    for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                    {
                        int countThisReel = 0;
                        foreach (int symbol in _screenSymbols[reelNum])
                            if (payArg.CanBe.ContainsKey(symbol))
                            {
                                countThisReel++;
                                if (payArg.CanBe[symbol] > 1) _multiplier *= payArg.CanBe[symbol];
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
                        _wins.Add(new WinArgs(_prefix == "" ? payArg.Name : _prefix + payArg.Name, payArg.CanBeWithoutMultipliers[0], payArg.Pays[match - 1] * count * _multiplier * BetLevel, match, _multiplier, cellNums));
                        anyWin = true;
                    }
                    _multiplier = 1; //Emrich added code to make wild multipliers reset after each pay evaluation
                }
            return anyWin;
        }
        internal virtual bool CalculateAnyWayLtoRRtoL(List<WinArgs> _wins, List<List<int>> _screenSymbols, int _multiplier = 1)
        {
            bool anyWin = false;
            string _prefix = InFreeGames ? cFreeGamePrefix : "";
            List<PayArgs> payTable = InFreeGames ? PayTablesFG : PayTables;
            foreach(PayArgs payArg in payTable)
            {
                if(!payArg.IsScatter)
                {
                    List<int> reelCounts = new List<int>();
                    for(int reel = 0; reel < ReelCount; reel++)
                    {
                        int count = 0;
                        foreach (int symbol in _screenSymbols[reel])
                        {
                            if(payArg.CanBe.ContainsKey(symbol))
                            {
                                count++;
                            }
                        }
                        reelCounts.Add(count);
                    }
                    int countFromRight = 0;
                    int countFromLeft = 0;
                    for(int aa = 0; aa < ReelCount; aa++)
                    {
                        if (reelCounts[aa] > 0) countFromLeft++;
                        else if (reelCounts[aa] == 0) break;
                    }
                    if(countFromLeft != ReelCount)
                    {
                        for(int ab = ReelCount-1; ab >= 0; ab--)
                        {
                            if (reelCounts[ab] > 0) countFromRight++;
                            else if (reelCounts[ab] == 0) break;
                        }
                    }
                    if(countFromLeft > 0 && payArg.Pays[countFromLeft - 1] > 0)
                    {
                        int ways = 1;
                        for(int ac = 0; ac < countFromLeft; ac++)
                        {
                            ways *= reelCounts[ac];
                        }
                        List<List<int>> cellNums = null;
                        if(!InSimulation)
                        {
                            cellNums = new List<List<int>>();
                            for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                            {
                                List<int> cells = new List<int>();
                                if (reelNum < 0 + countFromLeft)
                                {
                                    for (int i = 0; i < _screenSymbols[reelNum].Count; i++)
                                    {
                                        if (payArg.CanBe.ContainsKey(_screenSymbols[reelNum][i]))
                                            cells.Add(i);
                                    }
                                    cellNums.Add(cells);
                                }
                            }
                        }
                        anyWin = true;
                        _wins.Add(new WinArgs(_prefix == "" ? payArg.Name : _prefix + payArg.Name, payArg.CanBeWithoutMultipliers[0], payArg.Pays[countFromLeft - 1] * ways * _multiplier * BetLevel, countFromLeft, _multiplier, cellNums));
                    }
                    if(countFromRight > 0 && payArg.Pays[countFromRight - 1] > 0)
                    {
                        int ways = 1;
                        for (int ac = ReelCount - 1; ac >= (ReelCount - countFromRight); ac--)
                        {
                            ways *= reelCounts[ac];
                        }
                        List<List<int>> cellNums = null;
                        if (!InSimulation)
                        {
                            cellNums = new List<List<int>>();
                            for (int ad = 0; ad < ReelCount; ad++)
                            {
                                cellNums.Add(new List<int>());
                            }
                            for (int reelNum = ReelCount - 1; reelNum >= 1; reelNum--)
                            {
                                List<int> cells = new List<int>();
                                if (reelNum > (ReelCount - 1) - countFromRight)
                                {
                                    for (int i = 0; i < _screenSymbols[reelNum].Count; i++)
                                    {
                                        if (payArg.CanBe.ContainsKey(_screenSymbols[reelNum][i]))
                                            cells.Add(i);
                                    }
                                    cellNums[reelNum] = cells;
                                }
                            }
                        }
                        anyWin = true;
                        _wins.Add(new WinArgs(_prefix == "" ? payArg.Name : _prefix + payArg.Name, payArg.CanBeWithoutMultipliers[0], payArg.Pays[countFromRight - 1] * ways * _multiplier * BetLevel, countFromRight, _multiplier, cellNums));
                    }
                }
            }
            return anyWin;
        }
        internal virtual bool CalculateScatterWins(List<WinArgs> _wins, List<List<int>> _screenSymbols, ref int _freeGames,  int _multiplier = 1)
        {
            bool anyWin = false;
            List<PayArgs> payTable = InFreeGames ? PayTablesFG : PayTables;
            string _prefix = InFreeGames ? cFreeGamePrefix : "";
            Dictionary<string, Dictionary<int, int>> scatterToFG = InFreeGames ? m_scatterToFreeGamesFG : m_scatterToFreeGames;
            foreach (PayArgs payArg in payTable)
                if (payArg.IsScatter)
                {
                    int scatterCount = CountSymbolsOnScreen(_screenSymbols, payArg.CanBeWithoutMultipliers);
                    if (scatterCount > 0)
                    {
                        if (payArg.Pays[scatterCount - 1] > 0)
                        {
                            if (_wins != null)
                            {
                                List<List<int>> cellNums = new List<List<int>>();
                                if (!InSimulation)
                                {
                                    for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                                    {
                                        List<int> cellNumsForReel = new List<int>();
                                        for (int i = 0; i < _screenSymbols[reelNum].Count; i++)
                                            if (payArg.CanBeWithoutMultipliers.Contains(_screenSymbols[reelNum][i]))
                                                cellNumsForReel.Add(i);
                                        cellNums.Add(cellNumsForReel);
                                    }
                                }
                                _wins.Add(new WinArgs(_prefix == "" ? payArg.Name : _prefix + payArg.Name, payArg.CanBeWithoutMultipliers[0], payArg.Pays[scatterCount - 1] * BetLevel * _multiplier, scatterCount, 1, cellNums, true));
                            }
                            anyWin = true;
                        }
                        if (scatterToFG[payArg.Name].ContainsKey(scatterCount))
                            _freeGames += scatterToFG[payArg.Name][scatterCount];
                    }

                }
            return anyWin;
        }
        internal virtual bool CalculateScatterWinsLeftToRight(List<WinArgs> _wins, List<List<int>> _screenSymbols, ref int _freeGames, int _multiplier = 1)
        {
            bool anyWin = false;
            List<PayArgs> payTable = InFreeGames ? PayTablesFG : PayTables;
            string _prefix = InFreeGames ? cFreeGamePrefix : "";
            Dictionary<string, Dictionary<int, int>> scatterToFG = InFreeGames ? m_scatterToFreeGamesFG : m_scatterToFreeGames;
            foreach (PayArgs payArg in payTable)
                if (payArg.IsScatter)
                {
                    int scatterCount = 0;
                    foreach (List<int> reelColumn in _screenSymbols)
                    {
                        bool hasScatter = false;
                        foreach (int symbolCanBe in payArg.CanBeWithoutMultipliers)
                            if (reelColumn.Contains(symbolCanBe))
                            {
                                hasScatter = true;
                                _multiplier *= payArg.CanBe[symbolCanBe];
                                break;
                            }
                        if (hasScatter) scatterCount++;
                        else break;
                    }

                    if (scatterCount > 0)
                    {
                        if (payArg.Pays[scatterCount - 1] > 0)
                        {
                            List<List<int>> cellNums = new List<List<int>>();
                            if (!InSimulation)
                            {
                                for (int reelNum = 0; reelNum < scatterCount; reelNum++)
                                {
                                    List<int> cellNumsForReel = new List<int>();
                                    for (int i = 0; i < _screenSymbols[reelNum].Count; i++)
                                        if (payArg.CanBeWithoutMultipliers.Contains(_screenSymbols[reelNum][i]))
                                            cellNumsForReel.Add(i);
                                    cellNums.Add(cellNumsForReel);
                                }
                            }
                            if (_wins != null)
                                _wins.Add(new WinArgs(_prefix == "" ? payArg.Name : _prefix + payArg.Name, payArg.CanBeWithoutMultipliers[0], payArg.Pays[scatterCount - 1] * BetLevel * _multiplier, scatterCount, 1, cellNums, true));
                            anyWin = true;
                        }
                        if (scatterToFG[payArg.Name].ContainsKey(scatterCount))
                            _freeGames += scatterToFG[payArg.Name][scatterCount];
                    }

                }
            return anyWin;
        }
        internal virtual bool CalculateScatterWayWins(List<WinArgs> _wins, List<List<int>> _screenSymbols, ref int _freeGames, int _multiplier = 1)
        {
            bool anyWin = false;
            string _prefix = InFreeGames ? cFreeGamePrefix : "";
            List<PayArgs> payTable = InFreeGames ? PayTablesFG : PayTables;
            Dictionary<string, Dictionary<int, int>> scatterToFG = InFreeGames ? m_scatterToFreeGamesFG : m_scatterToFreeGames;
            foreach(PayArgs payArg in payTable)
            {
                if(payArg.IsScatter)
                {
                    int scatterWays = 1;
                    int reelsWithScatters = 0;
                    List<int> reelCounts = new List<int>();
                    for (int reel = 0; reel < ReelCount; reel++)
                    {
                        int count = 0;
                        foreach (int symbol in _screenSymbols[reel])
                        {
                            if (payArg.CanBe.ContainsKey(symbol))
                            {
                                count++;
                            }
                        }
                        reelCounts.Add(count);
                    }
                    for(int ad = 0; ad < ReelCount; ad++)
                    {
                        if(reelCounts[ad] > 0)
                        {
                            reelsWithScatters++;
                            scatterWays *= reelCounts[ad];
                        }
                    }
                    //Code specifically for FireWolf2
                    if (reelCounts[1] > 0 && reelCounts[2] > 0)
                        trigger_surge_feature = true;
                    else
                        trigger_surge_feature = false;

                    if(reelsWithScatters > 0)
                    {
                        if(payArg.Pays[reelsWithScatters - 1] > 0)
                        {
                            List<List<int>> cellNums = new List<List<int>>();
                            if(!InSimulation)
                            {
                                for (int reelNum = 0; reelNum < ReelCount; reelNum++)
                                {
                                    List<int> cellNumsForReel = new List<int>();
                                    for (int i = 0; i < _screenSymbols[reelNum].Count; i++)
                                    {
                                        if (payArg.CanBeWithoutMultipliers.Contains(_screenSymbols[reelNum][i])) cellNumsForReel.Add(i);
                                    }
                                    cellNums.Add(cellNumsForReel);
                                }
                            }
                            anyWin = true;
                            if (_wins != null)
                            {
                                _wins.Add(new WinArgs(_prefix == "" ? payArg.Name : _prefix + payArg.Name, payArg.CanBeWithoutMultipliers[0], payArg.Pays[reelsWithScatters - 1] * BetLevel * scatterWays, reelsWithScatters, 1, cellNums, true));
                            }
                        }
                        if (scatterToFG[payArg.Name].ContainsKey(reelsWithScatters)) _freeGames += (scatterToFG[payArg.Name][reelsWithScatters] * scatterWays);
                    }
                }
            }
            return anyWin;
        }
        internal void setProgressiveAwards(bool setTop, bool setBottom)
        {
            if(setTop)
            {
                int m_indexForTop = -1;
                m_indexForTop = m.RandomInteger(1000);
                if (m_indexForTop < 98) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 400000.00 + (double)(m_indexForTop * 1000));
                else if (m_indexForTop == 98 || m_indexForTop == 99) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 498000.00);
                else if (m_indexForTop > 99) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 499000.00);
            }
            //if (setMidTop)
            //{
            //    int m_indexForTop = -1;
            //    m_indexForTop = m.RandomInteger(1000);
            //    if (m_indexForTop < 98) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 400000.00 + (double)(m_indexForTop * 1000));
            //    else if (m_indexForTop == 98 || m_indexForTop == 99) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 498000.00);
            //    else if (m_indexForTop > 99) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 499000.00);
            //}
            //if (setMidBottom)
            //{
            //    int m_indexForTop = -1;
            //    m_indexForTop = m.RandomInteger(1000);
            //    if (m_indexForTop < 98) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 400000.00 + (double)(m_indexForTop * 1000));
            //    else if (m_indexForTop == 98 || m_indexForTop == 99) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 498000.00);
            //    else if (m_indexForTop > 99) m_currentProgressiveAwards[0] = (m.RandomDouble(1000) + 499000.00);
            //}
            if(setBottom)
            {
                int m_indexForBottom = -1;
                m_indexForBottom = m.RandomInteger(1000);
                if (m_indexForBottom < 39) m_currentProgressiveAwards[1] = (m.RandomDouble(750) + 20000.00 + (double)(750 * m_indexForBottom));
                else if (m_indexForBottom >= 39) m_currentProgressiveAwards[1] = (m.RandomDouble(750) + 20000.00 + (double)(750 * 39));
            }
        }
        internal List<WinArgs> getMustHitProgressiveWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
                bool winning = true;
                while(winning)
                {
                    if (m_progressives[0].CurrentValue >= m_currentProgressiveAwards[0])
                    {
                        winsToReturn.Add(new WinArgs("Top Progressive", (int)m_currentProgressiveAwards[0]));
                        m_progressives[0].getMustHitProgressiveandReset(m_currentProgressiveAwards[0]);
                        m_progressiveWinPG += (int)m_currentProgressiveAwards[0];
                        setProgressiveAwards(true, false);
                    }
                    if (m_progressives[1].CurrentValue >= m_currentProgressiveAwards[1])
                    {
                        winsToReturn.Add(new WinArgs("Bottom Progressive", (int)m_currentProgressiveAwards[1]));
                        m_progressives[1].getMustHitProgressiveandReset(m_currentProgressiveAwards[1]);
                        m_progressiveWinPG += (int)m_currentProgressiveAwards[1];
                        setProgressiveAwards(false, true);
                    }
                    if (m_progressives[0].CurrentValue >= m_currentProgressiveAwards[0] || m_progressives[1].CurrentValue >= m_currentProgressiveAwards[1]) winning = true;
                    else if (m_progressives[0].CurrentValue < m_currentProgressiveAwards[0] && m_progressives[1].CurrentValue < m_currentProgressiveAwards[1]) winning = false;                    
                }
            return winsToReturn;
        }
        internal List<WinArgs> getShamrockFortunesWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            for (int aa = 0; aa < m_progressives.Count(); aa++)
            {
                bool winning = true;
                while(winning)
                {
                    if(m_progressives[aa].CurrentValue >= m_currentProgressiveAwards[aa])
                    {
                        winsToReturn.Add(new WinArgs(m_progressives[aa].Name, (int)m_currentProgressiveAwards[aa]));
                        m_progressives[aa].getMustHitProgressiveandReset(m_currentProgressiveAwards[aa]);
                        m_progressiveWinPG += (int)m_currentProgressiveAwards[aa];
                        setShamrockFortunesProgressiveAwards(aa);
                    }
                    if (m_progressives[aa].CurrentValue < m_currentProgressiveAwards[aa]) winning = false;
                    else if (m_progressives[aa].CurrentValue >= m_currentProgressiveAwards[aa]) winning = true;
                }
            }
            return winsToReturn;
        }
        internal void setShamrockFortunesProgressiveAwards(int progLevel)
        {
            List<int> progLevelDifferences = new List<int> { 49999, 7499, 2499, 999 };
            List<int> progLevelResets = new List<int> { 99999, 14999, 4999, 1999 };
            List<int> weights = new List<int>();
            //int randomDraw = m.RandomInteger(6000);
            if (progLevel == 0)
            {
                int randomDraw = m.RandomInteger(28);
                int randomExtra = m.RandomInteger(2500) + 1;
                weights = new List<int> {1,2,3,4,5,6,7,8,9,10,11,12,13,14,16,18,20,22,25,28};
                for (int i = 0; i < 20; i++)
                {
                    if (randomDraw < weights[i])
                    {
                        m_currentProgressiveAwards[progLevel] = progLevelResets[progLevel] - progLevelDifferences[progLevel] + i * 2500 + randomExtra;
                        break;
                    }
                }
            }
            if (progLevel == 1)
            {
                int randomDraw = m.RandomInteger(19);
                int randomExtra = m.RandomInteger(750) + 1;
                weights = new List<int> { 1,2,3,4,5,6,8,11,15,19 };
                for (int i = 0; i < 10; i++)
                {
                    if (randomDraw < weights[i])
                    {
                        m_currentProgressiveAwards[progLevel] = progLevelResets[progLevel] - progLevelDifferences[progLevel] + i * 750 + randomExtra;
                        break;
                    }
                }
            }
            if (progLevel == 2)
            {
                int randomDraw = m.RandomInteger(20);
                int randomExtra = m.RandomInteger(250) + 1;
                weights = new List<int> { 1,2,3,4,5,6,8,11,15,20 };
                for (int i = 0; i < 10; i++)
                {
                    if (randomDraw < weights[i])
                    {
                        m_currentProgressiveAwards[progLevel] = progLevelResets[progLevel] - progLevelDifferences[progLevel] + i * 250 + randomExtra;
                        break;
                    }
                }
            }
            if (progLevel == 3)
            {
                int randomDraw = m.RandomInteger(20);
                int randomExtra = m.RandomInteger(100) + 1;
                weights = new List<int> { 1,2,3,4,5,6,8,11,15,20 };
                for (int i = 0; i < 10; i++)
                {
                    if (randomDraw < weights[i])
                    {
                        m_currentProgressiveAwards[progLevel] = progLevelResets[progLevel] - progLevelDifferences[progLevel] + i * 100 + randomExtra;
                        break;
                    }
                }
            }

            //if (randomDraw < 98) m_currentProgressiveAwards[progLevel] = m.RandomInteger(progLevelDifferences[progLevel] + 1) + progLevelResets[progLevel] 
            //                                                             + (randomDraw * (progLevelDifferences[progLevel] + 1));
            //else if (randomDraw == 98 || randomDraw == 99) m_currentProgressiveAwards[progLevel] = m.RandomInteger(progLevelDifferences[progLevel] + 1) + progLevelResets[progLevel]
            //                                                                                       + (98 * (progLevelDifferences[progLevel] + 1));
            //else if (randomDraw > 99) m_currentProgressiveAwards[progLevel] = m.RandomInteger(progLevelDifferences[progLevel] + 1) + progLevelResets[progLevel]
            //                                                                  + (99 * (progLevelDifferences[progLevel] + 1));
        }
        internal List<WinArgs> getDaJiDaLiWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            m_stringsForPick = new List<string>();
            for (int aa = 0; aa < m_progressives.Count(); aa++)
            {
                m_stringsForPick.Add(m_progressives[aa].Name);
            }
            if (m.RandomDouble(1) < m_progressiveChance * BetLevel || m_dajidaliCheat)
            {
                int prizeInteger = m.RandomIndex(m_progressiveCutoffs);
                if (!InSimulation)
                {
                    frmPickGame = new frmPickGame();
                    frmPickGame.m_numberOfChoices = 12;
                    frmPickGame.buildButtons();
                    frmPickGame.fillCurrentScript(m_stringsForPick, 3, prizeInteger);
                    frmPickGame.ShowDialog();
                }
                winsToReturn.Add(new WinArgs(m_progressives[prizeInteger].Name, m_progressives[prizeInteger].GetProgressiveAndReset()));
                if (!InSimulation) frmProg.ProgressiveValueTBs[prizeInteger].Text = string.Format("{0:$0,0.00}", (double)m_progressives[prizeInteger].CurrentValue / 100);
            }
            return winsToReturn;
        }
        internal List<WinArgs> safariWilds(int i)
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            int prizeInteger = i - 14;
            winsToReturn.Add(new WinArgs(m_progressives[prizeInteger].Name, m_progressives[prizeInteger].GetProgressiveAndReset()));
            if (!InSimulation) frmProg.ProgressiveValueTBs[prizeInteger].Text = string.Format("{0:$0,0.00}", (double)m_progressives[prizeInteger].CurrentValue / 100);
            return winsToReturn;
        }
        internal void incrementProgressives()
        {
            if (m_progressiveType != "shamrock fortunes")
            {
                for (int aa = 0; aa < m_progressives.Count(); aa++)
                {
                    m_progressives[aa].Increment(Bet);
                }
                if (m_bankSize > 1)
                {
                    for (int ab = 0; ab < m_bankSize - 1; ab++)
                    {
                        int randomDraw = m.RandomInteger(5);
                        int playersLevel = PossibleBetLevels[randomDraw];
                        for (int ac = 0; ac < m_progressives.Count(); ac++)
                        {
                            m_progressives[ac].Increment(MinBet * playersLevel);
                        }
                    }
                }
            }
        }
        internal int playShamrockFortunes()
        {
            int countOfBoosts = 0;
            int randomBetLevel = PossibleBetLevels[m.RandomInteger(5)];
            int randomBet = MinBet * randomBetLevel;
            for (int ab = 0; ab < m_progressives.Count(); ab++)
            {
                bool didItBoost = m_progressives[ab].IncrementShamrock(randomBet, ab, randomBetLevel);
                if (didItBoost) countOfBoosts++;
            }
            return countOfBoosts;
        }
        internal int playSurgeFeature(bool fg)
        {
            if(trigger_surge_feature == true)
            {
                if (fg == false)
                {
                    int prizeInteger = m.RandomIndex(m_surgeCutoffs);

                    if (prizeInteger == 0)
                        return 1;
                    else if (prizeInteger == 1)
                        return 2;
                    else if (prizeInteger == 2)
                        return 3;
                    else if (prizeInteger == 3)
                        return 4;
                    else
                    {
                        throw new Exception("Error in Surge Feature: Invalid Index for Expanding Reels. Index is " + prizeInteger);
                    }
                }
                else
                    return 1;
            }

            return 0;
        }

        internal void replaceSymbols(ReelType _reelType)
        {
            switch (_reelType)
            {
                case ReelType.PG:
                    ReplaceIndex = m.RandomIndex(m_SymReplacePGCutoffs);
                    break;
                case ReelType.FG:
                    ReplaceIndex = m.RandomIndex(m_SymReplaceFGCutoffs);
                    break;
            }
        }
        //Display
        internal virtual string GetSymbolName(int _reelNum, int _index, int _cellNumIfStopped, bool _spinStop)
        {
            List<List<int>> reels = currentReelSet;
            List<List<int>> currentInsertionSet = InFreeGames ? Reels.SSSymbolsFG : Reels.SSSymbolsBG;
            int index = (_index + reels[_reelNum].Count) % reels[_reelNum].Count;
            int symbolIndex = reels[_reelNum][index];

            if (InFreeGames == true && m_SymReplaceFGCutoffs != null && symbolIndex == m_insertionSymbol)
                symbolIndex = Reels.Symbols.IndexOf(Reels.Symbols[Reels.ReelsSymReplaceFG[_reelNum][ReplaceIndex]]);
            else if (InFreeGames == false && m_SymReplacePGCutoffs != null && symbolIndex == m_insertionSymbol)
                symbolIndex = Reels.Symbols.IndexOf(Reels.Symbols[Reels.ReelsSymReplacePG[_reelNum][ReplaceIndex]]);
            else if (symbolIndex == m_insertionSymbol)
                symbolIndex = currentInsertionSet[m_insertionIndex][_reelNum];

            return Reels.Symbols[symbolIndex];
        }
        internal virtual void CustomDrawBeforeDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped) { }
        internal virtual void CustomDrawAfterDrawReels(ReelsPanel _reelsPanel, bool _showingWins, bool _stopped) 
        { 
        
        }

        //Stats
        internal virtual void SetUpStats()
        {
            m_statsWinDistributionPaidGame = new WinDistributionChart("Paid Games", new List<string>(new string[] { "Paid Game", "Bonus" }), Bet, m.Multipliers);
            m_statsWinDistributionPaidGameHigh = new WinDistributionChart("Paid Games High", new List<string>(new string[] { "Paid Game", "Bonus" }), Bet, m.MultipliersHigh);
            m_statsWinDistributionPG = new WinDistributionChart("PG Wins", new List<string>(new string[] { "PG", "PG 2nd Feat" }), Bet, m.Multipliers);
            m_statsWinDistributionFG = new WinDistributionChart("FG Wins", new List<string>(new string[] { "FG", "FG 2nd Feat" }), Bet, m.Multipliers);
            m_winsByType = new SortedDictionary<int, long[]>();
            m_numberOfFreeGameSessions = 0;
        }
        internal virtual void SetUpTODStats()
        {
            List<int> bankrollsToUse = new List<int>();
            for (int aa = 0; aa < m.SpinsForSurvivability.Count(); aa++)
            {
                bankrollsToUse.Add(m.SpinsForSurvivability[aa] * Bet);
            }
            m_setsForSimulation = new List<setDistribution>();
            m_currentSessionsInProgress = new List<SessionStats>();
            currentSetIndex = new List<int>();
            for (int aa = 0; aa < m.bankrollsForTODStats.Count(); aa++)
            {
                m_setsForSimulation.Add(new setDistribution(m.bankrollsForTODStats[aa], Bet));
                m_setsForSimulation[aa].setsRecorded.Add(new SetsStats(Bet, m.bankrollsForTODStats[aa]));
                m_currentSessionsInProgress.Add(new SessionStats(m.bankrollsForTODStats[aa], Bet));
                currentSetIndex.Add(0);
                if (m.bankrollsForTODStats[aa] / Bet == 50) fiftyTimesBetAlreadyIncluded = true;
            }
            if (!fiftyTimesBetAlreadyIncluded)
            {
                m_setsForSimulation.Add(new setDistribution(50 * Bet, Bet));
                m_setsForSimulation[m_setsForSimulation.Count() - 1].setsRecorded.Add(new SetsStats(Bet, Bet * 50));
                m_currentSessionsInProgress.Add(new SessionStats(50 * Bet, Bet));
                currentSetIndex.Add(0);
            }
            if (m_progressiveType != "none")
            {
                for (int ab = 0; ab < m_progressives.Count(); ab++)
                {
                    m_progressives[ab].m_timesHit = 0;
                }
            }
        }
        internal virtual void AfterGameStatsCollection()
        {
            List<int> winsForStats = new List<int>();
            winsForStats.Add(WinsThisGame);
            winsForStats.Add(WinsThisGame - m_progressiveWinPG - m_totalProgressiveWinFG);
            winsForStats.Add(m_statsWinsThisPG);
            winsForStats.Add(m_statsWinsThisPG - m_progressiveWinPG);
            winsForStats.Add(WinsThisGame - m_statsWinsThisPG);
            winsForStats.Add(WinsThisGame - m_statsWinsThisPG - m_totalProgressiveWinFG);
            winsForStats.Add(m_progressiveWinPG + m_totalProgressiveWinFG);
            winsForStats.Add(m_progressiveWinPG);
            winsForStats.Add(m_totalProgressiveWinFG);
            if (m_progressiveWinPG > 0 || m_totalProgressiveWinFG > 0) m_progressiveGames++;
            if(DoDefaultWinDistributions)
            {
                m_statsWinDistributionPaidGame.StoreGame(WinsThisGame, 0);
                m_statsWinDistributionPaidGameHigh.StoreGame(WinsThisGame, 0);
                if(InFreeGames)
                {
                    m_statsWinDistributionPaidGame.StoreGame(WinsThisGame - m_statsWinsThisPG, 1);
                    m_statsWinDistributionPaidGameHigh.StoreGame(WinsThisGame - m_statsWinsThisPG, 1);
                    m_numberOfFreeGameSessions++;
                }
                for(int aa = 0; aa < winsForStats.Count() - 1; aa++)
                {
                    if (!InFreeGames && (aa < 4))
                    {
                        if (m_winsByType.Keys.Contains(winsForStats[aa])) m_winsByType[winsForStats[aa]][aa]++;
                        else if (!m_winsByType.Keys.Contains(winsForStats[aa]))
                        {
                            long[] arrayToAdd = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            arrayToAdd[aa] = 1;
                            m_winsByType[winsForStats[aa]] = arrayToAdd;
                        }
                    }
                    else if(InFreeGames && aa < 6)
                    {
                        if (m_winsByType.Keys.Contains(winsForStats[aa])) m_winsByType[winsForStats[aa]][aa]++;
                        else if (!m_winsByType.Keys.Contains(winsForStats[aa]))
                        {
                            long[] arrayToAdd = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            arrayToAdd[aa] = 1;
                            m_winsByType[winsForStats[aa]] = arrayToAdd;
                        }
                    }
                    else if (aa == 7 && m_progressiveWinPG > 0)
                    {
                        if (m_winsByType.Keys.Contains(winsForStats[aa])) m_winsByType[winsForStats[aa]][aa]++;
                        else if (!m_winsByType.Keys.Contains(winsForStats[aa]))
                        {
                            long[] arrayToAdd = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                            arrayToAdd[aa] = 1;
                            m_winsByType[winsForStats[aa]] = arrayToAdd;
                        }
                        m_pgWithProgressives++;
                    }
                }
                if (doTODStats)
                {
                    for (int ab = 0; ab < m_currentSessionsInProgress.Count(); ab++)
                    {
                        m_currentSessionsInProgress[ab].placeBet();
                        int currentSessionBankroll = 0;
                        m_currentSessionsInProgress[ab].processWin(WinsThisGame, InFreeGames, m_progressiveWinPG > 0 ? true : false);
                        if (m_currentSessionsInProgress[ab].currentCredits < Bet || m_currentSessionsInProgress[ab].currentCredits > m_currentSessionsInProgress[ab].cashoutAmount)
                        {
                            if (m_setsForSimulation[ab].setsRecorded.Count() <= 10)
                            {
                                currentSessionBankroll = m_currentSessionsInProgress[ab].startingBankroll;
                                m_setsForSimulation[ab].setsRecorded[currentSetIndex[ab]].processSession(m_currentSessionsInProgress[ab]);
                                m_currentSessionsInProgress[ab] = new SessionStats(currentSessionBankroll, Bet);
                                if (m_setsForSimulation[ab].setsRecorded[currentSetIndex[ab]].sessionsPlayed == 99999)
                                {
                                    m_setsForSimulation[ab].setsRecorded[currentSetIndex[ab]].processSet();
                                    m_setsForSimulation[ab].setsRecorded.Add(new SetsStats(Bet, currentSessionBankroll));
                                    currentSetIndex[ab]++;
                                }
                            }
                        }
                    }
                }
            }
        }
        internal virtual void DisplayStats(List<List<string>> _results)
        {
            m_statsWinDistributionPaidGame.InputResults(_results);
            m_statsWinDistributionPG.InputResults(_results);
            m_statsWinDistributionFG.InputResults(_results);
            m_statsWinDistributionPaidGameHigh.InputResults(_results);
        }
        internal virtual void DisplayTODStats(List<List<string>> _results)
        {
            for (int aa = 0; aa < m_setsForSimulation.Count(); aa++)
            {
                m_setsForSimulation[aa].setsRecorded[currentSetIndex[aa]].processSet();
                m_setsForSimulation[aa].InputResults(_results);
            }
        }
        internal virtual void DisplayJackpotStats(List<List<string>> _results, long _trialsSoFar)
        {
            ulong totalHits = 0;
            double oddsOfAJackpot = 0;
            double dollarsToAJackpots = 0;
            _results.Add(new List<string>(new string[] { "Jackpot Level", "Times Hit", "Odds", "$ to Jackpot"}));
            for(int ab = 0; ab < m_progressives.Count(); ab++)
            {
                string name = m_progressives[ab].Name;
                ulong timesHit = m_progressives[ab].m_timesHit;
                double oddsOfProg = (double)_trialsSoFar / (double)m_progressives[ab].m_timesHit;
                double dollarsToJackpots = (oddsOfProg * (double)Bet) / 100;
                _results.Add(new List<string>(new string[] { name, string.Format("{0:0}", timesHit), string.Format("{0:0.00}", oddsOfProg), string.Format("{0:$0.00}", dollarsToJackpots) }));
                totalHits += m_progressives[ab].m_timesHit;
            }
            oddsOfAJackpot = (double)_trialsSoFar / (double)totalHits;
            dollarsToAJackpots = (oddsOfAJackpot * (double)Bet) / 100;
            _results.Add(new List<string>(new string[] { "Overall", string.Format("{0:0}", totalHits), string.Format("{0:0.00}", oddsOfAJackpot), string.Format("{0:$0.00}", dollarsToAJackpots) }));
            _results.Add(new List<string>());
        }
        internal virtual void SetUpCustomStats() { }
        internal virtual void DisplayCustomStats(List<List<string>> _results) { }
        internal virtual void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            _payTablesSeperated = new Dictionary<string, Dictionary<string, PayCountArg>>();
            _payTablesTotal = new Dictionary<string, PayCountArg>();
            m.AddPayTableCount("Primary Game", "", m.Multipliers.Count + 1, PayTables, _payTablesSeperated, _payTablesTotal);
            m.AddPayTableCount("Free Game", cFreeGamePrefix, m.Multipliers.Count + 1, PayTables, _payTablesSeperated, _payTablesTotal);
        }
        internal virtual void DisplayWinsByType(List<List<string>> _results)
        {
            _results.Add(new List<string>(new string[] { "Win Amount", "Overall Hits", "Overall Hits without Progressives", "Base Game Hits", "Base Game Hits without Progressives", 
                                                         "Free Spins Bonus Hits", "Free Spins Bonus Hits without Progressives", "Overall Progressive Hits", "Base Game Progressive Hits",
                                                         "Free Spins Bonus Progressive Hits" }));
            foreach(int winAmount in m_winsByType.Keys)
            {
                _results.Add(new List<string>(new string[] { winAmount.ToString(), m_winsByType[winAmount][0].ToString(), m_winsByType[winAmount][1].ToString(), m_winsByType[winAmount][2].ToString(), 
                                                             m_winsByType[winAmount][3].ToString(), m_winsByType[winAmount][4].ToString(), m_winsByType[winAmount][5].ToString(),
                                                             m_winsByType[winAmount][6].ToString(), m_winsByType[winAmount][7].ToString(), m_winsByType[winAmount][8].ToString() }));
            }
        }
        //Internal////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void UpdateWins()
        {
            WinsThisSpin = CountTheseWins(WinsToShow);
            WinsThisGame = CountTheseWins(Wins);
        }
        //Protected///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        internal int CountTheseWins(List<WinArgs> _wins)
        {
            int totalWin = 0;
            foreach (WinArgs win in _wins)
                totalWin += win.Amount;
            return totalWin;
        }
        internal int CountSymbolsOnScreen(List<List<int>> _screenSymbols, List<int> _canBe)
        {
            int count = 0;
            foreach (int canBe in _canBe)
                foreach (List<int> reel in _screenSymbols)
                    if (reel.Contains(canBe))
                        foreach (int symbol in reel)
                            if (symbol == canBe)
                                count++;
            return count;
        }
        internal int CountSymbolsOnScreen(List<List<int>> _screenSymbols, int _canBe)
        {
            int count = 0;
            foreach (List<int> reel in _screenSymbols)
                if (reel.Contains(_canBe))
                    foreach (int symbol in reel)
                        if (symbol == _canBe)
                            count++;
            return count;
        }
        internal List<List<int>> GetActivesLinesBasedOnFirst2Reels(List<List<int>> _screenSymbols)
        {
            List<List<int>> activeLines = new List<List<int>>();
            foreach (List<int> line in m_lines)
            {
                int symbol1 = _screenSymbols[0][line[0]];
                int symbol2 = _screenSymbols[1][line[1]];
                bool isActive = false;
                foreach (PayArgs payArg in PayTables)
                    if (!payArg.IsScatter && payArg.CanBeWithoutMultipliers.Contains(symbol1) && payArg.CanBeWithoutMultipliers.Contains(symbol2))
                    {
                        isActive = true;
                        break;
                    }
                if (isActive)
                    activeLines.Add(line);
            }
            return activeLines;
        }
        internal List<int> GetSymbolsMatchingOnFirst2ReelsAndReturnNullIfWildOnBoth(List<List<int>> _screenSymbols, int _symbolWild)
        {
            List<int> symbolMatches = new List<int>();
            if (_screenSymbols[0].Contains(_symbolWild) && _screenSymbols[1].Contains(_symbolWild))
                symbolMatches = null;
            else
                foreach (int symbolOnFirstReel in _screenSymbols[0])
                {
                    if (symbolOnFirstReel == _symbolWild)
                    {
                        foreach (int symbolOnSecondReel in _screenSymbols[1])
                            if (!symbolMatches.Contains(symbolOnSecondReel))
                                symbolMatches.Add(symbolOnSecondReel);
                    }
                    else if (_screenSymbols[1].Contains(_symbolWild) || _screenSymbols[1].Contains(symbolOnFirstReel))
                        if (!symbolMatches.Contains(symbolOnFirstReel))
                            symbolMatches.Add(symbolOnFirstReel);
                }
            return symbolMatches;
        }
        //Processing//////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void ProcessGeneral(object[,] _values, int _col)
        {
            WildCanBes = new List<WildCanBeArg>();
            Dimensions = new List<int>();
            PossibleBetLevels = new List<int>();
            for (int row = 2; row <= _values.GetLength(0); row++)
            {
                string header = _values[row, _col] == null ? "" : _values[row, _col].ToString().ToLower();
                string value = _values[row, _col + 1] == null ? "" : _values[row, _col + 1].ToString();
                if (header == "") break;
                switch (header)
                {
                    case "game name": GameName = value; break;
                    case "bet": MinBet = int.Parse(value); break;
                    case "dimensions":
                       foreach (string dimension in value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                            Dimensions.Add(int.Parse(dimension));
                        ReelCount = Dimensions.Count;
                        break;
                    case "lines": Lines = int.Parse(value); break;
                    case "use reel weights": Reels.UseReelWeights = value.ToLower() == "true"; break;
                    case "progressive type": m_progressiveType = value.ToLower();  break;
                    case "symbol height": SymbolHeight = int.Parse(value); break;
                    case "original symbol height": originalBasicImageSize = int.Parse(value); break;
                    case "conversion ratio": conversionRatio = double.Parse(value); break;
                    case "normal evaluation type": m_normalEvaluationType = value.ToLower(); break;
                    case "scatter evaluation type": m_scatterEvaluationType = value.ToLower(); break;
                    case "bet level": BetLevel = int.Parse(value); break;
                    case "possible bet levels":
                        foreach (string possiblebet in value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                            PossibleBetLevels.Add(int.Parse(possiblebet));
                        break;
                    case "percentage": m_gamePayback = double.Parse(value); break;
                    default: m_extraGeneralData[header] = value; break;
                }
            }
            if(m_normalEvaluationType == "line" && (Lines == 0))
            {
                MessageBox.Show("Number of Lines does not exist for Line Game", "Data Error in Game Data", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                Application.Exit();
            }
            if(m_progressiveType == "must hit" || m_progressiveType == "shamrock fortunes" || m_progressiveType == "dajidali")
            {
                ProcessProgressives(null, 0);
            }
            if (m.MaxInt(Dimensions) == 3)
            {
                SymbolHeight = 213;
                originalBasicImageSize = 304;
                conversionRatio = (double)SymbolHeight / (double)originalBasicImageSize;
            }
            else if (m.MaxInt(Dimensions) == 4)
            {
                SymbolHeight = 156;
                originalBasicImageSize = 228;
                conversionRatio = (double)SymbolHeight / (double)originalBasicImageSize;
            }
            else if (m.MaxInt(Dimensions) == 5)
            {
                SymbolHeight = 156;
                originalBasicImageSize = 182;
                conversionRatio = (double)SymbolHeight / (double)originalBasicImageSize;
            }
            else if (m.MaxInt(Dimensions) == 6)
            {
                SymbolHeight = 142;
                originalBasicImageSize = 185;
                conversionRatio = (double)SymbolHeight / (double)originalBasicImageSize;
            }
            else if (m.MaxInt(Dimensions) == 7 || m.MaxInt(Dimensions) == 8)
            {
                SymbolHeight = 80;
                originalBasicImageSize = 190;
                conversionRatio = (double)SymbolHeight / (double)originalBasicImageSize;
            }
            else if (m.MaxInt(Dimensions) == 9)
            {
                SymbolHeight = 80;
                originalBasicImageSize = 205;
                conversionRatio = (double)SymbolHeight / (double)originalBasicImageSize;
            }
            if(PossibleBetLevels.Count() == 0)
            {
                PossibleBetLevels = new List<int> { 1, 2, 3, 5, 10 };
            }
            if (BetLevel == 0) BetLevel = PossibleBetLevels[0];
        }

        private void ProcessSymbols(object[,] _values, int _col)
        {
            List<string> symbols = new List<string>();
            List<string> symbolsOriginal = new List<string>();
            for (int row = 2; row <= _values.GetLength(0); row++)
            {
                string symbol = _values[row, _col] == null ? "" : _values[row, _col].ToString();
                if (symbol == "") break;
                symbols.Add(symbol.ToLower());
                symbolsOriginal.Add(symbol);
            }
            Reels.SymbolsOriginal = symbolsOriginal;
            Reels.Symbols = symbols;
            if (Reels.Symbols.Contains("ss") || Reels.Symbols.Contains("re"))
                m_insertionSymbol = Reels.Symbols.Contains("re") ? Reels.Symbols.IndexOf("re") : Reels.Symbols.IndexOf("ss");
        }

        public void ProcessOversized()
        {
            if(ReelImages.imageDimensions != null && ReelImages.imageDimensions.Keys.Count != 0)
            {
                foreach(string symbolName in ReelImages.imageDimensions.Keys)
                {
                    if(symbolName != "background" && symbolName != "l4" && symbolName != "freespinbackground")
                    {
                        if(ReelImages.imageDimensions[symbolName][0] != SymbolWidth || ReelImages.imageDimensions[symbolName][1] != SymbolHeight)
                        {
                            int offsetWidth = (int)((SymbolWidth - ReelImages.imageDimensions[symbolName][0])/2);
                            int offsetHeight = (int)((SymbolHeight - ReelImages.imageDimensions[symbolName][1]) / 2);
                            OversizedOffsets[symbolName] = new Point(offsetWidth, offsetHeight);
                        }
                    }
                }
            }
        }

        private void ProcessWilds(object[,] _values, int _col)
        {
            Dictionary<int, int> wilds = new Dictionary<int, int>();
            List<int> canBe = new List<int>();
            for (int row = 2; row <= _values.GetLength(0); row++)
            {
                string symbol = _values[row, _col] == null ? "" : _values[row, _col].ToString().ToLower();
                string multiplier = _values[row, _col + 1] == null ? "" : _values[row, _col + 1].ToString();
                if (symbol == "") break;
                wilds[Reels.Symbols.IndexOf(symbol)] = int.Parse(multiplier);
            }
            for (int row = 2; row <= _values.GetLength(0); row++)
            {
                string symbol = _values[row, _col + 2] == null ? "" : _values[row, _col + 2].ToString().ToLower();
                if (symbol == "") break;
                canBe.Add(Reels.Symbols.IndexOf(symbol));
            }
            WildCanBes.Add(new WildCanBeArg(wilds, canBe));
        }

        private void ProcessScatters(object[,] _values, int _col)
        {
            List<int> canBe = new List<int>();
            for (int row = 2; row <= _values.GetLength(0); row++)
            {
                string scatterSymbol = _values[row, _col] == null ? "" : _values[row, _col].ToString();
                string num = _values[row, _col] == null ? "" : _values[row, _col + 1].ToString().ToLower();
                string freeGames = _values[row, _col + 2] == null ? "" : _values[row, _col + 2].ToString();
                string freeGamesFG = _values[row, _col + 3] == null ? "" : _values[row, _col + 3].ToString();
                if (num == "") break;
                if (!m_scatterToFreeGames.ContainsKey(scatterSymbol)) m_scatterToFreeGames[scatterSymbol] = new Dictionary<int, int>();
                if (!m_scatterToFreeGamesFG.ContainsKey(scatterSymbol)) m_scatterToFreeGamesFG[scatterSymbol] = new Dictionary<int, int>();
                if (!m_freeGamesToScatter.ContainsKey(scatterSymbol)) m_freeGamesToScatter[scatterSymbol] = new Dictionary<int, int>();
                if (!m_freeGamesToScatterFG.ContainsKey(scatterSymbol)) m_freeGamesToScatterFG[scatterSymbol] = new Dictionary<int, int>();
                m_scatterToFreeGames[scatterSymbol][int.Parse(num)] = int.Parse(freeGames);
                m_scatterToFreeGamesFG[scatterSymbol][int.Parse(num)] = int.Parse(freeGamesFG);
                m_freeGamesToScatter[scatterSymbol][int.Parse(freeGames)] = int.Parse(num);
                m_freeGamesToScatterFG[scatterSymbol][int.Parse(freeGamesFG)] = int.Parse(num);
            }
        }

        private void ProcessPayTable(object[,] _values, int _col, bool _freeGamePayTable = false)
        {
            List<PayArgs> payTables = new List<PayArgs>();
            for (int row = 3; row <= _values.GetLength(0); row++)
            {
                string symbols = _values[row, _col] == null ? "" : _values[row, _col].ToString();
                string payName = _values[row, _col + 1] == null ? "" : _values[row, _col + 1].ToString();
                if (symbols == "") break;
                bool atLeastOnePay = false;

                List<int> canBeInitial = new List<int>();
                foreach (string symbolCanBeString in symbols.ToLower().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    canBeInitial.Add(Reels.Symbols.IndexOf(symbolCanBeString));
                List<int> pays = new List<int>();
                for (int col = _col + 2; col < _col + 2 + ReelCount; col++)
                {
                    string pay = _values[row, col] == null ? "" : _values[row, col].ToString();
                    if (pay == "")
                        pays.Add(0);
                    else
                    {
                        atLeastOnePay = true;
                        pays.Add(int.Parse(pay));
                    }
                }
                if (atLeastOnePay || m_scatterToFreeGames.ContainsKey(payName))
                {
                    Dictionary<int, int> canBe = new Dictionary<int, int>();
                    foreach (int symbolCanBe in canBeInitial)
                        canBe[symbolCanBe] = 1;
                    foreach (int symbolCanBe in canBeInitial)
                        foreach (WildCanBeArg wildCanBe in WildCanBes)
                            if (wildCanBe.CanBe.Contains(symbolCanBe))
                                foreach (int wildSymbol in wildCanBe.Wilds.Keys)
                                {
                                    canBe[wildSymbol] = wildCanBe.Wilds[wildSymbol];
                                }
                    payTables.Add(new PayArgs(payName, canBe, pays, m_scatterToFreeGames.ContainsKey(payName)));
                }
            }
            if (_freeGamePayTable)
                PayTablesFG = payTables;
            else
            {
                PayTables = payTables;
                if (PayTablesFG == null)
                    PayTablesFG = PayTables;
            }
        }

        private void ProcessExtraInformation(object[,] _values, int _col)
        {
            int tillCol = 0;
            int tillRow = 0;
            try
            {
                for (tillRow = 2; tillRow <= _values.GetLength(0) - 1; tillRow++)
                    if (_values[tillRow + 1, _col] == null || _values[tillRow + 1, _col].ToString() == "")
                        break;
                for (tillCol = _col; tillCol <= _values.GetLength(1) - 1; tillCol++)
                    if (_values[2, tillCol + 1] == null || _values[2, tillCol + 1].ToString() == "")
                        break;
            }
            catch
            {
                throw new ArgumentException(string.Format("Error in ProcessExtraInformation. Row ({0}), Col ({1})", tillRow, tillCol));
            }

            int row = 0;
            int col = 0;
            try
            {
                List<List<string>> data = new List<List<string>>();
                for (row = 2; row <= tillRow; row++)
                {
                    List<string> rowOfData = new List<string>();
                    for (col = _col; col <= tillCol; col++)
                        rowOfData.Add(_values[row, col].ToString().ToLower());
                    data.Add(rowOfData);
                }
                m_extraData[_values[1, _col].ToString().ToLower()] = data;
            }
            catch
            {
                throw new ArgumentException(string.Format("Error in ProcessExtraInformation. Row ({0}), Col ({1})", row, col));
            }
        }
        
        private void ProcessLines(object[,] _values, int _col)
        {
            m_lines = new List<List<int>>();
            for(int row = 2; row <= Lines + 1; row++)
            {
                List<int> reelPositions = new List<int>();
                for (int col = 0 +_col; col < ReelCount + _col; col++)
                {
                    if(_values[row,col] != null) reelPositions.Add(int.Parse(_values[row, col].ToString()));
                }
                m_lines.Add(reelPositions);
            }
        }

        private void ProcessProgressives(object[,] _values, int _col)
        {
            m_progressives = new List<ProgressiveData>();
            m_currentProgressiveAwards = new List<double>();
            if (m_progressiveType == "custom" || m_progressiveType == "dajidali")
            {
                for (int row = 2; row <= _values.GetLength(0); row++)
                {
                    if (_values[row, _col + 0] != null)
                    {
                        m_progressives.Add(new ProgressiveData(_values[row, _col + 0].ToString(),
                                                               int.Parse(_values[row, _col + 1].ToString()),
                                                               double.Parse(_values[row, _col + 2].ToString())));
                    }
                }
            }
            else if (m_progressiveType == "must hit")
            {
                //TUrbo Boost Config 0
                m_progressives.Add(new ProgressiveData("Major", 400000, .00266666667));
                m_progressives.Add(new ProgressiveData("Major", 20000, .004000));
                //m_progressives.Add(new ProgressiveData("Minor", 2500, .001000));
                //m_progressives.Add(new ProgressiveData("Mini", 1000, .001000));
                m_currentProgressiveAwards.Add(0);
                m_currentProgressiveAwards.Add(0);
                //m_currentProgressiveAwards.Add(0);
                //m_currentProgressiveAwards.Add(0);
                setProgressiveAwards(true, true);

            }
            else if (m_progressiveType == "shamrock fortunes")
            {
                m_progressives.Add(new ProgressiveData("Mega", 50000, .001));
                m_progressives.Add(new ProgressiveData("Major", 7500, .001));
                m_progressives.Add(new ProgressiveData("Minor", 2500, .001));
                m_progressives.Add(new ProgressiveData("Mini", 1000, .001));
                for(int aa = 0; aa < 4; aa++)
                {
                    m_currentProgressiveAwards.Add(0);
                    setShamrockFortunesProgressiveAwards(aa);
                }
            }
        }

        private void ProcessProgressiveChance(object[,] _values, int _col)
        {
            m_progressiveChance = double.Parse(_values[2, _col].ToString());
        }

        private void ProcessProgressiveWeights(object[,] _values, int _col)
        {
            List<int> m_progressiveWeights = new List<int>();
            m_progressiveCutoffs = new List<int>();
            for(int row = 2; row <= _values.GetLength(0); row++)
            {
                if(_values[row, _col+0] != null)
                {
                    m_progressiveWeights.Add(int.Parse(_values[row, _col].ToString()));
                }
            }
            m_progressiveCutoffs = m.MakeCutoffs(m_progressiveWeights);
        }

        private void ProcessJackpotScripts(object[,] _values, int _col)
        {
            m_jackpotScripting = new List<pickSciptData>();
            List<int> m_scriptWeights = new List<int>();
            List<List<string>> m_jackpotScripts = new List<List<string>>();
            int tillCol = 0;
            int tillRow = 0;
            try
            {
                for (tillRow = 2; tillRow <= _values.GetLength(0) - 1; tillRow++)
                    if (_values[tillRow + 1, _col] == null || _values[tillRow + 1, _col].ToString() == "")
                        break;
                for (tillCol = _col; tillCol <= _values.GetLength(1) - 1; tillCol++)
                    if (_values[2, tillCol + 1] == null || _values[2, tillCol + 1].ToString() == "")
                        break;
            }
            catch
            {
                throw new ArgumentException(string.Format("Error in ProcessJackpotScripts. Row ({0}), Col ({1})", tillRow, tillCol));
            }
            int row = 0;
            int col = 0;
            try
            {
                for (row = 2; row <= tillRow; row++)
                {
                    List<string> script = new List<string>();
                    for (col = _col; col < tillCol; col++)
                    {
                        script.Add(_values[row, col].ToString());
                    }
                    m_scriptWeights.Add(int.Parse(_values[row, tillCol].ToString()));
                    m_jackpotScripts.Add(script);
                }
                m_jackpotScripting.Add(new pickSciptData(m_jackpotScripts, m.MakeCutoffs(m_scriptWeights)));
            }
            catch
            {
                throw new ArgumentException(string.Format("Error in ProcessJackpotScripts. Row ({0}), Col ({1})", row, col));
            }
        }

        private void ProcessSurgeFeature(object[,] _values, int _col)
        {
            List<int> m_surgeWeights = new List<int>();
            for (int row = 3; row <= _values.GetLength(0); row++)
            {
                if (_values[row, _col + 1] != null)
                {
                    m_surgeWeights.Add(int.Parse(_values[row, _col+1].ToString()));
                }
            }
            m_surgeCutoffs = m.MakeCutoffs(m_surgeWeights);
        }

        private void ProcessRespin(object[,] _values, int _col)
        {
            string value = _values[2, _col].ToString();
            num_respins = int.Parse(value);
        }

        private void ProcessSymReplacementFeature(object[,] _values, int _col, ReelType _reelType, int _reelCount)
        {
            List<int> Weights = new List<int>();
            List<List<int>> reels = new List<List<int>>();

            for (int col = _col; col < _col + _reelCount; col++)
            {
                List<int> reel = new List<int>();
                for (int row = 3; row <= _values.GetLength(0); row++)
                {
                    string symbol = _values[row, col] == null ? "" : _values[row, col].ToString().ToLower();
                    if (symbol == "") break;
                    reel.Add(Reels.Symbols.IndexOf(symbol));
                }
                reels.Add(reel);
            }

            for (int row = 3; row <= _values.GetLength(0); row++)
            {
                if (_values[row, _col + _reelCount] != null)
                {
                    Weights.Add(int.Parse(_values[row, _col + _reelCount].ToString()));
                }
            }

            switch (_reelType)
            {
                case ReelType.PG:
                    Reels.ReelsSymReplacePG = reels;
                    m_SymReplacePGCutoffs = m.MakeCutoffs(Weights);
                    if (Reels.ReelsSymReplaceFG == null)
                    {
                        Reels.ReelsSymReplaceFG = reels;
                        m_SymReplaceFGCutoffs = m.MakeCutoffs(Weights);
                    }
                    break;
                case ReelType.FG:
                    Reels.ReelsSymReplaceFG = reels;
                    m_SymReplaceFGCutoffs = m.MakeCutoffs(Weights);
                    break;
            }
        }
    }
}