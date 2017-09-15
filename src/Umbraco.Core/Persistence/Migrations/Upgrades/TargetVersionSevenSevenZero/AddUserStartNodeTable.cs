using System.Linq;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("8.0.0", 2, Constants.System.UmbracoMigrationName)]
    public class AddUserStartNodeTable : MigrationBase
    {
        public AddUserStartNodeTable(IMigrationContext context)
            : base(context)
        { }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains("umbracoUserStartNode")) return;

            Create.Table<UserStartNodeDto>();

            MigrateUserStartNodes();

            //now remove the old columns

            Delete.Column("startStructureID").FromTable("umbracoUser");
            Delete.Column("startMediaID").FromTable("umbracoUser");
        }

        private void MigrateUserStartNodes()
        {
            Execute.Sql(@"INSERT INTO umbracoUserStartNode (userId, startNode, startNodeType)
                SELECT id, startStructureID, 1
                FROM umbracoUser
                WHERE startStructureID IS NOT NULL AND startStructureID > 0 AND startStructureID IN (SELECT id FROM umbracoNode WHERE nodeObjectType='" + Constants.ObjectTypes.Document + "')");

            Execute.Sql(@"INSERT INTO umbracoUserStartNode (userId, startNode, startNodeType)
                SELECT id, startMediaID, 2
                FROM umbracoUser
                WHERE startMediaID IS NOT NULL AND startMediaID > 0 AND startMediaID IN (SELECT id FROM umbracoNode WHERE nodeObjectType='" + Constants.ObjectTypes.Media + "')");
        }
    }
}