using System.Linq;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    [Migration("7.5.0", 103, GlobalSettings.UmbracoMigrationName)]
    public class AddRedirectUrlTable4 : MigrationBase
    {
        public AddRedirectUrlTable4(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            // defer, because we are making decisions based upon what's in the database
            Execute.Code(MigrationCode);
        }
        private string MigrationCode(UmbracoDatabase database)
        {
            var columns = SqlSyntax.GetColumnsInSchema(database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoRedirectUrl") && x.ColumnName.InvariantEquals("urlHash")))
                return null;

            var localContext = new LocalMigrationContext(database, Logger);

            localContext.Execute.Sql("DELETE FROM umbracoRedirectUrl"); // else cannot add non-nullable field

            localContext.Delete.Index("IX_umbracoRedirectUrl").OnTable("umbracoRedirectUrl");

            localContext.Delete.Column("hurl").FromTable("umbracoRedirectUrl");

            // SQL CE does not want to alter-add non-nullable columns ;-(
            // but it's OK to create as nullable then alter, go figure
            //localContext.Alter.Table("umbracoRedirectUrl")
            //    .AddColumn("urlHash").AsString(16).NotNullable();
            localContext.Alter.Table("umbracoRedirectUrl")
                .AddColumn("urlHash").AsString(16).Nullable();
            localContext.Alter.Table("umbracoRedirectUrl")
                .AlterColumn("urlHash").AsString(16).NotNullable();

            localContext.Create.Index("IX_umbracoRedirectUrl").OnTable("umbracoRedirectUrl")
                .OnColumn("urlHash")
                .Ascending()
                .OnColumn("contentKey")
                .Ascending()
                .OnColumn("createDateUtc")
                .Descending()
                .WithOptions().NonClustered();

            return localContext.GetSql();
        }

        public override void Down()
        { }
    }
}
