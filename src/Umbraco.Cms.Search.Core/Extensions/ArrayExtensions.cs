namespace Umbraco.Cms.Search.Core.Extensions;

/// <summary>
/// Provides extension methods for arrays.
/// </summary>
internal static class ArrayExtensions
{
    /// <summary>
    /// Returns the array, or null if it is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The array.</param>
    /// <returns>The array, or null if it has no elements.</returns>
    internal static T[]? NullIfEmpty<T>(this T[] source)
        => source.Length > 0 ? source : null;
}
