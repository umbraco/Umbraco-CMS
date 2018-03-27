using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class AuditMapperProfile : Profile
    {
        public AuditMapperProfile()
        {
            CreateMap<IAuditItem, AuditLog>()
                .ForMember(log => log.UserAvatars, expression => expression.Ignore())
                .ForMember(log => log.UserName, expression => expression.Ignore())
                .ForMember(log => log.NodeId, expression => expression.MapFrom(item => item.Id))
                .ForMember(log => log.Timestamp, expression => expression.MapFrom(item => item.CreateDate))
                .ForMember(log => log.LogType, expression => expression.MapFrom(item => item.AuditType));
        }
    }
}
