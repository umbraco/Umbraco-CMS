// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Umbraco.Cms.Core.Persistence;
// using Umbraco.Cms.Persistence.EFCore.Entities;
//
// namespace Umbraco.Cms.Persistence.EFCore;
//
// public class UmbracoEfCoreDatabaseFactory : IUmbracoEfCoreDatabaseFactory
// {
//     private readonly IDatabaseInfo _databaseInfo;
//     private readonly IServiceScopeFactory _scopeFactory;
//     private readonly IDbContextFactory<UmbracoEFContext> _dbContextFactory;
//     private IServiceScope? _scope;
//
//     public UmbracoEfCoreDatabaseFactory(IDatabaseInfo databaseInfo, IServiceScopeFactory scopeFactory, IDbContextFactory<UmbracoEFContext> dbContextFactory)
//     {
//         _databaseInfo = databaseInfo;
//         _scopeFactory = scopeFactory;
//         _dbContextFactory = dbContextFactory;
//     }
//
//     public IUmbracoEfCoreDatabase Create()
//     {
//         _scope ??= _scopeFactory.CreateScope();
//
//         UmbracoEFContext umbracoEfContext = _dbContextFactory.CreateDbContext();
//         return new UmbracoEfCoreDatabase(_databaseInfo, umbracoEfContext);
//     }
//
//     public void Dispose()
//     {
//         _scope?.Dispose();
//         _scope = null;
//     }
// }
