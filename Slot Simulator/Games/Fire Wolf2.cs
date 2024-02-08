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
    class Fire_Wolf2 : GameInfo
    {
        internal Fire_Wolf2(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");
            m_symbolScatter = Reels.Symbols.IndexOf("bn");
            m_insertionSymbol = Reels.Symbols.IndexOf("in");
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        DefaultGameStates m_gameState;
        int m_symbolScatter, m_symbolWild, m_additionalRows = 0, m_baseCycle, fg_triggers = 0, fg_retriggers = 0, total_fgs_played = 0, original_num_rows = 0;
        Dictionary<string, int> trigger_distribution = new Dictionary<string, int> ();
        Dictionary<string, int> retrigger_distribution = new Dictionary<string, int>();
        Dictionary<string, int> free_feature = new Dictionary<string, int>();
        Dictionary<string, double> free_hits = new Dictionary<string, double>();
        Dictionary<string, double> free_contr = new Dictionary<string, double>();

        Dictionary<string, double> full_game_hits = new Dictionary<string, double>();
        Dictionary<string, double> full_game_contr = new Dictionary<string, double>();

        Dictionary<string, double> base_feature_distr = new Dictionary<string, double>();
        Dictionary<string, double> base_win_distr = new Dictionary<string, double>();
        Dictionary<string, double> base_4_hits = new Dictionary<string, double>();
        Dictionary<string, double> base_5_hits = new Dictionary<string, double>();
        Dictionary<string, double> base_6_hits = new Dictionary<string, double>();
        Dictionary<string, double> base_7_hits = new Dictionary<string, double>();
        Dictionary<string, double> base_8_hits = new Dictionary<string, double>();
        Dictionary<string, double> prog_hits = new Dictionary<string, double>();
        Dictionary<string, double> base_4_contr = new Dictionary<string, double>();
        Dictionary<string, double> base_5_contr = new Dictionary<string, double>();
        Dictionary<string, double> base_6_contr = new Dictionary<string, double>();
        Dictionary<string, double> base_7_contr = new Dictionary<string, double>();
        Dictionary<string, double> base_8_contr = new Dictionary<string, double>();
        Dictionary<string, double> prog_contr = new Dictionary<string, double>();     

        //Overrides//////////////////////////////////////////////////////////////////////////////
        internal override void PreSpin(bool _showStacks)
        {
            replaceSymbols(ReelType.PG);
            m_baseCycle = m.RandomIntegerAlwaysRandom(2);
            base.PreSpinRandomBase(m_baseCycle);
            //Size of screen resets at the beginning of each base game
            for (int reelNum = 1; reelNum < ReelCount - 1; reelNum++)
            {
                Dimensions[reelNum] = 4;
            }
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    case System.Windows.Forms.Keys.Oemtilde://Cheat Key "~"
                        int freeGames = 0;
                        while (freeGames == 0)
                        {
                            List<WinArgs> dummyWins = new List<WinArgs>();
                            replaceSymbols(ReelType.PG);
                            m_baseCycle = m.RandomIntegerAlwaysRandom(2);
                            base.PreSpinRandomBase(m_baseCycle);
                            CalculateWins(dummyWins, GetScreenSymbols(currentReelSet), "scatter", ref freeGames);
                        }
                        break;
                    case System.Windows.Forms.Keys.Q://Cheat Key "Q"
                        int expand = 0, freeGames2 = 0;
                        while (expand == 0)
                        {
                            List<WinArgs> dummyWins = new List<WinArgs>();
                            replaceSymbols(ReelType.PG);
                            m_baseCycle = m.RandomIntegerAlwaysRandom(2);
                            base.PreSpinRandomBase(m_baseCycle);
                            CalculateWins(dummyWins, GetScreenSymbols(currentReelSet), "scatter", ref freeGames2);
                            expand = playSurgeFeature(false);
                        }
                        break;
                }
            }
            if (m_progressiveType != "none")
            {
                for (int aa = 0; aa < m_progressives.Count(); aa++)
                {
                    if (m_progressiveType == "shamrock fortunes") m_progressives[aa].IncrementShamrock(Bet, aa, BetLevel);
                    else if (m_progressiveType != "shamrock fortunes") m_progressives[aa].Increment(Bet);
                    if (!InSimulation) frmProg.ProgressiveValueTBs[aa].Text = string.Format("{0:$0,0.00}", (double)m_progressives[aa].CurrentValue / 100);
                }
            }
            m_gameState = DefaultGameStates.PGPostSpin;
        }
        internal override GameAction PostSpin()
        {
            int oldFreeGames = m_freeGamesLeft;
            int dummyFreeGames = 0;
            SpinOrder = cDefaultSpinOrder;
            List<List<int>> screenSymbols;

            SetDictionaries(base_feature_distr); SetDictionaries(base_win_distr);
            SetOddsDictionaries(base_4_hits); SetOddsDictionaries(base_5_hits); SetOddsDictionaries(base_6_hits); SetOddsDictionaries(base_7_hits); SetOddsDictionaries(base_8_hits); SetOddsDictionaries(prog_hits);
            SetOddsDictionaries(base_4_contr); SetOddsDictionaries(base_5_contr); SetOddsDictionaries(base_6_contr); SetOddsDictionaries(base_7_contr); SetOddsDictionaries(base_8_contr); SetOddsDictionaries(prog_contr);
            SetOddsDictionaries(free_hits); SetOddsDictionaries(free_contr); SetOddsDictionaries(full_game_hits); SetOddsDictionaries(full_game_contr);
            switch (m_gameState)
            {
                case DefaultGameStates.PGPostSpin:                    
                    SecondFeature = false;
                    screenSymbols = GetScreenSymbols(currentReelSet);
                    //Check to see if surge feature is triggered and if so, increase size of screen.
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref dummyFreeGames);
                    m_additionalRows = playSurgeFeature(false);
                    if (m_additionalRows > 0)
                    {
                        SecondFeature = true;
                        for (int reelNum = 1; reelNum < ReelCount-1; reelNum++)
                        {
                            Dimensions[reelNum] += m_additionalRows;
                        }

                        screenSymbols = GetScreenSymbols(currentReelSet);
                    }
                    WinsToShow = new List<WinArgs>();
                    int BGWins = 0;
                   
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);

                    //Get bg distributions for output
                    BGWins = CountTheseWins(WinsToShow);

                    if (Dimensions[1] == 4)
                    {
                        base_feature_distr["4 High"]++;
                        base_win_distr["4 High"] += BGWins;
                        AddToOdds(base_4_hits, base_4_contr, BGWins);
                    }
                    else if (Dimensions[1] == 5)
                    {
                        base_feature_distr["5 High"]++;
                        base_win_distr["5 High"] += BGWins;
                        AddToOdds(base_5_hits, base_5_contr, BGWins);
                    }
                    else if (Dimensions[1] == 6)
                    {
                        base_feature_distr["6 High"]++;
                        base_win_distr["6 High"] += BGWins;
                        AddToOdds(base_6_hits, base_6_contr, BGWins);
                    }
                    else if (Dimensions[1] == 7)
                    {
                        base_feature_distr["7 High"]++;
                        base_win_distr["7 High"] += BGWins;
                        AddToOdds(base_7_hits, base_7_contr, BGWins);
                    }
                    else if (Dimensions[1] == 8)
                    {
                        base_feature_distr["8 High"]++;
                        base_win_distr["8 High"] += BGWins;
                        AddToOdds(base_8_hits, base_8_contr, BGWins);
                    }
                    else
                    {
                        throw new ArgumentException("Error: Impossible Number of Rows in Fire Wolf 2");
                    }

                    //edit the below loop to calculate the progressive award
                    if (m_progressiveType != "none")
                    {
                        List<WinArgs> progWins = new List<WinArgs>();
                        if (m_progressiveType == "dajidali") progWins = getDaJiDaLiWin();
                        else if (m_progressiveType == "must hit") progWins = getMustHitProgressiveWin();
                        else if (m_progressiveType == "shamrock fortunes") progWins = getShamrockFortunesWin();
                        else if (m_progressiveType == "custom") progWins = getProgressiveWin();
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);
                        m_progressiveWinPG = CountTheseWins(progWins);

                        if (m_progressiveWinPG > 0)
                        {
                            base_feature_distr["Prog"]++;
                            base_win_distr["Prog"] += m_progressiveWinPG;
                            AddToOdds(prog_hits, prog_contr, m_progressiveWinPG);
                        }
                    }
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
                        string name = m_freeGamesLeft.ToString() + "\t" + Dimensions[1].ToString();

                        if (trigger_distribution.ContainsKey(name))
                            trigger_distribution[name]++;
                        else
                            trigger_distribution[name] = 1;

                        fg_triggers++;
                    }
                    break;
                case DefaultGameStates.FGPreSpin:
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);
                    total_fgs_played++;
                    //Set Up Free Game
                    replaceSymbols(ReelType.FG);

                    if(Dimensions[1] == 5)
                        GetFG5ReelStrips(false);
                    else if (Dimensions[1] == 6)
                        GetFG6ReelStrips(false);
                    else if (Dimensions[1] == 7)
                        GetFG7ReelStrips(false);
                    else if (Dimensions[1] == 8)
                        GetFG8ReelStrips(false);
                    
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
                    original_num_rows = Dimensions[1];
                    //Check to see if surge feature is triggered and if so, increase size of screen. Max size of screen is 8.
                    if (Dimensions[1] != 8)
                    {
                        CalculateWins(WinsToShow, screenSymbols, "scatter", ref dummyFreeGames);
                        m_additionalRows = playSurgeFeature(true);
                        if (m_additionalRows > 0)
                        {
                            string name = Dimensions[1].ToString();
                            if (free_feature.ContainsKey(name))
                                free_feature[name]++;
                            else
                                free_feature[name] = 1;  

                            for (int reelNum = 1; reelNum < ReelCount - 1; reelNum++)
                            {
                                Dimensions[reelNum] += m_additionalRows;
                            }

                            screenSymbols = GetScreenSymbols(currentReelSet);                             
                        }
                    }
                    WinsToShow = new List<WinArgs>();
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);
                    Wins.AddRange(WinsToShow);
                    //Do Stats
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);

                        AddToOdds(free_hits, free_contr, winsThisFG);
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
                if (InFreeGames && freeGamesWonThisSpin > 0)
                {
                    fg_retriggers++;

                    string name = freeGamesWonThisSpin.ToString() + "\t" + original_num_rows.ToString();

                    if (retrigger_distribution.ContainsKey(name))
                        retrigger_distribution[name]++;
                    else
                        retrigger_distribution[name] = 1;
                }

                InFreeGames = true;
                if (!InSimulation && freeGamesWonThisSpin > 0)
                    m.gameMessageBox.Text = string.Format("{0} FREE GAMES WON!!!", freeGamesWonThisSpin);                

                m_gameState = DefaultGameStates.FGPreSpin;
                return GameAction.ShowWinsInBetweenGames;
            }
            //Sum up wins and Stats
            WinsThisGame = CountTheseWins(Wins);
            AddToOdds(full_game_hits, full_game_contr, WinsThisGame);
            AfterGameStatsCollection();
            if (m_freeGamesLeft == 0 && m_freeGamesPlayed > 0 && !InSimulation) m.gameMessageBox.Text = string.Format("{0} Total Free Spins Bonus Win", WinsThisGame);
            return GameAction.End;
        }
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        private List<WinArgs> getProgressiveWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();
            return winsToReturn;
        }

        //Print info for stats
        void SetDictionaries(Dictionary<string, double> hits)
        {
            if (!hits.ContainsKey("4 High"))
                hits.Add("4 High", 0);
            if (!hits.ContainsKey("5 High"))
                hits.Add("5 High", 0);
            if (!hits.ContainsKey("6 High"))
                hits.Add("6 High", 0);
            if (!hits.ContainsKey("7 High"))
                hits.Add("7 High", 0);
            if (!hits.ContainsKey("8 High"))
                hits.Add("8 High", 0);
            if (!hits.ContainsKey("Prog"))
                hits.Add("Prog", 0);
        }

        void SetOddsDictionaries(Dictionary<string, double> hits)
        {
            if (!hits.ContainsKey("0xTB"))
                hits.Add("0xTB", 0);
            if (!hits.ContainsKey("0:1xTB"))
                hits.Add("0:1xTB", 0);
            if (!hits.ContainsKey("1:3xTB"))
                hits.Add("1:3xTB", 0);
            if (!hits.ContainsKey("3:5xTB"))
                hits.Add("3:5xTB", 0);
            if (!hits.ContainsKey("5:10xTB"))
                hits.Add("5:10xTB", 0);
            if (!hits.ContainsKey("10:25xTB"))
                hits.Add("10:25xTB", 0);
            if (!hits.ContainsKey("25:50xTB"))
                hits.Add("25:50xTB", 0);
            if (!hits.ContainsKey("50:100xTB"))
                hits.Add("50:100xTB", 0);
            if (!hits.ContainsKey("100:150xTB"))
                hits.Add("100:150xTB", 0);
            if (!hits.ContainsKey("150:200xTB"))
                hits.Add("150:200xTB", 0);
            if (!hits.ContainsKey("200:500xTB"))
                hits.Add("200:500xTB", 0);
            if (!hits.ContainsKey("500:infxTB"))
                hits.Add("500:infxTB", 0);
        }

        void AddToOdds(Dictionary<string, double> hits, Dictionary<string, double> contr, double award)
        {
            if (award == 0)
            {
                hits["0xTB"]++;
            }
            else if (award < 1 * Bet)
            {
                hits["0:1xTB"]++;
                contr["0:1xTB"] += award;
            }
            else if (award < 3 * Bet)
            {
                hits["1:3xTB"]++;
                contr["1:3xTB"] += award;
            }
            else if (award < 5 * Bet)
            {
                hits["3:5xTB"]++;
                contr["3:5xTB"] += award;
            }
            else if (award < 10 * Bet)
            {
                hits["5:10xTB"]++;
                contr["5:10xTB"] += award;
            }
            else if (award < 25 * Bet)
            {
                hits["10:25xTB"]++;
                contr["10:25xTB"] += award;
            }
            else if (award < 50 * Bet)
            {
                hits["25:50xTB"]++;
                contr["25:50xTB"] += award;
            }
            else if (award < 100 * Bet)
            {
                hits["50:100xTB"]++;
                contr["50:100xTB"] += award;
            }
            else if (award < 150 * Bet)
            {
                hits["100:150xTB"]++;
                contr["100:150xTB"] += award;
            }
            else if (award < 200 * Bet)
            {
                hits["150:200xTB"]++;
                contr["150:200xTB"] += award;
            }
            else if (award < 500 * Bet)
            {
                hits["200:500xTB"]++;
                contr["200:500xTB"] += award;
            }
            else
            {
                hits["500:infxTB"]++;
                contr["500:infxTB"] += award;
            }
        }

        internal override void DisplayCustomStats(List<List<string>> _results)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"Free Games Info.txt"))
            {
                file.WriteLine("Number of Free Game Triggers:\t" + fg_triggers);
                file.WriteLine("Number of Free Game Retriggers:\t" + fg_retriggers);
                file.WriteLine("Number of Free Games Played:\t" + total_fgs_played);
                file.WriteLine();
                file.WriteLine("FG Trigger Distribution");
                file.WriteLine("Free Games\tRows\tCount");
                foreach (KeyValuePair<string, int> name in trigger_distribution)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("FG Retrigger Distribution");
                file.WriteLine("Free Games\tRows\tCount");
                foreach (KeyValuePair<string, int> name in retrigger_distribution)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("FG Feature Trigger Distribution");
                file.WriteLine("Rows\tCount");
                foreach (KeyValuePair<string, int> name in free_feature)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("FG Hit Distribution");
                file.WriteLine("Award Range\tCount");
                foreach (KeyValuePair<string, double> name in free_hits)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("FG Win Distribution");
                file.WriteLine("Award Range\tTotal Wins");
                foreach (KeyValuePair<string, double> name in free_contr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"Base Games Info.txt"))
            {
                file.WriteLine("BG Hit Distribution");
                file.WriteLine("Rows\tCount");
                foreach (KeyValuePair<string, double> name in base_feature_distr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG Win Distribution");
                file.WriteLine("Rows\tTotal Wins");
                foreach (KeyValuePair<string, double> name in base_win_distr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 4 Rows Hit Distribution");
                file.WriteLine("Award Range\tCount");
                foreach (KeyValuePair<string, double> name in base_4_hits)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 5 Rows Hit Distribution");
                file.WriteLine("Award Range\tCount");
                foreach (KeyValuePair<string, double> name in base_5_hits)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 6 Rows Hit Distribution");
                file.WriteLine("Award Range\tCount");
                foreach (KeyValuePair<string, double> name in base_6_hits)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 7 Rows Hit Distribution");
                file.WriteLine("Award Range\tCount");
                foreach (KeyValuePair<string, double> name in base_7_hits)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 8 Rows Hit Distribution");
                file.WriteLine("Award Range\tCount");
                foreach (KeyValuePair<string, double> name in base_8_hits)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG Prog Hit Distribution");
                file.WriteLine("Award Range\tCount");
                foreach (KeyValuePair<string, double> name in prog_hits)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 4 Rows Win Distribution");
                file.WriteLine("Award Range\tTotal Wins");
                foreach (KeyValuePair<string, double> name in base_4_contr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 5 Rows Win Distribution");
                file.WriteLine("Award Range\tTotal Wins");
                foreach (KeyValuePair<string, double> name in base_5_contr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 6 Rows Win Distribution");
                file.WriteLine("Award Range\tTotal Wins");
                foreach (KeyValuePair<string, double> name in base_6_contr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 7 Rows Win Distribution");
                file.WriteLine("Award Range\tTotal Wins");
                foreach (KeyValuePair<string, double> name in base_7_contr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG 8 Rows Win Distribution");
                file.WriteLine("Award Range\tTotal Wins");
                foreach (KeyValuePair<string, double> name in base_8_contr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("BG Prog Win Distribution");
                file.WriteLine("Award Range\tTotal Wins");
                foreach (KeyValuePair<string, double> name in prog_contr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"Full Games Info.txt"))
            {
                file.WriteLine("Full Game Hit Distribution");
                file.WriteLine("Award Range\tCount");
                foreach (KeyValuePair<string, double> name in full_game_hits)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
                file.WriteLine();
                file.WriteLine("Full Game Win Distribution");
                file.WriteLine("Award Range\tTotal Wins");
                foreach (KeyValuePair<string, double> name in full_game_contr)
                {
                    file.WriteLine("{0}\t{1}", name.Key, name.Value);
                }
            }
        }
    }
}
