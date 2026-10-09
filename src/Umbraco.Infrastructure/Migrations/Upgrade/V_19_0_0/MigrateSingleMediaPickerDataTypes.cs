using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Moves each media picker data type that was configured to hold a single item onto the single media picker editor,
/// and drops the configuration values that used to select it.
/// </summary>
/// <remarks>
/// <para>
/// There is now one media picker editor per number of items a picker holds, so that the type a media picker property
/// yields follows from the editor rather than from the configuration of the data type it is used through. Pickers
/// holding any number of items stay on <c>Umbraco.MediaPicker3</c> and only lose the redundant configuration value.
/// </para>
/// <para>
/// The stored values are left as they are: both editors read the same JSON array, and the single media picker takes
/// the first item of one holding several.
/// </para>
/// </remarks>
public class MigrateSingleMediaPickerDataTypes : AsyncMigrationBase
{
    private const string MultipleConfigurationKey = "multiple";
    private const string ValidationLimitConfigurationKey = "validationLimit";
    private const string SingleMediaPickerEditorUiAlias = "Umb.PropertyEditorUi.MediaPicker.Single";

    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger<MigrateSingleMediaPickerDataTypes> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateSingleMediaPickerDataTypes"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="dataTypeService">The service used to load and update data types.</param>
    /// <param name="propertyEditors">The property editors, used to resolve the single media picker editor.</param>
    /// <param name="logger">The logger.</param>
    public MigrateSingleMediaPickerDataTypes(
        IMigrationContext context,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        ILogger<MigrateSingleMediaPickerDataTypes> logger)
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
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    internal static async Task ExecuteMigration(
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        ILogger logger)
    {
        IDataType[] dataTypes = (await dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.MediaPicker3)).ToArray();

        foreach (IDataType dataType in dataTypes)
        {
            var holdsSingleItem = HoldsSingleItem(dataType);

            // The configuration value no longer selects anything, so it goes whether or not the editor changes.
            var configurationChanged = dataType.ConfigurationData.Remove(MultipleConfigurationKey);

            if (holdsSingleItem is false)
            {
                if (configurationChanged)
                {
                    await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
                }

                continue;
            }

            if (propertyEditors.TryGet(Constants.PropertyEditors.Aliases.SingleMediaPicker, out IDataEditor? editor) is false)
            {
                logger.LogError(
                    "Could not move data type {DataTypeName} ({DataTypeKey}) onto {EditorAlias}, as no such property editor was found.",
                    dataType.Name,
                    dataType.Key,
                    Constants.PropertyEditors.Aliases.SingleMediaPicker);
                continue;
            }

            // The single media picker holds one item by definition, so it has no item count to validate.
            dataType.ConfigurationData.Remove(ValidationLimitConfigurationKey);

            dataType.Editor = editor;
            dataType.EditorUiAlias = SingleMediaPickerEditorUiAlias;

            await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

            logger.LogInformation(
                "Moved data type {DataTypeName} ({DataTypeKey}), configured to hold a single item, onto {EditorAlias}.",
                dataType.Name,
                dataType.Key,
                Constants.PropertyEditors.Aliases.SingleMediaPicker);
        }
    }

    private static bool HoldsSingleItem(IDataType dataType)
        => dataType.ConfigurationData.TryGetValue(MultipleConfigurationKey, out var multiple) is false
           || multiple?.ToString() is not { Length: > 0 } configured
           || bool.TryParse(configured, out var holdsMultiple) is false
           || holdsMultiple is false;
}
