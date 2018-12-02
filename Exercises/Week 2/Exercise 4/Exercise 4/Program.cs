/* Name:        David A. Clark, Jr.
 * Class:       MDV2229-O
 * Assignment:  2.2 - Coding Exercise - Exercise 4
 * Date:        2018-12-02
 * 
 * Tasks:       Present the user with a console application that allows the
 *              user to learn facts about his or her favorite color from the
 *              rainbow, and other colors as he or she wishes.
 *              
 * Note:        Uses SuperMenu, and various MenuOption subclasses from referenced
 *              Utilities class library.
 */
 
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Terminal;

namespace Exercise_4
{
    public class Program
    {
        // Why can't C# have more sensible enumerated types like
        // F# or Swift has.  
        private const string red    = "red";
        private const string orange = "orange";
        private const string yellow = "yellow";
        private const string green  = "green";
        private const string blue   = "blue";
        private const string indigo = "indigo";
        private const string violet = "violet";

        // Color-choice menu.
        private static SuperMenu menuColors = new SuperMenu
        (
            "", "That's not an available color.  Try again."
        )
        {
            new ActionOnlyOption( red.ToProper(),    () => DisplayColorFacts( red    ) )
        ,   new ActionOnlyOption( orange.ToProper(), () => DisplayColorFacts( orange ) )
        ,   new ActionOnlyOption( yellow.ToProper(), () => DisplayColorFacts( yellow ) )
        ,   new ActionOnlyOption( green.ToProper(),  () => DisplayColorFacts( green  ) )
        ,   new ActionOnlyOption( blue.ToProper(),   () => DisplayColorFacts( blue   ) )
        ,   new ActionOnlyOption( indigo.ToProper(), () => DisplayColorFacts( indigo ) )
        ,   new ActionOnlyOption( violet.ToProper(), () => DisplayColorFacts( violet ) )
        ,   
        };

        // Tell the user all about this color.
        private static void DisplayColorFacts( string color )
        {
            List< string > colorFacts; // Holds facts about colors.

            // Load up colorFacts with facts about a color using
            // a switch statement instead of a dictionary.
            switch( color )
            {
            case red:
                colorFacts = new List< string > {
                    $"Roses are {red}."
                ,   $"{red.ToProper()} was the color of British royalty."
                ,   $"Blood is actually {red}, despite popular misconceptions."
                };
                break;
            case orange:
                colorFacts = new List< string >() {
                    $"{orange.ToProper()} is considered a \"playful\" color."
                ,   $"Hunters wear {orange} colors to be seen by other hunters, which deer cannot distinguish."
                ,   $"The Monarche butterfly's wings are predominantly {orange}."
                };
                break;
            case yellow:
                colorFacts = new List< string >(){
                    $"The Sun is actually not {yellow}. It's white."
                ,   $"{yellow.ToProper()} #5, a common food dye, is rumored to cause health problems."
                ,   $"{yellow.ToProper()} is a common aposematic color for poisonous animals."
                };
                break;
            case green:
                colorFacts = new List< string >(){
                    $"Vegetation appears {green} from chlorophyll."
                ,   $"{green.ToProper()} expectorant signifies infection."
                ,   $"{green.ToProper()} is the most soothing of all colors."
                };
                break;
            case blue:
                colorFacts = new List< string >() {
                    $"{blue.ToProper()}berries and some grapes are among the only naturally {blue} foods."
                ,   $"On the visible electromagnetic spectrum, {blue} looks to be cyan."
                ,   $"{blue.ToProper()} was the name a popular song by the band Eiffle 65."
                };
                break;
            case indigo:
                colorFacts = new List< string >() {
                    $"Isaac Asimov thinks {indigo} is not a color worthy of note."
                ,   $"On the visible electromagnetic spectrum, {indigo} looks to be {blue}."
                ,   $"{indigo.ToProper()} dye--commonly used in {blue} jeans--comes from indigofera tinctoria."
                };
                break;
            case violet:
                colorFacts = new List< string >() {
                    $"{violet.ToProper()}s are {blue}."
                ,   $"{violet.ToProper()} is yet another color of British royalty."
                ,   $"{violet.ToProper()} is commonly called purple."
                };
                break;
            default:
                throw new Exception( $"Rainbow under/over-flow occurred with color \"{color}\"." );
            }

            var niceWords = new List< string >() { "pretty", "nice", "fine", "amazing", "great" };

            // Say something nice about the user's color choice.
            Console.WriteLine( "{0} is such a {1} color!\n", color.ToProper()
                             , niceWords.OrderBy( nw => Guid.NewGuid() ).First() );
            // Write all the facts to the console.
            colorFacts.ForEach( fact => Console.WriteLine( fact ) );
        }

        // Pause console output until user keystroke.
        private static void Pause()
        {
            // Let the user know we're paused.
            Console.WriteLine( "\nPress any key to continue..." );
            // Unpause on user keystroke.
            Console.ReadKey();
        }

        // Run a random entry from the color menu for the user.
        public static void RandomColor()
        {
            // We don't want to exit or choose Random Color from here,
            // so copy the options and remove those ones.
            var options = new List< MenuOption >( menuColors );
            options.RemoveAll( opt => opt.Text.Contains( "Exit" )
                                   || opt.Text.Contains( "Random" ) );

            // Had we assigned the menu options to variables before
            // inserting them into the menu, we could have removed them
            // by reference, but the above works fine considering the
            // size of the list.

            // Sort the options randomly and run the first one.
            options.OrderBy( opt => Guid.NewGuid() ).First().Run();
        }

        // Program entry point.
        public static void Main( string[] args )
        {
            // Clear the Console for nicer output.
            Console.Clear();

            // Get the user's favorite color and display facts about it.
            menuColors.Prompt = "What's your favorite color?";
            string favorite = menuColors.Run();

            Pause();
            
            // Prep the colors menu to show other color facts
            // aside from the user's favorite, at the users
            // discretion, including a randome option.
            menuColors.RemoveAll( opt => opt.Text == favorite );
            menuColors.Add( new ActionOnlyOption( "Random Color", RandomColor ) );
            menuColors.Add( new ExitOption() );
            menuColors.Prompt = "Would you like to know about some other colors?\n"
                              + "Choose another to learn about it, or Exit to quit.";

            // Run modified color menu until the user quits.
            while( true)
            {
                // Clear the Console for nicer output.
                Console.Clear();

                menuColors.Run();
                Pause();
            }
        }
    }
}
