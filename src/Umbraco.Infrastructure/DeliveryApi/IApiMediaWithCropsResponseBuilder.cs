using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

/// <summary>
/// Provides methods for constructing API responses that include media items along with their crop information.
/// </summary>
public interface IApiMediaWithCropsResponseBuilder
{
    /// <summary>
    /// Builds a response containing media information and crop data from the specified media content.
    /// </summary>
    /// <param name="media">The media content to generate the response from.</param>
    /// <returns>A response object containing media and crop information.</returns>
    IApiMediaWithCropsResponse Build(IPublishedContent media);
}
