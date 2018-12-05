using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Utilities.Terminal;
using static Utilities.Terminal.IO;

namespace Postcard
{
    public class Program
    {
        #region Constants

        private const string cs = "server=192.168.121.1;"
                                + "userid=dbremoteuser;"
                                + "password=password;"
                                + "database=Postcard;"
                                + "port=8889";

        #endregion

        #region Aggregate Types

        // Details about a user.
        private struct UserDetails {
            public int    userID;
            public string nameFirst;
            public string nameLast;
            public string nameUser;
            public string password;
            public bool   loggedIn;
        }

        #endregion

        #region Module-level State

        // Don't want too much here.  The less, the better.
        

        #endregion

        // Program entry point, essentially "Start".
        public static void Main( string[] args ) {
            var optLogin  = new ActionOnlyOption(
                "Log Into Your Postcard Account"
            ,   Login
            );

            var optJoin   = new ActionOnlyOption(
                "Join Today! - Register a New Postcard Account"
            ,   Join
            );

            var optExit   = new ExitOption(
                "Exit Postcard"
            );

            var menuStart = new SuperMenu(
                "This is Postcard!  Choose from below to get started."
            ,   "Oops!  That one isn't available.  Try again."
            ) {
                optJoin, optLogin, optExit
            };

            // Run start menu until user exits.
            while( true ) {
                Console.Clear();
                menuStart.Run();
                Pause();
            }
        }

        // Pause console output until user keypress.
        private static void Pause() {
            // Inform paused status.
            Console.WriteLine( "\nPress any key to continue..." );
            // Continue on keypress.
            Console.ReadKey();
        }

        // Abstract !string.IsNullOrEmpty() since it appears in
        // many PromptFor( string ) use cases.
        private static bool NotEmpty( string any ) =>
            !string.IsNullOrEmpty( any );

        // Login user, sending to welcome screen on success
        // and returning to start screen on failure.
        private static void Login() {
            // userID for logged in user.
            int?   userID;

            // Get the user's username and password.
            string username = GetUsername();
            string password = GetPassword();

            // Attempt to log the user in.
            bool   loggedIn = TryLogin( username, password, out userID );

            // Check if login succeeded, informing and diverting
            // the user accordinly.
            if( loggedIn ) {
                // Login succeeded.
                MarkLoggedIn( userID.Value );
                Welcome(      userID.Value );
            } else {
                // Login failed.
                Console.WriteLine( "Sorry!  That username or password didn't quite work." );
            }
        }

        // Query for login returns a bool normally to signal success or
        // failure and any matching userID through an out parameter.
        private static bool TryLogin( string username, string password, out int? userID ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a parameterized login query
                using( var cmd = con.CreateCommand() ) {
                    // Users where userID and password match user input.
                    cmd.CommandText = "select userID               "
                                    + "from   users                "
                                    + "where  nameUser = @Username "
                                    + "and    password = @Password ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@Username", username );
                    cmd.Parameters.AddWithValue( "@Password", password );

                    // Execute the query.
                    using( MySqlDataReader rdr = cmd.ExecuteReader() ) {
                        // Check the results.
                        if( rdr.HasRows ) {
                            // Login was successfull.
                            rdr.Read();

                            // Return the userID with success.
                            userID = int.Parse( rdr[ "userID" ].ToString() );

                            return true;
                        } else {
                            // Login was unsuccessfull.
                            // Return null with failure.
                            userID = null;

                            return false;
                        }
                    }
                }
            }
        }

        // Query to mark the logged in user as logged in so other
        // users on the application can see how many others are on.
        private static void MarkLoggedIn( int userID ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query to mark the user as logged in.
                using( var cmd = con.CreateCommand() ) {
                    // Logged in is true for matching userID
                    cmd.CommandText = "update users              "
                                    + "set    loggedIn = 1       "
                                    + "where  userID   = @UserID ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Get the user's username.
        private static string GetUsername() {
            return PromptFor< string >(
                "What is your username?"
            ,   "Oops!  Looks like you entered a blank username.  Try again."
            ,   any => NotEmpty( any )
            );
        }

        // Get the user's password.
        private static string GetPassword() {
            return PromptFor< string > (
                "What is your password?"
            ,   "Hmm...  Blank passwords don't work.  Try again."
            ,   any => NotEmpty( any )
            );
        }

        // Register a new account for the user, if possible,
        // sending diverting to the login screen on success,
        // and on failure, giving them the option to try again
        // or cancel.
        private static void Join() {
            string     username = null;
            bool       exists   = true;
            MenuOption choice   = null;

            // Get a new username from the user until its
            // one that isn't taken, or the user stops trying.
            while( exists && !( choice is CancelOption ) ) {
                username = GetNewUsername();

                // TODO: Query database for matching username
                //       toggling exists accordingly.
                //       See if the user wants to try again
                //       if it exists, or give up.
            }

            // Did the user quit trying?

        }

        private static string GetNewUsername() {
            return "";
        }

        private static string GetNewPassword() {
            return "";
        }

        private static string GetNewFirstName() {
            return "";
        }

        private static string GetNewLastName() {
            return "";
        }

        // Welcome the user and present the user with
        // a main menu.
        private static void Welcome( int userID ) {
            // Get the logged in user's details.
            UserDetails? details;

            // Logged in user should not fail
            bool gotDetails = TryGetUserDetails( userID, out details );

            // Check if details were found, prompting and diverting
            // the user accordingly.
            if( gotDetails) {
                // Issue welcome message.
                Console.WriteLine( $"Welcome to Postcard, { details.Value.nameFirst }!" );
            } else {
                // Something bad must have happened.
                Console.WriteLine( "Ut, oh...  Something went wrong!" );
                Pause();
            }
        }

        // Query user details from database with userID.
        private static bool TryGetUserDetails( int userID, out UserDetails? details ) {

            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a parameterized query for user details.
                using( var cmd = con.CreateCommand() ) {
                    // User where userID matches.
                    cmd.CommandText = "select *              "
                                    + "from users            "
                                    + "where userID = @UserID";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    using( MySqlDataReader rdr = cmd.ExecuteReader() ) {
                        // Check the results.
                        if( rdr.HasRows ) {
                            // User was found.
                            rdr.Read();

                            // Return the user details with success.
                            details = new UserDetails {
                                userID    = int.Parse(  rdr[ "userID"    ].ToString() )
                            ,   nameFirst =             rdr[ "nameFirst" ].ToString()
                            ,   nameLast  =             rdr[ "nameLast"  ].ToString()
                            ,   nameUser  =             rdr[ "nameUser"  ].ToString()
                            ,   password  =             rdr[ "password"  ].ToString()
                            ,   loggedIn  = bool.Parse( rdr[ "loggedIn"  ].ToString() )
                            };

                            return true;
                        } else {
                            // Login was unsuccessfull.
                            // Return null with failure.
                            details = null;

                            return false;
                        }
                    }
                }
            }
        }

        private static void Logout() {

        }

        private static void Profile() {

        }

        private static void Edit() {

        }

        private static void Postcards() {

        }

        private static void Postcard() {

        }

        private static void Users() {

        }

        private static void Write() {

        }
    }
}
