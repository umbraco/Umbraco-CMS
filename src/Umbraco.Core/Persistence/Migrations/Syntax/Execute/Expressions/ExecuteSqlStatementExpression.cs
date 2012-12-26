namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions
{
    public class ExecuteSqlStatementExpression : IMigrationExpression
    {
        public virtual string SqlStatement { get; set; }

        public override string ToString()
        {
            return SqlStatement;
        }
    }
}