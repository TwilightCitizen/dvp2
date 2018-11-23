using System;
using System.ComponentModel;

namespace Utilities.Terminal
{
    public static class IO
    {
        /* Prompt for and validate value from user. All my user prompts follow
        // the same general format.  As the user for a value of a certain kind
        // within certain constraints, make sure the user's input converts to
        // that kind and meets those constraints, let them know if it doesn't,
        // and if it does, use it and move on.  This function encapsulates all
        // that and handles any type, conversion, and condition.  
        */

        public static T PromptFor< T >( string promptInitial, string promptBadInput
                                      , Func< T, bool > checkConstraints )
        {
            string        input;
            T             retVal   = default;
            TypeConverter conv     = TypeDescriptor.GetConverter( typeof( T ) );
            bool          isValid;

            Console.WriteLine( "{0}\n", promptInitial );

            for(;;)//ever
            {
                input    = Console.ReadLine();
                isValid  = conv.IsValid( input );

                if( isValid )
                {
                    retVal   = (T) conv.ConvertFromString( input );
                    isValid &= checkConstraints( retVal );
                }

                if( isValid ) return retVal;

                Console.WriteLine( "{0}\n", promptBadInput );
            }
        }
    }
}
