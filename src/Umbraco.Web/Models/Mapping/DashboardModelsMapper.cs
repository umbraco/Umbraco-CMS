using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// A model mapper used to map models for the various dashboards
    /// </summary>
    internal class DashboardModelsMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<IRedirectUrl, ContentRedirectUrl>()
                .ForMember(x => x.OriginalUrl, expression => expression.MapFrom(item => item.Url))
                .ForMember(x => x.DestinationUrl, expression => expression.Ignore())
                .ForMember(x => x.RedirectId, expression => expression.MapFrom(item => item.Key));

            //for the logging controller (and assuming dashboard that is used in uaas? otherwise not sure what that controller is used for)
            config.CreateMap<LogItem, AuditLog>()
                  .ForMember(log => log.LogType, expression => expression.MapFrom(item => Enum<AuditLogType>.Parse(item.LogType.ToString())));
        }
    }
}