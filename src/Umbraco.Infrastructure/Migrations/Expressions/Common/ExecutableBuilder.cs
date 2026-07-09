namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

/// <summary>
/// Represents a builder used to construct and configure executable migration expressions within the migration framework.
/// </summary>
public class ExecutableBuilder : IExecutableBuilder
{
    private readonly IMigrationExpression _expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutableBuilder"/> class with the specified migration expression.
    /// </summary>
    /// <param name="expression">The migration expression to be executed by this builder.</param>
    public ExecutableBuilder(IMigrationExpression expression) => _expression = expression;

    /// <inheritdoc />
    public void Do() => _expression.Execute();
}
