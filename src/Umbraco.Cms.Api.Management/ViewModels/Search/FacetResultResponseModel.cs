using Umbraco.Cms.Core.Search.Querying.Faceting;

namespace Umbraco.Cms.Api.Management.ViewModels.Search;

/// <summary>
/// The Management API representation of a facet result for a single field.
/// </summary>
public class FacetResultResponseModel
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
