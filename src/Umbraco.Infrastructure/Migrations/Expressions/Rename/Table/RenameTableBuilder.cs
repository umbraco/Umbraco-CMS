using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Table;

public class RenameTableBuilder : ExpressionBuilderBase<RenameTableExpression>,
    IRenameTableBuilder, IExecutableBuilder
{
    public RenameTableBuilder(RenameTableExpression expression)
        : base(expression)
    {
    }

    /// <inheritdoc />
    public void Do() => Expression.Execute();

    /// <inheritdoc />
    public IExecutableBuilder To(string name)
    {
        Expression.NewName = name;
        return this;
    }
}
