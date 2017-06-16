using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionFourOneZero
{
    [Migration("4.1.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddPreviewXmlTable : MigrationBase
    {
        public AddPreviewXmlTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var tableName = "cmsPreviewXml";
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains(tableName)) return;

            Create.Table(tableName)
                .WithColumn("nodeId").AsInt32().NotNullable()
                .WithColumn("versionId").AsGuid().NotNullable()
                .WithColumn("timestamp").AsDateTime().NotNullable()
                .WithColumn("xml").AsString();
        }

        public override void Down()
        { }
    }
}