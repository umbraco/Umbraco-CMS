using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionEight
{
    [Migration("8.0.0", 100, Constants.System.UmbracoMigrationName)]
    public class AddRedirectUrlTable : MigrationBase
    {
        public AddRedirectUrlTable(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            var database = Database;
            var umbracoRedirectUrlTableName = "umbracoRedirectUrl";

            var tables = SqlSyntax.GetTablesInSchema(database).ToArray();

            if (tables.InvariantContains(umbracoRedirectUrlTableName))
            {
                var columns = SqlSyntax.GetColumnsInSchema(database).ToArray();
                if (columns.Any(x => x.TableName.InvariantEquals(umbracoRedirectUrlTableName) && x.ColumnName.InvariantEquals("id") && x.DataType == "uniqueidentifier"))
                    return;
                Delete.Table(umbracoRedirectUrlTableName).Do();
            }

            Create.Table(umbracoRedirectUrlTableName)
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey("PK_" + umbracoRedirectUrlTableName)
                .WithColumn("createDateUtc").AsDateTime().NotNullable()
                .WithColumn("url").AsString(2048).NotNullable()
                .WithColumn("contentKey").AsGuid().NotNullable()
                .WithColumn("urlHash").AsString(40).NotNullable()
                .Do();

            Create.Index("IX_" + umbracoRedirectUrlTableName).OnTable(umbracoRedirectUrlTableName)
                .OnColumn("urlHash")
                .Ascending()
                .OnColumn("contentKey")
                .Ascending()
                .OnColumn("createDateUtc")
                .Descending()
                .WithOptions().NonClustered()
                .Do();

            Create.ForeignKey("FK_" + umbracoRedirectUrlTableName)
                .FromTable(umbracoRedirectUrlTableName).ForeignColumn("contentKey")
                .ToTable("umbracoNode").PrimaryColumn("uniqueID")
                .Do();
        }

        public override void Down()
        { }
    }
}
