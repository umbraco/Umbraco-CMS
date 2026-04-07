using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Migrates data types based on the Umbraco.CheckBoxList property editor to store data in the text column without length restriction.
/// Also migrates the data for properties this property editor from the length restricted field (varcharValue - nvarchar(512)), to the
/// one without a restriction (textValue - nvarchar(max)).
/// </summary>
public class MigrateCheckboxListDataTypesAndPropertyData : AsyncMigrationBase
{
    private readonly IDataTypeService _dataTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrateCheckboxListDataTypesAndPropertyData"/> class.
    /// </summary>
    /// <param name="context">The migration context used to manage the migration process.</param>
    /// <param name="dataTypeService">The service used to access and manage data types.</param>
    public MigrateCheckboxListDataTypesAndPropertyData(IMigrationContext context, IDataTypeService dataTypeService)
        : base(context) => _dataTypeService = dataTypeService;

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        // Update the definition of the datatypes.
        IEnumerable<IDataType> dataTypes = await _dataTypeService.GetByEditorAliasAsync(Constants.PropertyEditors.Aliases.CheckBoxList);
        foreach (IDataType dataType in dataTypes)
        {
            dataType.DatabaseType = ValueStorageType.Ntext;
            await _dataTypeService.UpdateAsync(dataType, Constants.Security.SuperUserKey);
        }

        // Copy from varcharValue to textValue and set varcharValue to null for all property data stored using data types based on
        // the Umbraco.CheckBoxList property editor.
        string sql = $@"
UPDATE umbracoPropertyData
SET textValue = varcharValue, varcharValue = NULL
WHERE propertyTypeId IN (
	SELECT id
    FROM cmsPropertyType
	WHERE dataTypeId IN (
		SELECT nodeId
        FROM umbracoDataType
        WHERE propertyEditorAlias = '{Constants.PropertyEditors.Aliases.CheckBoxList}'
	)
)
AND varcharValue IS NOT NULL";
        await Database.ExecuteAsync(sql);
    }
}
