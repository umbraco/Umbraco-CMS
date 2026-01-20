using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Sync;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class SyncBootStateAccessorTest : UmbracoIntegrationTest
{
    private ILastSyncedManager LastSyncedManager => GetRequiredService<ILastSyncedManager>();

    private SyncBootStateAccessor CreateSyncBootStateAccessor() => new(
        GetRequiredService<ILogger<SyncBootStateAccessor>>(),
        GetRequiredService<IOptionsMonitor<GlobalSettings>>(),
        GetRequiredService<ICacheInstructionService>(),
        LastSyncedManager);

    [Test]
    public void Returns_ColdBoot_When_No_Last_Synced_Id()
    {
        // Arrange - the database is fresh with no last synced id saved
        var syncBootStateAccessor = CreateSyncBootStateAccessor();

        // Act
        var result = syncBootStateAccessor.GetSyncBootState();

        // Assert
        Assert.AreEqual(SyncBootState.ColdBoot, result);
    }

    [Test]
    public async Task Returns_WarmBoot_When_Last_Synced_Id_Exists()
    {
        // Arrange - create a cache instruction (ID is auto-incremented starting at 1)
        // and save the last synced external id matching it
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = new CacheInstructionRepository((IScopeAccessor)ScopeProvider);
            repo.Add(new CacheInstruction(0, DateTime.Now, "{}", "Test", 1));
            scope.Complete();
        }

        // The auto-incremented ID will be 1 in a fresh database
        await LastSyncedManager.SaveLastSyncedExternalAsync(1);

        var syncBootStateAccessor = CreateSyncBootStateAccessor();

        // Act
        var result = syncBootStateAccessor.GetSyncBootState();

        // Assert
        Assert.AreEqual(SyncBootState.WarmBoot, result);
    }
}
