using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DataTypeDefinitionFactory
    {
        private readonly Guid _nodeObjectTypeId;
        private int _primaryKey;

        public DataTypeDefinitionFactory(Guid nodeObjectTypeId)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
        }

        #region Implementation of IEntityFactory<DataTypeDefinition,DataTypeDto>

        public IDataTypeDefinition BuildEntity(DataTypeDto dto)
        {
            var dataTypeDefinition = new DataTypeDefinition(dto.PropertyEditorAlias)
                                         {
                                             CreateDate = dto.NodeDto.CreateDate,
                                             DatabaseType = dto.DbType.EnumParse<DataTypeDatabaseType>(true),
                                             Id = dto.DataTypeId,
                                             Key = dto.NodeDto.UniqueId,
                                             Level = dto.NodeDto.Level,
                                             UpdateDate = dto.NodeDto.CreateDate,
                                             Name = dto.NodeDto.Text,
                                             ParentId = dto.NodeDto.ParentId,
                                             Path = dto.NodeDto.Path,
                                             SortOrder = dto.NodeDto.SortOrder,
                                             Trashed = dto.NodeDto.Trashed,
                                             CreatorId = dto.NodeDto.UserId.Value
                                         };
            //on initial construction we don't want to have dirty properties tracked
            // http://issues.umbraco.org/issue/U4-1946
            dataTypeDefinition.ResetDirtyProperties(false);
            return dataTypeDefinition;
        }

        public DataTypeDto BuildDto(IDataTypeDefinition entity)
        {
            var dataTypeDto = new DataTypeDto
                                  {
                                      PropertyEditorAlias = entity.PropertyEditorAlias,
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

        private NodeDto BuildNodeDto(IDataTypeDefinition entity)
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
                                  UserId = entity.CreatorId
                              };

            return nodeDto;
        }
    }
}