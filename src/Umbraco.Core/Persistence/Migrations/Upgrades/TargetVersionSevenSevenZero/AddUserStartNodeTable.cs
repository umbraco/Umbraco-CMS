using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSevenZero
{
    [Migration("7.7.0", 2, Constants.System.UmbracoMigrationName)]
    public class AddUserStartNodeTable : MigrationBase
    {
        public AddUserStartNodeTable(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains("umbracoUserStartNode") == false)
            {
                Create.Table<UserStartNodeDto>();

                MigrateUserStartNodes();

                //now remove the old columns

                Delete.Column("startStructureID").FromTable("umbracoUser");
                Delete.Column("startMediaID").FromTable("umbracoUser");                
            }
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

        public override void Down()
        {
        }
    }
}