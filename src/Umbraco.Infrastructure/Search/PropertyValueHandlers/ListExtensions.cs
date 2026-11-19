namespace Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

/// <summary>
/// Provides extension methods for lists.
/// </summary>
internal static class ListExtensions
{
    /// <summary>
    /// Returns the list, or null if it is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The list.</param>
    /// <returns>The list, or null if it has no elements.</returns>
    internal static List<T>? NullIfEmpty<T>(this List<T> source)
        => source.Count > 0 ? source : null;
}
