using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Stubs
{
    [Migration("1.0.0", 0, "Test")]
    public class DropForeignKeyMigrationStub : MigrationBase
    {
        public DropForeignKeyMigrationStub(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Delete.ForeignKey().FromTable("umbracoUser2app").ForeignColumn("user").ToTable("umbracoUser").PrimaryColumn("id");
        }

        public override void Down()
        {
        }
    }
}