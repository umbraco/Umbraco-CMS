using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Tests.Search.Integration.Extensions;

internal static class IndexValueExtensions
{
    public static IEnumerable<string> AllTexts(this IndexValue indexValue)
        => indexValue.TextsR1.EmptyNull()
            .Union(indexValue.TextsR2.EmptyNull())
            .Union(indexValue.TextsR3.EmptyNull())
            .Union(indexValue.Texts.EmptyNull());
}
