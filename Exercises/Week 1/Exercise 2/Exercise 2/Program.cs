/* Name:        David A. Clark, Jr.
 * Class:       MDV2229-O
 * Assignment:  1.4 - Coding Exercise - Exercise 2
 * Date:        2018-11-24
 * 
 * Tasks:       Present the user with a console guessing game where the user is
 *              asked a question pertaining to November's subject matter of clothing
 *              brands.  Also prompt the user with up to 10 clues, checking for right,
 *              wrong, and close answers, giving appropriate feedback.
 *              
 * Note:        Uses SuperMenu, and various MenuOption subclasses from referenced
 *              Utilities class library.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Exercise_2
{
    public class Program
    {
        // The question...
        private const string question = "What is the first name of this iconic fashion designer?";

        // The correct answer...
        private const string answer   = "Thomas";

        // Close calls...
        private static IReadOnlyList< string > closeCalls = new List< string >()
        {
            "tommy", "tommey", "tom", "thom", "tomas", "tomie", "tommie"
        };

        // The clues...
        private static IReadOnlyList< string > clues = new  List< string >()
        {
            "His middle name is Jacob."
        ,   "His company is headquartered in Amsterdam."
        ,   "He allegedly made disparaging remarks about minorities wearing the clothing he designs."
        ,   "He got into a physical altercation with Guns & Roses front man, Axl Rose in a NY club."
        ,   "His line includes men's and women's fragances branded with his nickname."
        ,   "He has married twice."
        ,   "He was born in the state of NY."
        ,   "One of his daughters starred in MTV's reality series \"Rich Girls\"."
        ,   "The blue jeans he designs are among his most popular fashion items."
        ,   "His last name is Hilfiger."
        };

        // String constants to keep the main logic uncluttered.

        private const string game   = "Let's play a game.  I'll give you a question, and you have to guess"
                                    + "\nthe right answer by entering it into the console.  Remember, the"
                                    + "\nfirst letter of your answer MUST be capitalized...";
        private const string pause  = "\nPress any key to continue...";
        private const string right  = "\nThat was the correct answer!";
        private const string close  = "\nThat was incorrect, but you are close!  Make sure to capitalize"
                                    + "\nthe first letter of your answer, and try different variations"
                                    + "\non his name.";
        private const string wrong  = "\nSorry... That was the wrong answer.";
        private const string thanks = "\nThanks for playing!  Play again soon.";
        private const string clue   = "Here is your {0} clue.\n";
        private const string first  = "first";
        private const string last   = "last";
        private const string next   = "next";

        // Pauses the game to prevent output from disappearing before the
        // user can read it.
        private static void Pause()
        {
            // Pause and wait for user to press a key.
            Console.WriteLine( pause );
            Console.ReadKey();
        }

        // Program entry point.
        public static void Main( string[] args )
        {
            
            // Technically a nested loop: Generate a list of strings that
            // concatenate progressively more clues, up to all of them.
            var clueProgression =
                from i in Enumerable.Range( 0, clues.Count + 1 )
                select string.Join( "\n", clues.ToList().GetRange( 0, i ) );

            // Get the enumerator for the Clue Progression and move it
            // to the first Clue Progression.  This will significantly
            // simplify the main game loop.
            var clueProgIndex = clueProgression.GetEnumerator();

            clueProgIndex.MoveNext();

            // Tell the user about the game their about to play.
            Console.WriteLine( game );
            Pause();

            // Holds the user's answer.
            string entered = null;

            // Main game loop continues until the last Clue progression
            //  is exhausted or the user answers correctly.
            while( clueProgIndex.MoveNext() && entered != answer )
            {
                // Clear the screen and print the question.
                Console.Clear();
                Console.WriteLine( $"The question is, \"{ question }\"\n" );

                // Number of clues in this Clue Progression.
                var numClues  = clueProgIndex.Current.Split( '\n' ).Count();
                var whichClue = numClues == 1 ? first : numClues == clues.Count() ? last : next;

                // Print which Clue Progression the user is on,
                Console.WriteLine( clue, whichClue );
                // and then the current Progression of Clues.
                Console.WriteLine( clueProgIndex.Current );

                // Capture the user's answer,
                Console.Write( "\nEnter your answer here: " );
                // and protect against null entry.
                entered = Console.ReadLine() ?? "";

                // Check the user's answer and give appropriate feedback.
                if( entered == answer )
                    Console.WriteLine( right ); // Right!
                else if( closeCalls.Contains( entered.ToLower() ) )
                    Console.WriteLine( close ); // Close!
                else
                    Console.WriteLine( wrong ); // Wrong!

                Pause();
            }

            // Thank the user for playing.
            Console.WriteLine( thanks );
            Pause();
        }
    }
}
