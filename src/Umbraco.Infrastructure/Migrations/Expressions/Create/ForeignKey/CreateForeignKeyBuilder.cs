using System.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

/// <summary>
/// Provides a builder for creating a foreign key constraint in a database migration.
/// </summary>
public class CreateForeignKeyBuilder : ExpressionBuilderBase<CreateForeignKeyExpression>,
    ICreateForeignKeyFromTableBuilder,
    ICreateForeignKeyForeignColumnBuilder,
    ICreateForeignKeyToTableBuilder,
    ICreateForeignKeyPrimaryColumnBuilder,
    ICreateForeignKeyCascadeBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey.CreateForeignKeyBuilder"/> class.
    /// </summary>
    /// <param name="expression">The <see cref="CreateForeignKeyExpression"/> that defines the foreign key to be created.</param>
    public CreateForeignKeyBuilder(CreateForeignKeyExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public void Do() => Expression.Execute();

    /// <inheritdoc />
    public ICreateForeignKeyCascadeBuilder OnDelete(Rule rule)
    {
        Expression.ForeignKey.OnDelete = rule;
        return this;
    }

    /// <inheritdoc />
    public ICreateForeignKeyCascadeBuilder OnUpdate(Rule rule)
    {
        Expression.ForeignKey.OnUpdate = rule;
        return this;
    }

    /// <inheritdoc />
    public IExecutableBuilder OnDeleteOrUpdate(Rule rule)
    {
        Expression.ForeignKey.OnDelete = rule;
        Expression.ForeignKey.OnUpdate = rule;
        return new ExecutableBuilder(Expression);
    }

    /// <inheritdoc />
    public ICreateForeignKeyToTableBuilder ForeignColumn(string column)
    {
        Expression.ForeignKey.ForeignColumns.Add(column);
        return this;
    }

    /// <inheritdoc />
    public ICreateForeignKeyToTableBuilder ForeignColumns(params string[] columns)
    {
        foreach (var column in columns)
        {
            Expression.ForeignKey.ForeignColumns.Add(column);
        }

        return this;
    }

    /// <inheritdoc />
    public ICreateForeignKeyForeignColumnBuilder FromTable(string table)
    {
        Expression.ForeignKey.ForeignTable = table;
        return this;
    }

    /// <inheritdoc />
    public ICreateForeignKeyCascadeBuilder PrimaryColumn(string column)
    {
        Expression.ForeignKey.PrimaryColumns.Add(column);
        return this;
    }

    /// <inheritdoc />
    public ICreateForeignKeyCascadeBuilder PrimaryColumns(params string[] columns)
    {
        foreach (var column in columns)
        {
            Expression.ForeignKey.PrimaryColumns.Add(column);
        }

        return this;
    }

    /// <inheritdoc />
    public ICreateForeignKeyPrimaryColumnBuilder ToTable(string table)
    {
        Expression.ForeignKey.PrimaryTable = table;
        return this;
    }
}
