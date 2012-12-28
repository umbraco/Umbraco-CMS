using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    [MigrationAttribute("6.0.0", 1, "Test")]
    public class FourNineMigration : MigrationBase
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

    [MigrationAttribute("6.0.0", 2, "Test")]
    public class FourTenMigration : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("umbracoUser").AddColumn("secondEmail").AsString(255);
        }

        public override void Down()
        {
            Alter.Table("umbracoUser").AlterColumn("sendEmail").AsBoolean();
        }
    }

    [MigrationAttribute("4.11.0", 0, "Test")]
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