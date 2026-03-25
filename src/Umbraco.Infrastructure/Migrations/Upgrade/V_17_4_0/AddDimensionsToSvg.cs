using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;

/// <summary>
/// Adds umbracoWidth and umbracoHeight to Vector Graphics Media Type.
/// </summary>
public class AddDimensionsToSvg : AsyncMigrationBase
{
    private readonly ILogger<AddDimensionsToSvg> _logger;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IShortStringHelper _shortStringHelper;

    private readonly Guid _labelPixelsDataTypeKey = new(Constants.DataTypes.Guids.LabelPixels);

    /// <summary>
    /// Migration that adds umbracoWidth and umbracoHeight to Vector Graphics Media Type
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration operation.</param>
    /// <param name="logger">The <see cref="ILogger{AddDimensionsToSvg}"/> instance for logging.</param>
    /// <param name="mediaTypeService">The <see cref="IMediaTypeService"/> for managing media types.</param>
    /// <param name="dataTypeService">The <see cref="IDataTypeService"/> for managing data types</param>
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

    protected override async Task MigrateAsync()
    {
        IMediaType? vectorGraphicsMediaType = _mediaTypeService.Get(Core.Constants.Conventions.MediaTypes.VectorGraphicsAlias);

        if (vectorGraphicsMediaType is null)
        {
            _logger.LogInformation("No standard Vector Graphics Media Type configured, ignore adding width/height");
            return;
        }

        IDataType? labelPixelDataType = await _dataTypeService.GetAsync(_labelPixelsDataTypeKey);

        if (labelPixelDataType is null)
        {
            _logger.LogInformation("No Label Pixel Data Type configured, ignore adding width/height");
            return;
        }

        int? propertyGroupId = vectorGraphicsMediaType.PropertyGroups.First().Id;
        int highestSort = vectorGraphicsMediaType.PropertyTypes.Max(x => x.SortOrder);

        // Adding new properties (AddPropertyType handles duplicates)

        vectorGraphicsMediaType.AddPropertyType(new PropertyType(_shortStringHelper, labelPixelDataType, Constants.Conventions.Media.Width)
        {
            Name = "Width",
            SortOrder = highestSort + 1,
            PropertyGroupId = new Lazy<int>(()=> propertyGroupId.Value)
        });

        vectorGraphicsMediaType.AddPropertyType(new PropertyType(_shortStringHelper, labelPixelDataType, Constants.Conventions.Media.Height)
        {
            Name = "Height",
            SortOrder = highestSort + 2,
            PropertyGroupId = new Lazy<int>(()=> propertyGroupId.Value)
        });

        Attempt<ContentTypeOperationStatus> attempt = await _mediaTypeService.UpdateAsync(vectorGraphicsMediaType, Constants.Security.SuperUserKey);
        if (attempt.Success is false)
        {
            _logger.LogError(attempt.Exception, "Failed to update media type  '{Alias}' during migration.", vectorGraphicsMediaType.Alias);
        }

    }
}
