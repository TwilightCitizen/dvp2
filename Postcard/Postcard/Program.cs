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

        // Program entry point, essentially "Start".
        public static void Main( string[] args ) {
            // Options for start menu.
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

            // Start menu.
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
            int? userID = null;

            // Get the user's username and password.
            string username = GetUsername();
            Console.Clear();
            string password = GetPassword();
            Console.Clear();

            // Check if login and marking the user as logged in succeeded,
            // informing and diverting the user accordinly.
            if( TryLogin( username, password, out userID ) &&
                TryMarkLoggedIn( userID.Value ) ) {
                // Login succeeded.
                Welcome( userID.Value );
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
        private static bool TryMarkLoggedIn( int userID ) {
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

                    // Execute the query, returning success/failure.
                    return cmd.ExecuteNonQuery() == 1;
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
            // Options for existing username menu.
            var optAgain   = new ActionOnlyOption(
                "Try a Different Username"
            ,   () => { }
            );

            var optCancel  = new CancelOption(
                "Cancel Registration"
            );

            // Existing username menu.
            var menuExists = new SuperMenu(
                "Oh, no!  It looks like someone already has that username.\n"
            +   "What would you like to do?"
            ,   "Oops!  Can't do that here.  Try again."
            ) {
                optAgain, optCancel
            };

            string     username = null; // Desired username.
            bool       exists   = true; // Hold existence.
            MenuOption choice   = null; // Catch user's choice

            // Get the user's desired username.  Keep trying
            // until a unique one is provided or the user quits trying
            while( exists ) {
                username = GetNewUsername();
                exists   = UsernameExists( username );
                
                // Give the user the option to keep trying if it does.
                if( exists ) choice = menuExists.Run();

                // Handle the user giving up gracefully.
                if( choice == optCancel ) {
                    // Entreat the user to try again later.
                    Console.WriteLine(
                        "Sorry to see you go so soon!\n"
                    +   "Hopefully, you'll come back to try registering again later."
                    );

                    return;
                }
            }

            // Get the rest of the user's details.
            var details   = new UserDetails {
                nameUser  = username
            };

            Console.Clear();
            details.password  = GetNewPassword();
            Console.Clear();
            details.nameFirst = GetNewFirstName();
            Console.Clear();
            details.nameLast  = GetNewLastName();
            Console.Clear();
            details.loggedIn  = false;
            Console.Clear();

            // Show the user the registration details with a 
            // note that they can be changed later.
            Console.WriteLine(
                 "Here are the details you entered.\n"
            +   $"Username   - {details.nameUser}\n"
            +   $"Password   - {details.password}\n"
            +   $"First Name - {details.nameFirst}\n"
            +   $"Last Name  - {details.nameLast}\n\n"
            +    "If something looks wrong, don't worry...\n"
            +    "You can change any of it later after logging in.\n\n"
            );

            // Attempt to register the account, informing the user.
            if( TryRegistration( details ) ) {
                // Registration succeeded.
                Console.WriteLine( 
                    $"Congratulations, {details.nameFirst}!  Your account is registered with Postcard.\n"
                +   "You can now login from the start screen."
                );
            } else {
                // Registration failed.
                Console.WriteLine(
                    "Oh, no...  Something went wrong with your registration!"
                +   "Please check your connection and try again later.  Sorry about this."
                );
            }
        }

        // Query to add user details to users and report success/failure.
        private static bool TryRegistration( UserDetails details ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query to add the user.
                using( var cmd = con.CreateCommand() ) {
                    // Insert all the details.  UserID generated automatically.
                    cmd.CommandText = "insert into users( "
                                    + "  nameUser         "
                                    + ", password         "
                                    + ", nameFirst        "
                                    + ", nameLast         "
                                    + ", loggedIn )       "
                                    + "values(            "
                                    + "  @Username        "
                                    + ", @Password        "
                                    + ", @FirstName       "
                                    + ", @LastName        "
                                    + ", @LoggedIn )      ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@Username",  details.nameUser  );
                    cmd.Parameters.AddWithValue( "@Password",  details.password  );
                    cmd.Parameters.AddWithValue( "@FirstName", details.nameFirst );
                    cmd.Parameters.AddWithValue( "@LastName",  details.nameLast  );
                    cmd.Parameters.AddWithValue( "@LoggedIn",  details.loggedIn  );

                    // Execute the query.
                    return cmd.ExecuteNonQuery() == 1;
                }
            }
        }

        // Query to see if a username already exists in the database.
        private static bool UsernameExists( string username ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query to find the username.
                using( var cmd = con.CreateCommand() ) {
                    // Logged in is true for matching userID
                    cmd.CommandText = "select nameUser             "
                                    + "from   users                "
                                    + "where  nameUser = @Username ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@Username", username );

                    // Execute the query and return the status.
                    return cmd.ExecuteReader().HasRows;
                }
            }
        }

        // Gets the desired username from the user.
        private static string GetNewUsername() {
            return PromptFor< string >(
                "What username would you like?  You'll need it to login."
            ,   "Oops!  Looks like you entered a blank username.  Try again."
            ,   any => NotEmpty( any )
            );
        }

        private static string GetNewPassword() {
            return PromptFor< string >(
                "What password would you like?  Choose something easy to\n"
            +   "remember, but hard to guess.  You'll need it to login."
            ,   "Oops!  Looks like you entered a blank password.  Try again."
            ,   any => NotEmpty( any )
            );
        }

        private static string GetNewFirstName() {
            return PromptFor< string >(
                "What is your first name?"
            ,   "Oops!  Looks like you entered a blank first name.  Try again."
            ,   any => NotEmpty( any )
            );
        }

        private static string GetNewLastName() {
            return PromptFor< string >(
                "What is your last name?"
            ,   "Oops!  Looks like you entered a blank last name.  Try again."
            ,   any => NotEmpty( any )
            );
        }

        // Welcome the user and present the user with
        // a main menu.
        private static void Welcome( int userID ) {
            // Options for welcome menu.
            var optProfile   = new ActionOnlyOption(
                "View & Edit Your Profile"
            ,   Profile
            );

            var optUsers     = new ActionOnlyOption(
                "Read Your Postcards"
            ,   Postcards
            );

            var optLogout    = new ActionOnlyOption(
                "Logout of Your Postcard Account"
            ,   () => Logout( userID )
            );

            // Welcome menu.
            var menuWelcome = new SuperMenu(
                error : "Oops!  That one isn't available.  Try again."
            ) {
                optProfile, optUsers, optLogout
            };

            // Get the logged in user's details.
            UserDetails? details;

            // Check if details were found, prompting and diverting
            // the user accordingly.
            if( TryGetUserDetails( userID, out details ) ) {
                menuWelcome.Prompt = $"You are logged in, { details.Value.nameFirst }. "
                                   +  "What would you like to do now?";

                // Run the welcome menu until the user logs out.
                while( menuWelcome.Run() != optLogout ) {
                    Pause();
                    Console.Clear();
                }
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

        // On confirmation, log the user out.
        private static void Logout( int userID ) {
            // Make sure the user really wants to logout.
            if( Confirmed( "Are you sure you want to logout of Postcard?" ) ) {
                // Get the logged in user's details.
                UserDetails? details;
                string       name = "";

                // Customize farewell, if possible.
                if( TryGetUserDetails( userID, out details ) ) {
                    name = ", " + details.Value.nameFirst;
                }

                // Let the user know logout succeeded.  It can't fail because userID
                // goes out of scope when the login scope exits back to the start menu.
                Console.Clear();
                Console.WriteLine( "You've been logged out.  Come back soon{0}!", name );

                // Check if the user can be marked logged out, informing
                // the user accordingly.
                if( !TryMarkLoggedOut( userID ) ) {
                    // Marking as logged out failed somehow.
                    Console.WriteLine( "You may still appear as online to other users." );
                }
            }
        }

        // Query to mark the logged in user as logged out.
        private static bool TryMarkLoggedOut( int userID ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query to mark the user as logged out.
                using( var cmd = con.CreateCommand() ) {
                    // Logged in is false for matching userID
                    cmd.CommandText = "update users              "
                                    + "set    loggedIn = 0       "
                                    + "where  userID   = @UserID ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    if( cmd.ExecuteNonQuery() == 1) {
                        // Success means a row got updated.
                        return true;
                    } else {
                        // Anything else means something went wrong.
                        return false;
                    };
                }
            }
        }

        // Prompts the user to confirm some action.
        private static bool Confirmed( string prompt ) {
            return PromptFor< string >(
                prompt + "\nEnter \"YES\" in all caps to confirm."
            , "", any => true
            ) == "YES";
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
