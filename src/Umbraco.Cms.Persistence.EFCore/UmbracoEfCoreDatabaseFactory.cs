using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.EfCore;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore;

public class UmbracoEfCoreDatabaseFactory : IUmbracoEfCoreDatabaseFactory
{
    private readonly IDatabaseInfo _databaseInfo;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly UmbracoDbContextFactory _umbracoDbContextFactory;
    private IServiceScope? _scope;

    public UmbracoEfCoreDatabaseFactory(IDatabaseInfo databaseInfo, IServiceScopeFactory scopeFactory, UmbracoDbContextFactory umbracoDbContextFactory)
    {
        _databaseInfo = databaseInfo;
        _scopeFactory = scopeFactory;
        _umbracoDbContextFactory = umbracoDbContextFactory;
    }

    public IUmbracoEfCoreDatabase Create()
    {
        if (_scope is null)
        {
            _scope = _scopeFactory.CreateScope();
        }

        UmbracoEFContext umbracoEfContext = _scope.ServiceProvider.GetRequiredService<UmbracoEFContext>();
        return new UmbracoEfCoreDatabase(_databaseInfo, umbracoEfContext, _umbracoDbContextFactory);
    }


    public void Dispose()
    {
        _scope?.Dispose();
        _scope = null;
    }
}
