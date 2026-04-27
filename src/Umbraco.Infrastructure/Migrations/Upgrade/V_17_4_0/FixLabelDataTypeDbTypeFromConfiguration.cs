using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_4_0;

/// <summary>
/// Corrects the <see cref="IDataType.DatabaseType"/> for data types based on the <c>Umbraco.Label</c> property editor so it
/// matches the configured value type, and relocates any existing property data from <c>varcharValue</c> to <c>textValue</c>
/// for Label data types whose storage type is <see cref="ValueStorageType.Ntext"/>.
/// </summary>
/// <remarks>
/// Fixes https://github.com/umbraco/Umbraco-CMS/issues/22553. Before this migration, upgrading from older versions could
/// leave a Label data type configured as Long String with <c>dbType = Nvarchar</c>, causing SQL truncation when saving
/// content longer than 512 characters.
/// </remarks>
public class FixLabelDataTypeDbTypeFromConfiguration : AsyncMigrationBase
{
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FixLabelDataTypeDbTypeFromConfiguration"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="dataTypeService">The service used to load and update data types.</param>
    public FixLabelDataTypeDbTypeFromConfiguration(IMigrationContext context, IDataTypeService dataTypeService)
        : base(context)
        => _dataTypeService = dataTypeService;

    /// <inheritdoc/>
    protected override Task MigrateAsync() => ExecuteMigration(Database, _dataTypeService);

    /// <summary>
    /// Performs the migration: ensures each Label data type's <see cref="IDataType.DatabaseType"/> matches its configured
    /// <see cref="IConfigureValueType.ValueType"/>, then relocates any property data from <c>varcharValue</c> to
    /// <c>textValue</c> for Label data types whose storage type is <see cref="ValueStorageType.Ntext"/>.
    /// </summary>
    /// <remarks>
    /// Extracted into an internal static method to support integration testing.
    /// </remarks>
    internal static async Task ExecuteMigration(IUmbracoDatabase database, IDataTypeService dataTypeService)
    {
        IEnumerable<IDataType> dataTypes = await dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.Label);

        foreach (IDataType dataType in dataTypes)
        {
            var valueType = dataType.ConfigurationObject is IConfigureValueType configureValueType
                ? configureValueType.ValueType
                : ValueTypes.String;

            ValueStorageType expected = ValueTypes.ToStorageType(valueType);
            if (dataType.DatabaseType == expected)
            {
                continue;
            }

            dataType.DatabaseType = expected;
            await dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        }

        // Relocate any property data that was previously written to varcharValue on a Label data type whose dbType is now Ntext.
        // This runs whenever any Ntext Label data type exists — including data types this migration did not touch
        // (e.g. ones a user already re-saved as a workaround for #22553, leaving legacy rows behind in varcharValue).
        // Idempotent thanks to the AND varcharValue IS NOT NULL guard. On large umbracoPropertyData tables the copy
        // could exceed the default command timeout, so extend it here.
        EnsureLongCommandTimeout(database);

        var sql = $@"
UPDATE umbracoPropertyData
SET textValue = varcharValue, varcharValue = NULL
WHERE propertyTypeId IN (
    SELECT id
    FROM cmsPropertyType
    WHERE dataTypeId IN (
        SELECT nodeId
        FROM umbracoDataType
        WHERE propertyEditorAlias = '{Constants.PropertyEditors.Aliases.Label}'
        AND dbType = '{nameof(ValueStorageType.Ntext)}'
    )
)
AND varcharValue IS NOT NULL";
        await database.ExecuteAsync(sql);
    }
}
