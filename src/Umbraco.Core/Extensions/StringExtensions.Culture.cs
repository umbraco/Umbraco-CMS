// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;

namespace Umbraco.Extensions;

/// <summary>
/// Culture and invariant comparison extensions.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    ///     formats the string with invariant culture
    /// </summary>
    /// <param name="format">The format.</param>
    /// <param name="args">The args.</param>
    /// <returns></returns>
    public static string InvariantFormat(this string? format, params object?[] args) =>
        string.Format(CultureInfo.InvariantCulture, format ?? string.Empty, args);

    /// <summary>
    ///     Converts an integer to an invariant formatted string
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToInvariantString(this int str) => str.ToString(CultureInfo.InvariantCulture);

    public static string ToInvariantString(this long str) => str.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    ///     Compares 2 strings with invariant culture and case ignored
    /// </summary>
    /// <param name="compare">The compare.</param>
    /// <param name="compareTo">The compare to.</param>
    /// <returns></returns>
    public static bool InvariantEquals(this string? compare, string? compareTo) =>
        string.Equals(compare, compareTo, StringComparison.InvariantCultureIgnoreCase);

    public static bool InvariantStartsWith(this string compare, string compareTo) =>
        compare.StartsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);

    public static bool InvariantEndsWith(this string compare, string compareTo) =>
        compare.EndsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);

    public static bool InvariantContains(this string compare, string compareTo) =>
        compare.Contains(compareTo, StringComparison.OrdinalIgnoreCase);

    public static bool InvariantContains(this IEnumerable<string> compare, string compareTo) =>
        compare.Contains(compareTo, StringComparer.InvariantCultureIgnoreCase);

    public static int InvariantIndexOf(this string s, string value) =>
        s.IndexOf(value, StringComparison.OrdinalIgnoreCase);

    public static int InvariantLastIndexOf(this string s, string value) =>
        s.LastIndexOf(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Verifies the provided string is a valid culture code and returns it in a consistent casing.
    /// </summary>
    /// <param name="culture">Culture code.</param>
    /// <returns>Culture code in standard casing.</returns>
    public static string? EnsureCultureCode(this string? culture)
    {
        if (string.IsNullOrEmpty(culture) || culture == "*")
        {
            return culture;
        }

        // Create as CultureInfo instance from provided name so we can ensure consistent casing of culture code when persisting.
        // This will accept mixed case but once created have a `Name` property that is consistently and correctly cased.
        // Will throw in an invalid culture code is provided.
        return new CultureInfo(culture).Name;
    }
}
