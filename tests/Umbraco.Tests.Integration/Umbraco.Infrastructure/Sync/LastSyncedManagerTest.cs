using NUnit.Framework;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Sync;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Sync;


[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class LastSyncedManagerTest : UmbracoIntegrationTest
{
    private ILastSyncedManager manager => GetRequiredService<ILastSyncedManager>();

    [Test]
    public async Task Get_Last_Synced_Internal_Id()
    {
        var value = await manager.GetLastSyncedInternalAsync();
        Assert.IsNull(value);
    }

    [Test]
    public async Task Get_Last_Synced_External_Id()
    {
        var value = await manager.GetLastSyncedExternalAsync();
        Assert.IsNull(value);
    }

    [Test]
    public async Task Save_Last_Synced_Internal_Id()
    {
        Random random = new Random();
        int testId = random.Next();
        await manager.SaveLastSyncedInternalAsync(testId);
        int? lastSynced = await manager.GetLastSyncedInternalAsync();

        Assert.AreEqual(testId, lastSynced);
    }

    [Test]
    public async Task Save_Last_Synced_External_Id()
    {
        Random random = new Random();
        int testId = random.Next();
        await manager.SaveLastSyncedExternalAsync(testId);
        int? lastSynced = await manager.GetLastSyncedExternalAsync();

        Assert.AreEqual(testId, lastSynced);
    }
}
