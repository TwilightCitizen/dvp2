/* Name:        David A. Clark, Jr.
 * Class:       MDV2229-O
 * Assignment:  2.3 - Time Tracker Application
 * Date:        2018-12-02
 * 
 * Tasks:       Time Tracker App is a database-connected console application that
 *              allows users to login and/or register an account with which he or
 *              she can track time spent on activities throughout the day with
 *              greater functionality than the Google Sheets time tracker first
 *              used by students in this course.
 *              
 * Note:        Uses SuperMenu, various MenuOption subclasses, and PromptFor<T>
 *              from referenced Utilities class library, and MySQL libraries.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Terminal;
using static Utilities.Terminal.IO;
using MySql.Data.MySqlClient;

namespace TimeTracker
{
    public class Program
    {
        // Connection String to Database.
        private const string cs = "server=192.168.121.1;"
                                + "userid=dbremoteuser;"
                                + "password=password;"
                                + "database=DavidClark_MDV229_Database_201811;"
                                + "port=8889";

        // The logged in user, if any.
        private static User loggedIn = null;

        // Dictionaries can take care of many lookups.
        private static Dictionary< string,   int > categories = new Dictionary< string,   int >();
        private static Dictionary< string,   int > activities = new Dictionary< string,   int >();
        private static Dictionary< double,   int > durations  = new Dictionary< double,   int >();
        private static Dictionary< string,   int > weekdays   = new Dictionary< string,   int >();
        private static Dictionary< DateTime, int > dates      = new Dictionary< DateTime, int >();
        private static Dictionary< int,      int > days       = new Dictionary< int,      int >();

        // Options for Main Menu.
        private static MenuOption optRegister   = new ActionOnlyOption
        (
            "Register a New Account"
        ,   Register
        ,   conditions : new List< Func< bool >>()
            {
                () => loggedIn == null
            }
        );

        private static MenuOption optLogin      = new ActionOnlyOption
        ( 
            "Login to Your Account"
        ,   Login
        ,   conditions : new List< Func< bool > >()
            {
                () => loggedIn == null
            }
        );

        private static MenuOption optLogout     = new ActionOnlyOption
        ( 
            "Logout of Your Account"
        ,   Logout
        ,   conditions : new List< Func< bool > >()
            {
                () => loggedIn != null
            }
        );

        private static MenuOption optProfile    = new ActionOnlyOption
        (
            "View or Edit Your Profile"
        ,   Profile
        ,   conditions : new List< Func< bool > >()
            {
                () => loggedIn != null
            }
        );

        private static MenuOption optLookups    = new ActionOnlyOption
        (
            "View or Edit Lookup Tables"
        ,   Lookups
        ,   conditions : new List< Func< bool > >()
            {
                () => loggedIn != null
            }
        );

        private static MenuOption optLog        = new ActionOnlyOption
        (
            "View or Edit Your Activity Log"
        ,   Log
        ,   conditions : new List< Func < bool > >()
            {
                () => loggedIn != null
            }
        );

        private static MenuOption optAddEntry   = new ActionOnlyOption
        (
            "Add an Entry to Your Activity Log"
        ,   AddEntry
        ,   conditions : new List< Func < bool > >()
            {
                () => loggedIn != null
            }
        );

        private static MenuOption optExit       = new ExitOption();

        // Main Menu.
        private static SuperMenu menuMain       = new SuperMenu()
        {
            optRegister
        ,   optLogin
        ,   optLogout
        ,   optProfile
        ,   optLog
        ,   optAddEntry
        ,   optLookups
        ,   optExit
        };

        // Options for Profile Menu.
        
        private static MenuOption optFirstName  = new ActionOnlyOption
        (
            "Change Your First Name"
        ,   ChangeFirstName
        );

        private static MenuOption optLastName   = new ActionOnlyOption
        (
            "Change Your Last Name"
        ,   ChangeLastName
        );

        private static MenuOption optPassword   = new ActionOnlyOption
        (
            "Change Your Password"
        ,   ChangePassword
        );

        private static MenuOption optReturn     = new CancelOption
        (
            "Return to the Main Menu."
        );

        // Profile Menu.
        private static SuperMenu menuProfile    = new SuperMenu()
        {
            optFirstName
        ,   optLastName
        ,   optPassword
        ,   optReturn
        };

        // Options for Lookup Menu.

        private static MenuOption optCategories = new ActionOnlyOption
        (
            "Categories"
        ,   Categories
        );

        private static MenuOption optActivities = new ActionOnlyOption
        (
            "Activities"
        ,   Activities
        );

        // Lookup Menu.
        private static SuperMenu menuLookups    = new SuperMenu
        (
            "Which Lookup Table would you like to View or Edit?"
        )
        {
            optCategories
        ,   optActivities
        ,   optReturn
        };

        // Options for Lookup View Edit Submenus

        private static MenuOption optAddNew;
        private static MenuOption optGoBack = new CancelOption
        (
            "Return to the previous screen."
        );

        // Submenus for Lookup View or Edit.

        private static SuperMenu menuCategories = new SuperMenu
        (
            "Which Category would you like to View or Edit?"
        );

        private static SuperMenu menuActivities = new SuperMenu
        (
            "Which Activity would you like to View or Edit?"
        );

        // Log Day Menu.
        private static SuperMenu menuLogDays    = new SuperMenu
        (
            "Which Day of your Activity Log do you want to View or Edit?"
        );

        private static SuperMenu menuLogEntries = new SuperMenu
        (
            "Which Entry for this Day do you want to View or Edit?"
        );

        // Entry Menu.
        private static SuperMenu menuEntry      = new SuperMenu
        (
            "Which Field would you like to Edit?"
        );

        // Prompt the user for the User ID and Password to attempt
        // logging in.  Note that Login presently sends Pasword in
        // the clear because the Time Tracker database was prescribed
        // to be built with a password field too short to store a
        // secure hash.  Plus, we don't know how to establish
        // secure connections with public key encryption yet.
        // Successful login populates the user's Profile and Activity
        // log.
        private static void Login()
        {
            // Clear the console for nicer output.
            Console.Clear();

            // Get the user's ID.
            var id = PromptFor< int    >( "Enter your User ID Number."
                                        , "That does not appear to be a valid User ID Number.  Try again."
                                        , num => num > 0 );

            // Clear the console for nicer output.
            Console.Clear();

            // Get the user's Password.
            var pw = PromptFor< string >( "Enter your Password."
                                        , "The Password cannot be blank."
                                        , any => !string.IsNullOrEmpty( any ) );

            // Attempt to log the user in.
            // Nested using blocks are clearer and safer than trying
            // to free disposable resources in a try/catch/finally.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    // Users where ID and PW match user input.
                    cmd.CommandText = "select * from time_tracker_users "
                                    + "where user_id = @ID "
                                    + "and user_password = @PW";

                    // Look mom! No SQL Injection Attacks!
                    cmd.Parameters.AddWithValue( "@ID", id );
                    cmd.Parameters.AddWithValue( "@PW", pw );

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        if( rdr.HasRows )
                        {
                            // Login was successfull.
                            rdr.Read();

                            // Set up the logged in user.
                            loggedIn = new User();

                            loggedIn.ID        = id;
                            loggedIn.Password  = pw;
                            loggedIn.FirstName = rdr[ "user_firstname" ].ToString();
                            loggedIn.LastName  = rdr[ "user_lastname"  ].ToString();

                            LoadLog();

                            // Let the user know login was successfull.
                            Console.WriteLine( $"Welcome, {loggedIn.FirstName}!" );
                        }
                        else
                        {
                            // Login was unsuccessfull.
                            Console.WriteLine( "No user was found with that username and password.\n"
                                             + "Try logging in again or registering from the main menu." );
                        }
                    }
                }
            }
        }

        // Loads the user's Activity Log.  Could have been
        // done in Login, but wanted to keep the method
        // from getting far too long.
        private static void LoadLog()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    // Log entries where User ID matches Logged In User.
                    cmd.CommandText = "select * from activity_log "
                                    + "where user_id = @ID ";

                    // Technically don't need this now, but good habit.
                    // Why leave it to chance, you know?
                    cmd.Parameters.AddWithValue( "@ID", loggedIn.ID );

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        // Add the Activity Log Entries to the Logged In
                        // User's Log.
                        while( rdr.Read() )
                        {
                            var entry = new LogEntry();

                            /*entry.ID         = int.Parse( rdr[  "id"                    ].ToString() );
                            entry.UserID     = int.Parse( rdr[  "user_id"               ].ToString() );
                            entry.DayID      = int.Parse( rdr[ "calendar_day"           ].ToString() );
                            entry.DateID     = int.Parse( rdr[ "calendar_date"          ].ToString() );
                            entry.WeekdayID  = int.Parse( rdr[ "day_name"               ].ToString() );
                            entry.CategoryID = int.Parse( rdr[ "category_description"   ].ToString() );
                            entry.ActivityID = int.Parse( rdr[ "activity_description"   ].ToString() );
                            entry.DurationID = int.Parse( rdr[ "time_spent_on_activity" ].ToString() );*/

                            entry.ID         = rdr[  "id"                    ] as int? ?? null;
                            entry.UserID     = rdr[  "user_id"               ] as int? ?? null;
                            entry.DayID      = rdr[ "calendar_day"           ] as int? ?? null;
                            entry.DateID     = rdr[ "calendar_date"          ] as int? ?? null;
                            entry.WeekdayID  = rdr[ "day_name"               ] as int? ?? null;
                            entry.CategoryID = rdr[ "category_description"   ] as int? ?? null;
                            entry.ActivityID = rdr[ "activity_description"   ] as int? ?? null;
                            entry.DurationID = rdr[ "time_spent_on_activity" ] as int? ?? null;

                            loggedIn.Log.Add( entry );
                        }
                    }
                }
            }
        }

        // Logging out is easy.
        private static void Logout()
        {
            // Say bye to the user.
            Console.WriteLine( $"Goodbye, {loggedIn.FirstName}!" );
            
            loggedIn = null;
        }

        // Prompt the user for a First Name, Last Name, and Password,
        // generate a unique User ID, and write it to the database.
        // This does NOT log the user in.
        // This user registration is a bit weirder than most, since
        // the Time Tracker Users table has no concept of a User Name
        // chosen by the user.  Having the user pick a unique User ID
        // Number and checking that it doesn't already exist would
        // be even weirder still, so we just do it for them.
        private static void Register()
        {
            // Clear the console for nicer output.
            Console.Clear();

            // Get the user's First Name.
            var fn = PromptFor< string >( "Enter your First Name."
                                        , "Your First Name cannot be blank.  Try again."
                                        , any => !string.IsNullOrEmpty( any ) );

            // Clear the console for nicer output.
            Console.Clear();

            // Get the user's Last Name.
            var ln = PromptFor< string >( "Enter your Last Name."
                                        , "Your Last Name cannot be blank.  Try again."
                                        , any => !string.IsNullOrEmpty( any ) );
            // Clear the console for nicer output.
            Console.Clear();

            // Get the user's Password.
            var pw = PromptFor< string >( "Enter your Password, and make sure you remember it.\n"
                                        + "You will need it any time you log into the app!"
                                        , "The Password cannot be blank."
                                        , any => !string.IsNullOrEmpty( any ) );
            // Clear the console for nicer output.
            Console.Clear();

            // Set the user's ID up.
            var id = NextUserID();

            // Tell the user about his or her User ID,
            Console.WriteLine( $"Your User ID is {id}.  Make sure you remember it.\n"
                             + "You will need it along with your password to log in." );

            Pause();

            // Write the new user to the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "insert into time_tracker_users"
                                    + "( user_id"
                                    + ", user_password"
                                    + ", user_firstname"
                                    + ", user_lastname ) "
                                    + "values"
                                    + "( @ID"
                                    + ", @PW"
                                    + ", @FN"
                                    + ", @LN )";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@ID", id );
                    cmd.Parameters.AddWithValue( "@PW", pw );
                    cmd.Parameters.AddWithValue( "@FN", fn );
                    cmd.Parameters.AddWithValue( "@LN", ln );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know registration succeeded.
                    Console.WriteLine( $"Congratulations, {fn}...  Your account is registered!\n"
                                     + "You can now login from the main menu." );
                }
            }
        }

        // Finds the highest User ID Number in Time Tracker Users
        // and returns the next highest number.
        private static int NextUserID()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    // Users where ID and PW match user input.
                    cmd.CommandText = "select max( user_id ) as ID from time_tracker_users";

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        if( rdr.HasRows )
                        {
                            // Still need this check in the case that
                            // there are zero registered users in here.
                            rdr.Read();

                            return int.Parse( rdr[ "ID" ].ToString() ) + 1;
                        }
                        else
                        {
                            // The very first user has an ID of 1.
                            return 1;
                        }
                    }
                }
            }
        }

        // Display the user's profile to the Console and present
        // a menu that lets the user change select profile items.
        private static void Profile()
        {
            // For checking when the user exits.
            string choice = null;

            // Wait until the user wants to return to the Main Menu.
            while( choice != optReturn.Text )
            {
                // Clear the console for nicer ouput.
                Console.Clear();

                // Feed the user's profile data into the menu prompt.
                string profile = "Here's your profile...\n\n"
                               + $"First Name: {loggedIn.FirstName}\n"
                               + $"Last Name:  {loggedIn.LastName}\n"
                               + $"User ID:    {loggedIn.ID}\n"
                               + $"Password:   {loggedIn.Password}\n\n"
                               + "What would you like to do?";

                menuProfile.Prompt = profile;

                // Display the Profile Edit Menu.
                choice = menuProfile.Run();
                Pause();
            }
        }

        // The next three methods share a lot of functionality that
        // could be abstracted behind a method with parameters for the
        // different elements, but it could appear convoluted. 

        // Prompt the user for a new First Name, updating the profile
        // and writing it to the database.
        private static void ChangeFirstName()
        {
            // Clear the console for nicer output.
            Console.Clear();

            // Get the user's new First Name, updating the Logged In User.
            loggedIn.FirstName = PromptFor< string >( "Enter your new First Name."
                                                    , "Your new First Name cannot be blank or more than 25 characters.  Try again."
                                                    , any => !string.IsNullOrEmpty( any ) && any.Length <= 25 );

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update time_tracker_users "
                                    + "set user_firstname = @FN "
                                    + "where user_id = @ID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@FN", loggedIn.FirstName );
                    cmd.Parameters.AddWithValue( "@ID", loggedIn.ID        );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  Your First Name is updated!" );
                }
            }
        }

        // Prompt the user for a new Last Name, updating the profile
        // and writing it to the database.
        private static void ChangeLastName()
        {
            // Clear the console for nicer output.
            Console.Clear();

            // Get the user's new Last Name, updating the Logged In User.
            loggedIn.LastName = PromptFor< string >( "Enter your new Last Name."
                                                   , "Your new Last Name cannot be blank or more than 25 characters.  Try again."
                                                   , any => !string.IsNullOrEmpty( any ) && any.Length <= 25 );

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update time_tracker_users "
                                    + "set user_lastname = @LN "
                                    + "where user_id = @ID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@LN", loggedIn.LastName );
                    cmd.Parameters.AddWithValue( "@ID", loggedIn.ID       );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  Your Last Name is updated!" );
                }
            }
        }

        // Prompt the user for a new Password, updating the profile
        // and writing it to the database.
        private static void ChangePassword()
        {
            // Clear the console for nicer output.
            Console.Clear();

            // Get the user's new Password, updating the Logged In User.
            loggedIn.Password = PromptFor< string >( "Enter your new Password."
                                                   , "Your new Password cannot be blank or more than 10 characters.  Try again."
                                                   , any => !string.IsNullOrEmpty( any ) && any.Length <= 10 );

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update time_tracker_users "
                                    + "set user_password = @PW "
                                    + "where user_id = @ID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@PW", loggedIn.Password );
                    cmd.Parameters.AddWithValue( "@ID", loggedIn.ID       );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  Your password is updated!" );
                }
            }
        }

        // Pause Console output until user continues.
        // I should probably put this in Utilities already...
        private static void Pause()
        {
            // Let the user know we're paused.
            Console.WriteLine( "\nPress any key to continue..." );
            // Wait for user to hit a key.
            Console.ReadLine();
        }

        // The following six methods also repeat a lot of code that
        // could be abstracted behind a method exposing parameters
        // for the differing parts.  As before, it could appear
        // convoluted, and the database tables these pull from
        // could change in the future, undoing the effort.

        // Put the Categories in their list to simplify lookups.
        private static void LoadCategories()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "select * from activity_categories";

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        // Insert all Categories.
                        while( rdr.Read() )
                            categories[ rdr[ "category_description" ].ToString() ] =
                                int.Parse( rdr[ "activity_category_id"].ToString() );
                    }
                }
            }
        }

        // Put the Activities in their list to simplify lookups.
        private static void LoadActivities()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "select * from activity_descriptions";

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        // Insert all Activities.
                        while( rdr.Read() )
                            activities[ rdr[ "activity_description" ].ToString() ] =
                                int.Parse( rdr[ "activity_description_id"].ToString() );
                    }
                }
            }
        }

        // Put the Durations in their list to simplify lookups.
        private static void LoadDurations()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "select * from activity_times";

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        // Insert all Durations.
                        while( rdr.Read() )
                            durations[ double.Parse( rdr[ "time_spent_on_activity" ].ToString() ) ] =
                                int.Parse( rdr[ "activity_time_id"].ToString() );
                    }
                }
            }
        }

        // Put the Weekdays in their list to simplify lookups.
        private static void LoadWeekdays()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "select * from days_of_week";

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        // Insert all Durations.
                        while( rdr.Read() )
                            weekdays[ rdr[ "day_name" ].ToString() ] =
                                int.Parse( rdr[ "day_id"].ToString() );
                    }
                }
            }
        }

        // Put the Dates in their list to simplify lookups.
        private static void LoadDates()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "select * from tracked_calendar_dates";

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        // Insert all Dates.
                        while( rdr.Read() )
                            dates[ DateTime.Parse( rdr[ "calendar_date" ].ToString() ) ] =
                                int.Parse( rdr[ "calendar_date_id"].ToString() );
                    }
                }
            }
        }

        // Put the Days in their list to simplify lookups.
        private static void LoadDays()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "select * from tracked_calendar_days";

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        // Insert all Days.
                        while( rdr.Read() )
                            days[ int.Parse( rdr[ "calendar_numerical_day" ].ToString() ) ] =
                                  int.Parse( rdr[ "calendar_day_id" ].ToString() );

                    }
                }
            }
        }

        // Shows the Lookup View or Edit menu until the
        // user chooses to go back.
        private static void Lookups()
        {
            // For checking when the user exits.
            string choice = null;

            // Wait until the user wants to return to the Main Menu.
            while( choice != optReturn.Text )
            {
                // Clear the console for nicer ouput.
                Console.Clear();

                // Display the Lookups Edit Menu.
                choice = menuLookups.Run();
                Pause();
            }
        }

        // Clear out the Categories Menu and load it fresh
        // with the Categories to Edit, including options to add a 
        // new Category or Cancel. Then run it.
        private static void Categories()
        {
            // For checking when the user exits.
            string choice = null;

            // Wait until the user wants to go back.
            while( choice != optGoBack.Text )
            {
                // Clear the console for nicer ouput.
                Console.Clear();

                // Clear the menu.
                menuCategories.Clear();

                // Insert all the Categories as menu options
                // to Edit that Category.
                categories.Keys.ToList().ForEach( key =>
                    menuCategories.Add( new ActionOnlyOption( 
                        key, () => EditCategory( key ) ) )
                );

                // Option to add new Category.
                optAddNew = new ActionOnlyOption( "Add New", AddCategory );
                menuCategories.Add( optAddNew  );

                // And, an option to Go Back.
                menuCategories.Add( optGoBack );

                // Display the Categories Edit Menu.
                choice = menuCategories.Run();
                Pause();
            }
        }

        // Update the user-selected Category with the new
        // Category Name provided by the user.
        private static void EditCategory( string category )
        {
            // Clear the Console for cleaner output.
            Console.Clear();

            // Get the ID for the Category.
            int id = categories[ category ];

            // Get the new Category Name from the user.
            string newCat = PromptFor< string >( $"What new name would you like for the category \"{category}\"?"
                                               , "The category name cannot be blank or more than 10 characters.  Try again."
                                               , cat => !string.IsNullOrEmpty( cat ) && cat.Length <= 10 );

            // Update the database accordingly.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update activity_categories "
                                    + "set category_description = @CD "
                                    + "where activity_category_id = @ID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@CD", newCat );
                    cmd.Parameters.AddWithValue( "@ID", id );

                    // We don't need rows back from this one, but this could
                    // fail if the user tries to rename to an existing Category.
                    try
                    {
                        cmd.ExecuteNonQuery();

                        // Let the user know the change succeeded.
                        Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  \"{category}\" is now \"{newCat}\"!" );

                        // Add it as the key for the ID to the dictionary,
                        // and remove the old one.
                        categories[ newCat ] = id;
                        categories.Remove( category );
                    }
                    catch
                    {
                        // Let the user know the change failed.
                        Console.WriteLine( $"Sorry... It looks like category \"{category}\" already exists!" );
                    }
                }
            }
        }

        // Prompt the user for a new Category, adding it
        // to the list and database.
        private static void AddCategory()
        {
            // Clear the Console for cleaner output.
            Console.Clear();

            // Get a new ID for the Category-to-be.
            int id = categories.Values.Max() + 1;

            // Get the new Category Name from the user.
            string newCat = PromptFor< string >( $"What name would you like for this new category?"
                                               , "The category name cannot be blank or more than 10 characters.  Try again."
                                               , cat => !string.IsNullOrEmpty( cat ) && cat.Length <= 10 );

            // Update the database accordingly.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "insert into activity_categories "
                                    + "( activity_category_id "
                                    + ", category_description ) "
                                    + "values "
                                    + "( ID"
                                    + "  CD )";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@CD", newCat );
                    cmd.Parameters.AddWithValue( "@ID", id );

                    // We don't need rows back from this one, but this could
                    // fail if the user tries to add an existing Category.
                    try
                    {
                        cmd.ExecuteNonQuery();

                        // Let the user know the add succeeded.
                        Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  \"{newCat}\" has been added!" );

                        // Add it as the key for the ID to the dictionary.
                        categories[ newCat ] = id;
                    }
                    catch
                    {
                        // Let the user know the add failed.
                        Console.WriteLine( $"Sorry... It looks like category \"{newCat}\" already exists!" );
                    }
                }
            }
        }

        // Clear out the Activities Menu and load it fresh
        // with the Activities to Edit, including options to add a 
        // new Activity or Cancel.  Then run it.
        private static void Activities()
        {
            // For checking when the user exits.
            string choice = null;

            // Wait until the user wants to go back.
            while( choice != optGoBack.Text )
            {
                // Clear the console for nicer ouput.
                Console.Clear();

                // Clear the menu.
                menuActivities.Clear();

                // Insert all the Categories as menu options
                // to Edit that Category.
                activities.Keys.ToList().ForEach( key =>
                    menuActivities.Add( new ActionOnlyOption( 
                        key, () => EditActivity( key ) ) )
                );

                // Option to add new Category.
                optAddNew = new ActionOnlyOption( "Add New", AddActivity );
                menuActivities.Add( optAddNew  );

                // And, an option to Go Back.
                menuActivities.Add( optGoBack );

                // Display the Categories Edit Menu.
                choice = menuActivities.Run();
                Pause();
            }
        }

        // Update the user-selected Category with the new
        // Category Name provided by the user.
        private static void EditActivity( string activity )
        {
            // Clear the Console for cleaner output.
            Console.Clear();

            // Get the ID for the Category.
            int id = activities[ activity ];
            
            // Get the new Category Name from the user.
            string newAct = PromptFor< string >( $"What new name would you like for the activity \"{activity}\"?"
                                               , "The activity name cannot be blank or more than 25 characters.  Try again."
                                               , act => !string.IsNullOrEmpty( act ) && act.Length <= 25 );

            // Update the database accordingly.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update activity_descriptions "
                                    + "set activity_description = @AD "
                                    + "where activity_description_id = @ID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@AD", newAct );
                    cmd.Parameters.AddWithValue( "@ID", id );

                    // We don't need rows back from this one, but this could
                    // fail if the user tries to add an existing activity.
                    try
                    {
                        cmd.ExecuteNonQuery();

                        // Let the user know the change succeeded.
                        Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  \"{activity}\" is now \"{newAct}\"!" );

                        // Add it as the key for the ID to the dictionary
                        // and remove the old one.
                        activities[ newAct ] = id;
                        activities.Remove( activity );
                    }
                    catch
                    {
                        // Let the user know the change failed.
                        Console.WriteLine( $"Sorry... It looks like category \"{activity}\" already exists!" );
                    }
                }
            }
        }

        // Prompt the user for a new Activity, adding it
        // to the list and database.
        private static void AddActivity()
        {
            // Clear the Console for cleaner output.
            Console.Clear();

            // Get a new ID for the Activity-to-be.
            int id = activities.Values.Max() + 1;

            // Get the new Activity Name from the user.
            string newAct = PromptFor< string >( $"What name would you like for this new activity?"
                                               , "The activity name cannot be blank or more than 25 characters.  Try again."
                                               , act => !string.IsNullOrEmpty( act ) && act.Length <= 25 );

            // Update the database accordingly.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "insert into activity_descriptions "
                                    + "( activity_descripton_id "
                                    + ", activity_descripton ) "
                                    + "values "
                                    + "( ID"
                                    + "  CD )";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@CD", newAct );
                    cmd.Parameters.AddWithValue( "@ID", id );

                    // We don't need rows back from this one, but this could
                    // fail if the user tries to rename to an existing Category.
                    try
                    {
                        // Let the user know the change succeeded.
                        Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  \"{newAct}\" has been added!" );

                        // Add it as the key for the ID to the dictionary.
                        activities[ newAct ] = id;
                    }
                    catch
                    {
                        // Let the user know the change failed.
                        Console.WriteLine( $"Sorry... It looks like category \"{newAct}\" already exists!" );
                    }
                }
            }
        }

        // Prompt the user for the Numerical Day to drill down
        // into their Activity Log, sparing the user with too
        // many results to discern.  Give the option to go back.
        private static void Log()
        {
            // For checking when the user exits.
            string choice = null;

            // Wait until the user wants to go back.
            while( choice != optGoBack.Text )
            {
                // Clear the console for nicer ouput.
                Console.Clear();

                // Clear the menu.
                menuLogDays.Clear();

                // Insert all the Numerical Days as menu options
                // to drill down the log to that Day.
                days.Keys.ToList().ForEach( key =>
                    menuLogDays.Add( new ActionOnlyOption( 
                        key.ToString(), () => LogDay( key ) ) )
                );

                // Add an option to Go Back.
                menuLogDays.Add( optGoBack );

                // Display the Categories Edit Menu.
                choice = menuLogDays.Run();
                Pause();
            }
        }

        // Prompt the user for the Activity Log Entry to View
        // or Edit.  Give the user options to add new Activity
        // Log Entries for this Numerical Day and to go back.
        private static void LogDay( int day )
        {
            // For checking when the user exits.
            string choice = null;

            // Wait until the user wants to go back.
            while( choice != optGoBack.Text )
            {
                // Clear the menu.
                menuLogEntries.Clear();

                // Insert all the Activity Log Entries for the
                // chosen Numerical DAy as menu options.
                loggedIn.Log
                    .Where( entry => entry.DayID == day )
                    .ToList()
                    .ForEach( entry =>
                        menuLogEntries.Add( new ActionOnlyOption(
                            string.Format(
                                "{0}, {1}: {2}/{3} - {4}"
                                
                                // The way the simple lookup dictionaries are set up
                                // makes updating them easy, but lookups are a little
                                // more involving.  I'd rather have the destructive
                                // operations easier to get write.
                            ,   weekdays.Where( wd => wd.Value == entry.WeekdayID )
                                    .Select( wd => wd.Key ).First().ToString()
                            ,   dates.Where( dt => dt.Value == entry.DateID )
                                    .Select( dt => dt.Key ).First().ToString( "yyyy-MM-dd" )
                            ,   categories.Where( cat => cat.Value == entry.CategoryID )
                                    .Select( cat => cat.Key ).First().ToString()
                            ,   activities.Where( act => act.Value == entry.ActivityID )
                                    .Select( act => act.Key ).First().ToString()
                            ,   durations.Where(  dur => dur.Value == entry.DurationID )
                                    .Select( dur => dur.Key ).First().ToString() )
                        ,   () => LogEntry( entry ) ) )
                    );

                // Add an option to Go Back.
                menuLogEntries.Add( optGoBack );

                // Display the Categories Edit Menu.
                choice = menuLogEntries.Run();
                Pause();
            }
        }

        // Prompt the user with menu to edit the selected Activity
        // Log Entry or go back.
        private static void LogEntry( LogEntry entry )
        {
            // For checking when the user exits.
            string choice = null;

            // Wait until the user wants to return to the Main Menu, or
            // he or she deleted the selected entry.
            while( choice != optGoBack.Text && loggedIn.Log.Contains( entry ) )
            {
                // Clear the console for nicer ouput.
                Console.Clear();

                // Feed the chosen entry's data into the menu prompt.
                string text = "Here's the log entry you selected...\n\n"
                            + string.Format(
                                "Day:       {0}\n"
                            ,   days.Where( d => d.Value == entry.DayID )
                                    .Select( d => d.Key ).FirstOrDefault().ToString() ?? "" )
                            + string.Format( 
                                "Weekday:   {0}\n"
                            ,   weekdays.Where( wd => wd.Value == entry.WeekdayID )
                                    .Select( wd => wd.Key ).FirstOrDefault().ToString() ?? "" )
                            + string.Format(
                                "Date:      {0}\n"
                            ,   dates.Where( dt => dt.Value == entry.DateID )
                                    .Select( dt => dt.Key ).FirstOrDefault().ToString( "yyyy-MM-dd" ) ?? "" )
                            + string.Format(
                                "Category:  {0}\n"
                            ,   categories.Where( cat => cat.Value == entry.CategoryID )
                                    .Select( cat => cat.Key ).FirstOrDefault().ToString() ?? "" )
                            + string.Format(
                                "Activity:  {0}\n"
                            ,    activities.Where( act => act.Value == entry.ActivityID )
                                    .Select( act => act.Key ).FirstOrDefault().ToString() ?? "" )
                            + string.Format(
                                "Duration:  {0}\n\n"
                            ,    durations.Where( dur => dur.Value == entry.DurationID )
                                    .Select( dur => dur.Key ).FirstOrDefault().ToString() ?? "" )
                            + "What would you like to do?";

                // Set up the chosen entry's edit menu.  Must be done here so
                // we can pass the entry through the appropriate edit method.
                menuEntry.Clear();

                // The user should not be able to change the entry's ID or User ID.

                menuEntry.Add( new ActionOnlyOption(
                    "Change the Numerical Day"
                ,   () => ChangeDay( entry ) ) );

                menuEntry.Add( new ActionOnlyOption(
                    "Change the Date"
                ,   () => ChangeDate( entry ) ) );

                menuEntry.Add( new ActionOnlyOption(
                    "Change the Weekday"
                ,   () => ChangeWeekday( entry ) ) );

                menuEntry.Add( new ActionOnlyOption(
                    "Change the Category"
                ,   () => ChangeCategory( entry ) ) );

                menuEntry.Add( new ActionOnlyOption(
                    "Change the Activity"
                ,   () => ChangeActivity( entry ) ) );

                menuEntry.Add( new ActionOnlyOption(
                    "Change the Duration"
                ,   () => ChangeDuration( entry ) ) );

                menuEntry.Add( new ActionOnlyOption(
                    "Delete the Entry"
                ,   () => DeleteEntry( entry ) ) );

                // Make sure the user can cancel the edit.
                menuEntry.Add( optGoBack );

                menuEntry.Prompt = text;

                // Display the Profile Edit Menu.
                choice = menuEntry.Run();
                Pause();
            }
        }

        // Present the user with a menu of Numerical Days with which
        // the selected Activity Log Entry's Numerical Day can be set,
        // updating the entry with the user's choice and the database.
        private static void ChangeDay( LogEntry entry )
        {
            // Clear the console for nicer output.
            Console.Clear();

            var dayMenu = new SuperMenu( "With which Numerical Day would you like to update this entry?" );

            // Load all the Numerical Days into the Day Menu.
            days.Keys.ToList().ForEach( day =>
                dayMenu.Add( new ActionOnlyOption(
                    day.ToString(), () => { } ) ) );

            // Get the user's new Numerical Day.
            entry.DayID = int.Parse( dayMenu.Run() );

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update activity_log "
                                    + "set calendar_day = @CD "
                                    + "where user_id = @UID "
                                    + "and id = @EID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@CD",  entry.DayID );
                    cmd.Parameters.AddWithValue( "@UID", loggedIn.ID );
                    cmd.Parameters.AddWithValue( "@EID", entry.ID    );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  The numerical day is updated!" );
                }
            }
        }

        // Present the user with a menu of Dates with which
        // the selected Activity Log Entry's Date can be set,
        // updating the entry with the user's choice and the database.
        private static void ChangeDate( LogEntry entry )
        {
            // Clear the console for nicer output.
            Console.Clear();

            var dateMenu = new SuperMenu( "With which Date would you like to update this entry?" );

            // Load all the Dates into the Day Menu.
            dates.Keys.ToList().ForEach( date =>
                dateMenu.Add( new ActionOnlyOption(
                    date.ToString( "yyyy-MM-dd" ), () => { } ) ) );

            // Get the user's new Numerical Day.
            entry.DateID = dates[ DateTime.Parse( dateMenu.Run() ) ];

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update activity_log "
                                    + "set calendar_date = @CD "
                                    + "where user_id = @UID "
                                    + "and id = @EID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@CD",  entry.DateID );
                    cmd.Parameters.AddWithValue( "@UID", loggedIn.ID  );
                    cmd.Parameters.AddWithValue( "@EID", entry.ID     );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  The date is updated!" );
                }
            }
        }

        // Present the user with a menu of Weekdays with which
        // the selected Activity Log Entry's Weekday can be set,
        // updating the entry with the user's choice and the database.
        private static void ChangeWeekday( LogEntry entry )
        {
            // Clear the console for nicer output.
            Console.Clear();

            var weekdayMenu = new SuperMenu( "With which Weekday would you like to update this entry?" );

            // Load all the Dates into the Day Menu.
            weekdays.Keys.ToList().ForEach( wd =>
                weekdayMenu.Add( new ActionOnlyOption(
                    wd, () => { } ) ) );

            // Get the user's new Weekday.
            entry.WeekdayID = weekdays[ weekdayMenu.Run() ];

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update activity_log "
                                    + "set day_name = @DN "
                                    + "where user_id = @UID "
                                    + "and id = @EID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@DN",  entry.WeekdayID );
                    cmd.Parameters.AddWithValue( "@UID", loggedIn.ID     );
                    cmd.Parameters.AddWithValue( "@EID", entry.ID        );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  The weekday is updated!" );
                }
            }
        }

        // Present the user with a menu of Categories with which
        // the selected Activity Log Entry's Category can be set,
        // updating the entry with the user's choice and the database.
        private static void ChangeCategory( LogEntry entry )
        {
            // Clear the console for nicer output.
            Console.Clear();

            var catMenu = new SuperMenu( "With which Category would you like to update this entry?" );

            // Load all the Dates into the Day Menu.
            categories.Keys.ToList().ForEach( cat =>
                catMenu.Add( new ActionOnlyOption(
                    cat, () => { } ) ) );

            // Get the user's new Category.
            entry.CategoryID = categories[ catMenu.Run() ];

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update activity_log "
                                    + "set category_description = @CD "
                                    + "where user_id = @UID "
                                    + "and id = @EID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@CD",  entry.CategoryID );
                    cmd.Parameters.AddWithValue( "@UID", loggedIn.ID      );
                    cmd.Parameters.AddWithValue( "@EID", entry.ID         );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  The category is updated!" );
                }
            }
        }

        // Present the user with a menu of Activities with which
        // the selected Activity Log Entry's Activity can be set,
        // updating the entry with the user's choice and the database.
        private static void ChangeActivity( LogEntry entry )
        {
            // Clear the console for nicer output.
            Console.Clear();

            var actMenu = new SuperMenu( "With which Category would you like to update this entry?" );

            // Load all the Dates into the Day Menu.
            activities.Keys.ToList().ForEach( act =>
                actMenu.Add( new ActionOnlyOption(
                    act, () => { } ) ) );

            // Get the user's new Activity.
            entry.ActivityID = activities[ actMenu.Run() ];

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update activity_log "
                                    + "set activity_description = @AD "
                                    + "where user_id = @UID "
                                    + "and id = @EID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@AD",  entry.ActivityID );
                    cmd.Parameters.AddWithValue( "@UID", loggedIn.ID      );
                    cmd.Parameters.AddWithValue( "@EID", entry.ID         );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  The activity is updated!" );
                }
            }
        }

        // Present the user with a menu of Durations with which
        // the selected Activity Log Entry's Duration can be set,
        // updating the entry with the user's choice and the database.
        private static void ChangeDuration( LogEntry entry )
        {
            // Clear the console for nicer output.
            Console.Clear();

            var durMenu = new SuperMenu( "With which Duration would you like to update this entry?" );

            // Load all the Dates into the Day Menu.
            durations.Keys.ToList().ForEach( dur =>
                durMenu.Add( new ActionOnlyOption(
                    dur.ToString(), () => { } ) ) );

            // Get the user's new Duration.
            entry.DurationID = durations[ double.Parse( durMenu.Run() ) ];

            // Clear the console for nicer output.
            Console.Clear();

            // Update the user's information on the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "update activity_log "
                                    + "set time_spent_on_activity = @TS "
                                    + "where user_id = @UID "
                                    + "and id = @EID";

                    // Protect against SQL Injection Attacks.
                    cmd.Parameters.AddWithValue( "@TS",  entry.DurationID );
                    cmd.Parameters.AddWithValue( "@UID", loggedIn.ID      );
                    cmd.Parameters.AddWithValue( "@EID", entry.ID         );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the change succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  The duration is updated!" );
                }
            }
        }

        // Delete the selected Activity Log Entry
        private static void DeleteEntry( LogEntry entry )
        {
            // Clear the Console for nicer output.
            Console.Clear();

            // Confirm that the user wishes to delete the
            // selected Activity Log Entry.
            string sure = PromptFor< string >( "Are you SURE you want to delete this entry?\n"
                                             + "This cannot be undone...\n"
                                             + "Enter \"YES\" in all CAPS to confirm.\n"
                                             , "Should never see this error prompt."
                                             , any => true );

            // Delete the Activity Log Entry if the
            // user confirmed with resoundig YES.
            if( sure == "YES" )
            {
                // Update the user's information on the database.
                using( var con = new MySqlConnection( cs ) )
                {
                    con.Open();

                    using( var cmd = con.CreateCommand() )
                    {
                        cmd.CommandText = "delete from activity_log "
                                        + "where user_id = @UID "
                                        + "and id = @EID";

                        // Protect against SQL Injection Attacks.
                        cmd.Parameters.AddWithValue( "@TS",  entry.DurationID );
                        cmd.Parameters.AddWithValue( "@UID", loggedIn.ID      );
                        cmd.Parameters.AddWithValue( "@EID", entry.ID         );

                        // We don't need rows back from this one.
                        cmd.ExecuteNonQuery();

                        // Let the user know the delete succeeded.
                        Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  The delete succeeded!" );

                        loggedIn.Log.Remove( entry ); 
                    }
                }
            }
            else
            {
                // Let the user know the delete was canceled.
                Console.WriteLine( "The delete was canceled.  Phew...  Close call!" );
            }
        }

        // Adds a new, virtually empty Activity Log Entry for the
        // user and situates him or her to edit it.
        private static void AddEntry()
        {
            // Clear the Console for nicer output.
            Console.Clear();

            // Generate a new Activity Log ID.
            var id = NextUserID();

            // Write the new Activity Log Entry to the database.
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    cmd.CommandText = "insert into activity_log"
                                    + "( id"
                                    + ", user_id ) "
                                    + "values"
                                    + "( @EID"
                                    + ", @UID )";

                    // Protect against SQL Injection Attacks.
                    // Technically not neccessary here, but good habits..
                    cmd.Parameters.AddWithValue( "@EID", id          );
                    cmd.Parameters.AddWithValue( "@UID", loggedIn.ID );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know the addition succeeded.
                    Console.WriteLine( $"Congratulations, {loggedIn.FirstName}...  The new entry was added!\n"
                                     + "You will now be sent to the screen to edit it." );

                    // Keep the logged in user's log in sync with the database.
                    var entry = new LogEntry();

                    entry.ID     = id;
                    entry.UserID = loggedIn.ID;

                    loggedIn.Log.Add( entry );

                    Pause();

                    // Drop the user into editing the entry.
                    LogEntry( entry );
                }
            }
        }

        // Finds the highest Activity Log ID Number
        // and returns the next highest number.
        private static int NextLogID()
        {
            using( var con = new MySqlConnection( cs ) )
            {
                con.Open();

                using( var cmd = con.CreateCommand() )
                {
                    // Activity Log Entry IDs are unique across all users' entries.
                    cmd.CommandText = "select max( id ) as ID from activity_log";

                    using( MySqlDataReader rdr = cmd.ExecuteReader() )
                    {
                        if( rdr.HasRows )
                        {
                            // Still need this check in the case that
                            // there are zero log entries in here.
                            rdr.Read();

                            return int.Parse( rdr[ "ID" ].ToString() ) + 1;
                        }
                        else
                        {
                            // The very first Activity Log Entry has an ID of 1.
                            return 1;
                        }
                    }
                }
            }
        }

        // Program entry point.
        public static void Main( string[] args )
        {
            // Preload the simple lookup tables from the database.
            LoadCategories();
            LoadActivities();
            LoadDurations();
            LoadWeekdays();
            LoadDates();
            LoadDays();

            // Cycle the Main Menu until the user quits.
            while( true)
            {
                // Clear the Console for nicer ouput.
                Console.Clear();

                menuMain.Run();
                Pause();
            }
        }
    }
}
