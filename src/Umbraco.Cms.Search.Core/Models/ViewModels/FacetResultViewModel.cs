using Umbraco.Cms.Search.Core.Models.Searching.Faceting;

namespace Umbraco.Cms.Search.Core.Models.ViewModels;

public class FacetResultViewModel
{
    public required string FieldName { get; set; }

    public required IEnumerable<FacetValue> Values { get; set; }
}
