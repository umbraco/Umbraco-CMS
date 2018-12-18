using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// String extension methods used specifically to translate into SQL
    /// </summary>
    internal static class SqlExpressionExtensions
    {
        /// <summary>
        /// Indicates whether two nullable values are equal, substituting a fallback value for nulls.
        /// </summary>
        /// <typeparam name="T">The nullable type.</typeparam>
        /// <param name="value">The value to compare.</param>
        /// <param name="other">The value to compare to.</param>
        /// <param name="fallbackValue">The value to use when any value is null.</param>
        /// <remarks>Do not use outside of Sql expressions.</remarks>
        // see usage in ExpressionVisitorBase
        public static bool SqlNullableEquals<T>(this T? value, T? other, T fallbackValue)
            where T : struct
        {
            return (value ?? fallbackValue).Equals(other ?? fallbackValue);
        }

        public static bool SqlIn<T>(this IEnumerable<T> collection, T item)
        {
            return collection.Contains(item);
        }

        public static bool SqlWildcard(this string str, string txt, TextColumnType columnType)
        {
            var wildcardmatch = new Regex("^" + Regex.Escape(txt).
                                                    //deal with any wildcard chars %
                                                      Replace(@"\%", ".*") + "$");

            return wildcardmatch.IsMatch(str);
        }

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
