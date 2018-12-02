using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise_4
{
    // Extend the string class with some useful methods.
    public static class StringExtensions
    {
        // Gives string the ability to return the proper case
        // of itself, i.e. Proper, as opposed to UPPER or lower.
        public static string ToProper( this string any )
        {
            return string.IsNullOrEmpty( any ) ? any :  // Guard against null/empty.
                any.Length == 1 ? any.ToUpper() :       // Length of 1, just uppercase it.
                    any.Substring( 0, 1 ).ToUpper() +   // Otherwise, uppercase the first
                    any.Substring( 1, any.Length - 1 ); // letter and join with the rest.
        }
    }
}
