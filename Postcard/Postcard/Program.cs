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

        private const string cs = "";

        #endregion

        #region General Module Variables

        // The currently logged in user.
        private static int? userLoggedIn = null;

        // User other than logged in user.
        private static int? userOther    = null;

        #endregion

        #region Menus and Menu Options

            #region Start Menu

            private static MenuOption optLogin = new ActionOnlyOption(
                "Log Into Your Postcard Account"
            ,   Login
            ,   conditions : new List< Func< bool > >() {
                    () => userLoggedIn == null
                }
            );

            private static MenuOption optJoin  = new ActionOnlyOption(
                "Join Today! - Register a New Postcard Account"
            ,   Join
            ,   conditions : new List< Func< bool > >() {
                    () => userLoggedIn == null
                }
            );

            private static MenuOption optExit  = new ExitOption(
                "Exit Postcard"
            );

            private static SuperMenu menuStart = new SuperMenu(
                "This is Postcard!  Choose from below to get started."
            ,   "Oops!  That one isn't available.  Try again."
            ) {
                optJoin, optLogin, optExit
            };

            #endregion

        #endregion

        // Program entry point, essentially "Start".
        public static void Main( string[] args ) {
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

        // Login user, sending to welcome screen on success
        // and returning to start screen on failure.
        private static void Login() {
            // Get the user's username and password.
            string username = GetUsername();
            string password = GetPassword();

            // Attempt to log the user in.
            bool   matches  = false;

            // TODO: Query database for matching UserID, toggling
            //       matches accordingly.

            // Check if login succeeded, informing and diverting
            // the user accordinly.
            if( matches ) {
                // Login succeeded.
                // TODO: Query database to update logged in status.
                Welcome();
            } else {
                // Login failed.
                Console.WriteLine( "Sorry!  That username or password didn't quite work." );
                Pause();
            }
        }

        private static string GetUsername() {
            return "";
        }

        private static string GetPassword() {
            return "";
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

        private static void Welcome() {

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
