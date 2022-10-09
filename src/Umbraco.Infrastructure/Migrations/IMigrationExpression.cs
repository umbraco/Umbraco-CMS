namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
///     Marker interface for migration expressions
/// </summary>
public interface IMigrationExpression
{
    void Execute();
}
