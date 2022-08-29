using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore;


public class UmbracoDbContextFactory : DbContextFactory<UmbracoEFContext>
{
    public UmbracoDbContextFactory(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
}
public class DbContextFactory<TDbContext> where TDbContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DbContextFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<T> ExecuteWithContextAsync<T>(Func<TDbContext, Task<T>> method)
    {
        using(var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

            return await method(db);
        }
    }

    public async Task ExecuteWithContextAsync<T>(Func<TDbContext, Task> method) =>
        await ExecuteWithContextAsync(async db =>
        {
            await method(db);
            return true; // Do nothing
        });
}
