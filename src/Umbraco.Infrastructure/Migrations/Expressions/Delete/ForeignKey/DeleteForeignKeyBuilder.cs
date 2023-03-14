using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;

/// <summary>
///     Implements IDeleteForeignKey...
/// </summary>
public class DeleteForeignKeyBuilder : ExpressionBuilderBase<DeleteForeignKeyExpression>,
    IDeleteForeignKeyFromTableBuilder,
    IDeleteForeignKeyForeignColumnBuilder,
    IDeleteForeignKeyToTableBuilder,
    IDeleteForeignKeyPrimaryColumnBuilder,
    IDeleteForeignKeyOnTableBuilder
{
    public DeleteForeignKeyBuilder(DeleteForeignKeyExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public IDeleteForeignKeyToTableBuilder ForeignColumn(string column)
    {
        Expression.ForeignKey.ForeignColumns.Add(column);
        return this;
    }

    /// <inheritdoc />
    public IDeleteForeignKeyToTableBuilder ForeignColumns(params string[] columns)
    {
        foreach (var column in columns)
        {
            Expression.ForeignKey.ForeignColumns.Add(column);
        }

        return this;
    }

    /// <inheritdoc />
    public IDeleteForeignKeyForeignColumnBuilder FromTable(string foreignTableName)
    {
        Expression.ForeignKey.ForeignTable = foreignTableName;
        return this;
    }

    /// <inheritdoc />
    public IExecutableBuilder OnTable(string foreignTableName)
    {
        Expression.ForeignKey.ForeignTable = foreignTableName;
        return new ExecutableBuilder(Expression);
    }

    /// <inheritdoc />
    public IExecutableBuilder PrimaryColumn(string column)
    {
        Expression.ForeignKey.PrimaryColumns.Add(column);
        return new ExecutableBuilder(Expression);
    }

    /// <inheritdoc />
    public IExecutableBuilder PrimaryColumns(params string[] columns)
    {
        foreach (var column in columns)
        {
            Expression.ForeignKey.PrimaryColumns.Add(column);
        }

        return new ExecutableBuilder(Expression);
    }

    /// <inheritdoc />
    public IDeleteForeignKeyPrimaryColumnBuilder ToTable(string table)
    {
        Expression.ForeignKey.PrimaryTable = table;
        return this;
    }
}
