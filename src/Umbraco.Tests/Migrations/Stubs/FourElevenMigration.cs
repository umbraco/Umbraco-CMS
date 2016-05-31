using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Stubs
{
    [Migration("4.11.0", 0, "Test")]
    public class FourElevenMigration : MigrationBase
    {
        public FourElevenMigration(IMigrationContext context) 
            : base(context)
        { }


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