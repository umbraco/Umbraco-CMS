namespace Umbraco.Cms.Core.Extensions;

public static class CollectionExtensions
{
    [Obsolete("Please replace uses of this extension method with collection expression. This method will be removed in Umbraco 16.")]
    public static ICollection<T> ToSingleItemCollection<T>(this T item) =>
        new T[] { item };
}
