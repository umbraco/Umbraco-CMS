using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a builder that creates <see cref="IApiContent"/> instances from published content.
/// </summary>
public interface IApiContentBuilder
{
    /// <summary>
    ///     Builds an <see cref="IApiContent"/> instance from the specified published content.
    /// </summary>
    /// <param name="content">The published content to build from.</param>
    /// <returns>An <see cref="IApiContent"/> instance, or <c>null</c> if the content cannot be built.</returns>
    IApiContent? Build(IPublishedContent content);
}
