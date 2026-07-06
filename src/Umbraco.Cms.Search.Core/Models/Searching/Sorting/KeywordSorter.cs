using Umbraco.Cms.Core;

namespace Umbraco.Cms.Search.Core.Models.Searching.Sorting;

public record KeywordSorter(string FieldName, Direction Direction)
    : Sorter(FieldName, Direction)
{
}
