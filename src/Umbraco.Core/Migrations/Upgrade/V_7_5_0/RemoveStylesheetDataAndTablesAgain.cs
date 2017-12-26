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
            //Clear all stylesheet data if the tables exist
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();
            if (tables.InvariantContains("cmsStylesheetProperty"))
            {
                Delete.FromTable("cmsStylesheetProperty").AllRows().Do();
                Delete.FromTable("umbracoNode").Row(new { nodeObjectType = Constants.ObjectTypes.StylesheetProperty }).Do();

                Delete.Table("cmsStylesheetProperty").Do();
            }
            if (tables.InvariantContains("cmsStylesheet"))
            {
                Delete.FromTable("cmsStylesheet").AllRows().Do();
                Delete.FromTable("umbracoNode").Row(new { nodeObjectType = Constants.ObjectTypes.Stylesheet }).Do();

                Delete.Table("cmsStylesheet").Do();
            }
        }
    }
}
