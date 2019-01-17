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
        { }

        public override void Migrate()
        {
            Database.Execute($@"UPDATE umbracoPropertyData SET textValue = varcharValue, varcharValue = NULL
                WHERE textValue IS NULL AND id IN (
	                SELECT id FROM umbracoPropertyData WHERE propertyTypeId in (
		                SELECT id from cmsPropertyType where dataTypeId IN (
			                SELECT nodeId FROM umbracoDataType WHERE propertyEditorAlias = '{Constants.PropertyEditors.Aliases.MemberGroupPicker}'
                    )
                )
            )");

            Database.Execute($"UPDATE umbracoDataType SET dbType = 'Ntext' WHERE propertyEditorAlias = '{Constants.PropertyEditors.Aliases.MemberGroupPicker}'");
        }
    }
}
