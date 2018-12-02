using System;
using System.Collections.Generic;
using System.Linq;
using Utilities.Terminal;

using static Utilities.Terminal.IO;

// Aliases to shorten some declarations.
using TC = Exercise_3.TakenCourse;
using PG = Exercise_3.PercentGrade;

namespace Exercise_3
{
    public class Program
    {
        // Hardcoded list of Students with thier TakenCourses
        private static List< Student > students = new List< Student >()
        {
            new Student
            (
                "David"
            ,   "Clark"
            ,   new List< TakenCourse >()
                {
                    new TC( "Blue Jeans of the World",       new PG( 98.6M ) )
                ,   new TC( "Yoga Fashion 101",              new PG( 89.9M ) )
                ,   new TC( "Metal T-Shirts in Pop Culture", new PG( 100M  ) )
                ,   new TC( "Animal Parts in Fashion",       new PG( 74.9M ) )
                ,   new TC( "Trends in Leather",             new PG( 94.1M ) )
                }
            )

        ,   new Student
            (
                "Fascia"
            ,   "Nista"
            ,   new List< TakenCourse >()
                {
                    new TC( "T-Shirts of the World",         new PG( 99.9M ) )
                ,   new TC( "Night Club Fashion 101",        new PG( 100M  ) )
                ,   new TC( "Leather Pants in Pop Culture",  new PG( 100M  ) )
                ,   new TC( "Michael Jackson Jackets",       new PG( 100M  ) )
                ,   new TC( "Trends in Vegan Clothing",      new PG( 99.8M ) )
                }
            )

        // This one should have one of each letter grade.
        ,   new Student
            (
                "Basic"
            ,   "Person"
            ,   new List< TakenCourse >()
                {
                    new TC( "Fashion after Labor Day",       new PG( 89.5M ) )
                ,   new TC( "Shoes for All Occassions",      new PG( 79.5M ) )
                ,   new TC( "Sunglasses of North America",   new PG( 72.5M ) )
                ,   new TC( "How To Make Buttonups",         new PG( 69.5M ) )
                ,   new TC( "Emerging Fashion Materials",    new PG( 59.5M ) )
                }
            )

        ,   new Student
            (
                "Dez"
            ,   "Ayner"
            ,   new List< TakenCourse >()
                {
                    new TC( "Fur Undergarments 201",         new PG( 98.9M ) )
                ,   new TC( "Plastics in Fashion",           new PG( 100M  ) )
                ,   new TC( "Dressing Models & Stars",       new PG( 100M  ) )
                ,   new TC( "Concepts in Retail Design",     new PG( 100M  ) )
                ,   new TC( "Advanced Fabric Treatments",    new PG( 97.4M ) )
                }
            )

        ,   new Student
            (
                "Last"
            ,   "Student"
            ,   new List< TakenCourse >()
                {
                    new TC( "Fur Undergarments 201",         new PG( 98.9M ) )
                ,   new TC( "Plastics in Fashion",           new PG( 100M  ) )
                ,   new TC( "Dressing Models & Stars",       new PG( 100M  ) )
                ,   new TC( "Concepts in Retail Design",     new PG( 100M  ) )
                ,   new TC( "Advanced Fabric Treatments",    new PG( 97.4M ) )
                }
            )
        };

        /* These don't work as expected for this use case.  More below in comments.
         
        // Stacks for Undo/Redo Support.  Somewhere, there's an abstraction
        // brewing to handle this more cleanly.
        private static Stack< List< Student > > undos = new Stack< List< Student > >();
        private static Stack< List< Student > > redos = new Stack< List< Student > >(); */

        private static Stack< ( Action, Action ) > opUndos = new Stack< ( Action, Action ) >();
        private static Stack< ( Action, Action ) > opRedos = new Stack< ( Action, Action ) >();

        // Menu of Students to Review
        private static SuperMenu menuStudents = new SuperMenu
        (
            "" , "That is not an available student.  Try again."
        );

        // Main Menu
        private static SuperMenu menuMain = new SuperMenu()
        {
            new ActionSubMenuOption(
                "Review Students",      LoadStudentsReview,   menuStudents
            )

        ,   new ActionSubMenuOption(
                "Edit Students",        LoadStudentsEdit,     menuStudents
            )
        
        /* Yikes! Undo as originally implemented in the previous exercise
         * doesn't work so easily here. More information below in the
         * implementation comments
         * 
        ,   new ActionOnlyOption( "Undo the Last Action"
                                , Undo
                                , conditions : new List< Func< bool > >()
                                               {
                                                   () => undos.Any()
                                               } )

        ,   new ActionOnlyOption( "Redo the Last undone Action"
                                , Redo
                                , conditions : new List< Func< bool > >()
                                               {
                                                   () => redos.Any()
                                               } ) */

        // Setup for more workable--I hesitate to say better--undo.
        ,   new ActionOnlyOption( "Undo the Last Action"
                                , OpUndo
                                , conditions : new List< Func< bool > >()
                                               {
                                                    () => opUndos.Any()
                                               } )

        ,   new ActionOnlyOption( "Redo the Last Undone Action"
                                , OpRedo
                                , conditions : new List< Func< bool > >()
                                               {
                                                    () => opRedos.Any()
                                               } )

        ,   new ActionOnlyOption(
                "View Students GPAs",   ViewGPAs
            )

        ,   new ExitOption()
        };

        // Loads the Student Menu with Students to Review.
        private static void LoadStudentsReview()
        {
            // Clear out the Student Menu.  Not strictly necessary in
            // this app, but would be, were it able to add or remove
            // students.
            menuStudents.Clear();
            menuStudents.Prompt = "Which student would you like to review?";

            // Load all the Student into the Student Menu, wired up
            // for Review upon user selection.
            students.OrderBy( o => o.SortName )
                    .ToList()
                    .ForEach( s => 
                        menuStudents.Add(
                            new ActionOnlyOption(
                                s.SortName
                            ,   () => ReviewStudent( s )
                            ) ) );

            // Give the user a cancel option.
            menuStudents.Add( new CancelOption() );
        }

        // Loads the Stuent Menu with Students to Edit.
        private static void LoadStudentsEdit()
        {
            // Clear out the Student Menu.  Not strictly necessary in
            // this app, but would be, were it able to add or remove
            // students.
            menuStudents.Clear();
            menuStudents.Prompt = "Which student would you like to edit?";

            // Load all the Students into the Student Menu, wired up
            // with a submenu to edit their Taken Courses upon user selection.
            students.OrderBy( o => o.SortName )
                    .ToList()
                    .ForEach( s => {
                        var menu = new SuperMenu( 
                            $"Which course would you like to edit for {s.FullName}?"
                        ,   "That is not an available course"
                        );

                        // Load all the student's courses into the submenu for Editing.
                        s.TakenCourses.ForEach( tc => 
                            menu.Add( new ActionOnlyOption( 
                                tc.Name, () => EditCourse( s.FullName, tc )
                            ) )
                        );

                        // Need a Cancel option for each student's submenu.
                        menu.Add( new CancelOption() );

                        // Add the student's submenu as the Student Menu action.
                        menuStudents.Add(
                            new SubMenuOnlyOption(
                                s.SortName, menu ) ); 
                    } );

            // Give the user a Cancel option.
            menuStudents.Add( new CancelOption() );
        }

        // Displays Student Information to Console.
        private static void ReviewStudent( Student student )
        {
            // Clear the Console for nicer output.
            Console.Clear();

            // Display the Student's Name
            Console.WriteLine( $"Name: {student.FullName}\n" );

            // Display a Header for Course Details.
            Console.WriteLine( "Courses [Course Name - Letter Grade (Percent Grade)]:\n" );

            // Display details of all the Student's Taken Courses.
            student.TakenCourses.ForEach( tc =>
                // Course Name and Grades
                Console.WriteLine( $"{tc.Name} - {tc.Grade.Letter} ({tc.Grade.Value}%)" )
            );
        }

        // Prompt the user for the Student's new Percent Grade for the given course.
        private static void EditCourse( string name, TakenCourse course )
        {
            // This is the only destructive operation to the student's information, which
            // can benefit from undo/redo-ability.
            // PrepUndoRedo(); // Doesn't work as envisioned. More in implementation comments.

            // Get the new Percent Grade from the user and update the Taken Course with it.
            // Finally, this demonstrates the utility of PromptFor< T > from my utilities
            // class library.  PromptFor< T > takes as arugments a prompt to display to the user,
            // an error prompt to display in case of bad input, a delegate expression with which
            // to test for constraints beyond type, and of course, the type to which the user's
            // input should be converted from string.
            decimal newGrade = PromptFor< decimal >
            (
                $"What is the new percent grade for {name}'s {course.Name}?\n"
            +   "This must be between 0.00 and 100.00 percent.  Do not include\n"
            +   "a percent symbol - (\"%\")."
            ,   "That is not a valid percent grade.  Try again."
            ,   input => input >= PercentGrade.MinPercent
                      && input <= PercentGrade.MaxPercent 
            );

            // This is a little different than originally planned.  The previous statement once
            // updated course.Grade.Value directly, but we can't do this with the operative undo
            // and redo facilities.  So, we capture the new grade, capture the old grade, and
            // wrap them in closures along with the current references to course.Grade.Value that
            // can be played at any time to affect the value.
            decimal oldGrade = course.Grade.Value;

            // Closures.
            Action OldGrade = () => { course.Grade.Value = oldGrade; };
            Action NewGrade = () => { course.Grade.Value = newGrade; };

            // Prepare to undo/redo this change.
            OpPrepUndoRedo( OldGrade, NewGrade );

            // Then update the Grade in earnest.
            NewGrade();

            // That whole sequence to prepare for Undo/Redo without worrying about Deep Copying
            // objects/collections has it's own considerations for abstraction.  As it stands
            // right now, this is a thorny cross-cutting concern, but it should work.

            // Let the user know the edit happened.
            Console.WriteLine( $"{name}'s {course.Name} has been updated to {course.Grade.Value}%" );
        }

        private static void ViewGPAs()
        {
            // Clear the Console for nicer output.
            Console.Clear();

            // Display GPAs of all the Students.
            students.OrderBy( o => o.SortName )
                    .ToList()
                    .ForEach( s => 
                        Console.WriteLine( $"{s.SortName} - {s.TakenCourses.ToGPA()} GPA" ) );
        }

        // Pause after Console output until user continues.
        private static void Pause()
        {
            // Let the user know to hit a key to continue.
            Console.WriteLine( "\nPress any key to continue..." );
            Console.ReadKey();
        }

        /* So... Merely copying students into a new list before adding it
         * to the undo/redo stack does create new Student objects with their
         * own references.  BUT(!) its a shallow copy.  These new Student
         * objects still carry references to the underlying objects, namely
         * the Taken Courses.  Even worse!  C# has no native way to do a
         * Deep Copy.  Nope... You have to use serialization or reflection
         * within an extension method to come close.  This is terribly
         * lacking.
         * 
         * One way I see around this is to have an undo stack onto which
         * Actions are pushed by the programmer that operatively specify
         * how to undo the action about to be done.  I'm going to leave
         * this commented in to demonstrate learning and code in the
         * other way to do undo to see if that works.
         
        // Push a copy of the Student onto the Undo stack and clear
        // the Redo stack.  A copy is necessary since lists are ref types.
        private static void PrepUndoRedo()
        {
            undos.Push( new List< Student >( students ) );
            redos.Clear();
        }

        // Undo just pops the last pushed Student list from the undo
        // stack into Students.  To support redoing the undone
        // action, it pushes the current Students to the redoStack.
        private static void Undo()
        {
            redos.Push( new List< Student >( students ) );
            students = undos.Pop();

            // Let the user know the last action is undone.
            Console.WriteLine( "\nThe students have been restored to their previous state." );
        }

        // Undo the undo.  This can only be done if something hasn't been done
        // after an undo. 
        private static void Redo()
        {
            undos.Push( new List< Student >( students ) );
            students = redos.Pop();

            // Let the user know the last undone action is redone.
            Console.WriteLine( "\nThe students have been restored to their previous undone state." );
        } */

        // Push the actions onto the undo stack that restore to the current state,
        // and affect the projected state.
        private static void OpPrepUndoRedo( Action current, Action projected )
        {
            opUndos.Push( ( current, projected ) );
            opRedos.Clear();
        }

        // Pop the actions off the undo stack that restore the previous state and
        // redo the current state. Redo is an undo of the undo, so the reverse
        // gets pushed to the redo stack. Finally, restore the previous state
        // by executing the action that restores it.
        private static void OpUndo()
        {
            var ( previous, current ) = opUndos.Pop();
            opRedos.Push( ( current, previous ) );
            previous();
        }

        // This essentially does the same as undo, except opUndos and opRedos
        // trade roles.  Technically, looking at the two methods, these could
        // be further abstracted, but the resulting code would be confusing as
        // all get out.
        private static void OpRedo()
        {
            var ( previous, current ) = opRedos.Pop();
            opUndos.Push( ( current, previous ) );
            previous();
        }

        // Program entry point.
        public static void Main( string[] args )
        {
            // Display the Main Menu until the user exits.
            while( true )
            {
                // Clear the Console for nicer output.
                Console.Clear();

                menuMain.Run();
                Pause();
            }
        }
    }
}
