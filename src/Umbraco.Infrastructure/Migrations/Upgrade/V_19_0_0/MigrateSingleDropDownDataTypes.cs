using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Moves each dropdown data type that was configured to hold a single value onto the single dropdown editor, and
/// drops the configuration value that used to select it.
/// </summary>
/// <remarks>
/// <para>
/// There is now one dropdown editor per number of values it holds, so that the type a dropdown property yields
/// follows from the editor rather than from the configuration of the data type it is used through. Dropdowns holding
/// any number of values stay on <c>Umbraco.DropDown.Flexible</c> and only lose the redundant configuration value.
/// </para>
/// <para>
/// The stored values are left as they are: both editors read the same JSON array, and the single dropdown takes the
/// first value of one holding several.
/// </para>
/// </remarks>
public class MigrateSingleDropDownDataTypes : AsyncMigrationBase
{
    private const string MultipleConfigurationKey = "multiple";
    private const string SingleDropDownEditorUiAlias = "Umb.PropertyEditorUi.Dropdown.Single";

    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger<MigrateSingleDropDownDataTypes> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateSingleDropDownDataTypes"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="dataTypeService">The service used to load and update data types.</param>
    /// <param name="propertyEditors">The property editors, used to resolve the single dropdown editor.</param>
    /// <param name="logger">The logger.</param>
    public MigrateSingleDropDownDataTypes(
        IMigrationContext context,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        ILogger<MigrateSingleDropDownDataTypes> logger)
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
        IDataType[] dataTypes = (await dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.MultipleDropDown)).ToArray();

        foreach (IDataType dataType in dataTypes)
        {
            var holdsSingleValue = HoldsSingleValue(dataType);

            // The configuration value no longer selects anything, so it goes whether or not the editor changes.
            var configurationChanged = dataType.ConfigurationData.Remove(MultipleConfigurationKey);

            if (holdsSingleValue is false)
            {
                if (configurationChanged)
                {
                    await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
                }

                continue;
            }

            if (propertyEditors.TryGet(Constants.PropertyEditors.Aliases.SingleDropDown, out IDataEditor? editor) is false)
            {
                logger.LogError(
                    "Could not move data type {DataTypeName} ({DataTypeKey}) onto {EditorAlias}, as no such property editor was found.",
                    dataType.Name,
                    dataType.Key,
                    Constants.PropertyEditors.Aliases.SingleDropDown);
                continue;
            }

            dataType.Editor = editor;
            dataType.EditorUiAlias = SingleDropDownEditorUiAlias;

            await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

            logger.LogInformation(
                "Moved data type {DataTypeName} ({DataTypeKey}), configured to hold a single value, onto {EditorAlias}.",
                dataType.Name,
                dataType.Key,
                Constants.PropertyEditors.Aliases.SingleDropDown);
        }
    }

    private static bool HoldsSingleValue(IDataType dataType)
        => dataType.ConfigurationData.TryGetValue(MultipleConfigurationKey, out var multiple) is false
           || multiple?.ToString() is not { Length: > 0 } configured
           || bool.TryParse(configured, out var holdsMultiple) is false
           || holdsMultiple is false;
}
