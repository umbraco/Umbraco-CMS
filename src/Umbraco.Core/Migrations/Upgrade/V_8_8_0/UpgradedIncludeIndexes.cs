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
            var nodeDtoLevelIndex = $"IX_{NodeDto.TableName}_Level";
            CreateIndexes<NodeDto>(nodeDtoLevelIndex); // add the new definition

            var contentVersionNodeIdIndex = $"IX_{ContentVersionDto.TableName}_NodeId";
            DeleteIndexes<ContentVersionDto>(contentVersionNodeIdIndex); // delete existing ones
            CreateIndexes<ContentVersionDto>(contentVersionNodeIdIndex); // add the updated definition
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
