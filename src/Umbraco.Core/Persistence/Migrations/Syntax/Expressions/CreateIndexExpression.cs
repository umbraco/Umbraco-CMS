using System;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateIndexExpression : MigrationExpressionBase
    {

        public CreateIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, IndexDefinition index)
            : base(sqlSyntax, current, databaseProviders)
        {
            Index = index;
        }

        public CreateIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(sqlSyntax, current, databaseProviders)
        {
            Index = new IndexDefinition();
        }
        
        public IndexDefinition Index { get; set; }

        public override string ToString()
        {
            return SqlSyntax.Format(Index);
        }
    }
}