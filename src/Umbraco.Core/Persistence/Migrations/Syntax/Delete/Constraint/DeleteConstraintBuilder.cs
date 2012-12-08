using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Constraint
{
    public class DeleteConstraintBuilder : ExpressionBuilderBase<DeleteConstraintExpression>, IDeleteConstraintOnTableSyntax
    {
        public DeleteConstraintBuilder(DeleteConstraintExpression expression) : base(expression)
        {
        }

        public void FromTable(string tableName)
        {
            Expression.Constraint.TableName = tableName;
        }
    }
}