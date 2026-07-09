using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Constraint;

/// <summary>
/// Provides a builder for constructing migration expressions that delete database constraints.
/// </summary>
public class DeleteConstraintBuilder : ExpressionBuilderBase<DeleteConstraintExpression>,
    IDeleteConstraintBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConstraintBuilder"/> class.
    /// </summary>
    /// <param name="expression">The expression representing the constraint to be deleted.</param>
    public DeleteConstraintBuilder(DeleteConstraintExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public IExecutableBuilder FromTable(string tableName)
    {
        Expression.Constraint.TableName = tableName;
        return new ExecutableBuilder(Expression);
    }
}
