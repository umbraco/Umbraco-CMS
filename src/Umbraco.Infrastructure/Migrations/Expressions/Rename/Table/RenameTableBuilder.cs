using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Table;

/// <summary>
/// Provides a builder for constructing expressions to rename database tables during migrations.
/// </summary>
public class RenameTableBuilder : ExpressionBuilderBase<RenameTableExpression>,
    IRenameTableBuilder, IExecutableBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Table.RenameTableBuilder"/> class.
    /// </summary>
    /// <param name="expression">The <see cref="RenameTableExpression"/> that defines the details of the table rename operation.</param>
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
