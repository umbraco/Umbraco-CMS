namespace Umbraco.Cms.Core.Search.Querying.Sorting;

/// <summary>
/// Sorts search results by relevance score.
/// </summary>
/// <param name="Direction">The sort direction.</param>
public record ScoreSorter(Direction Direction)
    : Sorter(string.Empty, Direction)
{
}
