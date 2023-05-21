namespace Umbraco.Cms.Infrastructure.Migrations.Expressions;

/// <summary>
///     Provides a base class for expression builders.
/// </summary>
public abstract class ExpressionBuilderBase<TExpression>
    where TExpression : IMigrationExpression
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ExpressionBuilderBase{TExpression}" /> class.
    /// </summary>
    protected ExpressionBuilderBase(TExpression expression) => Expression = expression;

    /// <summary>
    ///     Gets the expression.
    /// </summary>
    public TExpression Expression { get; }
}
