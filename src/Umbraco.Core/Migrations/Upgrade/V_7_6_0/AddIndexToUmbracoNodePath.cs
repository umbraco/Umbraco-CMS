using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.V_7_6_0
{
    public class AddIndexToUmbracoNodePath : MigrationBase
    {
        public AddIndexToUmbracoNodePath(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(Context.Database);

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoNodePath")) == false)
            {
                Create.Index("IX_umbracoNodePath").OnTable("umbracoNode")
                    .OnColumn("path")
                    .Ascending()
                    .WithOptions()
                    .NonClustered()
                    .Do();
            }
        }
    }
}
