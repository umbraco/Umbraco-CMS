using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Execute.Expressions
{
    public class ExecuteSqlStatementExpression : MigrationExpressionBase
    {
        public ExecuteSqlStatementExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string SqlStatement { get; set; }

        protected override string GetSql()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return SqlStatement;
        }
    }
}
