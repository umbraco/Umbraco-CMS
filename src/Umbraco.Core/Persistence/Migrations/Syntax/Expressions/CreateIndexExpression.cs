using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateIndexExpression : MigrationExpressionBase
    {
        public CreateIndexExpression()
        {
            Index = new IndexDefinition();
        }

        public CreateIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
            Index = new IndexDefinition();
        }

        public virtual IndexDefinition Index { get; set; }

        public override string ToString()
        {
            return SyntaxConfig.SqlSyntaxProvider.Format(Index);
        }
    }
}