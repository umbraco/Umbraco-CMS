using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Create.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Create.Constraint
{
    public class CreateConstraintBuilder : ExpressionBuilderBase<CreateConstraintExpression>,
                                           ICreateConstraintOnTableBuilder,
                                           ICreateConstraintColumnsBuilder
    {
        public CreateConstraintBuilder(CreateConstraintExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public ICreateConstraintColumnsBuilder OnTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        /// <inheritdoc />
        public IExecutableBuilder Column(string columnName)
        {
            Expression.Constraint.Columns.Add(columnName);
            return new ExecutableBuilder(Expression);
        }

        /// <inheritdoc />
        public IExecutableBuilder Columns(string[] columnNames)
        {
            foreach (var columnName in columnNames)
                Expression.Constraint.Columns.Add(columnName);
            return new ExecutableBuilder(Expression);
        }
    }
}
