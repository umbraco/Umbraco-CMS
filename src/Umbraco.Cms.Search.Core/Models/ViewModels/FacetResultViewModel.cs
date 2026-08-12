using Umbraco.Cms.Search.Core.Models.Searching.Faceting;

namespace Umbraco.Cms.Search.Core.Models.ViewModels;

/// <summary>
/// The Management API representation of a facet result for a single field.
/// </summary>
public class FacetResultViewModel
{
    /// <summary>
    /// Gets or sets the name of the faceted field.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the facet values (buckets) and their match counts.
    /// </summary>
    public required IEnumerable<FacetValue> Values { get; set; }
}
