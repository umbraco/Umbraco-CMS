using AutoMapper;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A model mapper used to map models for the various dashboards
    /// </summary>
    internal class DashboardProfile : Profile
    {
        public DashboardProfile()
        {
            CreateMap<IRedirectUrl, ContentRedirectUrl>()
                .ForMember(x => x.OriginalUrl, expression => expression.MapFrom(item => UmbracoContext.Current.UrlProvider.GetUrlFromRoute(item.ContentId, item.Url)))
                .ForMember(x => x.DestinationUrl, expression => expression.Ignore())
                .ForMember(x => x.RedirectId, expression => expression.MapFrom(item => item.Key));
        }
    }
}