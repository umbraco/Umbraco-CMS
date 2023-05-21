namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IMigrationBuilder
{
    MigrationBase Build(Type migrationType, IMigrationContext context);
}
