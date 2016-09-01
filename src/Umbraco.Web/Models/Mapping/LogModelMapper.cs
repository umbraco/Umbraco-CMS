using AutoMapper;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Models;

namespace Umbraco.Web.Models.Mapping
{
    internal class LogModelMapper : ModelMapperConfiguration
    {
        public override void ConfigureMappings(IMapperConfiguration config)
        {
            config.CreateMap<AuditItem, AuditLog>()
                .ForMember(log => log.NodeId, expression => expression.Ignore())
                .ForMember(log => log.Timestamp, expression => expression.MapFrom(item => item.CreateDate))
                .ForMember(log => log.LogType, expression => expression.MapFrom(item => item.AuditType.ToString()));
        }
    }
}