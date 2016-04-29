using NPoco;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Execute.Expressions
{
    public class ExecuteSqlStatementExpression : MigrationExpressionBase
    {
        public ExecuteSqlStatementExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string SqlStatement { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return SqlStatement;
        }
    }
}