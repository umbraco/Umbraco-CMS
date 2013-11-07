using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;
using umbraco.BusinessLogic;

namespace Umbraco.Web.Models.Mapping
{
    internal class LogModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<LogItem, AuditLog>()
                  .ForMember(log => log.LogType, expression => expression.MapFrom(item => Enum<AuditLogType>.Parse(item.LogType.ToString())));
        }
    }
}