using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_3_0;

[Obsolete("Remove in Umbraco 18.")]
public class MigrateMediaTypeLabelProperties : AsyncMigrationBase
{
    private readonly IMediaTypeService _mediaTypeService;

    private readonly InstallDefaultDataSettings? _dataTypeSettings;
    private readonly InstallDefaultDataSettings? _mediaTypeSettings;

    private readonly Guid _labelBytesDataTypeKey = new(Constants.DataTypes.Guids.LabelBytes);
    private readonly Guid _labelPixelsDataTypeKey = new(Constants.DataTypes.Guids.LabelPixels);

    private readonly string[] _mediaTypeKeys =
    [
        "cc07b313-0843-4aa8-bbda-871c8da728c8", // Image
        "4c52d8ab-54e6-40cd-999c-7a5f24903e4d", // File
        "f6c515bb-653c-4bdc-821c-987729ebe327", // Video
        "a5ddeee0-8fd8-4cee-a658-6f1fcdb00de3", // Audio
        "a43e3414-9599-4230-a7d3-943a21b20122", // Article
        "c4b1efcf-a9d5-41c4-9621-e9d273b52a9c", // Vector Graphics (SVG)
    ];

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

        IfNotExistsCreateBytesLabel();
        IfNotExistsCreatePixelsLabel();
        await MigrateMediaTypeLabels();
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
        // update the media types with the new data-type references
        foreach (string mediaTypeKey in _mediaTypeKeys)
        {
            if (_mediaTypeSettings?.InstallData == InstallDefaultDataOption.Values && !_mediaTypeSettings.Values.InvariantContains(mediaTypeKey))
            {
                return;
            }

            if (_mediaTypeSettings?.InstallData == InstallDefaultDataOption.ExceptValues && _mediaTypeSettings.Values.InvariantContains(mediaTypeKey))
            {
                return;
            }

            IMediaType? mediaType = await _mediaTypeService.GetAsync(new Guid(mediaTypeKey));

            if (mediaType is null)
            {
                continue;
            }

            bool updated = false;

            foreach (IPropertyType propertyType in mediaType.PropertyTypes)
            {
                switch (propertyType.Alias)
                {
                    case Constants.Conventions.Media.Bytes:
                        if (propertyType.DataTypeId == Constants.DataTypes.LabelBigint)
                        {
                            propertyType.DataTypeId = Constants.DataTypes.LabelBytes;
                            propertyType.DataTypeKey = _labelBytesDataTypeKey;
                            updated = true;
                        }

                        break;

                    case Constants.Conventions.Media.Height:
                    case Constants.Conventions.Media.Width:
                        if (propertyType.DataTypeId == Constants.DataTypes.LabelInt)
                        {
                            propertyType.DataTypeId = Constants.DataTypes.LabelPixels;
                            propertyType.DataTypeKey = _labelPixelsDataTypeKey;
                            updated = true;
                        }

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
