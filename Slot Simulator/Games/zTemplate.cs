using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Slot_Simulator
{
    class Template : GameInfo
    {
        /*This function loads the game information revelant only to this game, any extra data in the game data needs to be loaded in this function*/
        internal Template(ExcelFile _excelFile)
            : base(_excelFile)
        {
            //Anything you would need to create when the excel file is loaded.
            //m_extraData holds all tables that are not default
            //m_extraGeneralData holds all data that is extra in General table
            m_symbolWild = Reels.Symbols.IndexOf("ww");                             //stores the symbol index as an integer for the WILD symbol
            m_symbolScatter = Reels.Symbols.IndexOf("bn");                          //stores the symbol index as an ineteger for the BONUS symbol
        }
        //Variables//////////////////////////////////////////////////////////////////////////////
        /*Declare all non-statistical variables here*/
        DefaultGameStates m_gameState;                                              //allows the game to switch between the game states (free games, base game, progressives, etc.)
        int m_symbolScatter, m_symbolWild;                                          //declaration of the symbol index integers for the bonus and scatter symbols
        //Overrides//////////////////////////////////////////////////////////////////////////////
        /*The functions below ovveride the corresponding functions in GameInfo.cs.  This is necessary so that the game can properly function without affecting other game classes*/
        internal override void PreSpin(bool _showStacks)
        {
            /*The below line calls GameInfo.PreSpin to get the appropriate reel strips, reel weights and sets the indices which the game will spin to.  Change false to true if the game has replacement!*/
            base.PreSpin(false);                                                    //see above
            /*If loop, for cheats when not running a simulation*/
            if (!InSimulation)
            {
                switch (CheatKey)
                {
                    /*Cheat for free spins.  If the game needs additional cheats, add similar to below as a case and run the appropriate logic.  
                     * To access the indices for a particular reel stop, use ReelIndexes[x] where x is the desired reel.
                     * Note: Reels are numbered 0 - 4.*/
                    case System.Windows.Forms.Keys.Oemtilde://Cheat Key "~"
                        int freeGames = 0;
                        while (freeGames == 0)
                        {
                            List<WinArgs> dummyWins = new List<WinArgs>();
                            base.PreSpin(false);
                            CalculateWins(dummyWins, GetScreenSymbols(currentReelSet), "scatter", ref freeGames);
                        }
                        break;
                }
            }
            /*If loop to increment progressives.  If the game has shamrock fortunes, see Emerald Fairy and Lucky Genie*/
            if (m_progressiveType != "none")
            {
                incrementProgressives();                                        //increments the progressives
            }
            m_gameState = DefaultGameStates.PGPostSpin;                         //sends game to postspin function to evaluate and control game state*/
        }
        /*function to evaluate and control current game state.  Any game evaluation logic goes here*/
        internal override GameAction PostSpin()
        {
            int oldFreeGames = m_freeGamesLeft;                                 //stores free game count so adding free games is easier
            SpinOrder = cDefaultSpinOrder;                                      //sets spin order (-1 means reel does not spin, number the reels so it spins, anything larger than the reel count delays the reel landing longer)
            List<List<int>> screenSymbols;                                      //list of lists storing the symbols currently on the screen
            /*switch statement to control the current game state of the game*/
            switch (m_gameState)
            {
                /*base game post spin evaluation and logic*/
                case DefaultGameStates.PGPostSpin:                                  
                    screenSymbols = GetScreenSymbols(currentReelSet);                               //gets the symbols on the screen
                    WinsToShow = new List<WinArgs>();                                               //sets up list of WinArgs to store the wins on the screen
                    //Calculate Post Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);        //calculates all non scatter wins
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);       //calculates scatter wins
                    /*If loop to calculate progressive awards*/
                    if (m_progressiveType != "none")
                    {
                        List<WinArgs> progWins = new List<WinArgs>();                               //sets up list of WinArgs to record the progressive wins
                        if (m_progressiveType == "dajidali") progWins = getDaJiDaLiWin();                   //gets wins for Da Ji Da Li jackpots
                        else if (m_progressiveType == "must hit") progWins = getMustHitProgressiveWin();    //gets standard 3% must hit by wins
                        else if (m_progressiveType == "shamrock fortunes") progWins = getShamrockFortunesWin();         //gets shamrock fortunes jackpot wins
                        else if (m_progressiveType == "custom") progWins = getProgressiveWin();                         //gets any custom jackpot wins
                        if (progWins.Count() > 0) WinsToShow.AddRange(progWins);                                        //adds the jackpot wins to the wins on screen
                        m_progressiveWinPG = CountTheseWins(progWins);                                                  //calculates total win of the jackpot wins
                    }
                    Wins.AddRange(WinsToShow);                                                                          //adds all wins into the win list
                    /*If loop to calculate statistics for simulation runs*/
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinsThisPG = CountTheseWins(WinsToShow);                                                 //calculates the total win for the base game
                        m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 0);                                       //stores the base game win in the appropriate contribution chart
                        //if there is an additional feature in the base game, this stores the base game win if the feature was triggered in the appropriate chart
                        if (SecondFeature) m_statsWinDistributionPG.StoreGame(m_statsWinsThisPG, 1);                    
                    }
                    /*If loop to calculate custom statistics for simulation runs for single base game spin*/
                    if (DoCustomStats)
                    {
                    }
                    break;                                                                                              //breaks the switch statement since evaluation is finished
                /*free spin pre spin setup so the free spin can be played*/
                case DefaultGameStates.FGPreSpin:
                    //edits message box on bottom right hand corner to display free spin count
                    if (!InSimulation) m.gameMessageBox.Text = string.Format("{0} out of {1} FREE GAMES", m_freeGamesPlayed + 1, m_freeGamesLeft + m_freeGamesPlayed);      
                    /*Gets free game reel strips (set value true if symbol insertion occurs, set to false if no symbol insertion)*/
                    GetFGReelStrips(true);
                    /*If loop for statistics calculation if in simulation mode*/
                    if (DoDefaultWinDistributions)
                    {
                        m_statsWinDistributionFG.IncGames();                                                            //increments games so Contribution and hit frequency can be calculated
                    }
                    m_progressiveWinFG = 0;                                                                             //sets jackpot win on Free Spin to 0.
                    m_gameState = DefaultGameStates.FGPostSpin;                                                         //sets game state to go into Post Spin so Free Spin can be evaluated
                    return GameAction.Spin;                                                                             //spins the reels
                /*free spin post spin evaluation and game logic*/
                case DefaultGameStates.FGPostSpin:
                    screenSymbols = GetScreenSymbols(currentReelSet);                                                   //gets symbols on the screen
                    WinsToShow = new List<WinArgs>();                                                                   //sets up list of WinArgs to store the wins on the screen
                    //Calculate Free Game
                    CalculateWins(WinsToShow, screenSymbols, "normal", ref m_freeGamesLeft);                            //calculates all non scatter wins
                    CalculateWins(WinsToShow, screenSymbols, "scatter", ref m_freeGamesLeft);                           //calculates scatter wins
                    Wins.AddRange(WinsToShow);                                                                          //adds all wins into the win list
                    /*If loop to calculate statistics for simulation runs*/
                    if (DoDefaultWinDistributions)
                    {
                        int winsThisFG = CountTheseWins(WinsToShow);                                                    //calculates the total win for this free spin
                        m_statsWinDistributionFG.StoreGame(winsThisFG, 0);                                              //stores the base game win in the appropriate contribution chart
                        //if there is an additional feature in the base game, this stores the base game win if the feature was triggered in the appropriate chart
                        if (SecondFeature) m_statsWinDistributionFG.StoreGame(winsThisFG, 1);
                    }
                    /*If loop to calculate custom statistics for simulation runs for individual free spin*/
                    if (DoCustomStats)
                    {
                    }
                    m_freeGamesLeft--;                                                                                  //Decrements free spins played
                    m_freeGamesPlayed++;                                                                                //increments free spins played
                    break;                                                                                              //breaks the switch statement since evaluation is finished
            }                                                                                                           //end of switch statement
            /*if loop to determine if free spins need to be played and play them as appropriate*/
            if (m_freeGamesLeft > 0)
            {
                int freeGamesWonThisSpin = InFreeGames ? m_freeGamesLeft - oldFreeGames + 1 : m_freeGamesLeft;          //determines how many free spins were won this spin
                InFreeGames = true;                                                                                     //sets boolean so game knows it is in free spins
                /*If loop to display wins if not in simulation mode*/
                if (!InSimulation && freeGamesWonThisSpin > 0)
                    m.gameMessageBox.Text = string.Format("{0} FREE GAMES WON!!!", freeGamesWonThisSpin);               //sets message box to display how many free spins are won
                m_gameState = DefaultGameStates.FGPreSpin;                                                              //sets game state to free spin pre spin
                return GameAction.ShowWinsInBetweenGames;                                                               //sets game action to show wins while waiting for the next spin to commence
            }
            WinsThisGame = CountTheseWins(Wins);                                                                        //adds all wins together for statistics
            AfterGameStatsCollection();                                                                                 //collects all necessary stats if not in simulation mode
            /*If loop to change game message box in lower right hand corner and display total free spins bonus win amount if game not in simulation*/
            if (m_freeGamesLeft == 0 && m_freeGamesPlayed > 0 && !InSimulation) m.gameMessageBox.Text = string.Format("{0} Total Free Spins Bonus Win", WinsThisGame);
            /*If loop to store all appropriate custom stats for the entire bought game*/
            if (DoCustomStats)
            {
            }
            return GameAction.End;                                                                                      //ends the bought game
        }
        /*function to override paytable counts if necessary*/
        internal override void GetPayTableCounts(out Dictionary<string, Dictionary<string, PayCountArg>> _payTablesSeperated, out Dictionary<string, PayCountArg> _payTablesTotal)
        {
            base.GetPayTableCounts(out _payTablesSeperated, out _payTablesTotal);
        }
        //Private Functions/////////////////////////////////////////////////////////////////////////
        /*All game specific functions go here.  For example if different reel strips are needed for different free spins bonuses, a function should be here to do so.  See Lucky Genie for example*/
        /*Custom jackpot game function.  This function should be designed to get custom jackpot wins.  See Quasi Shot for example.*/
        private List<WinArgs> getProgressiveWin()
        {
            List<WinArgs> winsToReturn = new List<WinArgs>();                                                       //List of WinArgs to return and store jackpot wins
            return winsToReturn;                                                                                    //Returns list of WinArgs so they can be displayed and stored properly
        }
        //Custom Stats//////////////////////////////////////////////////////////////////////////////
        /*All custom statistics should be declared here.  See Butterfly Game for example.*/
        /*Function to initialize all custom stats*/
        internal override void SetUpCustomStats()
        {
        }
        /*Function to display custom stats*/
        internal override void DisplayCustomStats(List<List<string>> _results)
        {
        }
    }
}
