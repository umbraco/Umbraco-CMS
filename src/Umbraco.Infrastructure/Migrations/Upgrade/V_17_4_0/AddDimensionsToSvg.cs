using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;

/// <summary>
/// Migration that adds umbracoWidth and umbracoHeight to Vector Graphics Media Type.
/// </summary>
public class AddDimensionsToSvg : AsyncMigrationBase
{
    private readonly ILogger<AddDimensionsToSvg> _logger;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IShortStringHelper _shortStringHelper;

    private readonly Guid _labelPixelsDataTypeKey = new(Constants.DataTypes.Guids.LabelPixels);

    /// <summary>
    /// Initializes a new instance of the <see cref="AddDimensionsToSvg"/> class.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration operation.</param>
    /// <param name="logger">The <see cref="ILogger{AddDimensionsToSvg}"/> instance for logging.</param>
    /// <param name="mediaTypeService">The <see cref="IMediaTypeService"/> for managing media types.</param>
    /// <param name="dataTypeService">The <see cref="IDataTypeService"/> for managing data types.</param>
    /// <param name="shortStringHelper">The <see cref="IShortStringHelper"/> for working with strings.</param>
    public AddDimensionsToSvg(
        IMigrationContext context,
        ILogger<AddDimensionsToSvg> logger,
        IMediaTypeService mediaTypeService,
        IDataTypeService dataTypeService,
        IShortStringHelper shortStringHelper)
        : base(context)
    {
        _logger = logger;
        _mediaTypeService = mediaTypeService;
        _dataTypeService = dataTypeService;
        _shortStringHelper = shortStringHelper;
    }

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        IMediaType? vectorGraphicsMediaType = _mediaTypeService.Get(Constants.Conventions.MediaTypes.VectorGraphicsAlias);

        if (vectorGraphicsMediaType is null)
        {
            _logger.LogInformation("No standard Vector Graphics Media Type configured, ignore adding width/height.");
            return;
        }

        IDataType? labelPixelDataType = await _dataTypeService.GetAsync(_labelPixelsDataTypeKey);

        if (labelPixelDataType is null)
        {
            _logger.LogInformation("No Label Pixel Data Type configured, ignore adding width/height.");
            return;
        }

        PropertyGroup? propertyGroup = vectorGraphicsMediaType.PropertyGroups.FirstOrDefault();
        if (propertyGroup is null)
        {
            _logger.LogWarning("Vector Graphics Media Type has no property groups, skipping adding width/height.");
            return;
        }

        int highestSort = vectorGraphicsMediaType.PropertyTypes.Any()
            ? vectorGraphicsMediaType.PropertyTypes.Max(x => x.SortOrder)
            : 0;

        // Add new properties (AddPropertyType handles duplicates, so the migration is idempotent).
        vectorGraphicsMediaType.AddPropertyType(new PropertyType(_shortStringHelper, labelPixelDataType, Constants.Conventions.Media.Width)
        {
            Name = "Width",
            SortOrder = highestSort + 1,
            PropertyGroupId = new Lazy<int>(()=> propertyGroup.Id),
        });
        vectorGraphicsMediaType.AddPropertyType(new PropertyType(_shortStringHelper, labelPixelDataType, Constants.Conventions.Media.Height)
        {
            Name = "Height",
            SortOrder = highestSort + 2,
            PropertyGroupId = new Lazy<int>(()=> propertyGroup.Id),
        });

        Attempt<ContentTypeOperationStatus> attempt = await _mediaTypeService.UpdateAsync(vectorGraphicsMediaType, Constants.Security.SuperUserKey);
        if (attempt.Success is false)
        {
            if (attempt.Exception is not null)
            {
                _logger.LogError(attempt.Exception, "Failed to update media type '{Alias}' during migration.", vectorGraphicsMediaType.Alias);
            }
            else
            {
                _logger.LogWarning("Failed to update media type '{Alias}' during migration. Status: {ResultStatus}", vectorGraphicsMediaType.Alias, attempt.Result);
            }
        }
    }
}
