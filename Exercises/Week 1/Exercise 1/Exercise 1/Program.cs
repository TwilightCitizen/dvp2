using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Terminal;

namespace Exercise_1
{
    public class Program
    {
        // Topic matters for November are Clothing Brands, NOT SORTED.
        // Might be better handled pulling from a file, but such a
        // simple list hardly demands the effort and potential pitfalls.
        // For that matter, a Dictionary of Lists Keyed on Month could
        // take care of ALL Topic Matters.
        private static List< string > topicMatters = new List< string >()
        {
            "Levis",                 "Tommy Hilfiger"
        ,   "Dockers",               "Nautica"
        ,   "Abercrombie & Fitch",   "Polo"
        ,   "Aeropostale",           "Gap"
        ,   "Lululemon",             "Amazon Basics"
        ,   "Ralph Lauren",          "Lucky Brand"
        ,   "Betabrand",             "Structure"
        ,   "Fubu",                  "Jordache"
        ,   "True Religion",         "Ecko Unlimited"
        ,   "Lacoste",               "Hugo Boss"
        };

        // Set up the console menu with the default prompt and error
        // message, and populate it with appropriate options.  The
        // requirements for this exercise are minimal, so this will
        // not showcase all it's able to do.
        private static SuperMenu menuMain = new SuperMenu()
        {
            // An Action-Only Option takes a string for the option
            // to be displayed in the console, and an Action to run
            // when the option is chosen by the user.
            new ActionOnlyOption( "Show the Topic Matters"
                                , ShowTopicMatters )

        ,   new ActionOnlyOption( "Sort the Topic Matters Alphabetically"
                                , SortTopicMatters )

        ,   new ActionOnlyOption( "Sort the Topic Matters in Reverse"
                                , ReverseTopicMatters )

        ,   new ActionOnlyOption( "Remove a Random Topic Matter"
                                , RemoveRandomTopicMatter )

            // Because you said to have fun with it, let's add a few
            // more options.  How about an un-sort to go with undo?

        ,   new ActionOnlyOption( "Shuffle the Topic Matters"
                                , ShuffleTopicMatters )

            // This options gets to show off a little bit.  Conditions is an
            // optional list of kinda-predicates that get called anytime
            // the menu is about to be displayed.  If they ALL signal true,
            // then the menu option is enabled.  Othewise, it's disabled.
            // In this case, we only need to check that the undoStack isn't
            // empty to avoid a stack underflow exception.
        ,   new ActionOnlyOption( "Undo the Last Action"
                                , Undo
                                , conditions : new List< Func< bool > >()
                                               {
                                                   () => undoStack.Count > 0
                                               } )

            // This one just encapsulates Environment.Exit(), otherwise,
            // just like an Action-Only Option.
        ,   new ExitOption()
        };

        // We'll need this to push the list onto before any changes are made to it.
        // Then, if the user wants to undo, we just pop the last one off and overwrite
        // the current list with the last one.  This is protected by a not-empty check,
        // so the user can't undo something that wasn't done and pop an empty stack.
        private static Stack< List< string > > undoStack = new Stack< List< string > >();

        // Write all the Topic Matters to the console.
        private static void ShowTopicMatters() =>
            // Write all the Topic Matters to the console.  A
            // loop and console output all rolled up into one. :)
            topicMatters.ForEach( tm => Console.WriteLine( tm ) );

        // Sort all the Topic Matters in alphabetical order, first pushing them
        // to the Undo stack so it can be undone later.
        private static void SortTopicMatters()
        {
            undoStack.Push( topicMatters );
            // List.Sort() defaults to ascending alphabetic sorting.
            topicMatters.Sort();
        }

        // Sort all the Topic Matter in reverse alphabetical order, first pushing
        // them to the Undo stack for undo later.
        private static void ReverseTopicMatters()
        {
            undoStack.Push( topicMatters );
            // We can't just reverse the Topic Matters assuming the user first
            // sorted them, so just do a proper reverse-alphabetical sort.
            topicMatters.Sort();
            topicMatters.Reverse();
        }

        // Removes a random Topic Matter using a LOOP.  Really, this sounds
        // like a job for a bounded pseudo-random number, but can can capture
        // the seconds on the clock, loop through the list that many times,
        // and just delete the Topic Mater we land on.  Also, we make sure to
        // prepare for an Undo.
        private static void RemoveRandomTopicMatter()
        {
            undoStack.Push( topicMatters );


        }

        private static void ShuffleTopicMatters()
        {

        }

        private static void Undo()
        {

        }

        // Program entry point.
        public static void Main( string[] args )
        {
            // Run the menu until the user Exits.  We'll clear the
            // screen and give the user a pause after choosing an
            // option so the output doesn't flit by unseen.
            while( true)
            {
                Console.Clear();
                menuMain.Run();
                // Let the user know we're paused here.
                Console.WriteLine( "\nPress any key to continue... " );
                Console.ReadKey();
            }
        }
    }
}
