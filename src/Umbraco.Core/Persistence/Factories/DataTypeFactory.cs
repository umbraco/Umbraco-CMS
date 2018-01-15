using System;
using System.Globalization;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DataTypeFactory
    {
        private readonly Guid _nodeObjectTypeId;
        private int _primaryKey;

        public DataTypeFactory(Guid nodeObjectTypeId)
        {
            _nodeObjectTypeId = nodeObjectTypeId;
        }

        #region Implementation of IEntityFactory<DataTypeDefinition,DataTypeDto>

        public IDataType BuildEntity(DataTypeDto dto)
        {
            var dataTypeDefinition = new DataType(dto.EditorAlias);


            try
            {
                dataTypeDefinition.DisableChangeTracking();

                dataTypeDefinition.CreateDate = dto.NodeDto.CreateDate;
                dataTypeDefinition.DatabaseType = dto.DbType.EnumParse<DataTypeDatabaseType>(true);
                dataTypeDefinition.Id = dto.DataTypeId;
                dataTypeDefinition.Key = dto.NodeDto.UniqueId;
                dataTypeDefinition.Level = dto.NodeDto.Level;
                dataTypeDefinition.UpdateDate = dto.NodeDto.CreateDate;
                dataTypeDefinition.Name = dto.NodeDto.Text;
                dataTypeDefinition.ParentId = dto.NodeDto.ParentId;
                dataTypeDefinition.Path = dto.NodeDto.Path;
                dataTypeDefinition.SortOrder = dto.NodeDto.SortOrder;
                dataTypeDefinition.Trashed = dto.NodeDto.Trashed;
                dataTypeDefinition.CreatorId = dto.NodeDto.UserId.Value;

                // reset dirty initial properties (U4-1946)
                dataTypeDefinition.ResetDirtyProperties(false);
                return dataTypeDefinition;
            }
            finally
            {
                dataTypeDefinition.EnableChangeTracking();
            }
        }

        public DataTypeDto BuildDto(IDataType entity)
        {
            var dataTypeDto = new DataTypeDto
                                  {
                                      EditorAlias = entity.EditorAlias,
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

        private NodeDto BuildNodeDto(IDataType entity)
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
