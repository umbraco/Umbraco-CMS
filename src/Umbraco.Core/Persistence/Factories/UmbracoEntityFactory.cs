using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class UmbracoEntityFactory : IEntityFactory<UmbracoEntity, NodeDto>
    {
        public UmbracoEntity BuildEntity(NodeDto dto)
        {
            var entity = new UmbracoEntity(dto.Trashed)
                             {
                                 CreateDate = dto.CreateDate,
                                 CreatorId = dto.UserId.Value,
                                 Id = dto.NodeId,
                                 Key = dto.UniqueId.Value,
                                 Level = dto.Level,
                                 Name = dto.Text,
                                 NodeObjectTypeId = dto.NodeObjectType.Value,
                                 ParentId = dto.ParentId,
                                 Path = dto.Path,
                                 SortOrder = dto.SortOrder,
                                 HasChildren = false,
                                 IsPublished = false
                             };
            return entity;
        }

        public NodeDto BuildDto(UmbracoEntity entity)
        {
            var node = new NodeDto
                           {
                               CreateDate = entity.CreateDate,
                               Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                               NodeId = entity.Id,
                               NodeObjectType = entity.NodeObjectTypeId,
                               ParentId = entity.ParentId,
                               Path = entity.Path,
                               SortOrder = entity.SortOrder,
                               Text = entity.Name,
                               Trashed = entity.Trashed,
                               UniqueId = entity.Key,
                               UserId = entity.CreatorId
                           };
            return node;
        }
    }
}