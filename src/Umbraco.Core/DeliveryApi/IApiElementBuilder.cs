using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a builder that creates <see cref="IApiElement"/> instances from published elements.
/// </summary>
public interface IApiElementBuilder
{
    /// <summary>
    ///     Builds an <see cref="IApiElement"/> instance from the specified published element.
    /// </summary>
    /// <param name="element">The published element to build from.</param>
    /// <returns>An <see cref="IApiElement"/> instance.</returns>
    IApiElement Build(IPublishedElement element);
}
