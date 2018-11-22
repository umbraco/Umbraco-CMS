using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    /// <summary>
    /// This is here to re-remove these tables, we dropped them in 7.3 but new installs created them again so we're going to re-drop them
    /// </summary>
    [Migration("7.5.0", 1, Constants.System.UmbracoMigrationName)]
    public class RemoveStylesheetDataAndTablesAgain : MigrationBase
    {
        public RemoveStylesheetDataAndTablesAgain(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            // defer, because we are making decisions based upon what's in the database
            Execute.Code(MigrationCode);
        }

        private string MigrationCode(Database database)
        {
            var localContext = new LocalMigrationContext(Context.CurrentDatabaseProvider, database, SqlSyntax, Logger);
            
            //Clear all stylesheet data if the tables exist
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("cmsStylesheetProperty"))
            {
                localContext.Delete.FromTable("cmsStylesheetProperty").AllRows();
                localContext.Delete.FromTable("umbracoNode").Row(new { nodeObjectType = new Guid(Constants.ObjectTypes.StylesheetProperty) });

                localContext.Delete.Table("cmsStylesheetProperty");
            }
            if (tables.InvariantContains("cmsStylesheet"))
            {
                localContext.Delete.FromTable("cmsStylesheet").AllRows();
                localContext.Delete.FromTable("umbracoNode").Row(new { nodeObjectType = new Guid(Constants.ObjectTypes.Stylesheet) });

                localContext.Delete.Table("cmsStylesheet");
            }

            return localContext.GetSql();
        }

        public override void Down()
        {            
        }
    }
}