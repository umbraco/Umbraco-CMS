using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 11, GlobalSettings.UmbracoMigrationName)]
    public class AddMigrationTable : MigrationBase
    {
        public AddMigrationTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Don't exeucte if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoMigration")) return;

            Create.Table("umbracoMigration")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey("PK_umbracoMigrations").Identity()
                .WithColumn("name").AsString(255).NotNullable()
                .WithColumn("version").AsString(50).NotNullable()
                .WithColumn("createDate").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            //unique constraint on name + version
            Create.Index("IX_umbracoMigration").OnTable("umbracoMigration")
                .OnColumn("name").Ascending()
                .OnColumn("version").Ascending()
                .WithOptions()
                .NonClustered()
                .WithOptions()
                .Unique();
        }

        public override void Down()
        {
        }
    }
}