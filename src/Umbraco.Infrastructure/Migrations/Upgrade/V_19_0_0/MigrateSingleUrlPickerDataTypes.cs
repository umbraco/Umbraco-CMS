using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Moves each URL picker data type that was configured to hold at most one link onto the single URL picker editor,
/// and drops the configuration values that used to select it.
/// </summary>
/// <remarks>
/// <para>
/// There is now one URL picker editor per number of links a picker holds, so that the type a URL picker property
/// yields follows from the editor rather than from the configuration of the data type it is used through. Pickers
/// holding any number of links stay on <c>Umbraco.MultiUrlPicker</c> and keep their link count validation.
/// </para>
/// <para>
/// A picker holding one link has no link count to validate, so both count values are dropped from the data types
/// that move. A <c>minNumber</c> of one used to make such a property required; that is now the property's own
/// mandatory setting, so requiredness is not carried over.
/// </para>
/// <para>
/// The stored values are left as they are: both editors read the same JSON array, and the single URL picker takes
/// the first link of one holding several.
/// </para>
/// </remarks>
public class MigrateSingleUrlPickerDataTypes : AsyncMigrationBase
{
    private const string MinNumberConfigurationKey = "minNumber";
    private const string MaxNumberConfigurationKey = "maxNumber";
    private const string SingleUrlPickerEditorUiAlias = "Umb.PropertyEditorUi.SingleUrlPicker";

    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger<MigrateSingleUrlPickerDataTypes> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateSingleUrlPickerDataTypes"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="dataTypeService">The service used to load and update data types.</param>
    /// <param name="propertyEditors">The property editors, used to resolve the single URL picker editor.</param>
    /// <param name="logger">The logger.</param>
    public MigrateSingleUrlPickerDataTypes(
        IMigrationContext context,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        ILogger<MigrateSingleUrlPickerDataTypes> logger)
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
        IDataType[] dataTypes = (await dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.MultiUrlPicker)).ToArray();

        foreach (IDataType dataType in dataTypes)
        {
            if (HoldsSingleLink(dataType) is false)
            {
                continue;
            }

            if (propertyEditors.TryGet(Constants.PropertyEditors.Aliases.SingleUrlPicker, out IDataEditor? editor) is false)
            {
                logger.LogError(
                    "Could not move data type {DataTypeName} ({DataTypeKey}) onto {EditorAlias}, as no such property editor was found.",
                    dataType.Name,
                    dataType.Key,
                    Constants.PropertyEditors.Aliases.SingleUrlPicker);
                continue;
            }

            dataType.ConfigurationData.Remove(MinNumberConfigurationKey);
            dataType.ConfigurationData.Remove(MaxNumberConfigurationKey);

            dataType.Editor = editor;
            dataType.EditorUiAlias = SingleUrlPickerEditorUiAlias;

            await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

            logger.LogInformation(
                "Moved data type {DataTypeName} ({DataTypeKey}), configured to hold a single link, onto {EditorAlias}.",
                dataType.Name,
                dataType.Key,
                Constants.PropertyEditors.Aliases.SingleUrlPicker);
        }
    }

    private static bool HoldsSingleLink(IDataType dataType)
        => dataType.ConfigurationData.TryGetValue(MaxNumberConfigurationKey, out var maxNumber)
           && maxNumber?.ToString() is { Length: > 0 } configured
           && int.TryParse(configured, out var maximum)
           && maximum == 1;
}
