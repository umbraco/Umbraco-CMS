using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DataTypeFactory
{
    public static IDataType BuildEntity(DataTypeDto dto, PropertyEditorCollection editors, ILogger<IDataType> logger, IConfigurationEditorJsonSerializer serializer)
    {
        // Check we have an editor for the data type.
        if (!editors.TryGet(dto.EditorAlias, out IDataEditor? editor))
        {
            logger.LogWarning(
                "Could not find an editor with alias {EditorAlias}, treating as Label. " + "The site may fail to boot and/or load data types and run.", dto.EditorAlias);

            // Create as special type, which downstream can be handled by converting to a LabelPropertyEditor to make clear
            // the situation to the user.
            editor = new MissingPropertyEditor();
        }

        var dataType = new DataType(editor, serializer);

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

    public static DataTypeDto BuildDto(IDataType entity, IConfigurationEditorJsonSerializer serializer)
    {
        var dataTypeDto = new DataTypeDto
        {
            EditorAlias = entity.EditorAlias,
            NodeId = entity.Id,
            DbType = entity.DatabaseType.ToString(),
            Configuration = ConfigurationEditor.ToDatabase(entity.Configuration, serializer),
            NodeDto = BuildNodeDto(entity),
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
            UserId = entity.CreatorId,
        };

        return nodeDto;
    }
}
