namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Marker interface for migration expressions
/// </summary>
public interface IMigrationExpression
{
    /// <summary>
    /// Executes the defined migration operation represented by this expression.
    /// </summary>
    void Execute();
}
