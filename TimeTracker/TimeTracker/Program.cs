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

        private static MenuOption optLogin     = new ActionOnlyOption
        ( 
            "Login to Your Account"
        ,   Login
        ,   conditions : new List< Func< bool > >()
            {
                () => loggedIn == null
            }
        );

        private static MenuOption optLogout    = new ActionOnlyOption
        ( 
            "Logout of Your Account"
        ,   Logout
        ,   conditions : new List< Func< bool >>()
            {
                () => loggedIn != null
            }
        );

        private static MenuOption optRegister  = new ActionOnlyOption
        (
            "Register a New Account"
        ,   Register
        ,   conditions : new List< Func< bool >>()
            {
                () => loggedIn == null
            }
        );

        private static MenuOption optProfile   = new ActionOnlyOption
        (
            "View or Edit Your Profile"
        ,   Profile
        ,   conditions : new List< Func< bool >>()
            {
                () => loggedIn != null
            }
        );

        private static MenuOption optExit      = new ExitOption();

        // Main Menu.
        private static SuperMenu menuMain      = new SuperMenu()
        {
            optLogin
        ,   optLogout
        ,   optProfile
        ,   optRegister
        ,   optExit
        };

        // Options for Profile Menu.
        
        private static MenuOption optFirstName = new ActionOnlyOption
        (
            "Change Your First Name"
        ,   ChangeFirstName
        );

        private static MenuOption optLastName  = new ActionOnlyOption
        (
            "Change Your Last Name"
        ,   ChangeLastName
        );

        private static MenuOption optPassword  = new ActionOnlyOption
        (
            "Change Your Password"
        ,   ChangePassword
        );

        private static MenuOption optReturn    = new CancelOption
        (
            "Return to the Main Menu."
        );

        // Profile Menu.
        private static SuperMenu menuProfile   = new SuperMenu()
        {
            optFirstName
        ,   optLastName
        ,   optPassword
        ,   optReturn
        };

        // Prompt the user for the User ID and Password to attempt
        // logging in.  Note that Login presently sends Pasword in
        // the clear because the Time Tracker database was prescribed
        // to be built with a password field too short to store a
        // secure hash.  Plus, we don't know how to establish
        // secure connections with public key encryption yet.
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
            var id = NextUniqueID();

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
        private static int NextUniqueID()
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
                                                    , "Your new First Name cannot be blank.  Try again."
                                                    , any => !string.IsNullOrEmpty( any ) );

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
                    cmd.Parameters.AddWithValue( "@ID", loggedIn.ID );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know registration succeeded.
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
                                                   , "Your new Last Name cannot be blank.  Try again."
                                                   , any => !string.IsNullOrEmpty( any ) );

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
                    cmd.Parameters.AddWithValue( "@ID", loggedIn.ID );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know registration succeeded.
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
                                                   , "Your new Password cannot be blank.  Try again."
                                                   , any => !string.IsNullOrEmpty( any ) );

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
                    cmd.Parameters.AddWithValue( "@ID", loggedIn.ID );

                    // We don't need rows back from this one.
                    cmd.ExecuteNonQuery();

                    // Let the user know registration succeeded.
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
