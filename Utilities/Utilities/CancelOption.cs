namespace Utilities.Terminal
{
    public class CancelOption : MenuOption
    {
        public CancelOption( string text = "Cancel and Go Back to the Previous Screen" ) : base( text, true, null ) { }

        // Cancel option should never be disabled.
        public new bool Enabled
        {
            get { return true;   }
            set { base.Enable(); }
        }

        // Cancel option should never signal against conditions.
        public new bool Signal { get { return true; } }

        public override bool Run() { return true; }
    }
}
