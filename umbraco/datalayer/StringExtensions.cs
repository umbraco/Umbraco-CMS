using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.DataLayer
{
    public static class StringExtensions
    {

        /// <summary>
        /// This trims all white spaces from beginning and end of the string and removes any line breaks (repacing with a space)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string TrimToOneLine(this string str)
        {
            return str.Replace(Environment.NewLine, " ").Trim();
        }

    }
}
