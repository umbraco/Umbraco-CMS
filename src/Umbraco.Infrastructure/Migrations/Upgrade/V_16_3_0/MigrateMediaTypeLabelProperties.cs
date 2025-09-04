using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_3_0;

[Obsolete("Remove in Umbraco 18.")]
public class MigrateMediaTypeLabelProperties : AsyncMigrationBase
{
    private readonly IMediaTypeService _mediaTypeService;

    private readonly InstallDefaultDataSettings? _dataTypeSettings;
    private readonly InstallDefaultDataSettings? _mediaTypeSettings;

    private readonly Guid _labelBytesDataTypeKey = new(Constants.DataTypes.Guids.LabelBytes);
    private readonly Guid _labelPixelsDataTypeKey = new(Constants.DataTypes.Guids.LabelPixels);

    public MigrateMediaTypeLabelProperties(
        IMigrationContext context,
        IMediaTypeService mediaTypeService,
        IOptionsMonitor<InstallDefaultDataSettings> installDefaultDataSettings)
        : base(context)
    {
        _mediaTypeService = mediaTypeService;

        _dataTypeSettings = installDefaultDataSettings.Get(Constants.Configuration.NamedOptions.InstallDefaultData.DataTypes);
        _mediaTypeSettings = installDefaultDataSettings.Get(Constants.Configuration.NamedOptions.InstallDefaultData.MediaTypes);
    }

    protected override async Task MigrateAsync()
    {
        if (_dataTypeSettings?.InstallData == InstallDefaultDataOption.None)
        {
            return;
        }

        if (_mediaTypeSettings?.InstallData == InstallDefaultDataOption.None)
        {
            return;
        }

        ToggleIndentityInsertForNodes(true);
        try
        {
            IfNotExistsCreateBytesLabel();
            IfNotExistsCreatePixelsLabel();
        }
        finally
        {
            ToggleIndentityInsertForNodes(false);
        }

        await MigrateMediaTypeLabels();
    }

    private void ToggleIndentityInsertForNodes(bool toggleOn)
    {
        if (SqlSyntax.SupportsIdentityInsert())
        {
            Database.Execute(new Sql($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(NodeDto.TableName)} {(toggleOn ? "ON" : "OFF")} "));
        }
    }

    private void IfNotExistsCreateBytesLabel()
    {
        if (Database.Exists<NodeDto>(Constants.DataTypes.LabelBytes))
        {
            return;
        }

        var nodeDto = new NodeDto
        {
            NodeId = Constants.DataTypes.LabelBytes,
            Trashed = false,
            ParentId = -1,
            UserId = -1,
            Level = 1,
            Path = "-1," + Constants.DataTypes.LabelBytes,
            SortOrder = 40,
            UniqueId = _labelBytesDataTypeKey,
            Text = "Label (bytes)",
            NodeObjectType = Constants.ObjectTypes.DataType,
            CreateDate = DateTime.Now,
        };

        _ = Database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, nodeDto);

        var dataTypeDto = new DataTypeDto
        {
            NodeId = Constants.DataTypes.LabelBytes,
            EditorAlias = Constants.PropertyEditors.Aliases.Label,
            EditorUiAlias = "Umb.PropertyEditorUi.Label",
            DbType = nameof(ValueStorageType.Nvarchar),
            Configuration = "{\"umbracoDataValueType\":\"BIGINT\", \"labelTemplate\":\"{=value | bytes}\"}",
        };

        _ = Database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, dataTypeDto);
    }

    private void IfNotExistsCreatePixelsLabel()
    {
        if (Database.Exists<NodeDto>(Constants.DataTypes.LabelPixels))
        {
            return;
        }

        var nodeDto = new NodeDto
        {
            NodeId = Constants.DataTypes.LabelPixels,
            Trashed = false,
            ParentId = -1,
            UserId = -1,
            Level = 1,
            Path = "-1," + Constants.DataTypes.LabelPixels,
            SortOrder = 41,
            UniqueId = _labelPixelsDataTypeKey,
            Text = "Label (pixels)",
            NodeObjectType = Constants.ObjectTypes.DataType,
            CreateDate = DateTime.Now,
        };

        _ = Database.Insert(Constants.DatabaseSchema.Tables.Node, "id", false, nodeDto);

        var dataTypeDto = new DataTypeDto
        {
            NodeId = Constants.DataTypes.LabelPixels,
            EditorAlias = Constants.PropertyEditors.Aliases.Label,
            EditorUiAlias = "Umb.PropertyEditorUi.Label",
            DbType = nameof(ValueStorageType.Integer),
            Configuration = "{\"umbracoDataValueType\":\"INT\", \"labelTemplate\":\"{=value}px\"}",
        };

        _ = Database.Insert(Constants.DatabaseSchema.Tables.DataType, "pk", false, dataTypeDto);
    }

    private async Task MigrateMediaTypeLabels()
    {
        // update all media types with the new data-type references
        IMediaType[] allMediaTypes = _mediaTypeService.GetAll().ToArray();
        foreach (IMediaType mediaType in allMediaTypes)
        {
            bool updated = false;

            foreach (IPropertyType propertyType in mediaType.PropertyTypes)
            {
                switch (propertyType.Alias)
                {
                    case Constants.Conventions.Media.Bytes when propertyType.DataTypeId == Constants.DataTypes.LabelBigint:
                        propertyType.DataTypeId = Constants.DataTypes.LabelBytes;
                        propertyType.DataTypeKey = _labelBytesDataTypeKey;

                        if (propertyType.Name == "Size")
                        {
                            propertyType.Name = "File size";
                        }

                        if (propertyType.Description == "in bytes")
                        {
                            propertyType.Description = null;
                        }

                        updated = true;

                        break;

                    case Constants.Conventions.Media.Height when propertyType.DataTypeId == Constants.DataTypes.LabelInt:
                    case Constants.Conventions.Media.Width when propertyType.DataTypeId == Constants.DataTypes.LabelInt:
                        propertyType.DataTypeId = Constants.DataTypes.LabelPixels;
                        propertyType.DataTypeKey = _labelPixelsDataTypeKey;

                        if (propertyType.Description == "in pixels")
                        {
                            propertyType.Description = null;
                        }

                        updated = true;

                        break;

                    case Constants.Conventions.Media.Extension when propertyType.DataTypeId == Constants.DataTypes.LabelString:
                        if (propertyType.Name == "Type")
                        {
                            propertyType.Name = "File extension";
                        }

                        updated = true;

                        break;

                    default:
                        break;
                }
            }

            if (updated)
            {
                Attempt<ContentTypeOperationStatus> attempt = await _mediaTypeService.UpdateAsync(mediaType, Constants.Security.SuperUserKey);
                if (!attempt.Success)
                {
                    Logger.LogError(attempt.Exception, $"Failed to update media type '{mediaType.Alias}' during migration.");
                }
            }
        }
    }
}
