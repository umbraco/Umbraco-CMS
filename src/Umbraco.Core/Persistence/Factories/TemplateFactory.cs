using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class TemplateFactory : IEntityFactory<Template, TemplateDto>
    {
        private readonly int _primaryKey;
        private readonly Guid _nodeObjectTypeId;

        public TemplateFactory()
        {}

        public TemplateFactory(Guid nodeObjectTypeId)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
        }

        public TemplateFactory(int primaryKey, Guid nodeObjectTypeId)
        {
            _primaryKey = primaryKey;
            _nodeObjectTypeId = nodeObjectTypeId;
        }

        #region Implementation of IEntityFactory<ITemplate,TemplateDto>
        
        public Template BuildEntity(TemplateDto dto)
        {
            return new Template(string.Empty, dto.NodeDto.Text, dto.Alias)
                       {
                           CreateDate = dto.NodeDto.CreateDate,
                           Id = dto.NodeId,
                           Key = dto.NodeDto.UniqueId.Value,
                           CreatorId = dto.NodeDto.UserId.Value,
                           Level = dto.NodeDto.Level,
                           ParentId = dto.NodeDto.ParentId,
                           SortOrder = dto.NodeDto.SortOrder,
                           NodePath = dto.NodeDto.Path
                       };
        }

        public TemplateDto BuildDto(Template entity)
        {
            var dto = new TemplateDto
                       {
                           Alias = entity.Alias,
                           Design = entity.Content,
                           NodeDto = BuildNodeDto(entity)
                       };

            if (entity.HasIdentity)
            {
                dto.NodeId = entity.Id;
                dto.PrimaryKey = _primaryKey;
            }

            return dto;
        }

        #endregion

        private NodeDto BuildNodeDto(Template entity)
        {
            var nodeDto = new NodeDto
                              {
                                  CreateDate = entity.CreateDate,
                                  NodeId = entity.Id,
                                  Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                                  NodeObjectType = _nodeObjectTypeId,
                                  ParentId = entity.ParentId,
                                  Path = entity.NodePath,
                                  SortOrder = entity.SortOrder,
                                  Text = entity.Name,
                                  Trashed = false,
                                  UniqueId = entity.Key,
                                  UserId = entity.CreatorId
                              };

            return nodeDto;
        }
    }
}