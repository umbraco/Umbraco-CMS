using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a builder that creates <see cref="IApiContentResponse"/> instances from published content.
/// </summary>
public interface IApiContentResponseBuilder
{
    /// <summary>
    ///     Builds an <see cref="IApiContentResponse"/> instance from the specified published content.
    /// </summary>
    /// <param name="content">The published content to build from.</param>
    /// <returns>An <see cref="IApiContentResponse"/> instance, or <c>null</c> if the content cannot be built.</returns>
    IApiContentResponse? Build(IPublishedContent content);
}
