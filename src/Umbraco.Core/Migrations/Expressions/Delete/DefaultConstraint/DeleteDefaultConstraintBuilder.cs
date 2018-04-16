using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Delete.DefaultConstraint
{
    /// <summary>
    /// Implements <see cref="IDeleteDefaultConstraintOnTableBuilder"/>, <see cref="IDeleteDefaultConstraintOnColumnBuilder"/>.
    /// </summary>
    public class DeleteDefaultConstraintBuilder : ExpressionBuilderBase<DeleteDefaultConstraintExpression>,
        IDeleteDefaultConstraintOnTableBuilder,
        IDeleteDefaultConstraintOnColumnBuilder
    {
        public DeleteDefaultConstraintBuilder(DeleteDefaultConstraintExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public IDeleteDefaultConstraintOnColumnBuilder OnTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public IExecutableBuilder OnColumn(string columnName)
        {
            Expression.ColumnName = columnName;
            return new ExecutableBuilder(Expression);
        }
    }
}
