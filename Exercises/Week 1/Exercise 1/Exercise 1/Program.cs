/* Name:        David A. Clark, Jr.
 * Class:       MDV2229-O
 * Assignment:  1.4 - Coding Exercise - Exercise 1
 * Date:        2018-11-24
 * 
 * Tasks:       Present the user with a menu-driven console application to perform
 *              various operations on an unsorted list of 20 Topic Matters.
 *              
 * Note:        Uses SuperMenu, and various MenuOption subclasses from referenced
 *              Utilities class library.
 */

using System;
using System.Collections.Generic;
using System.Linq;
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
            // more options.

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
                                                   () => undoStack.Any()
                                               } )

        ,   new ActionOnlyOption( "Redo the Last undone Action"
                                , Redo
                                , conditions : new List< Func< bool > >()
                                               {
                                                   () => redoStack.Any()
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

        // This is basically for the same purposes of undo, to undo an undo.
        private static Stack< List< string > > redoStack = new Stack< List< string > >();

        // Write all the Topic Matters to the console.
        private static void ShowTopicMatters() =>
            // Write all the Topic Matters to the console.  A
            // loop and console output all rolled up into one. :)
            topicMatters.ForEach( tm => Console.WriteLine( tm ) );

        // Sort all the Topic Matters in alphabetical order, first pushing them
        // to the Undo stack so it can be undone later, clearing the Redo stack.
        private static void SortTopicMatters()
        {
            PrepUndoRedo();

            // List.Sort() defaults to ascending alphabetic sorting.
            topicMatters.Sort();

            // Let the user know the sort is done.
            Console.WriteLine( "\nThe Topic Matters have been sorted in alphabetical order." );
        }

        // Sort all the Topic Matter in reverse alphabetical order, first pushing
        // them to the Undo stack for undo later, clearing the Redo stack.
        private static void ReverseTopicMatters()
        {
           PrepUndoRedo();

            // We can't just reverse the Topic Matters assuming the user first
            // sorted them, so just do a proper reverse-alphabetical sort.
            topicMatters.Sort();
            topicMatters.Reverse();

            // Let the user know the reverse sort is done.
            Console.WriteLine( "\nThe Topic Matters have been sorted in reverse alphabetical order." );
        }

        // Removes a random Topic Matter using a LOOP.  Really, this sounds
        // like a job for a bounded pseudo-random number, but we can capture
        // the seconds on the clock, loop through the list that many times,
        // modulo the length of the list, and just delete the Topic Mater we
        // land on.  Also, we make sure to prepare for an Undo and Redo.
        private static void RemoveRandomTopicMatter()
        {
            PrepUndoRedo();

            // For capturing the topic matter we stop at outside
            // of the loop's scope.
            string topicMatter = null;

            // Loop through the topicMatters, as many times as seconds are
            // in the current time, modulo length of the list.
            for( int i = 0; i < DateTime.Now.Second % topicMatters.Count; i++ )
                topicMatter = topicMatters[ i ];

            // Delete the last capured topic matter.
            topicMatters.Remove( topicMatter );

            // Let the user know the random delete is done.
            Console.WriteLine( "\nA random Topic Matter has been deleted." );
        }

        // Partition the topic matters into two approximately equal halves,
        // and recombine them into shuffled list by taking one off each half
        // at a time, of course first preparing for an Undo and Redo.
        // Not the most efficient shuffle in the world, but at the very
        // least, it's intuitive to follow.
        private static void ShuffleTopicMatters()
        {
            PrepUndoRedo();

            // Split the Topic Matters into two approximate halves.
            var leftHalf  = new Stack< string >( topicMatters.Take( topicMatters.Count / 2 ) );
            var rightHalf = new Stack< string >( topicMatters.Skip( topicMatters.Count / 2 ) );

            // Don't want to shuffle the Topic Matters into existing ones...
            topicMatters = new List< string >();

            // Insert one off each half into Topic Matters, alternating
            // until they're all gone.
            while( leftHalf.Any() && rightHalf.Any() )
            {
                topicMatters.Add( rightHalf.Pop() );
                topicMatters.Add( leftHalf.Pop()  );
            }

            // The right half could have a Topic Matter left over in the case
            // of an odd number of Topic Matters.
            if( rightHalf.Any() ) topicMatters.Add( rightHalf.Pop() );

            // Let the user know the shuffle is done.
            Console.WriteLine( "\nThe Topic Matters have been shuffled." );
        }

        // Push a copy of the Topic Matters onto the Undo stack and clear
        // the Redo stack.  A copy is necessary since lists are ref types.
        private static void PrepUndoRedo()
        {
            undoStack.Push( new List< string >( topicMatters ) );
            redoStack.Clear();
        }

        // Undo just pops the last pushed Topic Matters list from the undo
        // stack into the Topic Matters.  To support redoing the undone
        // action, it pushes the current Topic Matters to the redoStack.
        private static void Undo()
        {
            redoStack.Push( new List< string >( topicMatters ) );
            topicMatters = undoStack.Pop();

            // Let the user know the last action is undone.
            Console.WriteLine( "\nThe Topic Matters have been restored to their previous state." );
        }

        // Undo the undo.  This can only be done if something hasn't been done
        // after an undo. 
        private static void Redo()
        {
            undoStack.Push( new List< string >( topicMatters ) );
            topicMatters = redoStack.Pop();

            // Let the user know the last undone action is redone.
            Console.WriteLine( "\nThe Topic Matters have been restored to their previous undone state." );
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
