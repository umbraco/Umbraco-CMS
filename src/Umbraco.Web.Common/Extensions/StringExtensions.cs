namespace Umbraco.Cms.Web.Common.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// Provides a robust way to check if a path starts with another path, normalizing multiple slashes.
    /// </summary>
    internal static bool StartsWithNormalizedPath(this string path, string other, StringComparison comparisonType = StringComparison.Ordinal)
    {
        // First check without normalizing.
        if (path.StartsWith(other, comparisonType))
        {
            return true;
        }

        // Normalize paths by splitting them into segments, removing multiple slashes.
        var otherSegments = other.Split(Core.Constants.CharArrays.ForwardSlash, StringSplitOptions.RemoveEmptyEntries);
        var pathSegments = path.Split(Core.Constants.CharArrays.ForwardSlash, otherSegments.Length + 1, StringSplitOptions.RemoveEmptyEntries);

        // The path should have at least as many segments as the other path
        if (otherSegments.Length > pathSegments.Length)
        {
            return false;
        }

        // Check each segment.
        for (int i = otherSegments.Length - 1; i >= 0; i--)
        {
            if (!string.Equals(otherSegments[i], pathSegments[i], comparisonType))
            {
                return false;
            }
        }

        // All segments match.
        return true;
    }
}
