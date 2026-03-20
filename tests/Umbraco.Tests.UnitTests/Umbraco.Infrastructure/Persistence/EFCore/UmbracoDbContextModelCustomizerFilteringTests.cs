using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.EFCore;

[TestFixture]
public class UmbracoDbContextModelCustomizerFilteringTests
{
    // EF Core provider names as returned by Database.ProviderName.
    private const string SqliteEFCoreProviderName = Constants.ProviderNames.EFCore.SQLite;
    private const string SqlServerEFCoreProviderName = Constants.ProviderNames.EFCore.SQLServer;

    /// <summary>
    /// Builds a <see cref="UmbracoDbContext"/> backed by an in-memory SQLite database
    /// (<see cref="Database.ProviderName"/> = <c>"Microsoft.EntityFrameworkCore.Sqlite"</c>)
    /// so that <see cref="DbContext.Model"/> can be accessed without a real connection.
    /// </summary>
    /// <remarks>
    /// EF Core caches the model per options set, so a <see cref="PerInstanceModelCacheKeyFactory"/>
    /// is registered to ensure each context instance builds its own model, giving each test
    /// a clean slate regardless of execution order.
    /// </remarks>
    private static UmbracoDbContext CreateContext(params IEFCoreModelCustomizer[] customizers)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions();
        services.Configure<ConnectionStrings>(cs =>
        {
            cs.ConnectionString = "Data Source=:memory:";
            cs.ProviderName = null; // prevents ConfigureOptions from looking up a migration provider
        });

        IServiceProvider sp = services.BuildServiceProvider();

        DbContextOptions<UmbracoDbContext> options = new DbContextOptionsBuilder<UmbracoDbContext>()
            .UseSqlite("Data Source=:memory:")
            .UseApplicationServiceProvider(sp)
            .ReplaceService<IModelCacheKeyFactory, PerInstanceModelCacheKeyFactory>()
            .Options;

        return new UmbracoDbContext(options, customizers);
    }

    [Test]
    public void OnModelCreating_AlwaysAppliesCustomizer_WhenProviderNameIsNull()
    {
        // A null ProviderName means "apply to all providers".
        var customizer = new AnnotatingCustomizer(providerName: null);

        using UmbracoDbContext context = CreateContext(customizer);

        Assert.That(GetAnnotation(context), Is.EqualTo("applied"),
            "A customizer with null ProviderName should apply regardless of the active provider.");
    }

    [Test]
    public void OnModelCreating_AppliesCustomizer_WhenProviderNameMatchesActiveProvider()
    {
        // SQLite context + SQLite customizer → should apply.
        var customizer = new AnnotatingCustomizer(providerName: SqliteEFCoreProviderName);

        using UmbracoDbContext context = CreateContext(customizer);

        Assert.That(GetAnnotation(context), Is.EqualTo("applied"),
            "A customizer should be applied when its ProviderName matches the active EF Core provider.");
    }

    [Test]
    public void OnModelCreating_SkipsCustomizer_WhenProviderNameDoesNotMatchActiveProvider()
    {
        // SQLite context + SQL Server customizer → should be skipped.
        var customizer = new AnnotatingCustomizer(providerName: SqlServerEFCoreProviderName);

        using UmbracoDbContext context = CreateContext(customizer);

        Assert.That(GetAnnotation(context), Is.Null,
            "A customizer should be skipped when its ProviderName does not match the active EF Core provider.");
    }

    private static object? GetAnnotation(UmbracoDbContext context)
        => context.Model
            .FindEntityType(typeof(WebhookDto))
            ?.FindAnnotation(AnnotatingCustomizer.AnnotationName)
            ?.Value;

    /// <summary>
    /// Disables EF Core's model caching by using <see cref="DbContext.ContextId"/> as the
    /// cache key. Each <see cref="DbContext"/> instance gets a unique ID, so every context
    /// builds its own model — ensuring test isolation.
    /// </summary>
    private class PerInstanceModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => (context.ContextId, designTime);
    }

    private class AnnotatingCustomizer : IEFCoreModelCustomizer
    {
        public const string AnnotationName = "Test:CustomizerApplied";

        private readonly string? _providerName;

        public AnnotatingCustomizer(string? providerName) => _providerName = providerName;

        public string? ProviderName => _providerName;

        public void Apply(ModelBuilder modelBuilder)
            => modelBuilder.Entity<WebhookDto>().HasAnnotation(AnnotationName, "applied");
    }
}
