using Umbraco.Cms.Core;

namespace Umbraco.Cms.Search.Core.Models.Searching.Sorting;

public record DateTimeOffsetSorter(string FieldName, Direction Direction)
    : Sorter(FieldName, Direction)
{
}
