using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Persistence;

/// <summary>
///     String extension methods used specifically to translate into SQL
/// </summary>
public static class SqlExpressionExtensions
{
    /// <summary>
    ///     Indicates whether two nullable values are equal, substituting a fallback value for nulls.
    /// </summary>
    /// <typeparam name="T">The nullable type.</typeparam>
    /// <param name="value">The value to compare.</param>
    /// <param name="other">The value to compare to.</param>
    /// <param name="fallbackValue">The value to use when any value is null.</param>
    /// <remarks>Do not use outside of Sql expressions.</remarks>
    // see usage in ExpressionVisitorBase
    public static bool SqlNullableEquals<T>(this T? value, T? other, T fallbackValue)
        where T : struct =>
        (value ?? fallbackValue).Equals(other ?? fallbackValue);

    public static bool SqlIn<T>(this IEnumerable<T> collection, T item) => collection.Contains(item);

    public static bool SqlWildcard(this string str, string txt, TextColumnType columnType)
    {
        var wildcardmatch = new Regex("^" + Regex.Escape(txt).

            // deal with any wildcard chars %
            Replace(@"\%", ".*") + "$");

        return wildcardmatch.IsMatch(str);
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public static bool SqlContains(this string str, string txt, TextColumnType columnType) =>
        str.InvariantContains(txt);

    public static bool SqlEquals(this string str, string txt, TextColumnType columnType) => str.InvariantEquals(txt);

    public static bool SqlStartsWith(this string? str, string txt, TextColumnType columnType) =>
        str?.InvariantStartsWith(txt) ?? false;

    public static bool SqlEndsWith(this string str, string txt, TextColumnType columnType) =>
        str.InvariantEndsWith(txt);
#pragma warning restore IDE0060 // Remove unused parameter
}
