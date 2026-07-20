using Umbraco.Cms.Core;

namespace Umbraco.Cms.Search.Core.Models.Searching.Sorting;

public record ScoreSorter(Direction Direction)
    : Sorter(string.Empty, Direction)
{
}
