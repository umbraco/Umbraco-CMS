using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Moves each data type based on the <c>Umbraco.Label</c> property editor onto the label editor for the type of value
/// it was configured to hold, and drops the configuration value that used to select it.
/// </summary>
/// <remarks>
/// There is now one label editor per type of value a label can hold, so that the type a label property yields - and
/// the column its value is stored in - follow from the editor rather than from the configuration of the data type it
/// is used through. Data types configured to hold a string stay on <c>Umbraco.Label</c> and only lose the redundant
/// configuration value.
/// </remarks>
public class MigrateTypedLabelDataTypes : AsyncMigrationBase
{
    private readonly IDataTypeService _dataTypeService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly ILogger<MigrateTypedLabelDataTypes> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateTypedLabelDataTypes"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="dataTypeService">The service used to load and update data types.</param>
    /// <param name="propertyEditors">The property editors, used to resolve the label editor to move each data type onto.</param>
    /// <param name="logger">The logger.</param>
    public MigrateTypedLabelDataTypes(
        IMigrationContext context,
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        ILogger<MigrateTypedLabelDataTypes> logger)
        : base(context)
    {
        _dataTypeService = dataTypeService;
        _propertyEditors = propertyEditors;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override Task MigrateAsync() => ExecuteMigration(_dataTypeService, _propertyEditors, _logger);

    /// <summary>
    /// Gets the alias of the label editor that holds the given value type.
    /// </summary>
    /// <remarks>
    /// Every value type a label could be configured with maps onto an editor. The ones that only ever yielded a
    /// string are split by the column they are stored in, so that no value has to be relocated.
    /// </remarks>
    internal static string EditorAliasForValueType(string valueType) => EditorForValueType(valueType).EditorAlias;

    private static (string EditorAlias, string EditorUiAlias) EditorForValueType(string valueType)
        => valueType.ToUpperInvariant() switch
        {
            ValueTypes.Integer => (Constants.PropertyEditors.Aliases.LabelInteger, "Umb.PropertyEditorUi.Label.Integer"),
            ValueTypes.Bigint => (Constants.PropertyEditors.Aliases.LabelBigInt, "Umb.PropertyEditorUi.Label.BigInt"),
            ValueTypes.Decimal => (Constants.PropertyEditors.Aliases.LabelDecimal, "Umb.PropertyEditorUi.Label.Decimal"),
            ValueTypes.DateTime or ValueTypes.Date => (Constants.PropertyEditors.Aliases.LabelDateTime, "Umb.PropertyEditorUi.Label.DateTime"),
            ValueTypes.Time => (Constants.PropertyEditors.Aliases.LabelTime, "Umb.PropertyEditorUi.Label.Time"),
            ValueTypes.Text or ValueTypes.Json or ValueTypes.Xml => (Constants.PropertyEditors.Aliases.LabelText, "Umb.PropertyEditorUi.Label.Text"),
            _ => (Constants.PropertyEditors.Aliases.Label, "Umb.PropertyEditorUi.Label"),
        };

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
        IDataType[] dataTypes = (await dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.Label)).ToArray();

        foreach (IDataType dataType in dataTypes)
        {
            var valueType = ConfiguredValueType(dataType);
            (var editorAlias, var editorUiAlias) = EditorForValueType(valueType);

            // The configuration value no longer selects anything, so it goes whether or not the editor changes.
            var configurationChanged = dataType.ConfigurationData
                .Remove(Constants.PropertyEditors.ConfigurationKeys.DataValueType);

            if (editorAlias == Constants.PropertyEditors.Aliases.Label)
            {
                if (configurationChanged)
                {
                    await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
                }

                continue;
            }

            if (propertyEditors.TryGet(editorAlias, out IDataEditor? editor) is false)
            {
                logger.LogError(
                    "Could not move data type {DataTypeName} ({DataTypeKey}) onto {EditorAlias}, as no such property editor was found.",
                    dataType.Name,
                    dataType.Key,
                    editorAlias);
                continue;
            }

            dataType.Editor = editor;
            dataType.EditorUiAlias = editorUiAlias;

            // Take the storage type from the editor now holding the data type. Each label editor declares the value
            // type its predecessor's configuration did, so this is the column the values already occupy.
            dataType.DatabaseType = ValueTypes.ToStorageType(editor.GetValueEditor().ValueType);

            await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);

            logger.LogInformation(
                "Moved data type {DataTypeName} ({DataTypeKey}), configured as {ValueType}, onto {EditorAlias}.",
                dataType.Name,
                dataType.Key,
                valueType,
                editorAlias);
        }
    }

    private static string ConfiguredValueType(IDataType dataType)
        => dataType.ConfigurationData
               .TryGetValue(Constants.PropertyEditors.ConfigurationKeys.DataValueType, out var valueType)
           && valueType?.ToString() is { Length: > 0 } configuredValueType
            ? configuredValueType
            : ValueTypes.String;
}
