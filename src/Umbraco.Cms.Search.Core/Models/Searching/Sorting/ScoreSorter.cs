using Umbraco.Cms.Core;

namespace Umbraco.Cms.Search.Core.Models.Searching.Sorting;

/// <summary>
/// Sorts search results by relevance score.
/// </summary>
/// <param name="Direction">The sort direction.</param>
public record ScoreSorter(Direction Direction)
    : Sorter(string.Empty, Direction)
{
}
