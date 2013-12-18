using System;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// String extension methods used specifically to translate into SQL
    /// </summary>
    internal static class SqlStringExtensions
    {
        public static bool SqlContains(this string str, string txt, TextColumnType columnType)
        {
            return str.InvariantContains(txt);
        }

        public static bool SqlEquals(this string str, string txt, TextColumnType columnType)
        {
            return str.InvariantEquals(txt);
        }

        public static bool SqlStartsWith(this string str, string txt, TextColumnType columnType)
        {
            return str.InvariantStartsWith(txt);
        }

        public static bool SqlEndsWith(this string str, string txt, TextColumnType columnType)
        {
            return str.InvariantEndsWith(txt);
        }
    }
}