using Umbraco.Core.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Column
{
    /// <summary>
    /// Implements <see cref="IDeleteColumnBuilder"/>.
    /// </summary>
    public class DeleteColumnBuilder : ExpressionBuilderBase<DeleteColumnExpression>, IDeleteColumnBuilder
    {
        public DeleteColumnBuilder(DeleteColumnExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void FromTable(string tableName)
        {
            Expression.TableName = tableName;
            Expression.Execute();
        }

        /// <inheritdoc />
        public IDeleteColumnBuilder Column(string columnName)
        {
            Expression.ColumnNames.Add(columnName);
            return this;
        }
    }
}
