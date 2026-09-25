using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.EFCore;

/// <summary>
///     Builds an <see cref="UmbracoDbContext"/> for a given provider without the application container. EF Core
///     caches the model per options set, so a per-instance cache key keeps each test's model independent of the
///     others.
/// </summary>
internal static class UmbracoDbContextTestFactory
{
    public static UmbracoDbContext Create(
        Action<DbContextOptionsBuilder<UmbracoDbContext>> useProvider,
        IEnumerable<IEFCoreModelCustomizer> customizers)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<ConnectionStrings>(connectionStrings =>
        {
            connectionStrings.ConnectionString = "placeholder";
            connectionStrings.ProviderName = null;
        });

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        var optionsBuilder = new DbContextOptionsBuilder<UmbracoDbContext>()
            .UseApplicationServiceProvider(serviceProvider)
            .ReplaceService<IModelCacheKeyFactory, PerInstanceModelCacheKeyFactory>();
        useProvider(optionsBuilder);

        return new UmbracoDbContext(optionsBuilder.Options, customizers);
    }

    private class PerInstanceModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => (context.ContextId, designTime);
    }
}
