using System.Linq;
using Umbraco.Core.Migrations.Expressions.Execute.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_15_0
{
    public class UpgradedIncludeIndexes : MigrationBase
    {
        public UpgradedIncludeIndexes(IMigrationContext context)
            : base(context)
        {

        }

        public override void Migrate()
        {
            // Need to drop the FK for the redirect table before modifying the unique id index
            Delete.ForeignKey()
                .FromTable(Constants.DatabaseSchema.Tables.RedirectUrl)
                .ForeignColumn("contentKey")
                .ToTable(NodeDto.TableName)
                .PrimaryColumn("uniqueID")
                .Do();
            var nodeDtoIndexes = new[] { $"IX_{NodeDto.TableName}_UniqueId", $"IX_{NodeDto.TableName}_ObjectType", $"IX_{NodeDto.TableName}_Level" };
            DeleteIndexes<NodeDto>(nodeDtoIndexes);                       // delete existing ones
            CreateIndexes<NodeDto>(nodeDtoIndexes);                       // update/add 
            // Now re-create the FK for the redirect table
            Create.ForeignKey()
                .FromTable(Constants.DatabaseSchema.Tables.RedirectUrl)
                .ForeignColumn("contentKey")
                .ToTable(NodeDto.TableName)
                .PrimaryColumn("uniqueID")
                .Do();


            var contentVersionIndexes = new[] { $"IX_{ContentVersionDto.TableName}_NodeId", $"IX_{ContentVersionDto.TableName}_Current" };
            DeleteIndexes<ContentVersionDto>(contentVersionIndexes);        // delete existing ones
            CreateIndexes<ContentVersionDto>(contentVersionIndexes);        // update/add 
        }

        private void DeleteIndexes<T>(params string[] toDelete)
        {
            var tableDef = DefinitionFactory.GetTableDefinition(typeof(T), Context.SqlContext.SqlSyntax);

            foreach (var i in toDelete)
                if (IndexExists(i))
                    Delete.Index(i).OnTable(tableDef.Name).Do();

        }

        private void CreateIndexes<T>(params string[] toCreate)
        {
            var tableDef = DefinitionFactory.GetTableDefinition(typeof(T), Context.SqlContext.SqlSyntax);

            foreach (var c in toCreate)
            {
                // get the definition by name
                var index = tableDef.Indexes.First(x => x.Name == c);
                new ExecuteSqlStatementExpression(Context) { SqlStatement = Context.SqlContext.SqlSyntax.Format(index) }.Execute();
            }

        }
    }
}
