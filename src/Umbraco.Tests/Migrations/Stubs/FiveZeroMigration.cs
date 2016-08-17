using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Stubs
{
    [Migration("5.0.0", 0, "Test")]
    public class FiveZeroMigration : MigrationBase
    {
        public FiveZeroMigration(IMigrationContext context) 
            : base(context)
        { }


        public override void Up()
        {
        }

        public override void Down()
        {
        }
        
    }
}