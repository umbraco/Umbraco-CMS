using System;
using System.Linq;

namespace Umbraco.Core.Migrations.Upgrade.V_7_5_0
{
    /// <summary>
    /// This is here to re-remove these tables, we dropped them in 7.3 but new installs created them again so we're going to re-drop them
    /// </summary>
    public class RemoveStylesheetDataAndTablesAgain : MigrationBase
    {
        public RemoveStylesheetDataAndTablesAgain(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
        {
            // these have been obsoleted, need to copy the values here
            var stylesheetPropertyObjectType = new Guid("5555da4f-a123-42b2-4488-dcdfb25e4111");
            var stylesheetObjectType = new Guid("9F68DA4F-A3A8-44C2-8226-DCBD125E4840");

            //Clear all stylesheet data if the tables exist
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("cmsStylesheetProperty"))
            {
                Delete.FromTable("cmsStylesheetProperty").AllRows().Do();
                Delete.FromTable("umbracoNode").Row(new { nodeObjectType = stylesheetPropertyObjectType }).Do();

                Delete.Table("cmsStylesheetProperty").Do();
            }
            if (tables.InvariantContains("cmsStylesheet"))
            {
                Delete.FromTable("cmsStylesheet").AllRows().Do();
                Delete.FromTable("umbracoNode").Row(new { nodeObjectType = stylesheetObjectType }).Do();

                Delete.Table("cmsStylesheet").Do();
            }
        }
    }
}
