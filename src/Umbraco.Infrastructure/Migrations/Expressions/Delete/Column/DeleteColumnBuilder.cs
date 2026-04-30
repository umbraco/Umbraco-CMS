using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Column;

/// <summary>
/// Represents a builder used to define and execute the deletion of a column from a database table during a migration.
/// </summary>
public class DeleteColumnBuilder : ExpressionBuilderBase<DeleteColumnExpression>,
    IDeleteColumnBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteColumnBuilder"/> class.
    /// </summary>
    /// <param name="expression">The <see cref="DeleteColumnExpression"/> that defines the column to delete.</param>
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
