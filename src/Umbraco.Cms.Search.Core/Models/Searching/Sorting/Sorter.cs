using Umbraco.Cms.Core;

namespace Umbraco.Cms.Search.Core.Models.Searching.Sorting;

public abstract record Sorter(string FieldName, Direction Direction)
{
}
