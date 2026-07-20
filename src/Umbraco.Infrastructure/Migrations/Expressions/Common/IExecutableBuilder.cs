namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

/// <summary>
/// Represents a builder interface for creating and executing migration expressions.
/// Implementations of this interface allow migration steps to be defined and executed as part of a migration process.
/// </summary>
public interface IExecutableBuilder
{
    /// <summary>
    ///     Executes.
    /// </summary>
    void Do();
}
