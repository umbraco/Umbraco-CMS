using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwoZero
{
    

    [Migration("7.2.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AlterDataTypePreValueTable : MigrationBase
    {
        public override void Up()
        {
            //To text
            var textType = SqlSyntaxContext.SqlSyntaxProvider.GetSpecialDbType(SpecialDbTypes.NTEXT);
            Alter.Table("cmsDataTypePreValues").AlterColumn("value").AsCustom(textType).Nullable();
        }

        public override void Down()
        {
            //back to 2500 chars
            Alter.Table("cmsDataTypePreValues").AlterColumn("value").AsString(2500).Nullable();
        }
    }
}