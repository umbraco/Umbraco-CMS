using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Stubs
{
    [MigrationAttribute("6.0.0", 0, "Test")]
    public class AlterUserTableMigrationStub : MigrationBase
    {
        public AlterUserTableMigrationStub(IMigrationContext context) 
            : base(context)
        { }

        public override void Up()
        {
            Alter.Table("umbracoUser")
                 .AddColumn("Birthday")
                 .AsDateTime()
                 .Nullable();
        }

        public override void Down()
        { }
    }
}