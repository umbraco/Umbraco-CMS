using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Stubs
{
    [Migration("2.0.0", 0, "com.company.package")]
    public class PackageMigration2 : MigrationBase
    {
        private const string TableName = "RandomTableTest";
        private const string ColumnName = "MoreContent";

        public PackageMigration2(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Alter.Table(TableName).AddColumn(ColumnName).AsString(200).Nullable();
        }

        public override void Down()
        {
            Delete.Column(ColumnName).FromTable(TableName);
        }
    }
}