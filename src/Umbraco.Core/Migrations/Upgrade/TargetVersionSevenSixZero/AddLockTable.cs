using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 100, Constants.System.UmbracoMigrationName)]
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
                    .WithColumn("name").AsString(64).NotNullable()
                    .Do();
            }
        }

        public override void Down()
        {
            // not implemented
        }
    }
}
