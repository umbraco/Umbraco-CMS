using System.Linq;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionEight
{
    [Migration("8.0.0", 100, GlobalSettings.UmbracoMigrationName)]
    public class AddLockTable : MigrationBase
    {
        public AddLockTable(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("umbracoLock") == false)
            {
                Create.Table("umbracoLock")
                    .WithColumn("id").AsInt32().PrimaryKey("PK_umbracoLock")
                    .WithColumn("value").AsInt32().NotNullable()
                    .WithColumn("name").AsString(64).NotNullable();
            }
        }

        public override void Down()
        {
            // not implemented
        }
    }
}
