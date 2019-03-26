using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class AuditMapperProfile : IMapperProfile
    {
        public void SetMaps(Mapper mapper)
        {
            mapper.Define<IAuditItem, AuditLog>(source => new AuditLog(), Map);
        }

        // Umbraco.Code.MapAll -UserAvatars -UserName
        private void Map(IAuditItem source, AuditLog target)
        {
            target.UserId = source.UserId;
            target.NodeId = source.Id;
            target.Timestamp = source.CreateDate;
            target.LogType = source.AuditType.ToString();
            target.EntityType = source.EntityType;
            target.Comment = source.Comment;
            target.Parameters = source.Parameters;
        }
    }
}
