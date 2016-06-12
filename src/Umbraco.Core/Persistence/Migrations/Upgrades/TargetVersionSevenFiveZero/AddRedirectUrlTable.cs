using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    [Migration("7.5.0", 100, GlobalSettings.UmbracoMigrationName)]
    public class AddRedirectUrlTable : MigrationBase
    {
        public AddRedirectUrlTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // don't exeucte if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoRedirectUrl")) return;

            Create.Table("umbracoRedirectUrl")
                .WithColumn("id").AsInt32().Identity().PrimaryKey("PK_umbracoRedirectUrl")
                .WithColumn("contentId").AsInt32().NotNullable()
                .WithColumn("createDateUtc").AsDateTime().NotNullable()
                .WithColumn("url").AsString(2048).NotNullable();

            //Create.PrimaryKey("PK_umbracoRedirectUrl").OnTable("umbracoRedirectUrl").Columns(new[] { "id" });

            Create.Index("IX_umbracoRedirectUrl").OnTable("umbracoRedirectUrl")
                  .OnColumn("url")
                  .Ascending()
                  .OnColumn("createDateUtc")
                  .Ascending()
                  .WithOptions().NonClustered();
        }

        public override void Down()
        { }
    }
}
