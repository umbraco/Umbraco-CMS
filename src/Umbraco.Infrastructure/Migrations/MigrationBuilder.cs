using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

public class MigrationBuilder : IMigrationBuilder
{
    private readonly IServiceProvider _container;

    public MigrationBuilder(IServiceProvider container) => _container = container;

    public AsyncMigrationBase Build(Type migrationType, IMigrationContext context) =>
        (AsyncMigrationBase)_container.CreateInstance(migrationType, context);
}
