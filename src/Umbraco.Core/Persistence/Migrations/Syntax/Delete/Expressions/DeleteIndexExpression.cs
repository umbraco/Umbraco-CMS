using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteIndexExpression : MigrationExpressionBase
    {

        public DeleteIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(sqlSyntax, current, databaseProviders)
        {
            Index = new IndexDefinition();
        }

        public DeleteIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, IndexDefinition index) 
            : base(sqlSyntax, current, databaseProviders)
        {
            Index = index;
        }

        public IndexDefinition Index { get; private set; }

        public override string ToString()
        {
            return string.Format(SqlSyntax.DropIndex,
                                 SqlSyntax.GetQuotedName(Index.Name),
                                 SqlSyntax.GetQuotedTableName(Index.TableName));
        }
    }
}