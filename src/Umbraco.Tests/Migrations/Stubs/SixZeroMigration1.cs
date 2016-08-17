using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Stubs
{
    [MigrationAttribute("6.0.0", 1, "Test")]
    public class SixZeroMigration1 : MigrationBase
    {
        public SixZeroMigration1(IMigrationContext context) 
            : base(context)
        { }


        public override void Up()
        {
            Alter.Table("umbracoUser").AddColumn("secret").AsString(255);
        }

        public override void Down()
        {
            Alter.Table("umbracoUser").AlterColumn("passwordTip").AsString(100);
        }
    }
}