using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Execute.Expressions
{
    public class ExecuteSqlStatementExpression : MigrationExpressionBase
    {
        public ExecuteSqlStatementExpression(IMigrationContext context)
            : base(context)
        { }

        public virtual string SqlStatement { get; set; }

        protected override string GetSql()
        {
            return SqlStatement;
        }
    }
}
