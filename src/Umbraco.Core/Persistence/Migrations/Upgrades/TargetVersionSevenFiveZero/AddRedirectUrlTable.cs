using System.Linq;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    [Migration("7.5.0", 100, GlobalSettings.UmbracoMigrationName)]
    public class AddRedirectUrlTable : MigrationBase
    {
        public AddRedirectUrlTable(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            // defer, because we are making decisions based upon what's in the database
            Execute.Code(MigrationCode);
        }

        private string MigrationCode(UmbracoDatabase database)
        {
            // don't execute if the table is already there
            var tables = SqlSyntax.GetTablesInSchema(database).ToArray();
            if (tables.InvariantContains("umbracoRedirectUrl")) return null;

            var localContext = new LocalMigrationContext(database, Logger);

            localContext.Create.Table("umbracoRedirectUrl")
                .WithColumn("id").AsInt32().Identity().PrimaryKey("PK_umbracoRedirectUrl")
                .WithColumn("contentId").AsInt32().NotNullable()
                .WithColumn("createDateUtc").AsDateTime().NotNullable()
                .WithColumn("url").AsString(2048).NotNullable();

            localContext.Create.Index("IX_umbracoRedirectUrl")
                .OnTable("umbracoRedirectUrl")
                .OnColumn("url").Ascending()
                .OnColumn("createDateUtc").Ascending()
                .WithOptions().NonClustered();

            return localContext.GetSql();
        }

        public override void Down()
        { }
    }
}
