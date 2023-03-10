using Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Api.Management.Factories;

public class RedirectUrlViewModelFactory : IRedirectUrlViewModelFactory
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public RedirectUrlViewModelFactory(IPublishedUrlProvider publishedUrlProvider)
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
            ContentKey = source.ContentKey,
            Created = source.CreateDateUtc,
            Culture = source.Culture,
            Key = source.Key,
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
