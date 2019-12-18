using Umbraco.Core.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    public class SixZeroMigration1 : MigrationBase
    {
        public SixZeroMigration1(IMigrationContext context)
            : base(context)
        { }


        public override void Migrate()
        {
            Alter.Table("umbracoUser").AddColumn("secret").AsString(255);
        }
    }
}
