using System.Linq;
using Umbraco.Core.Migrations.Expressions.Execute.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.PropertyEditors;

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
            // Rebuild keys and indexes for these tables, unfortunately we cannot use the Delete.KeysAndIndexes
            // procedure since for some reason that tries to drop the PK and we don't want that and it would be a breaking
            // change to add another parameter to that method so we'll just manually do it.
                        
            var nodeDtoObjectTypeIndex = $"IX_{NodeDto.TableName}_ObjectType"; // this is the one we'll rebuild
            // delete existing ones
            DeleteIndexes<NodeDto>($"IX_{NodeDto.TableName}_ParentId", $"IX_{NodeDto.TableName}_Trashed", nodeDtoObjectTypeIndex);
            CreateIndexes<NodeDto>(nodeDtoObjectTypeIndex);

            var contentVersionNodeIdIndex = $"IX_{ContentVersionDto.TableName}_NodeId";
            // delete existing ones
            DeleteIndexes<ContentVersionDto>(contentVersionNodeIdIndex);
            CreateIndexes<ContentVersionDto>(contentVersionNodeIdIndex);
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
