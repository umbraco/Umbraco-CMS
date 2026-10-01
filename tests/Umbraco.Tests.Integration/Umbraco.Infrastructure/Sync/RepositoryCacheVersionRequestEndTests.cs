// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Sync;

/// <summary>
///     With <c>LoadBalanceIsolatedCaches()</c>, a server that sees a new repository cache version syncs its isolated
///     caches from the pending cache instructions. The version must therefore never be published before the
///     instruction that describes the change: in a request both are deferred until the request ends, and the
///     instruction is written first.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RepositoryCacheVersionRequestEndTests : UmbracoIntegrationTestWithContent
{
    private static readonly string _contentCacheKey = typeof(IContent).FullName!;
    private static readonly string _mediaCacheKey = typeof(IMedia).FullName!;

    private IRepositoryCacheVersionService CacheVersionService => GetRequiredService<IRepositoryCacheVersionService>();

    private IRepositoryCacheVersionRepository CacheVersionRepository => GetRequiredService<IRepositoryCacheVersionRepository>();

    private ICacheInstructionService CacheInstructionService => GetRequiredService<ICacheInstructionService>();

    // Real caches with a request cache that is always available: the whole test is one request, which ends when the
    // test calls EndRequest().
    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddSingleton(AppCaches.Create(new DictionaryAppCache()));

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.LoadBalanceIsolatedCaches();
        builder.Services.AddUnique<IServerMessenger, BatchedDatabaseServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
    }

    public override void CreateTestData()
    {
        base.CreateTestData();

        // The fixture's own saves batched instructions and deferred versions; start each test from a quiet request.
        EndRequest();
    }

    [Test]
    public void Save_PublishesTheCacheVersion_OnlyAfterTheCacheInstruction()
    {
        var versionBefore = VersionInDatabase(_contentCacheKey);
        var maxInstructionIdBefore = CacheInstructionService.GetMaxInstructionId();

        Textpage.Name = "Renamed";
        ContentService.Save(Textpage);

        Assert.Multiple(() =>
        {
            Assert.That(VersionInDatabase(_contentCacheKey), Is.EqualTo(versionBefore), "The cache version was published before the request ended.");
            Assert.That(CacheInstructionService.GetMaxInstructionId(), Is.EqualTo(maxInstructionIdBefore), "The cache instruction was written before the request ended.");
        });

        EndRequest();

        Assert.Multiple(() =>
        {
            Assert.That(VersionInDatabase(_contentCacheKey), Is.Not.EqualTo(versionBefore).And.Not.Null, "The cache version was not published when the request ended.");
            Assert.That(CacheInstructionService.GetMaxInstructionId(), Is.GreaterThan(maxInstructionIdBefore), "The cache instruction was not written when the request ended.");
            Assert.That(CacheVersionService.IsCacheSyncedAsync<IContent>().GetAwaiter().GetResult(), Is.True, "The server that published the version does not consider itself synced.");
        });
    }

    [Test]
    public void RequestEnd_PublishesTheCacheVersion_WhenNoInstructionWasBatched()
    {
        var versionBefore = VersionInDatabase(_mediaCacheKey);

        using (IScope scope = ScopeProvider.CreateScope())
        {
            CacheVersionService.SetCacheUpdatedAsync<IMedia>().GetAwaiter().GetResult();
            scope.Complete();
        }

        Assume.That(VersionInDatabase(_mediaCacheKey), Is.EqualTo(versionBefore), "The cache version was published before the request ended, so this test proves nothing.");

        EndRequest();

        Assert.That(VersionInDatabase(_mediaCacheKey), Is.Not.EqualTo(versionBefore).And.Not.Null, "A deferred cache version without a cache instruction was not published.");
    }

    [Test]
    public void RolledBackSave_DoesNotPublishACacheVersion()
    {
        var versionBefore = VersionInDatabase(_contentCacheKey);

        using (IScope scope = ScopeProvider.CreateScope())
        {
            Textpage.Name = "Rolled back";
            ContentService.Save(Textpage);
        }

        EndRequest();

        Assert.That(VersionInDatabase(_contentCacheKey), Is.EqualTo(versionBefore), "A rolled-back change published a cache version.");
    }

    /// <summary>Runs the request-end handler directly, so no other request-end handler runs.</summary>
    private void EndRequest()
        => ActivatorUtilities.CreateInstance<DatabaseServerMessengerNotificationHandler>(Services)
            .Handle(new UmbracoRequestEndNotification(Mock.Of<IUmbracoContext>()));

    // Read from the database rather than through the accessor, whose request cache would hide the change.
    private string? VersionInDatabase(string cacheKey)
    {
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        return CacheVersionRepository.GetAsync(cacheKey).GetAwaiter().GetResult()?.Version;
    }
}
