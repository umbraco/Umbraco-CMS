using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    [Migration("7.5.0", 102, GlobalSettings.UmbracoMigrationName)]
    public class AddRedirectUrlTable3 : MigrationBase
    {
        public AddRedirectUrlTable3(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoRedirectUrl") && x.ColumnName.InvariantEquals("hurl")))
                return;

            Execute.Sql("DELETE FROM umbracoRedirectUrl"); // else cannot add non-nullable field

            Delete.Index("IX_umbracoRedirectUrl").OnTable("umbracoRedirectUrl");

            Alter.Table("umbracoRedirectUrl")
                .AddColumn("hurl").AsString(16).NotNullable();

            Create.Index("IX_umbracoRedirectUrl").OnTable("umbracoRedirectUrl")
                .OnColumn("hurl")
                .Ascending()
                .OnColumn("contentKey")
                .Ascending()
                .OnColumn("createDateUtc")
                .Descending()
                .WithOptions().NonClustered();
        }

        public override void Down()
        { }
    }
}
