namespace Umbraco.Cms.Infrastructure.Migrations;

public interface IEFCoreMigrationBuilder
{
    EfCoreMigrationBase Build(Type migrationType, IEFCoreMigrationContext context);
}
