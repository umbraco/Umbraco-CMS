using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations;

public class EFCoreMigrationBuilder : IEFCoreMigrationBuilder
{
    private readonly IServiceProvider _serviceProvider;

    public EFCoreMigrationBuilder(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public EfCoreMigrationBase Build(Type migrationType, IEFCoreMigrationContext context) => (EfCoreMigrationBase)_serviceProvider.CreateInstance(migrationType, context);
}
