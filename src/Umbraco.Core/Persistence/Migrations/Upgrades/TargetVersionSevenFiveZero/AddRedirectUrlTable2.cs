using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter;
using Umbraco.Core.Persistence.Migrations.Syntax.Create;
using Umbraco.Core.Persistence.Migrations.Syntax.Delete;
using Umbraco.Core.Persistence.Migrations.Syntax.Execute;
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
            // defer, because we are making decisions based upon what's in the database
            Execute.Code(MigrationCode);
        }

        private string MigrationCode(Database database)
        {
            var columns = SqlSyntax.GetColumnsInSchema(database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("umbracoRedirectUrl") && x.ColumnName.InvariantEquals("contentKey")))
                return null;

            var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);

            localContext.Execute.Sql("DELETE FROM umbracoRedirectUrl"); // else cannot add non-nullable field

            localContext.Delete.Column("contentId").FromTable("umbracoRedirectUrl");

            // SQL CE does not want to alter-add non-nullable columns ;-(
            // but it's OK to create as nullable then alter, go figure
            //localContext.Alter.Table("umbracoRedirectUrl")
            //    .AddColumn("contentKey").AsGuid().NotNullable();
            localContext.Alter.Table("umbracoRedirectUrl")
                .AddColumn("contentKey").AsGuid().Nullable();
            localContext.Alter.Table("umbracoRedirectUrl")
                .AlterColumn("contentKey").AsGuid().NotNullable();

            localContext.Create.ForeignKey("FK_umbracoRedirectUrl")
                .FromTable("umbracoRedirectUrl").ForeignColumn("contentKey")
                .ToTable("umbracoNode").PrimaryColumn("uniqueID");

            return localContext.GetSql();
        }

        public override void Down()
        { }
    }
}
