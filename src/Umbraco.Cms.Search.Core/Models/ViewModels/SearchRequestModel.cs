using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Search.Core.Models.ViewModels;

public class SearchRequestModel
{
    public required string IndexAlias { get; set; }

    public string? Query { get; set; }

    public IEnumerable<Filter>? Filters { get; set; }

    public IEnumerable<Facet>? Facets { get; set; }

    public IEnumerable<Sorter>? Sorters { get; set; }

    public string? Culture { get; set; }

    public string? Segment { get; set; }
}
