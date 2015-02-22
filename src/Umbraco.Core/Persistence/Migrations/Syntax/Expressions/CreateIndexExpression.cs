using System;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateIndexExpression : MigrationExpressionBase
    {
        public CreateIndexExpression(ISqlSyntaxProvider sqlSyntax, DatabaseProviders currentDatabaseProvider, DatabaseProviders[] supportedDatabaseProviders = null)
            : base(sqlSyntax, currentDatabaseProvider, supportedDatabaseProviders)
        {
            Index = new IndexDefinition();
        }

        public virtual IndexDefinition Index { get; set; }

        public override string ToString()
        {
            return SqlSyntax.Format(Index);
        }
    }
}