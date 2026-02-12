using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a builder that creates <see cref="IApiContentRoute"/> instances from published content.
/// </summary>
public interface IApiContentRouteBuilder
{
    /// <summary>
    ///     Builds an <see cref="IApiContentRoute"/> instance from the specified published content.
    /// </summary>
    /// <param name="content">The published content to build from.</param>
    /// <param name="culture">The culture to use for the route, or <c>null</c> for the default culture.</param>
    /// <returns>An <see cref="IApiContentRoute"/> instance, or <c>null</c> if the route cannot be built.</returns>
    IApiContentRoute? Build(IPublishedContent content, string? culture = null);
}
