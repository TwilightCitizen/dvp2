using System;
using System.Collections.Generic;

namespace Utilities.General
{
    interface ISignal
    {
        List< Func< bool > > Conditions { get; set; }

        bool Signal { get; }
    }
}
