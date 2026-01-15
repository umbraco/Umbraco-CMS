namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IMigrationBuilder
{
    AsyncMigrationBase Build(Type migrationType, IMigrationContext context);
}
