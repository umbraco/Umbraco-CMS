using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Constraint;

/// <summary>
/// Provides a fluent builder for defining database constraints as part of migration expressions.
/// Use this builder to specify constraint details when creating constraints in database migrations.
/// </summary>
public class CreateConstraintBuilder : ExpressionBuilderBase<CreateConstraintExpression>,
    ICreateConstraintOnTableBuilder,
    ICreateConstraintColumnsBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Constraint.CreateConstraintBuilder"/> class using the specified constraint expression.
    /// </summary>
    /// <param name="expression">The <see cref="CreateConstraintExpression"/> that defines the constraint to be created.</param>
    public CreateConstraintBuilder(CreateConstraintExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public IExecutableBuilder Column(string columnName)
    {
        Expression.Constraint.Columns.Add(columnName);
        return new ExecutableBuilder(Expression);
    }

    /// <inheritdoc />
    public IExecutableBuilder Columns(string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            Expression.Constraint.Columns.Add(columnName);
        }

        return new ExecutableBuilder(Expression);
    }

    /// <inheritdoc />
    public ICreateConstraintColumnsBuilder OnTable(string tableName)
    {
        Expression.Constraint.TableName = tableName;
        return this;
    }
}
