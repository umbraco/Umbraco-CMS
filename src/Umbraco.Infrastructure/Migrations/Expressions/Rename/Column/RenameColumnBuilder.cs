using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;

public class RenameColumnBuilder : ExpressionBuilderBase<RenameColumnExpression>,
    IRenameColumnToBuilder, IRenameColumnBuilder, IExecutableBuilder
{
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
