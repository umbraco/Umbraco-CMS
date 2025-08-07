using NUnit.Framework;
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
    public void Get_Last_Synced_Internal_Id()
    {
        var value = manager.GetLastSyncedInternalAsync();
        Assert.IsNotNull(value);

        value = manager.GetLastSyncedExternalAsync();
        Assert.IsNotNull(value);
    }

    [Test]
    public void Get_Last_Synced_External_Id()
    {
        var value = manager.GetLastSyncedExternalAsync();
        Assert.IsNotNull(value);
    }

    [Test]
    public void Save_Last_Synced_Internal_Id()
    {
        Random random = new Random();
        int testId = random.Next();
        manager.SaveLastSyncedInternalAsync(testId);
        int? lastSynced = manager.GetLastSyncedInternalAsync().Result;

        Assert.AreEqual(testId, lastSynced);
    }

    [Test]
    public void Save_Last_Synced_External_Id()
    {
        Random random = new Random();
        int testId = random.Next();
        manager.SaveLastSyncedExternalAsync(testId);
        int? lastSynced = manager.GetLastSyncedExternalAsync().Result;

        Assert.AreEqual(testId, lastSynced);
    }
}
