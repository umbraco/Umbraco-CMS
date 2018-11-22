using System;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A model mapper used to map models for the various dashboards
    /// </summary>
    internal class MiscModelsMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IRedirectUrl, ContentRedirectUrl>()
                .ForMember(x => x.OriginalUrl, expression => expression.MapFrom(item => UmbracoContext.Current.UrlProvider.GetUrlFromRoute(item.ContentId, item.Url)))
                .ForMember(x => x.DestinationUrl, expression => expression.Ignore())
                .ForMember(x => x.RedirectId, expression => expression.MapFrom(item => item.Key));

            //for the logging controller (and assuming dashboard that is used in uaas? otherwise not sure what that controller is used for)
            config.CreateMap<LogItem, AuditLog>()
                .ForMember(log => log.UserAvatars, expression => expression.Ignore())
                .ForMember(log => log.UserName, expression => expression.Ignore())
                .ForMember(log => log.LogType, expression => expression.MapFrom(item => Enum<AuditType>.Parse(item.LogType.ToString())));

            config.CreateMap<IAuditItem, AuditLog>()
                .ForMember(log => log.UserAvatars, expression => expression.Ignore())
                .ForMember(log => log.UserName, expression => expression.Ignore())
                .ForMember(log => log.NodeId, expression => expression.MapFrom(item => item.Id))
                .ForMember(log => log.Timestamp, expression => expression.MapFrom(item => item.CreateDate))
                .ForMember(log => log.LogType, expression => expression.MapFrom(item => item.AuditType));
        }
    }
}