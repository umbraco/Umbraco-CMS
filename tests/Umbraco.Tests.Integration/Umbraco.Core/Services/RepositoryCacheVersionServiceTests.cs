using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class RepositoryCacheVersionServiceTests : UmbracoIntegrationTest
{
    private IRepositoryCacheVersionService RepositoryCacheVersionService => GetRequiredService<IRepositoryCacheVersionService>();

    private IRepositoryCacheVersionRepository RepositoryCacheVersionRepository => GetRequiredService<IRepositoryCacheVersionRepository>();

    private ICoreScopeProvider CoreScopeProvider => GetRequiredService<ICoreScopeProvider>();

    protected override void CustomTestSetup(IUmbracoBuilder builder) => builder.LoadBalanceIsolatedCaches();

    [Test]
    public async Task Cache_Is_Initially_Synced()
    {
        var isSynced = await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>();
        Assert.IsTrue(isSynced, "Cache should be initially synced.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_Writes_Version_To_Database()
    {
        using var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true);
        var initial = await RepositoryCacheVersionRepository.GetAsync(GetCacheKey());
        Assert.IsNull(initial?.Version, "Initial cache version should be null before update.");

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();

        var cacheVersion = await RepositoryCacheVersionRepository.GetAsync(GetCacheKey());
        Assert.IsNotNull(cacheVersion, "Cache version should exist in the database after update.");
        Assert.IsFalse(string.IsNullOrEmpty(cacheVersion.Version), "Cache version string should not be null or empty.");
    }

    [Test]
    public async Task Cache_Is_Out_Of_Sync_If_Updated_Remotely()
    {
        // Simulate an update
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        var isSynced = await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>();

        // We should be synced now.
        Assert.IsTrue(isSynced);

        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            // Simulate a remote update to the database
            await RepositoryCacheVersionRepository.SaveAsync(GetRepositoryRandomCacheVersion());
            scope.Complete();
        }

        // Now the cache should be out of sync
        isSynced = await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>();
        Assert.IsFalse(isSynced, "Cache should be out of sync after remote update.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_Updates_Cache_Version()
    {
        using var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true);

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        var initialCacheVersion = await RepositoryCacheVersionRepository.GetAsync(GetCacheKey());
        Assert.IsNotNull(initialCacheVersion, "Initial cache version should not be null.");

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        var updatedCacheVersion = await RepositoryCacheVersionRepository.GetAsync(GetCacheKey());
        Assert.IsNotNull(updatedCacheVersion, "Updated cache version should not be null.");

        Assert.AreNotEqual(initialCacheVersion.Version, updatedCacheVersion.Version, "Cache version should be updated.");
    }

    [Test]
    public async Task CacheVersion_Is_Unique_Per_Repository_Type()
    {
        using var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true);

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IMedia>();

        var contentVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        var mediaKey = ((RepositoryCacheVersionService)RepositoryCacheVersionService).GetCacheKey<IMedia>();
        var mediaVersion = (await RepositoryCacheVersionRepository.GetAsync(mediaKey))?.Version;

        Assert.IsNotNull(contentVersion);
        Assert.IsNotNull(mediaVersion);
        Assert.AreNotEqual(contentVersion, mediaVersion, "Cache versions should be unique for different repository types.");
    }

    [Test]
    public async Task SetCachesSyncedAsync_RestoresSyncAfterRemoteUpdate()
    {
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        Assert.IsTrue(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>());

        // Simulate a remote update — local service is now out of sync.
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            await RepositoryCacheVersionRepository.SaveAsync(GetRepositoryRandomCacheVersion());
            scope.Complete();
        }

        Assert.IsFalse(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), "Expected out of sync after remote update.");

        // Re-sync from the database.
        await RepositoryCacheVersionService.SetCachesSyncedAsync();

        Assert.IsTrue(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), "Expected in sync after SetCachesSyncedAsync.");
    }

    [Test]
    public async Task IsCacheSyncedAsync_ReturnsTrueWhenLocalIsUninitializedButDbHasVersion()
    {
        // Write directly to DB — simulates another instance having written a version
        // before this instance ever checked.
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            await RepositoryCacheVersionRepository.SaveAsync(GetRepositoryRandomCacheVersion());
            scope.Complete();
        }

        // First check: local has no version, DB has one → service initialises from DB and returns true.
        var isSynced = await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>();
        Assert.IsTrue(isSynced, "Should be synced when local is uninitialised but DB has a version.");

        // Second check: local version now matches DB version → still synced.
        isSynced = await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>();
        Assert.IsTrue(isSynced, "Should remain synced on subsequent checks.");
    }

    [Test]
    public async Task SetCachesSyncedAsync_SyncsAllEntityTypesFromDb()
    {
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IMedia>();

        Assert.IsTrue(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>());
        Assert.IsTrue(await RepositoryCacheVersionService.IsCacheSyncedAsync<IMedia>());

        var mediaKey = ((RepositoryCacheVersionService)RepositoryCacheVersionService).GetCacheKey<IMedia>();

        // Simulate remote updates for both entity types.
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            await RepositoryCacheVersionRepository.SaveAsync(GetRepositoryRandomCacheVersion());
            await RepositoryCacheVersionRepository.SaveAsync(new RepositoryCacheVersion
            {
                Identifier = mediaKey,
                Version = Guid.NewGuid().ToString(),
            });
            scope.Complete();
        }

        Assert.IsFalse(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), "IContent should be out of sync.");
        Assert.IsFalse(await RepositoryCacheVersionService.IsCacheSyncedAsync<IMedia>(), "IMedia should be out of sync.");

        await RepositoryCacheVersionService.SetCachesSyncedAsync();

        Assert.IsTrue(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), "IContent should be synced after SetCachesSyncedAsync.");
        Assert.IsTrue(await RepositoryCacheVersionService.IsCacheSyncedAsync<IMedia>(), "IMedia should be synced after SetCachesSyncedAsync.");
    }

    private RepositoryCacheVersion GetRepositoryRandomCacheVersion()
        => new()
        {
            Identifier = GetCacheKey(),
            Version = Guid.NewGuid().ToString(),
        };

    private string GetCacheKey()
        => ((RepositoryCacheVersionService)RepositoryCacheVersionService).GetCacheKey<IContent>();
}
