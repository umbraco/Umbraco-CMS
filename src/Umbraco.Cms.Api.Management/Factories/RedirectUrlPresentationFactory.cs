using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods for creating redirect URL presentation objects.
/// </summary>
public class RedirectUrlPresentationFactory : IRedirectUrlPresentationFactory
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectUrlPresentationFactory"/> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The provider used to generate published URLs for redirects.</param>
    public RedirectUrlPresentationFactory(IPublishedUrlProvider publishedUrlProvider)
    {
        _publishedUrlProvider = publishedUrlProvider;
    }

    /// <summary>
    /// Creates a <see cref="RedirectUrlResponseModel"/> instance based on the provided <see cref="IRedirectUrl"/> source.
    /// This method maps the properties of the source redirect URL to a response model suitable for API output.
    /// </summary>
    /// <param name="source">The <see cref="IRedirectUrl"/> instance containing redirect URL information to be transformed.</param>
    /// <returns>
    /// A <see cref="RedirectUrlResponseModel"/> populated with details from the source, including original and destination URLs, document reference, creation date, culture, and identifier.
    /// </returns>
    public RedirectUrlResponseModel Create(IRedirectUrl source)
    {
        var destinationUrl = source.ContentId > 0
            ? _publishedUrlProvider.GetUrl(source.ContentId, culture: source.Culture)
            : "#";

        var originalUrl = _publishedUrlProvider.GetUrlFromRoute(source.ContentId, source.Url, source.Culture);

        // Even if the URL could not be extracted from the route, if we have a path as a the route for the original URL, we should display it.
        if (originalUrl == "#" && source.Url.StartsWith('/'))
        {
            originalUrl = source.Url;
        }

        return new RedirectUrlResponseModel
        {
            OriginalUrl = originalUrl,
            DestinationUrl = destinationUrl,
            Document = new ReferenceByIdModel(source.ContentKey),
            Created = source.CreateDateUtc,
            Culture = source.Culture,
            Id = source.Key,
        };
    }

    /// <summary>
    /// Creates a collection of <see cref="RedirectUrlResponseModel"/> from a collection of <see cref="IRedirectUrl"/> sources.
    /// </summary>
    /// <param name="sources">The collection of <see cref="IRedirectUrl"/> instances to convert.</param>
    /// <returns>An enumerable of <see cref="RedirectUrlResponseModel"/> representing the converted sources.</returns>
    public IEnumerable<RedirectUrlResponseModel> CreateMany(IEnumerable<IRedirectUrl> sources)
    {
        foreach (IRedirectUrl source in sources)
        {
            yield return Create(source);
        }
    }
}
