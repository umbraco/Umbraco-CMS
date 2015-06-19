using System;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateIndexExpression : MigrationExpressionBase
    {
        public CreateIndexExpression(ISqlSyntaxProvider sqlSyntax)
            : base(sqlSyntax)
        {
            Index = new IndexDefinition();
        }
        
        public CreateIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
        {
            Index = new IndexDefinition();
        }

        [Obsolete("Use alternate ctor specifying ISqlSyntaxProvider instead")]
        public CreateIndexExpression()
            : this(SqlSyntaxContext.SqlSyntaxProvider)
        {

        }

        [Obsolete("Use alternate ctor specifying ISqlSyntaxProvider instead")]
        public CreateIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders)
            : this(current, databaseProviders, SqlSyntaxContext.SqlSyntaxProvider)
        {
        }

        public virtual IndexDefinition Index { get; set; }

        public override string ToString()
        {
            return SqlSyntax.Format(Index);
        }
    }
}