using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions
{
    public class ExecuteSqlStatementExpression : MigrationExpressionBase
    {

        public ExecuteSqlStatementExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
        {
        }

        public virtual string SqlStatement { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return SqlStatement;
        }
    }
}