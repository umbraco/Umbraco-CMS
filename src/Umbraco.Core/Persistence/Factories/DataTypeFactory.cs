using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Persistence.Factories
{
    internal static class DataTypeFactory
    {
        public static IDataType BuildEntity(DataTypeDto dto, PropertyEditorCollection editors, ILogger logger)
        {
            if (!editors.TryGet(dto.EditorAlias, out var editor))
            {
                logger.Warn(typeof(DataTypeFactory), "Could not find an editor with alias {EditorAlias}, converting to label", dto.EditorAlias);
                //convert to label
                editor = new LabelPropertyEditor(logger);
            }

            var dataType = new DataType(editor);

            try
            {
                dataType.DisableChangeTracking();

                dataType.CreateDate = dto.NodeDto.CreateDate;
                dataType.DatabaseType = dto.DbType.EnumParse<ValueStorageType>(true);
                dataType.Id = dto.NodeId;
                dataType.Key = dto.NodeDto.UniqueId;
                dataType.Level = dto.NodeDto.Level;
                dataType.UpdateDate = dto.NodeDto.CreateDate;
                dataType.Name = dto.NodeDto.Text;
                dataType.ParentId = dto.NodeDto.ParentId;
                dataType.Path = dto.NodeDto.Path;
                dataType.SortOrder = dto.NodeDto.SortOrder;
                dataType.Trashed = dto.NodeDto.Trashed;
                dataType.CreatorId = dto.NodeDto.UserId ?? Constants.Security.UnknownUserId;

                dataType.SetLazyConfiguration(dto.Configuration);

                // reset dirty initial properties (U4-1946)
                dataType.ResetDirtyProperties(false);
                return dataType;
            }
            finally
            {
                dataType.EnableChangeTracking();
            }
        }

        public static DataTypeDto BuildDto(IDataType entity)
        {
            var dataTypeDto = new DataTypeDto
            {
                EditorAlias = entity.EditorAlias,
                NodeId = entity.Id,
                DbType = entity.DatabaseType.ToString(),
                Configuration = ConfigurationEditor.ToDatabase(entity.Configuration),
                NodeDto = BuildNodeDto(entity)
            };

            return dataTypeDto;
        }

        private static NodeDto BuildNodeDto(IDataType entity)
        {
            var nodeDto = new NodeDto
            {
                CreateDate = entity.CreateDate,
                NodeId = entity.Id,
                Level = Convert.ToInt16(entity.Level),
                NodeObjectType = Constants.ObjectTypes.DataType,
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
