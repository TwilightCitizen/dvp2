using System;
using System.Collections.Generic;

namespace Utilities.Terminal
{
    public class SubMenuOnlyOption : MenuOption
    {
        public SuperMenu Menu { get; set; }

        public SubMenuOnlyOption( string text, SuperMenu menu, bool enabled = true, List< Func< bool > > conditions = null )
            : base( text, enabled, conditions )
        {
            this.Menu = menu;
        }

        public override bool Run()
        {
            if( this.Enabled && this.Signal )
            {
                this.Menu.Run();

                return true;
            }

            return false;
        }
    }
}
