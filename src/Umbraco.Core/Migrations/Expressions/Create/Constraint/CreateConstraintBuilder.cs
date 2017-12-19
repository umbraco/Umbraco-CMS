using Umbraco.Core.Migrations.Expressions.Create.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Create.Constraint
{
    public class CreateConstraintBuilder : ExpressionBuilderBase<CreateConstraintExpression>,
                                           ICreateConstraintOnTableBuilder,
                                           ICreateConstraintColumnsBuilder
    {
        public CreateConstraintBuilder(CreateConstraintExpression expression) : base(expression)
        {
        }

        public ICreateConstraintColumnsBuilder OnTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
            return this;
        }

        public void Column(string columnName)
        {
            Expression.Constraint.Columns.Add(columnName);
        }

        public void Columns(string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                Expression.Constraint.Columns.Add(columnName);
            }
        }
    }
}
