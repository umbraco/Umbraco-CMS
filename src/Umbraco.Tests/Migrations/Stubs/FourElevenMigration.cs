using Umbraco.Core.Migrations;

namespace Umbraco.Tests.Migrations.Stubs
{
    public class FourElevenMigration : MigrationBase
    {
        public FourElevenMigration(IMigrationContext context)
            : base(context)
        { }


        public override void Migrate()
        {
            Alter.Table("umbracoUser").AddColumn("companyPhone").AsString(255);
        }
    }
}
