using System;
using System.Security.Cryptography;
using System.Text;

namespace Utilities.Crypto
{
    public static class MD5Extensions
    {
        /* ToMD5 extends the string primitive type with a method
         * that converts it to and returns an MD5 hash string.
         * */

        public static string ToMD5( this string any )
        {
            MD5 md5 = MD5.Create();

            byte[]        inBytes  = Encoding.ASCII.GetBytes( any );
            byte[]        outBytes = md5.ComputeHash( inBytes );
            StringBuilder builder  = new StringBuilder();

            foreach( byte b in outBytes ) builder.Append( b.ToString( "X2" ) );

            return builder.ToString();
        }
    }
}
