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

    private RepositoryCacheVersion GetRepositoryRandomCacheVersion()
        => new()
        {
            Identifier = GetCacheKey(),
            Version = Guid.NewGuid().ToString(),
        };

    private string GetCacheKey()
        => ((RepositoryCacheVersionService)RepositoryCacheVersionService).GetCacheKey<IContent>();
}
