using System;

namespace Utilities.Terminal
{
    public class ExitOption : MenuOption
    {
        public ExitOption( string text = "Exit the Application" ) : base( text, true, null ) { }

        // Exit option should never be disabled.
        public new bool Enabled
        {
            get { return true;   }
            set { base.Enable(); }
        }

        // Exit option should never signal against conditions.
        public new bool Signal { get { return true; } }

        public override bool Run()
        {
            Environment.Exit( 1 );

            return true;
        }
    }
}
