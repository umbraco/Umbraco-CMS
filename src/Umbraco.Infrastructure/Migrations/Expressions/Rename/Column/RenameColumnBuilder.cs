using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;

/// <summary>
/// Provides a builder for renaming a column in a database table during a migration.
/// </summary>
public class RenameColumnBuilder : ExpressionBuilderBase<RenameColumnExpression>,
    IRenameColumnToBuilder, IRenameColumnBuilder, IExecutableBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameColumnBuilder"/> class with the specified rename column expression.
    /// </summary>
    /// <param name="expression">The <see cref="RenameColumnExpression"/> that defines the details of the column rename operation.</param>
    public RenameColumnBuilder(RenameColumnExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public void Do() => Expression.Execute();

    /// <inheritdoc />
    public IRenameColumnToBuilder OnTable(string tableName)
    {
        Expression.TableName = tableName;
        return this;
    }

    /// <inheritdoc />
    public IExecutableBuilder To(string name)
    {
        Expression.NewName = name;
        return this;
    }
}
