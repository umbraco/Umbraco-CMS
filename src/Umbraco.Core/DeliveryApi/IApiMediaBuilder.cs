using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a builder that creates <see cref="IApiMedia"/> instances from published media.
/// </summary>
public interface IApiMediaBuilder
{
    /// <summary>
    ///     Builds an <see cref="IApiMedia"/> instance from the specified published media.
    /// </summary>
    /// <param name="media">The published media to build from.</param>
    /// <returns>An <see cref="IApiMedia"/> instance.</returns>
    IApiMedia Build(IPublishedContent media);
}
