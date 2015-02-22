using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 5, GlobalSettings.UmbracoMigrationName)]
    public class AddPublicAccessTables : MigrationBase
    {
        public AddPublicAccessTables(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoAccess")) return;

            Create.Table("umbracoAccess")
                .WithColumn("id").AsInt32().NotNullable().Identity().PrimaryKey("PK_umbracoAccess")
                .WithColumn("nodeId").AsInt32().NotNullable().ForeignKey("FK_umbracoAccess_umbracoNode_id", "umbracoNode", "id")
                .WithColumn("loginNodeId").AsInt32().NotNullable().ForeignKey("FK_umbracoAccess_umbracoNode_id1", "umbracoNode", "id")
                .WithColumn("noAccessNodeId").AsInt32().NotNullable().ForeignKey("FK_umbracoAccess_umbracoNode_id2", "umbracoNode", "id");

            Create.Table("umbracoAccessRule")
                .WithColumn("id").AsInt32().NotNullable().Identity().PrimaryKey("PK_umbracoAccessRule")
                .WithColumn("accessId").AsInt32().NotNullable().ForeignKey("FK_umbracoAccessRule_umbracoAccess_id", "umbracoAccess", "id")
                .WithColumn("claim").AsString().NotNullable()
                .WithColumn("claimType").AsString().NotNullable();

            //Create.PrimaryKey("PK_cmsContentType2ContentType")
            //      .OnTable("cmsContentType2ContentType")
            //      .Columns(new[] { "parentContentTypeId", "childContentTypeId" });
        }

        public override void Down()
        {
        }
    }
}