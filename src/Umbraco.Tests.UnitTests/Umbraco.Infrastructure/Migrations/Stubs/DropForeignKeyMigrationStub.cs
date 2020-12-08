using Umbraco.Core.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    public class DropForeignKeyMigrationStub : MigrationBase
    {
        public DropForeignKeyMigrationStub(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Delete.ForeignKey().FromTable("umbracoUser2app").ForeignColumn("user").ToTable("umbracoUser").PrimaryColumn("id").Do();
        }
    }
}
