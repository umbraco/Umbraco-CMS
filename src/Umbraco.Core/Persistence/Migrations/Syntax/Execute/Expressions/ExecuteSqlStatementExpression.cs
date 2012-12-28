namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions
{
    public class ExecuteSqlStatementExpression : MigrationExpressionBase
    {
        public ExecuteSqlStatementExpression()
        {
        }

        public ExecuteSqlStatementExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
        }

        public virtual string SqlStatement { get; set; }

        public override string ToString()
        {
            return SqlStatement;
        }
    }
}