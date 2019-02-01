using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourteenZero
{
    /// <summary>
    /// Migrates member group picker properties from NVarchar to NText. See https://github.com/umbraco/Umbraco-CMS/issues/3268.
    /// </summary>
    [Migration("7.14.0", 1, Constants.System.UmbracoMigrationName)]
    public class UpdateMemberGroupPickerData : MigrationBase
    {
        public UpdateMemberGroupPickerData(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            // move the data for all member group properties from the NVarchar to the NText column and clear the NVarchar column
            Execute.Sql($@"UPDATE cmsPropertyData SET dataNtext = dataNvarchar, dataNvarchar = NULL
                WHERE dataNtext IS NULL AND id IN (
	                SELECT id FROM cmsPropertyData WHERE propertyTypeId in (
		                SELECT id from cmsPropertyType where dataTypeID IN (
			                SELECT nodeId FROM cmsDataType WHERE propertyEditorAlias = '{Constants.PropertyEditors.MemberGroupPickerAlias}'
		                )
	                )
                )");

            // ensure that all exising member group properties are defined as NText
            Execute.Sql($"UPDATE cmsDataType SET dbType = 'Ntext' WHERE propertyEditorAlias = '{Constants.PropertyEditors.MemberGroupPickerAlias}'");
        }

        public override void Down()
        {
        }
    }
}
