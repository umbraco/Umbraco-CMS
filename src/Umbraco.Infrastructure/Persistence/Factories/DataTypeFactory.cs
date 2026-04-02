using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class DataTypeFactory
{
    /// <summary>
    /// Constructs an <see cref="Umbraco.Cms.Core.Models.IDataType"/> entity from the specified <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DataTypeDto"/>.
    /// If the property editor specified by the DTO cannot be found, a <see cref="MissingPropertyEditor"/> is used and a warning is logged.
    /// </summary>
    /// <param name="dto">The data transfer object containing the persisted data type information.</param>
    /// <param name="editors">A collection of property editors used to resolve the editor by alias.</param>
    /// <param name="logger">The logger used to record warnings if the editor is missing.</param>
    /// <param name="serializer">The serializer for configuration data.</param>
    /// <param name="dataValueEditorFactory">The factory used to create data value editors, including for missing editors.</param>
    /// <returns>
    /// An <see cref="Umbraco.Cms.Core.Models.IDataType"/> instance populated from the DTO and associated configuration.
    /// </returns>
    public static IDataType BuildEntity(
        DataTypeDto dto,
        PropertyEditorCollection editors,
        ILogger<IDataType> logger,
        IConfigurationEditorJsonSerializer serializer,
        IDataValueEditorFactory dataValueEditorFactory)
    {
        // Check we have an editor for the data type.
        if (!editors.TryGet(dto.EditorAlias, out IDataEditor? editor))
        {
            logger.LogWarning(
                "Could not find an editor with alias {EditorAlias}, treating as Missing. " + "The site may fail to boot and/or load data types and run.",
                dto.EditorAlias);
            editor =
                new MissingPropertyEditor(
                    dto.EditorAlias,
                    dataValueEditorFactory);
        }

        var dataType = new DataType(editor, serializer);

        try
        {
            dataType.DisableChangeTracking();

            dataType.CreateDate = dto.NodeDto.CreateDate.EnsureUtc();
            dataType.DatabaseType = dto.DbType.EnumParse<ValueStorageType>(true);
            dataType.Id = dto.NodeId;
            dataType.Key = dto.NodeDto.UniqueId;
            dataType.Level = dto.NodeDto.Level;
            dataType.UpdateDate = dto.NodeDto.CreateDate.EnsureUtc();
            dataType.Name = dto.NodeDto.Text;
            dataType.ParentId = dto.NodeDto.ParentId;
            dataType.Path = dto.NodeDto.Path;
            dataType.SortOrder = dto.NodeDto.SortOrder;
            dataType.Trashed = dto.NodeDto.Trashed;
            dataType.CreatorId = dto.NodeDto.UserId ?? Constants.Security.UnknownUserId;
            dataType.EditorUiAlias = editor is MissingPropertyEditor ? "Umb.PropertyEditorUi.Missing" : dto.EditorUiAlias;

            dataType.SetConfigurationData(editor.GetConfigurationEditor().FromDatabase(dto.Configuration, serializer));

            // reset dirty initial properties (U4-1946)
            dataType.ResetDirtyProperties(false);
            return dataType;
        }
        finally
        {
            dataType.EnableChangeTracking();
        }
    }

    /// <summary>
    /// Creates a <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DataTypeDto"/> from the specified <see cref="Umbraco.Cms.Core.Models.IDataType"/> entity, serializing its configuration using the provided JSON serializer.
    /// </summary>
    /// <param name="entity">The <see cref="Umbraco.Cms.Core.Models.IDataType"/> to convert.</param>
    /// <param name="serializer">The <see cref="Umbraco.Cms.Core.PropertyEditors.IConfigurationEditorJsonSerializer"/> used to serialize the configuration data.</param>
    /// <returns>A <see cref="Umbraco.Cms.Infrastructure.Persistence.Dtos.DataTypeDto"/> representing the converted data type entity.</returns>
    public static DataTypeDto BuildDto(IDataType entity, IConfigurationEditorJsonSerializer serializer)
    {
        var dataTypeDto = new DataTypeDto
        {
            EditorAlias = entity.EditorAlias,
            EditorUiAlias = entity.EditorUiAlias,
            NodeId = entity.Id,
            DbType = entity.DatabaseType.ToString(),
            Configuration = entity.Editor?.GetConfigurationEditor().ToDatabase(entity.ConfigurationData, serializer),
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
