/* Name:        David A. Clark, Jr.
 * Class:       MDV2229-O
 * Assignment:  3.3 - Coding Exercise - Exercise 5
 * Date:        2018-12-09
 * 
 * Tasks:       Console game against the computer of "Rock, Paper, Scissors, Lizard,
 *              Spock" that pits the user against the computer for 10 matches, with
 *              feedback about each round and a match summary.
 *              
 * Note:        Uses SuperMenu, various MenuOption subclasses, and PromptFor<T> from 
 *              referenced Utilities class library.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Terminal;
using static Utilities.Terminal.IO;

namespace Exercise_5
{
    public class Program
    {
        // Players.
        public const string human    = "Human";
        public const string computer = "Computer";

        // Game moves.
        public const string rock     = "Rock";
        public const string paper    = "Paper";
        public const string scissors = "Scissors";
        public const string lizard   = "Lizzard";
        public const string spock    = "Spock";

        // Player wins.
        public static Dictionary< string, List< string > > playerWins =
            new Dictionary< string, List< string> >() {
                [ human    ] = null
            ,   [ computer ] = null
        };

        // Match transcript.
        public static List< string > transcript = null;

        // Winning game move combos.  All others are draws.
        public static Dictionary< ( string, string ), string > winCombos =
            new Dictionary< ( string, string ), string >() {
                [ ( rock,     scissors ) ] = "mangles"
            ,   [ ( rock,     lizard   ) ] = "bruises"
            ,   [ ( paper,    rock     ) ] = "covers"
            ,   [ ( paper,    spock    ) ] = "disproves"
            ,   [ ( scissors, paper    ) ] = "cuts"
            ,   [ ( scissors, lizard   ) ] = "decapitates"
            ,   [ ( lizard,   paper    ) ] = "eats"
            ,   [ ( lizard,   spock    ) ] = "poisons"
            ,   [ ( spock,    scissors ) ] = "dulls"
            ,   [ ( spock,    rock     ) ] = "vaporizes"
        };

        // Roll up the game moves in a list for later usage.
        public static List< string > gameMoves =
            new List< string >() { rock, paper, scissors, lizard, spock };

        // Roll up the game moves for the game instructions.
        public static string gameMovesInstr = string.Join( ", ", gameMoves );

        // Roll up the winning combos for the game instructions.
        public static string winCombosInstr = string.Join( "\n",
                winCombos.Select( pair => string.Join( " ",
                    " -", pair.Key.Item1, pair.Value, pair.Key.Item2 ) ) );

        // Game moves menu.
        public static SuperMenu menuMoves = new SuperMenu();

        // Game welcome and instructions.
        public static string instructions =
            $"Welcome to the game of \"{ gameMovesInstr }\",\n" +
            $"Where a human player is pitted against a computer player for 10 rounds.\n\n" +
            $"Each round, the human player chooses one of the game's moves,\n" +
            $"and the computer player will, too.  The player that chooses a\n" +
            $"move that beats the other wins that round.  If both players\n" +
            $"choose the same move, that round is a draw.  After 10 rounds,\n" +
            $"whichever player had the most winning moves wins!  If both\n" +
            $"players have equal wins, the whole match is a draw.\n\n" +
            $"Here are the winning combinations of game moves:\n\n" +
            winCombosInstr;

        // Progam entry point.
        public static void Main( string[] args )
        {
            // Show the instructions, and wait to continue.
            Console.WriteLine( instructions );
            Pause();

            // Run the game until the user wants to quit.
            do {
                // Set up the menu,
                LoadMovesMenu();
                // player wins,
                playerWins.Keys.ToList().ForEach( key => 
                    playerWins[ key ] = new List< string >() );
                // and game transcript.
                transcript = new List< string >();

                // Take the player through 10 rounds.
                Enumerable.Range( 1, 10 ).ToList().ForEach( round => {
                    // Customize the menu prompt for the round.
                    menuMoves.Prompt =
                        $"\"{ gameMovesInstr }\"\n\n" +
                        $"Round { round } - { human }'s Move\n" +
                        $"What's your move, { human }?";

                    // Show the moves menu and get the human's move.
                    string humanMove    = menuMoves.Run().Text;

                    // Get the computer's random move.
                    string computerMove = gameMoves.OrderBy( move =>
                        Guid.NewGuid() ).First();

                    // Let the player know how the round went.
                    Console.WriteLine(
                        $"The { human    } chose { humanMove    }, and " +
                        $"the { computer } chose { computerMove }.\n\n"  +
                        GetRoundWinner( round, humanMove, computerMove )
                    );

                    Pause();
                } );

                // Let the player know how the match went.
                Console.Clear();
                Console.WriteLine( GetMatchWinner() );
            
                // See if the user wants to see the match transcript.
                if( Yes( "\nDo you want to see the match transcript?" ) ) {
                    // User said yes.
                    Console.Clear();
                    // Header for transcript.
                    Console.WriteLine( "Here's the transcript for that match:\n" );
                    // Display the transcript.
                    transcript.ForEach( entry =>
                        Console.WriteLine( entry ) );
                    Pause();
                }

                Console.Clear();
            } while( Yes( "Would you like to play again?" ) );
        }

        // Prompt the user to choose yes or no.
        public static bool Yes( string prompt ) {
            // Return the user's response.
            return PromptFor< string >( 
                prompt + "\nEnter \"yes\" or \"no\".",
                "That isn't a \"yes\" or \"no\".  Try again.",
                any => any.ToLower() == "yes" || any.ToLower() == "no"
            ).ToLower() == "yes";
        }

        // See which player won the whole match, and report it.
        public static string GetMatchWinner() {
            string outcome = null; // The match's outcome.

            // Tally up the wins for each player.
            var talliedWins = playerWins.ToDictionary( pair => 
                pair.Key, pair => pair.Value.Count );

            // Let the player know how the whole match went.
            if( talliedWins[ human ] > talliedWins[ computer ] ) {
                // The human player won the match.
                outcome =
                    $"The { human } player won the match " +
                    $"with { talliedWins[ human ] } total wins against " +
                    $"the { computer }'s { talliedWins[ computer ] } total wins!";
            } else if( talliedWins[ computer ] > talliedWins[ human ] ) {
                // The computer player won the match.
                outcome =
                    $"The { computer } player won the match " +
                    $"with { talliedWins[ computer ] } total wins against " +
                    $"the { human }'s { talliedWins[ human ] } total wins!";
            } else {
                // The whole match was a draw.
                outcome =
                    $"Both player's had { talliedWins[ human ] }, a total draw!";
            }

            return outcome;
        }

        // See which player won and report the winning combo, recording it
        // in the winning players wins.
        public static string GetRoundWinner( int round, string humanMove, string computerMove ) {
            string outcome = null; // The round's outcome.

            // See which player won.
            if( winCombos.ContainsKey( ( humanMove, computerMove ) ) ) {
                // The human player won.
                outcome =
                    $"The { human }'s { humanMove } " +
                    $"{ winCombos[ ( humanMove, computerMove ) ] } " +
                    $"the { computer }'s { computerMove }...\n" +
                    $"The { human } wins round { round }!";

                // Record it in human wins.
                playerWins[ human ].Add( outcome );
            } else if( winCombos.ContainsKey( ( computerMove, humanMove ) ) ) {
                // The computer player won.
                 outcome =
                    $"The { computer }'s { computerMove } " +
                    $"{ winCombos[ ( computerMove, humanMove ) ] } " +
                    $"the { human }'s { humanMove }...\n" +
                    $"The { computer } wins round { round }!";

                // Record it in computer wins.
                playerWins[ computer ].Add( outcome );
            } else {
                // The round was a draw.
                 outcome =
                    $"Both players chose { humanMove }.  " +
                    $"Round { round } is a draw!";
            }

            // Record it in the match transcript.
            transcript.Add( outcome );
            return outcome;
        }

        // Load the moves menu.
        public static void LoadMovesMenu() {
            // Only load it if it isn't loaded.
            if( !menuMoves.Any() ) {
                // Roll moves into a list of options
                // to add to the menu, adding each.
                gameMoves.ForEach( move => 
                    menuMoves.Add( new CancelOption( move )
                ) );
            }
        }

        // Pause console output until player continues.
        public static void Pause() {
            // Let the player know we're paused.
            Console.WriteLine( "\nPress any key to continue..." );
            // Continue on player keypress.
            Console.ReadKey();
        }
    }
}
