namespace Umbraco.Core.Migrations.Upgrade.V_7_14_0
{
    /// <summary>
    /// Migrates member group picker properties from NVarchar to NText. See https://github.com/umbraco/Umbraco-CMS/issues/3268.
    /// </summary>
    public class UpdateMemberGroupPickerData : MigrationBase
    {
        /// <summary>
        /// Migrates member group picker properties from NVarchar to NText. See https://github.com/umbraco/Umbraco-CMS/issues/3268.
        /// </summary>
        public UpdateMemberGroupPickerData(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            Database.Execute($@"UPDATE cmsPropertyData SET dataNtext = dataNvarchar, dataNvarchar = NULL
                WHERE dataNtext IS NULL AND id IN (
	                SELECT id FROM cmsPropertyData WHERE propertyTypeId in (
		                SELECT id from cmsPropertyType where dataTypeID IN (
			                SELECT nodeId FROM cmsDataType WHERE propertyEditorAlias = '{Constants.PropertyEditors.Aliases.MemberGroupPicker}'
                    )
                )
            )");

            Database.Execute($"UPDATE cmsDataType SET dbType = 'Ntext' WHERE propertyEditorAlias = '{Constants.PropertyEditors.Aliases.MemberGroupPicker}'");
        }
    }
}
