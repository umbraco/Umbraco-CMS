using System.Data;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 100, Constants.System.UmbracoMigrationName)]
    public class AddDocumentSearchTable : MigrationBase
    {
        public AddDocumentSearchTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("cmsDocumentSearch") == false)
            {
                Create.Table("cmsDocumentSearch")
                    .WithColumn("NodeId").AsInt32().PrimaryKey("PK_cmsDocumentSearch")
                    .WithColumn("SearchText").AsString().NotNullable();

                Create.ForeignKey("FK_cmsDocumentSearch_umbracoNode_id")
                    .FromTable("cmsDocumentSearch").ForeignColumn("NodeId")
                    .ToTable("umbracoNode").PrimaryColumn("id").OnDeleteOrUpdate(Rule.Cascade);
            }
        }

        public override void Down()
        {
            // not implemented
        }
    }
}
