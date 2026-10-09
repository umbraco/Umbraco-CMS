namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// A facet that counts matching documents per distinct keyword value.
/// </summary>
public class KeywordFacetRequestModel : IFacetRequestModel
{
    /// <summary>
    /// Gets or sets the name of the field to facet on.
    /// </summary>
    public required string FieldName { get; set; }
}
