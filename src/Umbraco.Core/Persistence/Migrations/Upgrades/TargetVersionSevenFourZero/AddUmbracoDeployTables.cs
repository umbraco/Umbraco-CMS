using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourZero
{
    [Migration("7.4.0", 5, GlobalSettings.UmbracoMigrationName)]
    public class AddUmbracoDeployTables : MigrationBase
    {
        public AddUmbracoDeployTables(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            //Don't exeucte if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoDeployChecksum")) return;

            Create.Table("umbracoDeployChecksum")
                .WithColumn("id").AsInt32().Identity().PrimaryKey("PK_umbracoDeployChecksum")
                .WithColumn("entityType").AsString(32).NotNullable()
                .WithColumn("entityGuid").AsGuid().Nullable()
                .WithColumn("entityPath").AsString(256).Nullable()
                .WithColumn("localChecksum").AsString(32).NotNullable()
                .WithColumn("compositeChecksum").AsString(32).Nullable();

            Create.Table("umbracoDeployDependency")
                .WithColumn("sourceId").AsInt32().NotNullable().ForeignKey("FK_umbracoDeployDependency_umbracoDeployChecksum_id1", "umbracoDeployChecksum", "id")
                .WithColumn("targetId").AsInt32().NotNullable().ForeignKey("FK_umbracoDeployDependency_umbracoDeployChecksum_id2", "umbracoDeployChecksum", "id")
                .WithColumn("mode").AsInt32().NotNullable();

            Create.PrimaryKey("PK_umbracoDeployDependency").OnTable("umbracoDeployDependency").Columns(new[] {"sourceId", "targetId"});

            Create.Index("IX_umbracoDeployChecksum").OnTable("umbracoDeployChecksum")
                  .OnColumn("entityType")
                  .Ascending()
                  .OnColumn("entityGuid")
                  .Ascending()
                  .OnColumn("entityPath")
                  .Unique();
        }

        public override void Down()
        { }
    }
}