namespace Umbraco.Cms.Core.Extensions;

public static class CollectionExtensions
{
    // Easiest way to return a collection with 1 item, probably not the most performant
    public static ICollection<T> ToSingleItemCollection<T>(this T item) =>
        new T[] { item };
}
