using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 100, Constants.System.UmbracoMigrationName)]
    public class AddLockTable : MigrationBase
    {
        public AddLockTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
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
