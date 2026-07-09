using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Facilitates the construction and execution of database migrations.
/// </summary>
public class MigrationBuilder : IMigrationBuilder
{
    private readonly IServiceProvider _container;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.MigrationBuilder"/> class.
    /// </summary>
    /// <param name="container">The service provider used to resolve migration dependencies.</param>
    public MigrationBuilder(IServiceProvider container) => _container = container;

    /// <summary>
    /// Creates and returns an instance of the specified migration type using the provided migration context.
    /// The migration type must inherit from <see cref="Umbraco.Cms.Infrastructure.Migrations.AsyncMigrationBase"/>.
    /// </summary>
    /// <param name="migrationType">The <see cref="Type"/> of the migration to instantiate.</param>
    /// <param name="context">The <see cref="IMigrationContext"/> to pass to the migration constructor.</param>
    /// <returns>An instance of <see cref="Umbraco.Cms.Infrastructure.Migrations.AsyncMigrationBase"/> created via dependency injection.</returns>
    public AsyncMigrationBase Build(Type migrationType, IMigrationContext context) =>
        (AsyncMigrationBase)_container.CreateInstance(migrationType, context);
}
