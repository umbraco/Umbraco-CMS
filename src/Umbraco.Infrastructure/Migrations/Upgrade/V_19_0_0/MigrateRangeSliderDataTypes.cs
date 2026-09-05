using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Moves each slider data type that was configured to hold a range onto the range slider editor, and drops the
/// configuration value that used to select it.
/// </summary>
/// <remarks>
/// <para>
/// There is now one slider editor per shape of value a slider holds, so that the type a slider property yields
/// follows from the editor rather than from the configuration of the data type it is used through. Sliders holding a
/// single value stay on <c>Umbraco.Slider</c> and only lose the redundant configuration value.
/// </para>
/// <para>
/// The stored values are left as they are: both editors read the same string, and each tolerates the other's shape.
/// </para>
/// </remarks>
public class MigrateRangeSliderDataTypes : AsyncMigrationBase
{
    private const string EnableRangeConfigurationKey = "enableRange";
    private const string RangeSliderEditorUiAlias = "Umb.PropertyEditorUi.Slider.Range";

    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger<MigrateRangeSliderDataTypes> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateRangeSliderDataTypes"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="dataTypeService">The service used to load and update data types.</param>
    /// <param name="propertyEditors">The property editors, used to resolve the range slider editor.</param>
    /// <param name="logger">The logger.</param>
    public MigrateRangeSliderDataTypes(
        IMigrationContext context,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        ILogger<MigrateRangeSliderDataTypes> logger)
        : base(context)
    {
        _dataTypeService = dataTypeService;
        _propertyEditors = propertyEditors;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override Task MigrateAsync() => ExecuteMigration(_dataTypeService, _propertyEditors, _logger);

    /// <summary>
    /// Performs the migration.
    /// </summary>
    /// <remarks>
    /// Extracted into an internal static method to support integration testing.
    /// </remarks>
    internal static async Task ExecuteMigration(
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        ILogger logger)
    {
        IDataType[] dataTypes = (await dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.Slider)).ToArray();

        foreach (IDataType dataType in dataTypes)
        {
            var holdsRange = HoldsRange(dataType);

            // The configuration value no longer selects anything, so it goes whether or not the editor changes.
            var configurationChanged = dataType.ConfigurationData.Remove(EnableRangeConfigurationKey);

            if (holdsRange is false)
            {
                if (configurationChanged)
                {
                    await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
                }

                continue;
            }

            if (propertyEditors.TryGet(Constants.PropertyEditors.Aliases.RangeSlider, out IDataEditor? editor) is false)
            {
                logger.LogError(
                    "Could not move data type {DataTypeName} ({DataTypeKey}) onto {EditorAlias}, as no such property editor was found.",
                    dataType.Name,
                    dataType.Key,
                    Constants.PropertyEditors.Aliases.RangeSlider);
                continue;
            }

            dataType.Editor = editor;
            dataType.EditorUiAlias = RangeSliderEditorUiAlias;

            await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

            logger.LogInformation(
                "Moved data type {DataTypeName} ({DataTypeKey}), configured to hold a range, onto {EditorAlias}.",
                dataType.Name,
                dataType.Key,
                Constants.PropertyEditors.Aliases.RangeSlider);
        }
    }

    private static bool HoldsRange(IDataType dataType)
        => dataType.ConfigurationData.TryGetValue(EnableRangeConfigurationKey, out var enableRange)
           && enableRange?.ToString() is { Length: > 0 } configured
           && bool.TryParse(configured, out var holdsRange)
           && holdsRange;
}
