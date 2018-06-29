using Umbraco.Core.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    public class AlterUserTableMigrationStub : MigrationBase
    {
        public AlterUserTableMigrationStub(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Alter.Table("umbracoUser")
                 .AddColumn("Birthday")
                 .AsDateTime()
                 .Nullable();
        }
    }
}
