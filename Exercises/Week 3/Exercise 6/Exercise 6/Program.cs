/* Name:        David A. Clark, Jr.
 * Class:       MDV2229-O
 * Assignment:  3.3 - Coding Exercise - Exercise 6
 * Date:        2018-12-09
 * 
 * Tasks:       Console card game where four users are dealt a deck of cards, the
 *              total point value of which they are dealt determines their placing,
 *              1st through 4th place.
 *              
 * Note:        Uses SuperMenu, various MenuOption subclasses, and PromptFor<T> from 
 *              referenced Utilities class library.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Terminal;
using static Utilities.Terminal.IO;

namespace Exercise_6
{
    public class Program
    {
        // Player placings.
        private static List< string > places =
            new List< string >() {
                "1st", "2nd", "3rd", "4th"
            };

        // Suit names.
        private static List< string > suits =
            new List< string >() {
                "Clubs", "Hearts", "Spades", "Diamonds"
            };

        // Named cards names and points.
        private static Dictionary< string, int > named =
            new Dictionary< string, int >() {
                [ "Jack" ] = 12, [ "Queen" ] = 12, [ "King" ] = 12, [ "Ace" ] = 15
            };

        // Unnamed cards names and points.
        private static IEnumerable< int > unnamed =
            Enumerable.Range( 2, 9 );

        // A card.
        private struct Card {
            public string name;
            public string suit;
            public int    value;
        }

        // A fresh deck of cards.
        private static IEnumerable< Card > freshDeck = (
            // Unnamed cards
            from name in unnamed
            from suit in suits
            select new Card {
                name  = name.ToString(),
                suit  = suit,
                value = name
            } ).Union(
            // Named cards.
            from pair in named
            from suit in suits
            select new Card {
                name  = pair.Key,
                suit  = suit,
                value = pair.Value
            } );

        // Program entry point.
        public static void Main( string[] args )
        {
            // Play until the user wants to quit.
            do {
                // Introduce the player to the game.
                Console.Clear();
                Console.WriteLine( "Welcome to the card deck game!\n" );

                // Get 4 player names from the user.
                var players = GetFourPlayers();

                // Set players up with empty hands.
                var hands   = (
                    from player in players
                    select new KeyValuePair<
                        string, List< Card >
                    >( player, new List< Card >() )
                ).ToDictionary( pair =>
                    pair.Key, pair =>
                    pair.Value
                );

                // Tell user the next step.
                Console.Clear();
                Console.WriteLine(
                    "I will now shuffle a deck of cards and deal a card each to\n"
                +   string.Join( ", ", players.Take( 3 ).ToList() )
                +   ", and " + players.Last() + " until the deck is fully dealt."
                );

                Pause();

                // Get a fresh deck of cards and shuffle them.
                var deck = new Stack< Card >( freshDeck.OrderBy( card => Guid.NewGuid() ) );

                // Until the deck is empty,
                while( deck.Any() ) {
                    // Deal one card to each player in turn.
                    hands.Keys.ToList().ForEach( hand =>
                        hands[ hand ].Add( deck.Pop() ) );
                }

                // Tell user the next step.
                Console.Clear();
                Console.WriteLine(
                    "All the cards have been dealt.  It's time for all players to\n"
                +   "show their hands and see where each placed against the other!"
                );

                Pause();

                // Place the players according to their score.
                var placings = hands.OrderByDescending( hand =>
                    hand.Value.Sum( card => card.value ) );

                // Tell the user all the player's placings, cards, and total score.
                Console.Clear();
                Console.WriteLine( "Here are the players' places, cards, and total scores:" );

                // Display each player's information in order.
                places.Zip( placings, ( place, placing ) => (
                    place
                ,   placing.Key
                ,   placing.Value
                ,   placing.Value.Sum( card => card.value ) )
                ).ToList().ForEach( detail => {
                    // Ouput the player's name, place, and score.
                    Console.WriteLine( 
                        $"\nName:  { detail.Key   }\n" +
                        $"Place: { detail.place }\n" +
                        $"Score: { detail.Item4 }\n" +
                        $"Cards:"
                    );

                    // Ouput all the player's cards.
                    detail.Value.ForEach( card =>
                        // Show the cards namea and suit.
                        Console.WriteLine( $" - { card.name } of { card.suit }" )
                    );
                } );

                // Display the total of all players' scores.
                Console.WriteLine(
                    "\nThe total of all players' scores is {0}\n"
                    // Sum the value of all the dealt hands.
                ,   hands.Values.Sum( cards => cards.Sum( card => card.value ) )
                );

            } while( Yes( "Would you like to play again?" ) );

            Pause();
        }

        // Pause console output until user continues.
        private static void Pause() {
            // Inform user of paused state.
            Console.WriteLine( "\nPress any key to continue..." );
            // Wait for user keypress.
            Console.ReadKey();
        }

        // Gets four player names from the user.
        private static List< string > GetFourPlayers() {
            return PromptFor< string >(
                "Enter a list of four player names, separated with spaces.\n"
            +   "For instance, you could enter \"Sam Alice Fred Jenny\"."
            ,   "Hmmm... That doesn't seem like four player names.  Try again."
            ,   any => {
                    var names = any.Split( ' ' );
                    return 
                        names.Count() == 4 &&
                        names.All( name => !string.IsNullOrEmpty( name ) );
                }
            ).Split( ' ' ).ToList();
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
    }
}
