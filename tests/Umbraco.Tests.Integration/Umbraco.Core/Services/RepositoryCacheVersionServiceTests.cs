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
        // Each service call runs in its own implicit root scope so deduplication does not apply.
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        string? initialVersion;
        using (var readScope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            initialVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        }
        Assert.IsNotNull(initialVersion, "Initial cache version should not be null.");

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        string? updatedVersion;
        using (var readScope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            updatedVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        }
        Assert.IsNotNull(updatedVersion, "Updated cache version should not be null.");

        Assert.AreNotEqual(initialVersion, updatedVersion, "Cache version should be updated.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_OnlyWritesOncePerScopeForSameEntityType()
    {
        using var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true);

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        var firstVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        Assert.IsNotNull(firstVersion);

        // Second call within the same scope must be a no-op.
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        var secondVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;

        Assert.AreEqual(firstVersion, secondVersion, "Second call within the same scope must not write a new version.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_DeduplicationResetsAfterScopeExit()
    {
        string? firstScopeVersion;
        using (var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
            firstScopeVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        }
        Assert.IsNotNull(firstScopeVersion);

        // After the scope exits the deduplication state is cleared; a new scope must be able to write.
        string? secondScopeVersion;
        using (var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
            secondScopeVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        }

        Assert.IsNotNull(secondScopeVersion);
        Assert.AreNotEqual(firstScopeVersion, secondScopeVersion, "A new scope must produce a fresh version after the previous scope exited.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_DifferentEntityTypesDeduplicatedIndependently()
    {
        var mediaKey = ((RepositoryCacheVersionService)RepositoryCacheVersionService).GetCacheKey<IMedia>();

        using var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true);

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IMedia>();

        var contentVersionAfterFirst = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        var mediaVersionAfterFirst = (await RepositoryCacheVersionRepository.GetAsync(mediaKey))?.Version;
        Assert.IsNotNull(contentVersionAfterFirst);
        Assert.IsNotNull(mediaVersionAfterFirst);

        // Second calls — must be no-ops for each type independently.
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IMedia>();

        var contentVersionAfterSecond = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        var mediaVersionAfterSecond = (await RepositoryCacheVersionRepository.GetAsync(mediaKey))?.Version;

        Assert.AreEqual(contentVersionAfterFirst, contentVersionAfterSecond, "Second IContent call in same scope must not write a new version.");
        Assert.AreEqual(mediaVersionAfterFirst, mediaVersionAfterSecond, "Second IMedia call in same scope must not write a new version.");
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
