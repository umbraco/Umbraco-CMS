using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddIndexToUser2NodePermission : MigrationBase
    {
        public AddIndexToUser2NodePermission(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(Context.Database);

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoUser2NodePermission_nodeId")) == false)
            {
                Create.Index("IX_umbracoUser2NodePermission_nodeId").OnTable("umbracoUser2NodePermission")
                    .OnColumn("nodeId")
                    .Ascending()
                    .WithOptions()
                    .NonClustered()
                    .Do();
            }
        }

        public override void Down()
        {
            Delete.Index("IX_umbracoUser2NodePermission_nodeId").OnTable("cmsMember").Do();
        }
    }
}
