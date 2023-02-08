using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore;

public class UmbracoEfCoreDatabaseFactory : IUmbracoEfCoreDatabaseFactory
{
    private readonly IDatabaseInfo _databaseInfo;
    private readonly IServiceScopeFactory _scopeFactory;
    private IServiceScope? _scope;

    public UmbracoEfCoreDatabaseFactory(IDatabaseInfo databaseInfo, IServiceScopeFactory scopeFactory)
    {
        _databaseInfo = databaseInfo;
        _scopeFactory = scopeFactory;
    }

    public IUmbracoEfCoreDatabase Create()
    {
        _scope ??= _scopeFactory.CreateScope();

        UmbracoEFContext umbracoEfContext = _scope.ServiceProvider.GetRequiredService<UmbracoEFContext>();
        return new UmbracoEfCoreDatabase(_databaseInfo, umbracoEfContext);
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _scope = null;
    }
}
