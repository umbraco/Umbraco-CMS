using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Column;

public class DeleteColumnBuilder : ExpressionBuilderBase<DeleteColumnExpression>,
    IDeleteColumnBuilder
{
    public DeleteColumnBuilder(DeleteColumnExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public IExecutableBuilder FromTable(string tableName)
    {
        Expression.TableName = tableName;
        return new ExecutableBuilder(Expression);
    }

    /// <inheritdoc />
    public IDeleteColumnBuilder Column(string columnName)
    {
        Expression.ColumnNames.Add(columnName);
        return this;
    }
}
