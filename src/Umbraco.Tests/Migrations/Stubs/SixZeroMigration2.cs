using Umbraco.Core.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    public class SixZeroMigration2 : MigrationBase
    {
        public SixZeroMigration2(IMigrationContext context)
            : base(context)
        { }


        public override void Migrate()
        {
            Alter.Table("umbracoUser").AddColumn("secondEmail").AsString(255);
        }
    }
}
