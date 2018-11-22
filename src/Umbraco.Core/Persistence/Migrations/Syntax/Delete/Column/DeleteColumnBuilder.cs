using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Column
{
    public class DeleteColumnBuilder : ExpressionBuilderBase<DeleteColumnExpression>, IDeleteColumnFromTableSyntax
    {
        public DeleteColumnBuilder(DeleteColumnExpression expression) : base(expression)
        {
        }

        public void FromTable(string tableName)
        {
            Expression.TableName = tableName;
        }

        public IDeleteColumnFromTableSyntax Column(string columnName)
        {
            Expression.ColumnNames.Add(columnName);
            return this;
        }
    }
}