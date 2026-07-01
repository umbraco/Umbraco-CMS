using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.DeliveryApi;

/// <summary>
/// Represents a builder for creating API media objects that include crop information.
/// </summary>
public interface IApiMediaWithCropsBuilder
{
    /// <summary>Builds an <see cref="Umbraco.Cms.Infrastructure.DeliveryApi.IApiMediaWithCrops"/> instance from the specified <see cref="Umbraco.Cms.Core.Models.MediaWithCrops"/> media.</summary>
    /// <param name="media">The media with crops to build from.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.DeliveryApi.IApiMediaWithCrops"/> representing the built media with crops.</returns>
    IApiMediaWithCrops Build(MediaWithCrops media);

    /// <summary>Builds an <see cref="Umbraco.Cms.Infrastructure.DeliveryApi.IApiMediaWithCrops"/> instance from the specified <see cref="Umbraco.Cms.Core.Models.IPublishedContent"/> media.</summary>
    /// <param name="media">The published media content to build from.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.DeliveryApi.IApiMediaWithCrops"/> representing the built media with crops.</returns>
    IApiMediaWithCrops Build(IPublishedContent media);
}
