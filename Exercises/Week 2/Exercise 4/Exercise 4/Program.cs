using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Terminal;
using System.Collections.ObjectModel;
using static Utilities.Terminal.IO;

namespace Exercise_4
{
    public class Program
    {
        // Why can't C# have more sensible enumerated types like
        // F# or Swift has.  
        private const string red    = "Red";
        private const string orange = "Orange";
        private const string yellow = "Yellow";
        private const string green  = "Green";
        private const string blue   = "Blue";
        private const string indigo = "Indigo";
        private const string violet = "Violet";

        // Awesome facts about colors.
        private IReadOnlyDictionary< string, List< string > > colorFacts = new Dictionary< string, List< string > >()
        {
            [ red ]    = new List< string >()
            {
                "Roses are red. (Wait for it...)"
            ,   "Red was the color of British royalty."
            ,   "Blood is actually red, despite popular belief."
            ,   
            }

        ,   [ orange ] = new List< string >()
            {
                "Orange is considered a \"playful\" color."
            ,   "Hunters wear orange colors to be seen by other hunters, which deer cannot distinguish."
            ,   "The Monarche butterfly's wings are predominantly orange."
            ,   
            }

        ,   [ yellow ] = new List< string >()
            {
                "The Sun is actually not yellow. It's white."
            ,   "Neither is Yellow Mellow soda.  It's clear."
            ,   "Yellow is a common aposematic color for poisonous animals."
            ,
            }

        ,   [ green ]  = new List< string >()
            {
                "Vegetation appears green from chlorophyll."
            ,   "Green expectorant signifies infection."
            ,   "Green is the most soothing of all colors."
            ,
            }

        ,   [ blue ]   = new List< string >()
            {
                "Blueberries and some grapes are among the only naturally blue foods."
            ,   "Blue is the preferred dye of denim pants because of it fading."
            ,   "Blue was the name a popular song by the band Eiffle 65."
            ,
            }

        ,   [ indigo ] = new List< string >()
            {
                "Isaac Asimov thinks indigo is a stupic color."
            ,   "Indigo is basically violet with less red in it."
            ,   "Indigo would make more sense to people if their ocular nerves didn't confuse red and blue."
            ,
            }

        ,   [ violet ] = new List< string >()
            {
                "Violets are blue. (Told you it was coming!)"
            ,   "Violet is yet another color of British royalty."
            ,   "Violet is commonly called purple, which is the best flavor of all juices."
            ,
            }
        ,   
        };

        // Color-choice menu.
        private static SuperMenu menuColors = new SuperMenu
        (
            "", "That's not an available color.  Try again."
        )
        {
            new ActionOnlyOption( red,    () => DisplayColorFacts( red    ) )
        ,   new ActionOnlyOption( orange, () => DisplayColorFacts( orange ) )
        ,   new ActionOnlyOption( yellow, () => DisplayColorFacts( yellow ) )
        ,   new ActionOnlyOption( green,  () => DisplayColorFacts( green  ) )
        ,   new ActionOnlyOption( blue,   () => DisplayColorFacts( blue   ) )
        ,   new ActionOnlyOption( indigo, () => DisplayColorFacts( indigo ) )
        ,   new ActionOnlyOption( violet, () => DisplayColorFacts( violet ) )
        ,   
        };

        // Tell the user all about this color.
        private static void DisplayColorFacts( string color )
        {
            
        }

        public static void Main( string[] args )
        {
            // Clear the Console for nicer output.
            Console.Clear();

            // Get the user's favorite color.
            menuColors.Prompt = "What's your favorite color?";
            string favorite = menuColors.Run();

            // Tell the user about his/her favorite color.
            DisplayColorFacts( favorite );
            
            menuColors.Remove( menuColors );
        }
    }
}
