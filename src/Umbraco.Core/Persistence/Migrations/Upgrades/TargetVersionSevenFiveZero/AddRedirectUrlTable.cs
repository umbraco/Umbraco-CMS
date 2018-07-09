using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    [Migration("7.5.0", 100, Constants.System.UmbracoMigrationName)]
    public class AddRedirectUrlTable : MigrationBase
    {
        public AddRedirectUrlTable(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // defer, because we are making decisions based upon what's in the database
            Execute.Code(MigrationCode);
        }

        private string MigrationCode(Database database)
        {
            var umbracoRedirectUrlTableName = "umbracoRedirectUrl";

            var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

            var tables = SqlSyntax.GetTablesInSchema(database).ToArray();
            
            if (tables.InvariantContains(umbracoRedirectUrlTableName))
            {
                var columns = SqlSyntax.GetColumnsInSchema(database).ToArray();
                if (columns.Any(x => x.TableName.InvariantEquals(umbracoRedirectUrlTableName) && x.ColumnName.InvariantEquals("id") && x.DataType == "uniqueidentifier"))
                    return null;
                localContext.Delete.Table(umbracoRedirectUrlTableName);
            }
            
            localContext.Create.Table(umbracoRedirectUrlTableName)
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey("PK_" + umbracoRedirectUrlTableName)
                .WithColumn("createDateUtc").AsDateTime().NotNullable()
                .WithColumn("url").AsString(2048).NotNullable()
                .WithColumn("contentKey").AsGuid().NotNullable()
                .WithColumn("urlHash").AsString(40).NotNullable();

            localContext.Create.Index("IX_" + umbracoRedirectUrlTableName).OnTable(umbracoRedirectUrlTableName)
                .OnColumn("urlHash")
                .Ascending()
                .OnColumn("contentKey")
                .Ascending()
                .OnColumn("createDateUtc")
                .Descending()
                .WithOptions().NonClustered();

            localContext.Create.ForeignKey("FK_" + umbracoRedirectUrlTableName)
                .FromTable(umbracoRedirectUrlTableName).ForeignColumn("contentKey")
                .ToTable("umbracoNode").PrimaryColumn("uniqueID");

            return localContext.GetSql();
        }

        public override void Down()
        { }
    }
}
