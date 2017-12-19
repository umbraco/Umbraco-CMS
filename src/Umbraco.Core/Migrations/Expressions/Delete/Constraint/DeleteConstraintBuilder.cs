using Umbraco.Core.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Constraint
{
    /// <summary>
    /// Implements <see cref="IDeleteConstraintBuilder"/>.
    /// </summary>
    public class DeleteConstraintBuilder : ExpressionBuilderBase<DeleteConstraintExpression>, IDeleteConstraintBuilder
    {
        public DeleteConstraintBuilder(DeleteConstraintExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            Expression.Execute();
        }
    }
}
