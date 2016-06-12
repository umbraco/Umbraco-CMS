using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    [Migration("7.5.0", 100, GlobalSettings.UmbracoMigrationName)]
    public class CreateContentUrlRuleTable : MigrationBase
    {
        public CreateContentUrlRuleTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // don't exeucte if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoContentUrlRule")) return;

            Create.Table("umbracoContentUrlRule")
                .WithColumn("id").AsInt32().Identity().PrimaryKey("PK_umbracoContentUrlRule")
                .WithColumn("contentId").AsInt32().NotNullable()
                .WithColumn("createDateUtc").AsDateTime().NotNullable()
                .WithColumn("url").AsString(2048).NotNullable();

            Create.PrimaryKey("PK_umbracoContentUrlRule").OnTable("umbracoContentUrlRule").Columns(new[] { "id" });

            Create.Index("IX_umbracoContenUrlRule").OnTable("umbracoContentUrlRule")
                  .OnColumn("url")
                  .Ascending()
                  .OnColumn("createDateUtc")
                  .Ascending();
        }

        public override void Down()
        { }
    }
}
