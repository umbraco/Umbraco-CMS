using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    [MigrationAttribute("6.0.0", 0, "Test")]
    public class AlterUserTableMigrationStub : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("umbracoUser")
                 .AddColumn("Birthday")
                 .AsDateTime()
                 .Nullable();
        }

        public override void Down()
        {
        }
    }


    [MigrationAttribute("1.0.0", 0, "Test")]
    public class DropForeignKeyMigrationStub : MigrationBase
    {
        public override void Up()
        {
            Delete.ForeignKey().FromTable("umbracoUser2app").ForeignColumn("user").ToTable("umbracoUser").PrimaryColumn("id");
        }

        public override void Down()
        {
        }
    }
}