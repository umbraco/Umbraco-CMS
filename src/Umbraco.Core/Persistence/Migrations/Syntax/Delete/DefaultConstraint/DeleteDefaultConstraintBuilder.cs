using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.DefaultConstraint
{
    public class DeleteDefaultConstraintBuilder : ExpressionBuilderBase<DeleteDefaultConstraintExpression>,
                                                  IDeleteDefaultConstraintOnTableSyntax,
                                                  IDeleteDefaultConstraintOnColumnSyntax
    {
        public DeleteDefaultConstraintBuilder(DeleteDefaultConstraintExpression expression)
            : base(expression)
        {
        }

        public IDeleteDefaultConstraintOnColumnSyntax OnTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }

        public void OnColumn(string columnName)
        {
            Expression.ColumnName = columnName;
        }
    }
}