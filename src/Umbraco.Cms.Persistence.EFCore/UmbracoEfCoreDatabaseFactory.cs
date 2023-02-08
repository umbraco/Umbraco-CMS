using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore;

public class UmbracoEfCoreDatabaseFactory : IUmbracoEfCoreDatabaseFactory
{
    private readonly IDatabaseInfo _databaseInfo;
    private readonly IDbContextFactory<UmbracoEFContext> _dbContextFactory;

    public UmbracoEfCoreDatabaseFactory(IDatabaseInfo databaseInfo, IDbContextFactory<UmbracoEFContext> dbContextFactory)
    {
        _databaseInfo = databaseInfo;
        _dbContextFactory = dbContextFactory;
    }

    public IUmbracoEfCoreDatabase Create()
    {
        UmbracoEFContext umbracoEfContext = _dbContextFactory.CreateDbContext();
        return new UmbracoEfCoreDatabase(_databaseInfo, umbracoEfContext);
    }
}
