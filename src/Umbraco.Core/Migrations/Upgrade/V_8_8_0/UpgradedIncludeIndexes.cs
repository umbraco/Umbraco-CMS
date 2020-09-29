using System.Linq;
using Umbraco.Core.Migrations.Expressions.Execute.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_8_0
{
    public class UpgradedIncludeIndexes : MigrationBase
    {
        public UpgradedIncludeIndexes(IMigrationContext context)
            : base(context)
        {

        }

        public override void Migrate()
        {
            var indexesToReplace = new[] { $"IX_{NodeDto.TableName}_UniqueId", $"IX_{NodeDto.TableName}_ObjectType" };
            DeleteIndexes<NodeDto>(indexesToReplace);                       // delete existing ones
            CreateIndexes<NodeDto>(indexesToReplace);                       // replace 
            CreateIndexes<NodeDto>($"IX_{NodeDto.TableName}_Level");        // add the new definitions

            var contentVersionNodeIdIndex = $"IX_{ContentVersionDto.TableName}_NodeId";
            DeleteIndexes<ContentVersionDto>(contentVersionNodeIdIndex);                    // delete existing ones
            CreateIndexes<ContentVersionDto>(contentVersionNodeIdIndex);                    // replace
            CreateIndexes<ContentVersionDto>($"IX_{ContentVersionDto.TableName}_Current");  // add the new definitions
        }

        private void DeleteIndexes<T>(params string[] toDelete)
        {
            var tableDef = DefinitionFactory.GetTableDefinition(typeof(T), Context.SqlContext.SqlSyntax);

            foreach (var i in toDelete)
                Delete.Index(i).OnTable(tableDef.Name).Do();
        }

        private void CreateIndexes<T>(params string[] toCreate)
        {
            var tableDef = DefinitionFactory.GetTableDefinition(typeof(T), Context.SqlContext.SqlSyntax);

            foreach(var c in toCreate)
            {
                // get the definition by name
                var index = tableDef.Indexes.First(x => x.Name == c);
                new ExecuteSqlStatementExpression(Context) { SqlStatement = Context.SqlContext.SqlSyntax.Format(index) }.Execute();
            }
            
        }
    }
}
