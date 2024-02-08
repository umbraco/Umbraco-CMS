using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Api.Management.Factories;

public class RedirectUrlPresentationFactory : IRedirectUrlPresentationFactory
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public RedirectUrlPresentationFactory(IPublishedUrlProvider publishedUrlProvider)
    {
        _publishedUrlProvider = publishedUrlProvider;
    }

    public RedirectUrlResponseModel Create(IRedirectUrl source)
    {
        var destinationUrl = source.ContentId > 0
            ? _publishedUrlProvider.GetUrl(source.ContentId, culture: source.Culture)
            : "#";

        var originalUrl = _publishedUrlProvider.GetUrlFromRoute(source.ContentId, source.Url, source.Culture);

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

    public IEnumerable<RedirectUrlResponseModel> CreateMany(IEnumerable<IRedirectUrl> sources)
    {
        foreach (IRedirectUrl source in sources)
        {
            yield return Create(source);
        }
    }
}
