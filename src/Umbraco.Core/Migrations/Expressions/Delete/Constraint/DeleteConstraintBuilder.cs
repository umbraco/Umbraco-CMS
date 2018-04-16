using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Constraint
{
    public class DeleteConstraintBuilder : ExpressionBuilderBase<DeleteConstraintExpression>,
        IDeleteConstraintBuilder
    {
        public DeleteConstraintBuilder(DeleteConstraintExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public IExecutableBuilder FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return new ExecutableBuilder(Expression);
        }
    }
}
