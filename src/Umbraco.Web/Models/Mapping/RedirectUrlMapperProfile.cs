using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class RedirectUrlMapperProfile : IMapperProfile
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public RedirectUrlMapperProfile(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        private UmbracoContext UmbracoContext => _umbracoContextAccessor.UmbracoContext;

        public void DefineMaps(Mapper mapper)
        {
            mapper.Define<IRedirectUrl, ContentRedirectUrl>((source, context) => new ContentRedirectUrl(), Map);
        }

        // Umbraco.Code.MapAll
        private void Map(IRedirectUrl source, ContentRedirectUrl target, MapperContext context)
        {
            target.ContentId = source.ContentId;
            target.CreateDateUtc = source.CreateDateUtc;
            target.Culture = source.Culture;
            target.DestinationUrl = source.ContentId > 0 ? UmbracoContext?.UrlProvider?.GetUrl(source.ContentId, source.Culture) : "#";
            target.OriginalUrl = UmbracoContext?.UrlProvider?.GetUrlFromRoute(source.ContentId, source.Url, source.Culture);
            target.RedirectId = source.Key;
        }
    }
}
