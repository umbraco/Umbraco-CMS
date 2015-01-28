using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 6, GlobalSettings.UmbracoMigrationName)]
    public class AddPublicAccessTables : MigrationBase
    {
        public override void Up()
        {
            //Don't exeucte if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoAccess")) return;

            Create.Table("umbracoAccess")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey("PK_umbracoAccess")
                .WithColumn("nodeId").AsInt32().NotNullable().ForeignKey("FK_umbracoAccess_umbracoNode_id", "umbracoNode", "id")
                .WithColumn("loginNodeId").AsInt32().NotNullable().ForeignKey("FK_umbracoAccess_umbracoNode_id1", "umbracoNode", "id")
                .WithColumn("noAccessNodeId").AsInt32().NotNullable().ForeignKey("FK_umbracoAccess_umbracoNode_id2", "umbracoNode", "id")
                .WithColumn("createDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("updateDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            //unique constraint on node id = 1:1
            Create.Index("IX_umbracoAccess_nodeId").OnTable("umbracoAccess").OnColumn("nodeId").Unique();

            Create.Table("umbracoAccessRule")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey("PK_umbracoAccessRule")
                .WithColumn("accessId").AsGuid().NotNullable().ForeignKey("FK_umbracoAccessRule_umbracoAccess_id", "umbracoAccess", "id")
                .WithColumn("ruleValue").AsString().NotNullable()
                .WithColumn("ruleType").AsString().NotNullable()
                .WithColumn("createDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("updateDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            //unique constraint on node + ruleValue + ruleType
            Create.Index("IX_umbracoAccessRule").OnTable("umbracoAccessRule")
                .OnColumn("accessId").Ascending()
                .OnColumn("ruleValue").Ascending()
                .OnColumn("ruleType").Ascending()
                .WithOptions()
                .Unique();
        }

        public override void Down()
        {
        }
    }
}