using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Core.Models.Mapping;

public class RedirectUrlMapDefinition : IMapDefinition
{
    private readonly IPublishedUrlProvider _publishedUrlProvider;

    public RedirectUrlMapDefinition(IPublishedUrlProvider publishedUrlProvider) =>
        _publishedUrlProvider = publishedUrlProvider;

    public void DefineMaps(IUmbracoMapper mapper) =>
        mapper.Define<IRedirectUrl, ContentRedirectUrl>((source, context) => new ContentRedirectUrl(), Map);

    // Umbraco.Code.MapAll
    private void Map(IRedirectUrl source, ContentRedirectUrl target, MapperContext context)
    {
        target.ContentId = source.ContentId;
        target.CreateDateUtc = source.CreateDateUtc;
        target.Culture = source.Culture;
        target.DestinationUrl = source.ContentId > 0
            ? _publishedUrlProvider?.GetUrl(source.ContentId, culture: source.Culture)
            : "#";
        target.OriginalUrl = _publishedUrlProvider?.GetUrlFromRoute(source.ContentId, source.Url, source.Culture);
        target.RedirectId = source.Key;
    }
}
