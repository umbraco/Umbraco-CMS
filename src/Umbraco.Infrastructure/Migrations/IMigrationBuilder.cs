namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Represents a builder that defines the contract for constructing and configuring database migrations.
/// </summary>
public interface IMigrationBuilder
{
    /// <summary>
    /// Creates an instance of the specified migration type using the provided migration context.
    /// </summary>
    /// <param name="migrationType">The type of migration to instantiate. Must derive from <see cref="AsyncMigrationBase"/>.</param>
    /// <param name="context">The migration context to use for the migration instance.</param>
    /// <returns>An instance of <see cref="AsyncMigrationBase"/> representing the migration.</returns>
    AsyncMigrationBase Build(Type migrationType, IMigrationContext context);
}
