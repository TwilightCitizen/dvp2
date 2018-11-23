using System;
using System.Collections.Generic;

namespace Utilities.Terminal
{
    public class ActionSubMenuOption : MenuOption
    {
        public Action    Action { get; set; }
        public SuperMenu Menu   { get; set; }

        public ActionSubMenuOption( string text, Action action, SuperMenu menu, bool enabled = true, List< Func< bool > > conditions = null )
            : base( text, enabled, conditions )
        {
            this.Action = action;
            this.Menu   = menu;
        }

        public override bool Run()
        {
            if( this.Enabled && this.Signal )
            {
                this.Action();
                this.Menu.Run();

                return true;
            }

            return false;
        }
    }
}
