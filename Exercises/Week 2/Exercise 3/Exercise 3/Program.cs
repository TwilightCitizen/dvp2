using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // Get the new Percent Grade from the user and update the Taken Course with it.
            // Finally, this demonstrates the utility of PromptFor< T > from my utilities
            // class library.  PromptFor< T > takes as arugments a prompt to display to the user,
            // an error prompt to display in case of bad input, a delegate expression with which
            // to test for constraints beyond type, and of course, the type to which the user's
            // input should be converted from string.
            course.Grade.Value = PromptFor< decimal >
            (
                $"What is the new percent grade for {name}'s {course.Name}?\n"
            +   "This must be between 0.00 and 100.00 percent.  Do not include\n"
            +   "a percent symbol - (\"%\")."
            ,   "That is not a valid percent grade.  Try again."
            ,   input => input >= PercentGrade.MinPercent
                      && input <= PercentGrade.MaxPercent 
            );

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
