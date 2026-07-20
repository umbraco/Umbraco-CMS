namespace Umbraco.Cms.Search.Core.Extensions;

internal static class ListExtensions
{
    public static List<T>? NullIfEmpty<T>(this List<T> source)
        => source.Count > 0 ? source : null;
}
