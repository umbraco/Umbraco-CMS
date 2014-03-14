using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    [Migration("4.11.0", 0, "Test")]
    public class FourElevenMigration : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("umbracoUser").AddColumn("companyPhone").AsString(255);
        }

        public override void Down()
        {
            Alter.Table("umbracoUser").AlterColumn("regularPhone").AsString(255);
        }
    }
}