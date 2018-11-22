using Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Constraint
{
    public class CreateConstraintBuilder : ExpressionBuilderBase<CreateConstraintExpression>,
                                           ICreateConstraintOnTableSyntax,
                                           ICreateConstraintColumnsSyntax
    {
        public CreateConstraintBuilder(CreateConstraintExpression expression) : base(expression)
        {
        }

        public ICreateConstraintColumnsSyntax OnTable(string tableName)
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