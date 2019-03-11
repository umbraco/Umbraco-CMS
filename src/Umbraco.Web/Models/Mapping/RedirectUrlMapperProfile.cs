using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Models.Mapping
{
    internal class RedirectUrlMapperProfile : Profile
    {

        public RedirectUrlMapperProfile(IUmbracoContextAccessor umbracoContextAccessor)
        {
            CreateMap<IRedirectUrl, ContentRedirectUrl>()
                .ForMember(x => x.OriginalUrl, expression => expression.MapFrom(item => Current.UmbracoContext.UrlProvider.GetUrlFromRoute(item.ContentId, item.Url, item.Culture)))
                .ForMember(x => x.DestinationUrl, expression => expression.MapFrom(item => item.ContentId > 0 ? GetUrl(umbracoContextAccessor, item) : "#"))
                .ForMember(x => x.RedirectId, expression => expression.MapFrom(item => item.Key));
        }

        private static string GetUrl(IUmbracoContextAccessor umbracoContextAccessor, IRedirectUrl item) => umbracoContextAccessor?.UmbracoContext?.UrlProvider?.GetUrl(item.ContentId, item.Culture);
    }
}
