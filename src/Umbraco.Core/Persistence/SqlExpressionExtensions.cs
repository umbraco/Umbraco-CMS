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

    /// <summary>
    ///     Indicates whether a collection contains a specified item for use in SQL IN expressions.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="item">The item to locate.</param>
    /// <returns><c>true</c> if the item is found in the collection; otherwise, <c>false</c>.</returns>
    /// <remarks>Do not use outside of SQL expressions.</remarks>
    public static bool SqlIn<T>(this IEnumerable<T> collection, T item) => collection.Contains(item);

    /// <summary>
    ///     Performs a wildcard match on a string, where % represents any sequence of characters.
    /// </summary>
    /// <param name="str">The string to match against.</param>
    /// <param name="txt">The pattern containing % wildcards.</param>
    /// <param name="columnType">The type of the text column.</param>
    /// <returns><c>true</c> if the string matches the pattern; otherwise, <c>false</c>.</returns>
    /// <remarks>Do not use outside of SQL expressions.</remarks>
    public static bool SqlWildcard(this string str, string txt, TextColumnType columnType)
    {
        var wildcardmatch = new Regex("^" + Regex.Escape(txt).

            // deal with any wildcard chars %
            Replace(@"\%", ".*") + "$");

        return wildcardmatch.IsMatch(str);
    }

#pragma warning disable IDE0060 // Remove unused parameter
    /// <summary>
    ///     Indicates whether a string contains another string using culture-invariant comparison.
    /// </summary>
    /// <param name="str">The string to search within.</param>
    /// <param name="txt">The string to find.</param>
    /// <param name="columnType">The type of the text column.</param>
    /// <returns><c>true</c> if the string contains the specified text; otherwise, <c>false</c>.</returns>
    /// <remarks>Do not use outside of SQL expressions.</remarks>
    public static bool SqlContains(this string str, string txt, TextColumnType columnType) =>
        str.InvariantContains(txt);

    /// <summary>
    ///     Indicates whether two strings are equal using culture-invariant comparison.
    /// </summary>
    /// <param name="str">The first string to compare.</param>
    /// <param name="txt">The second string to compare.</param>
    /// <param name="columnType">The type of the text column.</param>
    /// <returns><c>true</c> if the strings are equal; otherwise, <c>false</c>.</returns>
    /// <remarks>Do not use outside of SQL expressions.</remarks>
    public static bool SqlEquals(this string str, string txt, TextColumnType columnType) => str.InvariantEquals(txt);

    /// <summary>
    ///     Indicates whether a string starts with another string using culture-invariant comparison.
    /// </summary>
    /// <param name="str">The string to search within.</param>
    /// <param name="txt">The string to find at the start.</param>
    /// <param name="columnType">The type of the text column.</param>
    /// <returns><c>true</c> if the string starts with the specified text; otherwise, <c>false</c>.</returns>
    /// <remarks>Do not use outside of SQL expressions.</remarks>
    public static bool SqlStartsWith(this string? str, string txt, TextColumnType columnType) =>
        str?.InvariantStartsWith(txt) ?? false;

    /// <summary>
    ///     Indicates whether a string ends with another string using culture-invariant comparison.
    /// </summary>
    /// <param name="str">The string to search within.</param>
    /// <param name="txt">The string to find at the end.</param>
    /// <param name="columnType">The type of the text column.</param>
    /// <returns><c>true</c> if the string ends with the specified text; otherwise, <c>false</c>.</returns>
    /// <remarks>Do not use outside of SQL expressions.</remarks>
    public static bool SqlEndsWith(this string str, string txt, TextColumnType columnType) =>
        str.InvariantEndsWith(txt);
#pragma warning restore IDE0060 // Remove unused parameter
}
