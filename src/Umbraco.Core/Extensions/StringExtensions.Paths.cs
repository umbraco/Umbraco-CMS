// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Extensions;

public static partial class StringExtensions
{
    /// <summary>
    /// Determines whether a tree path is below another tree path.
    /// </summary>
    /// <param name="path">The path to test, expected as a comma-delimited collection of integers (e.g., "-1,1234,5678").</param>
    /// <param name="ancestorPath">The path of the potential ancestor.</param>
    /// <returns><c>true</c> if the path is below the ancestor path; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Paths are comma-delimited IDs, so they have to be compared a whole segment at a time. Comparing the raw prefix
    /// would treat "-1,21" as being below "-1,2", as one ID happens to begin with the other.
    /// </remarks>
    public static bool IsDescendantOfPath(this string path, string ancestorPath)
        => path.StartsWith($"{ancestorPath},", StringComparison.Ordinal);

    /// <summary>
    /// Determines whether a tree path is another tree path, or below it.
    /// </summary>
    /// <param name="path">The path to test, expected as a comma-delimited collection of integers (e.g., "-1,1234,5678").</param>
    /// <param name="ancestorPath">The path of the potential ancestor, or of the node itself.</param>
    /// <returns><c>true</c> if the path is the ancestor path or below it; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Both paths get a trailing comma, so that a whole final segment has to match. Comparing the raw prefix would
    /// treat "-1,21" as being below "-1,2", as one ID happens to begin with the other.
    /// </remarks>
    public static bool IsDescendantOrSelfOfPath(this string path, string ancestorPath)
        => $"{path},".StartsWith($"{ancestorPath},", StringComparison.Ordinal);
}
