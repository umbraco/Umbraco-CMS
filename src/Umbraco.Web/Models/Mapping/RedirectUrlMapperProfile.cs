using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class RedirectUrlMapperProfile : Profile
    {
        public RedirectUrlMapperProfile()
        {
            CreateMap<IRedirectUrl, ContentRedirectUrl>()
                .ForMember(x => x.OriginalUrl, expression => expression.MapFrom(item => Current.UmbracoContext.UrlProvider.GetUrlFromRoute(item.ContentId, item.Url)))
                .ForMember(x => x.DestinationUrl, expression => expression.Ignore())
                .ForMember(x => x.RedirectId, expression => expression.MapFrom(item => item.Key));
        }
    }
}
