using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DataTypeDefinitionFactory : IEntityFactory<DataTypeDefinition, DataTypeDto>
    {
        private readonly Guid _nodeObjectTypeId;
        private int _primaryKey;

        public DataTypeDefinitionFactory(Guid nodeObjectTypeId)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
        }

        #region Implementation of IEntityFactory<DataTypeDefinition,DataTypeDto>

        public DataTypeDefinition BuildEntity(DataTypeDto dto)
        {
            var dataTypeDefinition = new DataTypeDefinition(dto.ControlId)
                                         {
                                             CreateDate = dto.NodeDto.CreateDate,
                                             DatabaseType = dto.DbType.EnumParse<DataTypeDatabaseType>(true),
                                             Id = dto.DataTypeId,
                                             Key =
                                                 dto.NodeDto.UniqueId.HasValue
                                                     ? dto.NodeDto.UniqueId.Value
                                                     : dto.DataTypeId.ToGuid(),
                                             Level = dto.NodeDto.Level,
                                             UpdateDate = dto.NodeDto.CreateDate,
                                             Name = dto.NodeDto.Text,
                                             ParentId = dto.NodeDto.ParentId,
                                             Path = dto.NodeDto.Path,
                                             SortOrder = dto.NodeDto.SortOrder,
                                             Trashed = dto.NodeDto.Trashed,
                                             UserId = dto.NodeDto.UserId.HasValue ? dto.NodeDto.UserId.Value : 0
                                         };

            return dataTypeDefinition;
        }

        public DataTypeDto BuildDto(DataTypeDefinition entity)
        {
            var dataTypeDto = new DataTypeDto
                                  {
                                      ControlId = entity.ControlId,
                                      DataTypeId = entity.Id,
                                      DbType = entity.DatabaseType.ToString(),
                                      NodeDto = BuildNodeDto(entity)
                                  };

            if (_primaryKey > 0)
            {
                dataTypeDto.PrimaryKey = _primaryKey;
            }

            return dataTypeDto;
        }

        #endregion

        public void SetPrimaryKey(int primaryKey)
        {
            _primaryKey = primaryKey;
        }

        private NodeDto BuildNodeDto(DataTypeDefinition entity)
        {
            var nodeDto = new NodeDto
                              {
                                  CreateDate = entity.CreateDate,
                                  NodeId = entity.Id,
                                  Level = short.Parse(entity.Level.ToString(CultureInfo.InvariantCulture)),
                                  NodeObjectType = _nodeObjectTypeId,
                                  ParentId = entity.ParentId,
                                  Path = entity.Path,
                                  SortOrder = entity.SortOrder,
                                  Text = entity.Name,
                                  Trashed = entity.Trashed,
                                  UniqueId = entity.Key,
                                  UserId = entity.UserId
                              };

            return nodeDto;
        }
    }
}