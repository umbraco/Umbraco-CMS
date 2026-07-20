namespace Umbraco.Cms.Search.Core.Extensions;

internal static class ArrayExtensions
{
    internal static T[]? NullIfEmpty<T>(this T[] source)
        => source.Length > 0 ? source : null;
}
