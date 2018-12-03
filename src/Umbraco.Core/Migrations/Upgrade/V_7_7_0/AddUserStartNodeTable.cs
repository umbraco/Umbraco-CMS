using System.Linq;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_7_7_0
{
    public class AddUserStartNodeTable : MigrationBase
    {
        public AddUserStartNodeTable(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database).ToArray();

            if (tables.InvariantContains("umbracoUserStartNode")) return;

            Create.Table<UserStartNodeDto>().Do();

            MigrateUserStartNodes();

            //now remove the old columns

            Delete.Column("startStructureID").FromTable("umbracoUser").Do();
            Delete.Column("startMediaID").FromTable("umbracoUser").Do();
        }

        private void MigrateUserStartNodes()
        {
            Database.Execute(@"INSERT INTO umbracoUserStartNode (userId, startNode, startNodeType)
                SELECT id, startStructureID, 1
                FROM umbracoUser
                WHERE startStructureID IS NOT NULL AND startStructureID > 0 AND startStructureID IN (SELECT id FROM umbracoNode WHERE nodeObjectType='" + Constants.ObjectTypes.Document + "')");

            Database.Execute(@"INSERT INTO umbracoUserStartNode (userId, startNode, startNodeType)
                SELECT id, startMediaID, 2
                FROM umbracoUser
                WHERE startMediaID IS NOT NULL AND startMediaID > 0 AND startMediaID IN (SELECT id FROM umbracoNode WHERE nodeObjectType='" + Constants.ObjectTypes.Media + "')");
        }
    }
}
