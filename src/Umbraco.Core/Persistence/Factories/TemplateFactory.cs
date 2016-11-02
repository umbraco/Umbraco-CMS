﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class TemplateFactory
    {
        private readonly int _primaryKey;
        private readonly Guid _nodeObjectTypeId;

        public TemplateFactory()
        {
            
        }

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

        public Template BuildEntity(TemplateDto dto, IEnumerable<IUmbracoEntity> childDefinitions, Func<File, string> getFileContent)
        {
            var template = new Template(dto.NodeDto.Text, dto.Alias, getFileContent);

            try
            {
                template.DisableChangeTracking();

                template.CreateDate = dto.NodeDto.CreateDate;
                template.Id = dto.NodeId;
                template.Key = dto.NodeDto.UniqueId;
                template.Path = dto.NodeDto.Path;

                template.IsMasterTemplate = childDefinitions.Any(x => x.ParentId == dto.NodeId);

                if (dto.NodeDto.ParentId > 0)
                    template.MasterTemplateId = new Lazy<int>(() => dto.NodeDto.ParentId);

                //on initial construction we don't want to have dirty properties tracked
                // http://issues.umbraco.org/issue/U4-1946
                template.ResetDirtyProperties(false);
                return template;
            }
            finally
            {
                template.EnableChangeTracking();
            }
        }

        public TemplateDto BuildDto(Template entity)
        {
            var dto = new TemplateDto
                       {
                           Alias = entity.Alias,
                           Design = entity.Content ?? string.Empty,
                           NodeDto = BuildNodeDto(entity)
                       };

            if (entity.MasterTemplateId != null && entity.MasterTemplateId.Value > 0)
            {
                dto.NodeDto.ParentId = entity.MasterTemplateId.Value;
            }

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
                                  Level = 1,
                                  NodeObjectType = _nodeObjectTypeId,
                                  ParentId = entity.MasterTemplateId.Value,
                                  Path = entity.Path,
                                  Text = entity.Name,
                                  Trashed = false,
                                  UniqueId = entity.Key
                              };

            return nodeDto;
        }
    }
}