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
        public RemoveStylesheetDataAndTablesAgain(IMigrationContext context)
            : base(context)
        {
        }

        public override void Up()
        {
            // defer, because we are making decisions based upon what's in the database
            Execute.Code(MigrationCode);
        }

        private string MigrationCode(IUmbracoDatabase database)
        {
            var local = Context.GetLocalMigration();
            
            //Clear all stylesheet data if the tables exist
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("cmsStylesheetProperty"))
            {
                local.Delete.FromTable("cmsStylesheetProperty").AllRows();
                local.Delete.FromTable("umbracoNode").Row(new { nodeObjectType = new Guid(Constants.ObjectTypes.StylesheetProperty) });

                local.Delete.Table("cmsStylesheetProperty");
            }
            if (tables.InvariantContains("cmsStylesheet"))
            {
                local.Delete.FromTable("cmsStylesheet").AllRows();
                local.Delete.FromTable("umbracoNode").Row(new { nodeObjectType = new Guid(Constants.ObjectTypes.Stylesheet) });

                local.Delete.Table("cmsStylesheet");
            }

            return local.GetSql();
        }

        public override void Down()
        { }
    }
}