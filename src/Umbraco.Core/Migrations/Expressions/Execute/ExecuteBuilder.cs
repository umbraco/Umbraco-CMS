using NPoco;
using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Execute.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Execute
{
    public class ExecuteBuilder : ExpressionBuilderBase<ExecuteSqlStatementExpression>,
        IExecuteBuilder, IExecutableBuilder
    {
        public ExecuteBuilder(IMigrationContext context)
            : base(new ExecuteSqlStatementExpression(context))
        { }

        /// <inheritdoc />
        public void Do() => Expression.Execute();

        /// <inheritdoc />
        public IExecutableBuilder Sql(string sqlStatement)
        {
            Expression.SqlStatement = sqlStatement;
            return this;
        }
    }
}
