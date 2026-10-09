using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Api.Management.ViewModels.Search.Facets;

/// <summary>
/// Represents one bucket of a facet result. The concrete bucket type is identified by the "$type" discriminator.
/// </summary>
public interface IFacetValueResponseModel : IOpenApiDiscriminator
{
    /// <summary>
    /// Gets or sets the number of matching documents in this bucket.
    /// </summary>
    long Count { get; set; }
}
