using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddIndexToUser2NodePermission : MigrationBase
    {
        public AddIndexToUser2NodePermission(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
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
                    .NonClustered();
            }
        }

        public override void Down()
        {
            Delete.Index("IX_umbracoUser2NodePermission_nodeId").OnTable("cmsMember");
        }
    }
}