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
        Assert.That(isSynced, Is.True, "Cache should be initially synced.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_Writes_Version_To_Database()
    {
        using var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true);
        var initial = await RepositoryCacheVersionRepository.GetAsync(GetCacheKey());
        Assert.That(initial?.Version, Is.Null, "Initial cache version should be null before update.");

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();

        var cacheVersion = await RepositoryCacheVersionRepository.GetAsync(GetCacheKey());
        Assert.That(cacheVersion, Is.Not.Null, "Cache version should exist in the database after update.");
        Assert.That(string.IsNullOrEmpty(cacheVersion.Version), Is.False, "Cache version string should not be null or empty.");
    }

    [Test]
    public async Task Cache_Is_Out_Of_Sync_If_Updated_Remotely()
    {
        // Simulate an update
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        var isSynced = await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>();

        // We should be synced now.
        Assert.That(isSynced, Is.True);

        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            // Simulate a remote update to the database
            await RepositoryCacheVersionRepository.SaveAsync(GetRepositoryRandomCacheVersion());
            scope.Complete();
        }

        // Now the cache should be out of sync
        isSynced = await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>();
        Assert.That(isSynced, Is.False, "Cache should be out of sync after remote update.");
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
        Assert.That(initialVersion, Is.Not.Null, "Initial cache version should not be null.");

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        string? updatedVersion;
        using (var readScope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            updatedVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        }
        Assert.That(updatedVersion, Is.Not.Null, "Updated cache version should not be null.");

        Assert.That(updatedVersion, Is.Not.EqualTo(initialVersion), "Cache version should be updated.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_OnlyWritesOncePerScopeForSameEntityType()
    {
        using var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true);

        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        var firstVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        Assert.That(firstVersion, Is.Not.Null);

        // Second call within the same scope must be a no-op.
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        var secondVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;

        Assert.That(secondVersion, Is.EqualTo(firstVersion), "Second call within the same scope must not write a new version.");
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
        Assert.That(firstScopeVersion, Is.Not.Null);

        // After the scope exits the deduplication state is cleared; a new scope must be able to write.
        string? secondScopeVersion;
        using (var scope = CoreScopeProvider.CreateCoreScope(autoComplete: true))
        {
            await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
            secondScopeVersion = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        }

        Assert.That(secondScopeVersion, Is.Not.Null);
        Assert.That(secondScopeVersion, Is.Not.EqualTo(firstScopeVersion), "A new scope must produce a fresh version after the previous scope exited.");
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
        Assert.That(contentVersionAfterFirst, Is.Not.Null);
        Assert.That(mediaVersionAfterFirst, Is.Not.Null);

        // Second calls — must be no-ops for each type independently.
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IMedia>();

        var contentVersionAfterSecond = (await RepositoryCacheVersionRepository.GetAsync(GetCacheKey()))?.Version;
        var mediaVersionAfterSecond = (await RepositoryCacheVersionRepository.GetAsync(mediaKey))?.Version;

        Assert.That(contentVersionAfterSecond, Is.EqualTo(contentVersionAfterFirst), "Second IContent call in same scope must not write a new version.");
        Assert.That(mediaVersionAfterSecond, Is.EqualTo(mediaVersionAfterFirst), "Second IMedia call in same scope must not write a new version.");
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

        Assert.That(contentVersion, Is.Not.Null);
        Assert.That(mediaVersion, Is.Not.Null);
        Assert.That(mediaVersion, Is.Not.EqualTo(contentVersion), "Cache versions should be unique for different repository types.");
    }

    [Test]
    public async Task SetCachesSyncedAsync_RestoresSyncAfterRemoteUpdate()
    {
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), Is.True);

        // Simulate a remote update — local service is now out of sync.
        using (var scope = CoreScopeProvider.CreateCoreScope())
        {
            await RepositoryCacheVersionRepository.SaveAsync(GetRepositoryRandomCacheVersion());
            scope.Complete();
        }

        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), Is.False, "Expected out of sync after remote update.");

        // Re-sync from the database.
        await RepositoryCacheVersionService.SetCachesSyncedAsync();

        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), Is.True, "Expected in sync after SetCachesSyncedAsync.");
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
        Assert.That(isSynced, Is.True, "Should be synced when local is uninitialised but DB has a version.");

        // Second check: local version now matches DB version → still synced.
        isSynced = await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>();
        Assert.That(isSynced, Is.True, "Should remain synced on subsequent checks.");
    }

    [Test]
    public async Task SetCachesSyncedAsync_SyncsAllEntityTypesFromDb()
    {
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IContent>();
        await RepositoryCacheVersionService.SetCacheUpdatedAsync<IMedia>();

        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), Is.True);
        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IMedia>(), Is.True);

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

        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), Is.False, "IContent should be out of sync.");
        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IMedia>(), Is.False, "IMedia should be out of sync.");

        await RepositoryCacheVersionService.SetCachesSyncedAsync();

        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IContent>(), Is.True, "IContent should be synced after SetCachesSyncedAsync.");
        Assert.That(await RepositoryCacheVersionService.IsCacheSyncedAsync<IMedia>(), Is.True, "IMedia should be synced after SetCachesSyncedAsync.");
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
