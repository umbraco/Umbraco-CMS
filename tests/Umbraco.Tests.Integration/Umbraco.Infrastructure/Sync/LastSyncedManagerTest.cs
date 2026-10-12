using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Sync;


[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class LastSyncedManagerTest : UmbracoIntegrationTest
{
    private LastSyncedManager manager => (LastSyncedManager)GetRequiredService<ILastSyncedManager>();

    [Test]
    public async Task Last_Synced_Internal_Id_Is_Initially_Null()
    {
        var value = await manager.GetLastSyncedInternalAsync();
        Assert.IsNull(value);
    }

    [Test]
    public async Task Last_Synced_External_Id_Is_Initially_Null()
    {
        var value = await manager.GetLastSyncedExternalAsync();
        Assert.IsNull(value);
    }

    [Test]
    public async Task Last_Synced_Internal_Id_Cannot_Be_Negative()
    {
        Assert.Throws<ArgumentException>(() => manager.SaveLastSyncedInternalAsync(-1).GetAwaiter().GetResult());
    }

    [Test]
    public async Task Last_Synced_External_Id_Cannot_Be_Negative()
    {
        Assert.Throws<ArgumentException>(() => manager.SaveLastSyncedExternalAsync(-1).GetAwaiter().GetResult());
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
    public async Task Save_Last_Synced_Internal_Id_Ignores_Ids_That_Do_Not_Move_Forward()
    {
        await manager.SaveLastSyncedInternalAsync(5);

        await manager.SaveLastSyncedInternalAsync(3);

        Assert.AreEqual(5, await manager.GetLastSyncedInternalAsync());
    }

    [Test]
    public async Task Last_Synced_Internal_Id_Is_Kept_In_Memory_Only()
    {
        await manager.SaveLastSyncedInternalAsync(5);
        Assume.That(await manager.GetLastSyncedInternalAsync(), Is.EqualTo(5));

        manager.ClearLocalCache();

        Assert.Multiple(() =>
        {
            Assert.That(manager.GetLastSyncedInternalAsync().GetAwaiter().GetResult(), Is.Null, "The internal id was persisted.");
            using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
            Assert.That(GetRequiredService<ILastSyncedRepository>().GetInternalIdAsync().GetAwaiter().GetResult(), Is.Null, "The internal id was written to the database.");
        });
    }

    [Test]
    public async Task Last_Synced_Internal_Id_Starts_From_The_Persisted_External_Id()
    {
        await manager.SaveLastSyncedExternalAsync(7);
        manager.ClearLocalCache();

        Assert.AreEqual(7, await manager.GetLastSyncedInternalAsync());
    }

    [Test]
    public async Task Last_Synced_Internal_Id_Does_Not_Fall_Behind_The_Persisted_External_Id()
    {
        await manager.SaveLastSyncedExternalAsync(7);
        manager.ClearLocalCache();

        await manager.SaveLastSyncedInternalAsync(3);

        Assert.AreEqual(7, await manager.GetLastSyncedInternalAsync());
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

    [Test]
    public async Task Delete_Old_Synced_External_Id()
    {
        Random random = new Random();
        int testId = random.Next();
        await manager.SaveLastSyncedExternalAsync(testId);
        manager.ClearLocalCache();

        // Make sure not to delete if not too old.
        await manager.DeleteOlderThanAsync(DateTime.Now - TimeSpan.FromDays(1));
        int? lastSynced = await manager.GetLastSyncedExternalAsync();
        Assert.NotNull(lastSynced);
        manager.ClearLocalCache();

        // Make sure to delete if too old.
        await manager.DeleteOlderThanAsync(DateTime.Now + TimeSpan.FromDays(1));
        lastSynced = await manager.GetLastSyncedExternalAsync();
        Assert.Null(lastSynced);
    }

    [Test]
    public async Task Delete_Out_Of_Sync_Id()
    {
        using (ScopeProvider.CreateScope())
        {
            var repo = new CacheInstructionRepository((IScopeAccessor)ScopeProvider);
            repo.Add(new CacheInstruction(0, DateTime.Now, "{}", "Test", 1));

            Assert.IsTrue(repo.Exists(1));

            await manager.SaveLastSyncedExternalAsync(2);
            await manager.SaveLastSyncedInternalAsync(2);
            manager.ClearLocalCache();


            Assert.NotNull(await manager.GetLastSyncedExternalAsync());
            manager.ClearLocalCache();

            await manager.DeleteOlderThanAsync(DateTime.Now);

            Assert.Null(await manager.GetLastSyncedExternalAsync());
        }
    }
}
