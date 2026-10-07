using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// Represents a facet in a search request. The concrete facet type is identified by the "$type" discriminator.
/// </summary>
public interface IFacetRequestModel : IOpenApiDiscriminator
{
    /// <summary>
    /// Gets or sets the name of the field to facet on.
    /// </summary>
    string FieldName { get; set; }
}
