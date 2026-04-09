using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Default implementation of <see cref="IApiContentPathProvider"/> that provides content paths for the Delivery API.
/// </summary>
/// <remarks>
///     This class is left unsealed on purpose so it is extendable.
/// </remarks>
public class ApiContentPathProvider : IApiContentPathProvider
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentPathProvider"/> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    public ApiContentPathProvider(IPublishedUrlProvider publishedUrlProvider)
        => _publishedUrlProvider = publishedUrlProvider;

    /// <inheritdoc />
    public virtual string GetContentPath(IPublishedContent content, string? culture)
        => _publishedUrlProvider.GetUrl(content, UrlMode.Relative, culture);
}
