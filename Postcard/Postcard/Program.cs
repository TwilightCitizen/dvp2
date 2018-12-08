/* Name:        David A. Clark, Jr.
 * Class:       MDV2229-O
 * Assignment:  3.1 - Project 1c: Coding App & Database (Postcard)
 * Date:        2018-12-03
 * 
 * Tasks:       Postcard is a database-connected console application that
 *              allows users to login and/or register an account, modify their
 *              profiles, read their postcards, browse other users' profiles,
 *              and read other users' postcards.
 *              
 * Note:        Uses SuperMenu, various MenuOption subclasses, and PromptFor<T>
 *              from referenced Utilities class library, and MySQL libraries.
 */

/* Because of how available menu options are presented in
 * subsequently deeper calls from the start screen's menu,
 * and those menus, no menu options require conditions to
 * signal whether or not they should be enabled.If an
 * option shouldn't be enabled, it doesn't appear on the
 * screen.  More or less, this is represenational state
 * transfer, or RESTful, design. */
 
/* In some cases where a menu option doesn't need to run
 * an associated action, a CancelOption is used in it's 
 * stead, which is caught in a variable, examined, and
 * acted on accordingly.  It's a bit of an abuse of the
 * CancelOption, which was originally designed specifically
 * for cancellations by the user, but it works.  However,
 * it calls for an eventual addition of a NoActionOption,
 * NullOption, or whatever it might be called, the
 * purpose of which is clearer than a CancelOption. */

/* App screens and menus, under any reasonable conditions,
 * won't fail, but database updates can fail for any
 * number of reasons.  Because of this, all database
 * transactions are coded in their own methods, prefixed
 * with Try, which return a boolean to indicate success
 * or failure, and an out or ref parameter through which
 * to return any query results, if needed by the app.
 * Code that calls these methods naturally results in
 * a defensive programming posture against failures. */

/* Rather than using classes to represent certain
 * database entities, structs are used in their stead.
 * This was more, than anything, to get some practice
 * with them, as they behave similarly to classes, but
 * are value types rather than reference types.  Time-
 * dependent, of course, but you may see some instances
 * where some refactoring is in order due to learning
 * their peculiarities from classes.  Aside from that,
 * programming with them in C# makes C# feel a bit more
 * functional like F#.  Some struct mutation does feature,
 * which isn't very F#-ish, no struct has methods.
 * Rather, the program module has methods, some of which
 * accept and act on structs. */

/* A way to go more F#-like here would be to change all
  * the Try-prefixed methods from those that return a bool
  * and feature an out or ref parameter to ones that return
  * a tuple of a success value and a possibly modified
  * copy of whatever struct--if any--were passed in. */

/* Originally, many Private Static menus and menu options
 * were envisioned.  To tame them at the head of the
 * module, collapsible regions were introduced.  These
 * really aren't neccessary now, as all menus and menu
 * options reside within the screen methods that need
 * them, but still something worthwhile to explore as
 * codebases grow in line count. */

/* SuperMenu's Run() method clears the screen before
 * displaying it's menu options.  It was designed this way
 * with DRY in mind:  Why write Console.Clear() before
 * every call to whateverMenu.Run()?  Well, as it turns
 * out, sometimes--maybe more often than not--it's useful
 * not to clear the screen before displaying the menu
 * automatically.  In it's current design, anything a
 * user of the class wants to write to screen to appear
 * before the menu is display must be constructed and
 * fed into the menu as the prompt.  It's a workaround
 * that features often in this app.  A boolean withClear
 * parameter to SuperMenu's constructor could alleviate
 * this accidentally designed idiom. */

using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Utilities.Terminal;
using static Utilities.Terminal.IO;

namespace Postcard {
    public class Program {
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
            public int      userID;
            public string   nameFirst;
            public string   nameLast;
            public string   nameUser;
            public string   password;
            public bool     loggedIn;
        }

        // Details about a postcard.
        private struct CardDetails {
            public int      cardID;
            public int      fromID;
            public int      toID;
            public DateTime date;
            public string   message;
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
            int userID = 0;

            // Get the user's username and password.
            string username = GetUsername();
            Console.Clear();
            string password = GetPassword();
            Console.Clear();

            // Check if login and marking the user as logged in succeeded,
            // informing and diverting the user accordinly.
            if( TryLogin( username, password, out userID ) &&
                TryMarkLoggedIn( userID ) ) {
                // Login succeeded.
                Welcome( userID );
            } else {
                // Login failed.
                Console.WriteLine( "Sorry!  That username or password didn't quite work." );
            }
        }

        // Query for login returns a bool normally to signal success or
        // failure and any matching userID through an out parameter.
        private static bool TryLogin( string username, string password, out int userID ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a parameterized login query
                using( var cmd = con.CreateCommand() ) {
                    // Users where userID and password match user input.
                    cmd.CommandText = "select userID               "
                                    + "from   Users                "
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
                            userID = 0;

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
                    cmd.CommandText = "update Users              "
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
                 "Here are the details you entered.\n\n"
            +   $"- Username:   { details.nameUser  }\n"
            +   $"- Password:   { details.password  }\n"
            +   $"- First Name: { details.nameFirst }\n"
            +   $"- Last Name:  { details.nameLast  }\n\n"
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
                    "Oh, no...  Something went wrong with your registration!\n"
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
                    cmd.CommandText = "insert into Users( "
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
                                    + "from   Users                "
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
            ,   () => ProfileOwn( userID )
            );

            var optRead      = new ActionOnlyOption(
                "Read Your Postcards!"
            ,   () => Postcards( userID, userID )
            );

            var optUsers     = new ActionOnlyOption(
                "Browse Other Users Profiles!"
            ,   () => Users( userID )
            );

            var optLogout    = new CancelOption(
                "Logout of Postcard"
            );

            // Welcome menu.
            var menuWelcome = new SuperMenu(
                error : "Oops!  That one isn't available.  Try again."
            ) {
                optProfile, optRead, optUsers, optLogout
            };

            // Get the logged in user's details.
            UserDetails details;

            // Check if details were found, prompting and diverting
            // the user accordingly.
            if( TryGetUserDetails( userID, out details ) ) {
                MenuOption choice = null; // Catch user's choice.
                int        online = 0;    // Number of users online.
                int        unread = 0;    // Number of user's unread postcards.
                string     prompt = "";   // For building menu prompt.

                // Run the welcome menu until the user logs out.
                while( choice != optLogout ) {
                    prompt = $"Welcome, { details.nameFirst }!";

                    // Get the number of user's online.
                    if( TryGetUsersOnline( out online ) ) {
                        online -= 1;
                        prompt += string.Format(
                            "\n\nThere {0} online right now."
                        ,   online == 0 ? "are no other users"
                                        : online == 1 ? "is 1 other user" 
                                                      : $"are { online } other users"
                        );
                    }

                    // Get the number of user's unread postcards.
                    if( TryGetUnreadPostcards( userID, out unread ) ) {
                        prompt += string.Format( 
                            "\nAlso, you have {0}."
                        ,   unread == 0 ? "no unread postcards"
                                        : unread == 1 ? "1 unread postcard"
                                                      : $"{ unread } unread postcards"
                        );
                    }

                    menuWelcome.Prompt = prompt + "\n\nWhat would you like to do now?";
                    choice             = menuWelcome.Run();

                    // Confirm the user wants to logout.
                    if( choice == optLogout &&
                        Confirmed( "Are you sure you want to logout of Postcard?" ) ) {
                        // The user really wants to logout.
                        Logout( userID );
                    } else {
                        // The user wants to stay logged in.
                        choice = null;
                    }

                    // Don't pause twice on logout.
                    if( choice != optLogout ) Pause();
                };
            } else {
                // Something bad must have happened.
                Console.WriteLine(
                    "Oh, no...  Something went wrong logging you in!\n"
                +   "Please check your connection and try again later.  Sorry about this."
                );

                Pause();
            }
        }

        private static bool TryGetUnreadPostcards( int userID, out int count ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query for user's unread postcards.
                using( var cmd = con.CreateCommand() ) {
                    // Count of postcards where toID matches userID and
                    // userID isn't a reader for it.
                    cmd.CommandText = "select count( P.cardID ) as unread    "
                                    + "from   PostCards as P                 "
                                    + "where  P.toID = @UserID               "
                                    + "and    not exists(                    "
                                    + "         select null                  "
                                    + "         from   Readers as R          "
                                    + "         where  R.userID = P.toID     "
                                    + "         and    R.cardID = P.cardID ) ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    using( MySqlDataReader rdr = cmd.ExecuteReader() ) {
                        // Check the results.
                        if( rdr.HasRows ) {
                            // We got a number back.
                            rdr.Read();

                            // Return the number of unread postcards with success.
                            count = int.Parse( rdr[ "unread" ].ToString() );

                            return true;
                        } else {
                            // Return 0 with failure.
                            count = 0;

                            return false;
                        }
                    }
                }
            }
        }

        // Query how many users are online right now.
        private static bool TryGetUsersOnline( out int count ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query for online users.
                using( var cmd = con.CreateCommand() ) {
                    // Count of users where loggedIn is true.
                    cmd.CommandText = "select count( userID ) as online "
                                    + "from  Users                      "
                                    + "where loggedIn = 1               ";

                    // Execute the query.
                    using( MySqlDataReader rdr = cmd.ExecuteReader() ) {
                        // Check the results.
                        if( rdr.HasRows ) {
                            // We got a number back.
                            rdr.Read();

                            // Return the number of online users with success.
                            count = int.Parse( rdr[ "online" ].ToString() );

                            return true;
                        } else {
                            // Return 0 with failure.
                            count = 0;

                            return false;
                        }
                    }
                }
            }
        }

        // Query user details from database with userID.
        private static bool TryGetUserDetails( int userID, out UserDetails details ) {

            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a parameterized query for user details.
                using( var cmd = con.CreateCommand() ) {
                    // User where userID matches.
                    cmd.CommandText = "select *              "
                                    + "from Users            "
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
                            details = new UserDetails { };

                            return false;
                        }
                    }
                }
            }
        }

        // On confirmation, log the user out.
        private static void Logout( int userID ) {
            // Get the logged in user's details.
            UserDetails details;

            // Customize farewell, if possible.
            string name = ( TryGetUserDetails( userID, out details ) ? ", " + details.nameFirst : "" );

            // Let the user know logout succeeded.  It can't fail because userID
            // goes out of scope when the login scope exits back to the start menu.
            // But, marking the user as logged out could...
            Console.Clear();
            Console.WriteLine(
                $"You've been logged out.  Come back soon{ name }!"
            +   ( TryMarkLoggedOut( userID ) ? "" : "You may still appear as online to other users." )
            );
        }

        // Query to mark the logged in user as logged out.
        private static bool TryMarkLoggedOut( int userID ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query to mark the user as logged out.
                using( var cmd = con.CreateCommand() ) {
                    // Logged in is false for matching userID
                    cmd.CommandText = "update Users              "
                                    + "set    loggedIn = 0       "
                                    + "where  userID   = @UserID ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    if( cmd.ExecuteNonQuery() == 1 ) {
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

        // Displays a user profile to the console with a contextual
        // menu for editing the user's own or writing to another's.
        private static void ProfileOwn( int userID ) {
            // Get the logged in user's details.
            var details = new UserDetails { };

            // Options for profile menu.
            var optUsername     = new ActionOnlyOption(
                "Change Your Username"
            ,   () => Username(  ref details ) 
            );

            var optPassword     = new ActionOnlyOption(
                "Change Your Password"
            ,   () => Password(  ref details ) 
            );

            var optFirstName    = new ActionOnlyOption(
                "Change Your First Name"
            ,   () => FirstName( ref details ) 
            );

            var optLastName     = new ActionOnlyOption(
                "Change Your Last Name"
            ,   () => LastName(  ref details ) 
            );

            var optDone     = new CancelOption(
                "Done - Go Back to the Last Screen"
            );

            // Profile menu.
            var menuProfile = new SuperMenu(
                error : "Oops!  That one isn't available.  Try again."
            ) {
                optUsername, optPassword, optFirstName, optLastName, optDone
            };

            // Check if details were found, prompting and diverting
            // the user accordingly.
            if( TryGetUserDetails( userID, out details ) ) {
                MenuOption choice = null; // Catch user's choice.
                string     prompt = null; // For building menu prompt.

                // Run the welcome menu until the user logs out.
                while( choice != optDone ) {
                    prompt = "Here is your profile.\n\n"
                           +   $"- Username:   { details.nameUser  }\n"
                           +   $"- Password:   { details.password  }\n"
                           +   $"- First Name: { details.nameFirst }\n"
                           +   $"- Last Name:  { details.nameLast  }\n\n";

                    menuProfile.Prompt = prompt + "What would you like to do now?";
                    choice             = menuProfile.Run();

                    // Don't pause twice on exit.
                    if( choice != optDone ) Pause();
                };
            } else {
                // Failed to load profile.
                Console.WriteLine(
                    "Oh, no...  Something went wrong loading the profile!\n"
                +   "Please check your connection and try again later.  Sorry about this."
                );

                Pause();
            }
        }

        // Prompt the user for a new username that does not exist, giving
        // the user the option to try again if it does, or give up.  Upon
        // successfully finding a new user name, attempt to update the user's
        // profile on the database.  Let the user rechoose the same username
        // and simply do not write it in this case.
        private static bool Username( ref UserDetails details ) {
            // Options for existing username menu.
            var optAgain   = new ActionOnlyOption(
                "Try a Different Username"
            ,   () => { }
            );

            var optCancel  = new CancelOption(
                "Cancel - Go Back"
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
            while( exists && username != details.nameUser ) {
                username = GetNewUsername();
                exists   = UsernameExists( username );
                
                // Give the user the option to keep trying if it does.
                if( exists && username != details.nameUser ) {
                    choice = menuExists.Run();
                }

                // Handle the user giving up gracefully.
                if( choice == optCancel ) {
                    // Entreat the user to try again later.
                    Console.WriteLine(
                        "Oh, well... Maybe try again later when you think of another."
                    );

                    return false;
                }
            }

            // No need to update the database for same username.
            if( details.nameUser == username ) {
                // Same username.
                return true;
            } else {
                // Different username.
                var updated   = new UserDetails {
                    userID    = details.userID
                ,   nameUser  = username
                ,   password  = details.password
                ,   nameFirst = details.nameFirst
                ,   nameLast  = details.nameLast
                };

                // Attempt to write the change to the database.
                if( TryUpdateUserDetails( updated ) ) {
                    // Update succeeded. User will see it in profile.
                    details = updated;

                    return true;
                } else {
                    // Update failed.
                    Console.WriteLine(
                        "Oh, no...  Something went wrong updating your username!\n"
                    +   "Please check your connection and try again later.  Sorry about this."
                    );

                    return false;
                }
            }
        }

        // Query to update the user's profile details.
        private static bool TryUpdateUserDetails( UserDetails details ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query to update the user's details.
                using( var cmd = con.CreateCommand() ) {
                    // Update everything but userID and loggedIn.
                    cmd.CommandText = "update Users                   "
                                    + "set    nameUser  = @Username,  "
                                    + "       password  = @Password,  "
                                    + "       nameFirst = @FirstName, " 
                                    + "       nameLast  = @LastName   "
                                    + "where  userID    = @UserID     ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@UserID",    details.userID    );
                    cmd.Parameters.AddWithValue( "@Username",  details.nameUser  );
                    cmd.Parameters.AddWithValue( "@Password",  details.password  );
                    cmd.Parameters.AddWithValue( "@FirstName", details.nameFirst );
                    cmd.Parameters.AddWithValue( "@LastName",  details.nameLast  );

                    // Execute the query.
                    if( cmd.ExecuteNonQuery() == 1 ) {
                        // Success means a row got updated.
                        return true;
                    } else {
                        // Anything else means something went wrong.
                        return false;
                    };
                }
            }
        }

        // Prompt the user for a new password and attempt to update the user's
        // profile on the database.  Do not write same password to database.
        private static bool Password( ref UserDetails details ) {
            // Get the user's new password.
            string password = GetNewPassword();

            // No need to update the database for same password.
            if( details.nameUser == password ) {
                // Same password.
                return true;
            } else {
                // Different password.
                var updated   = new UserDetails {
                    userID    = details.userID
                ,   nameUser  = details.nameUser
                ,   password  = password
                ,   nameFirst = details.nameFirst
                ,   nameLast  = details.nameLast
                };

                // Attempt to write the change to the database.
                if( TryUpdateUserDetails( updated ) ) {
                    // Update succeeded. User will see it in profile.
                    details = updated;

                    return true;
                } else {
                    // Update failed.
                    Console.WriteLine(
                        "Oh, no...  Something went wrong updating your password!\n"
                    +   "Please check your connection and try again later.  Sorry about this."
                    );

                    return false;
                }
            }
        }

        // Prompt the user for a new first name and attempt to update the user's
        // profile on the database.  Do not write same first name to database.
        private static bool FirstName( ref UserDetails details ) {
            // Get the user's new first name.
            string firstName = GetNewFirstName();

            // No need to update the database for same first name.
            if( details.nameFirst == firstName ) {
                // Same first name.
                return true;
            } else {
                // Different first name.
                var updated   = new UserDetails {
                    userID    = details.userID
                ,   nameUser  = details.nameUser
                ,   password  = details.password
                ,   nameFirst = firstName
                ,   nameLast  = details.nameLast
                };

                // Attempt to write the change to the database.
                if( TryUpdateUserDetails( updated ) ) {
                    // Update succeeded. User will see it in profile.
                    details = updated;

                    return true;
                } else {
                    // Update failed.
                    Console.WriteLine(
                        "Oh, no...  Something went wrong updating your first name!\n"
                    +   "Please check your connection and try again later.  Sorry about this."
                    );

                    return false;
                }
            }
        }

        // Prompt the user for a new last name and attempt to update the user's
        // profile on the database.  Do not write same last name to database.
        private static bool LastName( ref UserDetails details ) {
            // Get the user's new last name.
            string lastName = GetNewLastName();

            // No need to update the database for same first name.
            if( details.nameLast == lastName ) {
                // Same first name.
                return true;
            } else {
                // Different first name.
                var updated   = new UserDetails {
                    userID    = details.userID
                ,   nameUser  = details.nameUser
                ,   password  = details.password
                ,   nameFirst = details.nameFirst
                ,   nameLast  = lastName
                };

                // Attempt to write the change to the database.
                if( TryUpdateUserDetails( updated ) ) {
                    // Update succeeded. User will see it in profile.
                    details = updated;

                    return true;
                } else {
                    // Update failed.
                    Console.WriteLine(
                        "Oh, no...  Something went wrong updating your last name!\n"
                    +   "Please check your connection and try again later.  Sorry about this."
                    );

                    return false;
                }
            }
        }

        // Prompt the user with a menu to see postcards to or from
        // the selected user--or his or her own--subsequently.
        private static void Postcards( int userID, int viewerID ) {
            // Determine if the user is viewing his- or her own
            // postcards, or someone else's.
            bool viewingOwn = userID == viewerID;

            // Other user's details.
            UserDetails details;

            // If user details can't load, likely neither can postcards.
            // Inform and divert the user accordingly.
            if( !TryGetUserDetails( userID, out details ) ) {
                // Getting the user's details failed.
                Console.WriteLine(
                    "Oh, no...  Something went wrong getting the postcards!\n"
                +   "Please check your connection and try again later.  Sorry about this."
                );

                // Get out of dodge.
                return;
            }

            // Options for postcards filter menu.

            var optTo   = new CancelOption(
                $"Postcards to { ( viewingOwn ? "You" : details.nameUser ) }."
            );

            var optFrom = new CancelOption(
                $"Postcards from { ( viewingOwn ? "You" : details.nameUser ) }."
            );

            var optCancel = new CancelOption(
                "Nevermind - Go Back."
            );

            // Postcards filter menu.
            var menuFilter = new SuperMenu(
                $"Which postcards of { ( viewingOwn ? "yours" : details.nameUser + "'s" ) } did you want to see?"
            ) {
                optTo, optFrom, optCancel
            };

            MenuOption          choice    = null; // Catch the user's choice.
            List< CardDetails > postcards = null; // Catch the postcards.

            // Run the filter menu until the user quits.
            while( choice != optCancel ) {
                choice = menuFilter.Run();

                // Get the postcards the user chose.
                if( TryGetPostcards( userID, out postcards, choice == optFrom ) ) {
                    // Getting postcards succeeded.

                    // Postcards menu.
                    var menuPostcards = new SuperMenu(
                        $"Which postcard { ( choice == optFrom ? "from" : "to" ) } "
                    +   $"{ ( viewingOwn ? "you" : details.nameUser ) } do you want to view?"
                    );

                    // Load all the postcard options into the postcards menu.
                    postcards.ForEach( postcard => {
                        // Load the right user's details to show.
                        _ = choice == optFrom ? TryGetUserDetails( postcard.toID, out details )
                                              : TryGetUserDetails( postcard.fromID,   out details );

                        // Format the postcard option.
                        var opt = new ActionOnlyOption(
                            ( choice == optFrom ? $"To: { details.nameUser }, "
                                                + $"Date: { postcard.date.ToString( "yyyy-MM-dd" ) }"
                                                : $"From: { details.nameUser }, "
                                                + $"Date: { postcard.date.ToString( "yyyy-MM-dd" ) }"
                                                + ( TryGetUnreadBy( postcard.cardID, viewerID ) ? " (Unread)" : "" ) )
                        ,   () => { }
                        );

                        // Set up it's action to refer to itself for marking read.
                        opt.Action = () => {
                            // Display the postcard to the user.
                            Postcard( postcard );
                            // Silently try to mark it as read by the viewer, ignoring failures.
                            if( TryMarkAsRead( postcard.cardID, viewerID ) ) {
                                // Remove "[Unread]" from postcard option.
                                opt.Text = opt.Text.Replace( " (Unread)", "" );
                            }
                        };

                        // Add the option to the postcards menu.
                        menuPostcards.Add( opt );
                    } );

                    // Need a separate cancel option.  Reusing the one for the
                    // outer menu causes weird numbering.
                    var optNone = new CancelOption(  "Nevermind - Go Back" );

                    menuPostcards.Add( optNone  );

                    MenuOption card = null; // Catch the user's choice.

                    // Run the postcards menu until the user quits.
                    while( card != optNone && choice != optCancel ) {
                        card = menuPostcards.Run();

                        // Don't pause twice on cancel.
                        if( card != optNone && choice != optCancel ) Pause();
                    }

                } else {
                    // Getting the user's postcards failed.
                    Console.WriteLine(
                        "Hmmm...  There doesn't appear to be any postcards.\n"
                    +   "Also, check your connection and try again.  That could be the problem."
                );

                    // Get out of dodge.
                    return;
                }
                
                // Don't pause twice on cancel.
                if( choice != optCancel ) Pause();
            }
        }

        // Query to check if a postcard is unread by a selected user.
        private static bool TryGetUnreadBy( int cardID, int userID ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a parameterized query of matching readers.
                using( var cmd = con.CreateCommand() ) {
                    // Readers where cardID and userID match.
                    cmd.CommandText = "select null             "
                                    + "from   Readers          "
                                    + "where  cardID = @CardID "
                                    + "and    userID = @UserID ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@CardID", cardID );
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    using( MySqlDataReader rdr = cmd.ExecuteReader() ) {
                        // Check the results.
                        if( rdr.HasRows ) {
                            // We have a matching reader.
                            return false;
                        } else {
                            // We don't have a matching reader.
                            return true;
                        }
                    }
                }
            }
        }

        // Query to get postcards to or from the selected userID.
        private static bool TryGetPostcards(
            int userID, out List< CardDetails > postcards, bool from = false 
        ) {
            // Other users' details.
            var cards = new List< CardDetails >();

            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a parameterized query of cards' details.
                using( var cmd = con.CreateCommand() ) {
                    // Users where userID do not match userID.
                    cmd.CommandText =  "select *                                          "
                                    +  "from   Postcards                                  "
                                    + $"where  { ( from ? "fromID" : "toID" ) } = @UserID ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    using( MySqlDataReader rdr = cmd.ExecuteReader() ) {
                        // Check the results.
                        if( rdr.HasRows ) {
                            // We have postcards.
                            while( rdr.Read() ) {
                                // Add postcard's details to list.
                                cards.Add( new CardDetails {
                                    cardID  = int.Parse(      rdr[ "cardID"  ].ToString() )
                                ,   fromID  = int.Parse(      rdr[ "fromID"  ].ToString() )
                                ,   toID    = int.Parse(      rdr[ "toID"    ].ToString() )
                                ,   date    = DateTime.Parse( rdr[ "date"    ].ToString() )
                                ,   message =                 rdr[ "message" ].ToString()
                                } );
                            }

                            // Return the users' details with success.
                            postcards = cards;

                            return true;
                        } else {
                            // Return empty with failure.
                            postcards = cards;

                            return false;
                        }
                    }
                }
            }
        }

        // Display the selected postcard to the console.
        private static void Postcard( CardDetails postcard ) {
            UserDetails from;            // Details of the sender.
            UserDetails to;              // Details of the recipient.
            List< UserDetails > readers; // Details of readers of the postcard.

            // Get details of users that read the postcard, if any.
            bool hasReaders = TryGetReaders( postcard.cardID, out readers );

            // Format is nicely.
            var  readerList = string.Join(
                ", "
            ,   readers.Select( reader => 
                    reader.nameUser
                +   ( reader.loggedIn ? " (Online)" : "" )
                )
            );

            // Try to get the sender's and recipient's details.
            if( TryGetUserDetails( postcard.fromID, out from ) &&
                TryGetUserDetails( postcard.toID,   out to   ) ) {
                // Loading sender's, recipient's, and readers' details succeeded.
                Console.WriteLine( 
                     "Here's the postcard you selected:\n\n"
                +   $"From:    { from.nameUser }\n"
                +   $"To:      { to.nameUser   }\n"
                +   $"Date:    { postcard.date.ToString( "yyyy-MM-dd") }\n"
                +   $"Read By: { ( hasReaders ? readerList : "Nobody" ) }\n\n"
                +    "Message:\n\n"
                +   postcard.message
                );
            } else {
                // Loading sender and recipient details failed.
                Console.WriteLine(
                    "Oh, no...  Something went wrong reading the postcard!\n"
                +   "Please check your connection and try again later.  Sorry about this."
                );
            }
        }

        // Query to get details of users that read a postcard, if any.
        private static Boolean TryGetReaders( int cardID, out List< UserDetails > readers ) {
            // Other users' details.
            var users = new List< UserDetails >();

            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a parameterized query of users' details.
                using( var cmd = con.CreateCommand() ) {
                    // Users where userID do not match userID.
                    cmd.CommandText = "select *                   "
                                    + "from   Readers as R        "
                                    + "join   Users   as U        "
                                    + "on     R.userID = U.userID "
                                    + "where  R.cardID = @CardID  ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@CardID", cardID );

                    // Execute the query.
                    using( MySqlDataReader rdr = cmd.ExecuteReader() ) {
                        // Check the results.
                        if( rdr.HasRows ) {
                            // We have other users.
                            while( rdr.Read() ) {
                                // Add user's details to list.
                                users.Add( new UserDetails {
                                    userID    = int.Parse(  rdr[ "userID"    ].ToString() )
                                ,   nameUser  =             rdr[ "nameUser"  ].ToString()
                                ,   nameFirst =             rdr[ "nameFirst" ].ToString()
                                ,   nameLast  =             rdr[ "nameLast"  ].ToString()
                                ,   loggedIn  = bool.Parse( rdr[ "loggedIn"  ].ToString() )
                                ,   password  = ""
                                } );
                            }

                            // Return the readers' details with success.
                            readers = users;

                            return true;
                        } else {
                            // Return empty with failure.
                            readers = users;

                            return false;
                        }
                    }
                }
            }
        }

        // Prompt the user with a menu of user profiles other
        // than the user, if there are any.  If not, divert the
        // user accordingly.
        private static void Users( int userID ) {
            // Other user menu options.
            var optCancel = new CancelOption(
                "Nevermind - Go Back"
            );

            // Other user menu.
            var menuOthers = new SuperMenu(
                "Which user's profile do you want to check out?"
            );

            
            var        others = new List< UserDetails >(); // Other user's details.
            MenuOption choice = null;                      // Cath the user's choice.

            // Attempt to get the other users' details.
            if( TryGetOthersDetails( userID, out others ) ) {
                // There are other users and; consequently, no connectivity issues.
                others.ForEach( details =>
                    menuOthers.Add( new ActionOnlyOption( 
                        details.nameUser
                    +   ( details.loggedIn ? " - Online Now!" : "" )
                    ,   () => ProfileOther( userID, details )
                    ) )
                );

                menuOthers.Add( optCancel );

                // Run the menu until the user is done.
                while( choice != optCancel ) {
                    choice = menuOthers.Run();

                    // Don't pause twice on logout.
                    if( choice != optCancel ) Pause();
                }

            } else {
                Console.WriteLine(
                    "Hmmm...  There doesn't appear to be any other users.\n"
                +   "Also, check your connection and try again.  That could be the problem."
                );
            }
        }

        // Query for all users details, other than the logged in user.
        private static bool TryGetOthersDetails( int userID, out List< UserDetails > others ) {
            // Other users' details.
            var users = new List< UserDetails >();

            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a parameterized query of users' details.
                using( var cmd = con.CreateCommand() ) {
                    // Users where userID do not match userID.
                    cmd.CommandText = "select *                 "
                                    + "from   Users             "
                                    + "where  userID <> @UserID ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    using( MySqlDataReader rdr = cmd.ExecuteReader() ) {
                        // Check the results.
                        if( rdr.HasRows ) {
                            // We have other users.
                            while( rdr.Read() ) {
                                // Add user's details to list.
                                users.Add( new UserDetails {
                                    userID    = int.Parse(  rdr[ "userID"    ].ToString() )
                                ,   nameUser  =             rdr[ "nameUser"  ].ToString()
                                ,   nameFirst =             rdr[ "nameFirst" ].ToString()
                                ,   nameLast  =             rdr[ "nameLast"  ].ToString()
                                ,   loggedIn  = bool.Parse( rdr[ "loggedIn"  ].ToString() )
                                ,   password  = ""
                                } );
                            }

                            // Return the users' details with success.
                            others = users;

                            return true;
                        } else {
                            // Return empty with failure.
                            others = users;

                            return false;
                        }
                    }
                }
            }
        }

        // Display the selected user's profile with options to
        // send the user a postcard or go back.
        private static void ProfileOther( int userID, UserDetails details ) {
            // Options for profile menu.
            var optWrite    = new ActionOnlyOption(
                $"Send { details.nameUser } a Postcard!"
            ,   () => Write( userID, details )
            );

            var optRead     = new ActionOnlyOption(
                $"Read { details.nameUser }'s Postcards!"
            ,   () => Postcards( details.userID, userID )
            );

            var optDone     = new CancelOption(
                "Done - Go Back to the Last Screen"
            );

            // Profile menu.
            var menuProfile = new SuperMenu(
                error : "Oops!  That one isn't available.  Try again."
            ) {
                optWrite, optRead, optDone
            };


            MenuOption choice = null; // Catch user's choice.
            string     prompt = null; // For building menu prompt.

            // Run the welcome menu until the user logs out.
            while( choice != optDone ) {
                prompt = $"Here { details.nameUser }'s profile.\n\n"
                        +   $"- Username:   { details.nameUser  }\n"
                        +   $"- First Name: { details.nameFirst }\n"
                        +   $"- Last Name:  { details.nameLast  }\n\n";

                menuProfile.Prompt = prompt + "What would you like to do now?";
                choice             = menuProfile.Run();

                // Don't pause twice on exit.
                if( choice != optDone ) Pause();
            };
        }

        // Prompt the user for a postcard message to send to the selected user.
        // Long-form user-entry in the console is problematic, at best, so
        // give the user a chance to review and send the message, or cancel
        // sending the postcard with confirmation.
        private static void Write( int userID, UserDetails details ) {
            // The whole postcard.
            var postcard = new CardDetails {
                fromID   = userID
            ,   toID     = details.userID
            ,   date     = DateTime.Today
            ,   message  = GetNewMessage( details.nameUser )
            };

            // Write menu options.
            var optSend = new CancelOption(
                $"Send Your Postcard to { details.nameUser } Now!"
            );

            var optEdit = new ActionOnlyOption(
                "Change Your Postcard's Message"
            ,   () => {
                    postcard.message = GetNewMessage(
                        details.nameUser, postcard.message );
                }
            );

            var optDiscard = new CancelOption(
                $"Discard Your Postcard to { details.nameUser } and Go Back"
            );

            // Local function to return up-to-date menu prompt.
            string getPrompt() => 
                "Here is your postcard:\n\n"
            +   "From: You\n"
            +  $"To:   { details.nameUser }\n"
            +  $"Date: { DateTime.Today.ToString( "yyyy-MM-dd") }\n\n"
            +  $"Body: \n\n{ postcard.message }"
            + "\n\nWhat would you like to do?";

            // Write menu.
            var menuWrite  = new SuperMenu(
                error : "That's not in the menu!  Try again."
            ) {
                optSend, optEdit, optDiscard
            };

            MenuOption choice = null; // Catch the user's choice.
            
            // Run the write menu until the user sends or discards the postcard.
            while( choice != optDiscard && choice != optSend ) {
                menuWrite.Prompt = getPrompt();
                choice = menuWrite.Run();

                // Confirm the user really wants to discard.
                if( choice == optDiscard ) {
                    // User canceled.
                    if( !Confirmed( "Are you sure you want to discard this postcard?" ) ) {
                    choice = null;
                    } else {
                        // The cancel is a sure thing.  Leave Dodge.
                        return;
                    }
                }
            }

            // Send the postcard and mark it as read by the user (sender)
            // if possible, informing the user of the outcome.
            if( choice == optSend && TrySendPostcard( ref postcard ) ) {
                // Postcard sent successfully.  Marking it as read by the user
                // (sender) could fail, but not a catastrophe.  Let the user know.
                Console.WriteLine( 
                    $"Congratulations... {details.nameFirst} got your postcard!\n"
                +   ( TryMarkAsRead( postcard.cardID, userID ) ? "" : "Your postcard may appear unread by you." )
                );
            } else {
                // Postcard failed to send.
                Console.WriteLine(
                    "Oh, no...  Something went wrong sending your postcard!\n"
                +   "Please check your connection and try again later.  Sorry about this."
                );
            }

            // Don't pause twice on logout.
            if( choice != optDiscard && choice != optSend ) Pause();
        }

        // Query to insert a postcard and it's reader into Readers.
        private static bool TryMarkAsRead( int cardID, int userID ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query to insert the postcard.
                using( var cmd = con.CreateCommand() ) {
                    // Logged in is false for matching userID
                    cmd.CommandText = "insert ignore into Readers "
                                    + "set cardID = @CardID,      "
                                    + "    userID = @UserID       ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@CardID", cardID );
                    cmd.Parameters.AddWithValue( "@UserID", userID );

                    // Execute the query.
                    if( cmd.ExecuteNonQuery() == 1 ) {
                        // Success means a row got inserted.
                        return true;
                    } else {
                        // Anything else means something went wrong.
                        return false;
                    };
                }
            }
        }

        // Query to insert postcard to Postcards table in the database.
        private static bool TrySendPostcard( ref CardDetails postcard ) {
            // Connect to the database.
            using( var con = new MySqlConnection( cs ) ) {
                con.Open();

                // Issue a query to insert the postcard.
                using( var cmd = con.CreateCommand() ) {
                    // Logged in is false for matching userID
                    cmd.CommandText = "insert into Postcards( "
                                    + "  fromID               "
                                    + ", toID                 "
                                    + ", date                 "
                                    + ", message )            "
                                    + "values(                "
                                    + "  @FromID              "
                                    + ", @ToID                "
                                    + ", @Date                "
                                    + ", @Message )           ";

                    // Parameterize to avoid SQL injection.
                    cmd.Parameters.AddWithValue( "@FromID",  postcard.fromID  );
                    cmd.Parameters.AddWithValue( "@ToID",    postcard.toID    );
                    cmd.Parameters.AddWithValue( "@Date",    postcard.date    );
                    cmd.Parameters.AddWithValue( "@Message", postcard.message );

                    // Execute the query.
                    if( cmd.ExecuteNonQuery() == 1 ) {
                        // Success means a row got inserted.
                        // Update the postcard's cardID.
                        postcard.cardID = (int) cmd.LastInsertedId;

                        return true;
                    } else {
                        // Anything else means something went wrong.
                        return false;
                    };
                }
            }
        }

        // Prompt user for message to send to selected user.  Provide a copy
        // of the previous draft, if any, for user reference.
        private static string GetNewMessage( string username, string message = null ) {
            return PromptFor< string > (
                ( message == null ? "" : $"Here's your previous draft for reference:\n\n{ message }\n\n")
            +   $"What is your postcard's { ( message == null ? "" : "new " ) }message to { username }?\n"
            +   "Make sure it isn't empty or over 1,024 characters.\n"
            +   "Keep in mind that hitting <Enter> finishes the message,\n"
            +   "so don't try to separate paragraphs with blank lines."
            ,   "Oops... That was too long or too short.  Can't send that!  Try again."
            ,   any => !string.IsNullOrEmpty( any ) && any.Length <= 1024
            );
        }
    }
}
