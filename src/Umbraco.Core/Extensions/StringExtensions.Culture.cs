// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;

namespace Umbraco.Extensions;

public static partial class StringExtensions
{
    /// <summary>
    /// Determines whether the specified character is lowercase using invariant culture rules.
    /// </summary>
    /// <param name="ch">The character to check.</param>
    /// <returns><c>true</c> if the character is lowercase; otherwise, <c>false</c>.</returns>
    public static bool IsLowerCase(this char ch) => ch == char.ToLowerInvariant(ch);

    /// <summary>
    /// Determines whether the specified character is uppercase using invariant culture rules.
    /// </summary>
    /// <param name="ch">The character to check.</param>
    /// <returns><c>true</c> if the character is uppercase; otherwise, <c>false</c>.</returns>
    public static bool IsUpperCase(this char ch) => ch == char.ToUpperInvariant(ch);

    /// <summary>
    /// Formats the string using the invariant culture.
    /// </summary>
    /// <param name="format">The composite format string.</param>
    /// <param name="args">The objects to format.</param>
    /// <returns>A formatted string using invariant culture formatting rules.</returns>
    public static string InvariantFormat(this string? format, params object?[] args) =>
        string.Format(CultureInfo.InvariantCulture, format ?? string.Empty, args);

    /// <summary>
    /// Converts an integer to its string representation using the invariant culture.
    /// </summary>
    /// <param name="str">The integer value to convert.</param>
    /// <returns>The string representation of the integer using invariant culture formatting.</returns>
    public static string ToInvariantString(this int str) => str.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts a long integer to its string representation using the invariant culture.
    /// </summary>
    /// <param name="str">The long integer value to convert.</param>
    /// <returns>The string representation of the long integer using invariant culture formatting.</returns>
    public static string ToInvariantString(this long str) => str.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Compares two strings for equality using case-insensitive invariant culture comparison.
    /// </summary>
    /// <param name="compare">The first string to compare.</param>
    /// <param name="compareTo">The second string to compare.</param>
    /// <returns><c>true</c> if the strings are equal ignoring case; otherwise, <c>false</c>.</returns>
    public static bool InvariantEquals(this string? compare, string? compareTo) =>
        string.Equals(compare, compareTo, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Determines whether the beginning of this string matches the specified string using case-insensitive invariant culture comparison.
    /// </summary>
    /// <param name="compare">The string to check.</param>
    /// <param name="compareTo">The string to compare to the beginning of this string.</param>
    /// <returns><c>true</c> if the string starts with the specified value ignoring case; otherwise, <c>false</c>.</returns>
    public static bool InvariantStartsWith(this string compare, string compareTo) =>
        compare.StartsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Determines whether the end of this string matches the specified string using case-insensitive invariant culture comparison.
    /// </summary>
    /// <param name="compare">The string to check.</param>
    /// <param name="compareTo">The string to compare to the end of this string.</param>
    /// <returns><c>true</c> if the string ends with the specified value ignoring case; otherwise, <c>false</c>.</returns>
    public static bool InvariantEndsWith(this string compare, string compareTo) =>
        compare.EndsWith(compareTo, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// Determines whether the string contains the specified substring using case-insensitive ordinal comparison.
    /// </summary>
    /// <param name="compare">The string to search within.</param>
    /// <param name="compareTo">The substring to search for.</param>
    /// <returns><c>true</c> if the string contains the specified substring ignoring case; otherwise, <c>false</c>.</returns>
    public static bool InvariantContains(this string compare, string compareTo) =>
        compare.Contains(compareTo, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the collection contains the specified string using case-insensitive invariant culture comparison.
    /// </summary>
    /// <param name="compare">The collection of strings to search within.</param>
    /// <param name="compareTo">The string to search for.</param>
    /// <returns><c>true</c> if the collection contains the specified string ignoring case; otherwise, <c>false</c>.</returns>
    public static bool InvariantContains(this IEnumerable<string> compare, string compareTo) =>
        compare.Contains(compareTo, StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified string using case-insensitive ordinal comparison.
    /// </summary>
    /// <param name="s">The string to search within.</param>
    /// <param name="value">The string to search for.</param>
    /// <returns>The zero-based index position of the value if found, or -1 if not found.</returns>
    public static int InvariantIndexOf(this string s, string value) =>
        s.IndexOf(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Reports the zero-based index of the last occurrence of the specified string using case-insensitive ordinal comparison.
    /// </summary>
    /// <param name="s">The string to search within.</param>
    /// <param name="value">The string to search for.</param>
    /// <returns>The zero-based index position of the last occurrence of the value if found, or -1 if not found.</returns>
    public static int InvariantLastIndexOf(this string s, string value) =>
        s.LastIndexOf(value, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns a copy of the string with the first character converted to uppercase.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A string with the first character in uppercase, or the original string if it is null or whitespace.</returns>
    public static string ToFirstUpper(this string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToUpper() + input[1..];

    /// <summary>
    /// Returns a copy of the string with the first character converted to lowercase.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A string with the first character in lowercase, or the original string if it is null or whitespace.</returns>
    public static string ToFirstLower(this string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToLower() + input[1..];

    /// <summary>
    /// Returns a copy of the string with the first character converted to uppercase using the casing rules of the specified culture.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="culture">The culture that provides the casing rules.</param>
    /// <returns>A string with the first character in uppercase, or the original string if it is null or whitespace.</returns>
    public static string ToFirstUpper(this string input, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToUpper(culture) + input[1..];

    /// <summary>
    /// Returns a copy of the string with the first character converted to lowercase using the casing rules of the specified culture.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <param name="culture">The culture that provides the casing rules.</param>
    /// <returns>A string with the first character in lowercase, or the original string if it is null or whitespace.</returns>
    public static string ToFirstLower(this string input, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToLower(culture) + input[1..];

    /// <summary>
    /// Returns a copy of the string with the first character converted to uppercase using the casing rules of the invariant culture.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A string with the first character in uppercase, or the original string if it is null or whitespace.</returns>
    public static string ToFirstUpperInvariant(this string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToUpperInvariant() + input[1..];

    /// <summary>
    /// Returns a copy of the string with the first character converted to lowercase using the casing rules of the invariant culture.
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>A string with the first character in lowercase, or the original string if it is null or whitespace.</returns>
    public static string ToFirstLowerInvariant(this string input) =>
        string.IsNullOrWhiteSpace(input)
            ? input
            : input[..1].ToLowerInvariant() + input[1..];

    /// <summary>
    /// Verifies the provided string is a valid culture code and returns it with consistent casing.
    /// </summary>
    /// <param name="culture">The culture code to validate and normalise.</param>
    /// <returns>
    /// The culture code in standard casing, or the original value if null, empty, or the wildcard "*".
    /// </returns>
    /// <exception cref="CultureNotFoundException">Thrown when the culture code is not valid.</exception>
    public static string? EnsureCultureCode(this string? culture)
    {
        if (string.IsNullOrEmpty(culture) || culture == "*")
        {
            return culture;
        }

        // Create as CultureInfo instance from provided name so we can ensure consistent casing of culture code when persisting.
        // This will accept mixed case but once created have a `Name` property that is consistently and correctly cased.
        // Will throw if an invalid culture code is provided.
        return new CultureInfo(culture).Name;
    }
}
