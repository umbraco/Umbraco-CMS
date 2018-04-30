using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Models.Mapping
{
    internal class RedirectUrlMapperProfile : Profile
    {
        private readonly UrlProvider _urlProvider;

        public RedirectUrlMapperProfile(UrlProvider urlProvider)
        {
            _urlProvider = urlProvider;
        }

        public RedirectUrlMapperProfile()
        {
            CreateMap<IRedirectUrl, ContentRedirectUrl>()
                .ForMember(x => x.OriginalUrl, expression => expression.MapFrom(item => _urlProvider.GetUrlFromRoute(item.ContentId, item.Url, null)))
                .ForMember(x => x.DestinationUrl, expression => expression.Ignore())
                .ForMember(x => x.RedirectId, expression => expression.MapFrom(item => item.Key));
        }
    }
}
