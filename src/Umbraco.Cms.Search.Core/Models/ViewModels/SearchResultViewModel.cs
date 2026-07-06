namespace Umbraco.Cms.Search.Core.Models.ViewModels;

public class SearchResultViewModel
{
    public long Total { get; set; }

    public required IEnumerable<DocumentViewModel> Documents { get; set; }

    public required IEnumerable<FacetResultViewModel> Facets { get; set; }
}
