using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    [Migration("7.5.0", 101, GlobalSettings.UmbracoMigrationName)]
    public class AddRedirectUrlTable2 : MigrationBase
    {
        public AddRedirectUrlTable2(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoRedirectUrl") && x.ColumnName.InvariantEquals("contentKey")))
                return;

            Execute.Sql("DELETE FROM umbracoRedirectUrl"); // else cannot add non-nullable field

            Delete.Column("contentId").FromTable("umbracoRedirectUrl");

            Alter.Table("umbracoRedirectUrl")
                .AddColumn("contentKey").AsGuid().NotNullable();

            Create.ForeignKey("FK_umbracoRedirectUrl")
                .FromTable("umbracoRedirectUrl").ForeignColumn("contentKey")
                .ToTable("umbracoNode").PrimaryColumn("uniqueID");
        }

        public override void Down()
        { }
    }
}
