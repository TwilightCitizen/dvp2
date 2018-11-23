using System;
using System.Collections.Generic;

namespace Utilities.Terminal
{
    public class ActionOnlyOption : MenuOption
    {
        public Action Action { get; set; }

        public ActionOnlyOption( string text, Action action, bool enabled = true, List< Func< bool > > conditions = null )
            : base( text, enabled, conditions )
        {
            this.Action = action;
        }

        public override bool Run()
        {
            if( this.Enabled && this.Signal )
            {
                this.Action();

                return true;
            }

            return false;
        }
    }
}
