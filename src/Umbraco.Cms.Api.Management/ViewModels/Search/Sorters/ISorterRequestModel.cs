using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Search.Sorters;

/// <summary>
/// Represents a sorter in a search request. The concrete sorter type is identified by the "$type" discriminator.
/// </summary>
public interface ISorterRequestModel : IOpenApiDiscriminator
{
    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    Direction Direction { get; set; }
}
