using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    [MigrationAttribute("6.0.0", 1, "Test")]
    public class SixZeroMigration1 : MigrationBase
    {
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