using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Stubs
{
    [Migration("1.0.0", 0, "com.company.package")]
    public class PackageMigration1 : MigrationBase
    {
        private const string TableName = "RandomTableTest";

        public PackageMigration1(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Create.Table(TableName)
                .WithColumn("Id").AsGuid().Unique().NotNullable().PrimaryKey("awesome_id")
                .WithColumn("Content").AsString(500).NotNullable()
                .WithColumn("TimeStamp").AsDateTime().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}