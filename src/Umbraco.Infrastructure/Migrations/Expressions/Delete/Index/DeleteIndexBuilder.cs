using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Index;

/// <summary>
/// Provides a builder for constructing expressions that define the deletion of an index in a database migration.
/// </summary>
public class DeleteIndexBuilder : ExpressionBuilderBase<DeleteIndexExpression>,
    IDeleteIndexForTableBuilder, IExecutableBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteIndexBuilder"/> class with the specified delete index expression.
    /// </summary>
    /// <param name="expression">The <see cref="DeleteIndexExpression"/> that defines the index to be deleted.</param>
    public DeleteIndexBuilder(DeleteIndexExpression expression)
        : base(expression)
    {
    }

    /// <summary>
    /// Specifies the table on which the index to be deleted exists.
    /// </summary>
    /// <param name="tableName">The name of the table containing the index.</param>
    /// <returns>An executable builder to continue the migration expression.</returns>
    public IExecutableBuilder OnTable(string tableName)
    {
        Expression.Index.TableName = tableName;
        return this;
    }

    /// <inheritdoc />
    public void Do() => Expression.Execute();
}
