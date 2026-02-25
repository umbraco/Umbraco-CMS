// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class CacheInstructionServiceTests : UmbracoIntegrationTest
{
    private const string LocalIdentity = "localIdentity";
    private const string AlternateIdentity = "alternateIdentity";

    private CancellationToken CancellationToken => CancellationToken.None;

    private CacheRefresherCollection CacheRefreshers => GetRequiredService<CacheRefresherCollection>();

    [Test]
    public void Confirms_Cold_Boot_Required_When_Instructions_Exist_And_None_Have_Been_Synced()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        var instructions = CreateInstructions();
        sut.DeliverInstructions(instructions, LocalIdentity);

        var result = sut.IsColdBootRequired(0);

        Assert.IsTrue(result);
    }

    [Test]
    public void Confirms_Cold_Boot_Required_When_Last_Synced_Instruction_Not_Found()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        var instructions = CreateInstructions();
        sut.DeliverInstructions(instructions, LocalIdentity); // will create with Id = 1

        var result = sut.IsColdBootRequired(2);

        Assert.IsTrue(result);
    }

    [Test]
    public void Confirms_Cold_Boot_Not_Required_When_Last_Synced_Instruction_Found()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        var instructions = CreateInstructions();
        sut.DeliverInstructions(instructions, LocalIdentity); // will create with Id = 1

        var result = sut.IsColdBootRequired(1);

        Assert.IsFalse(result);
    }

    [TestCase(1, 10, false, 4)]
    [TestCase(1, 3, true, 4)]
    public void Confirms_Instruction_Count_Over_Limit(int lastId, int limit, bool expectedResult, int expectedCount)
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        CreateAndDeliveryMultipleInstructions(sut); // 3 records, each with 2 instructions = 6.

        var result = sut.IsInstructionCountOverLimit(lastId, limit, out var count);

        Assert.AreEqual(expectedResult, result);
        Assert.AreEqual(expectedCount, count);
    }

    [Test]
    public void Can_Get_Max_Instruction_Id()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        CreateAndDeliveryMultipleInstructions(sut);

        var result = sut.GetMaxInstructionId();

        Assert.AreEqual(3, result);
    }

    [Test]
    public void Can_Deliver_Instructions()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        var instructions = CreateInstructions();

        sut.DeliverInstructions(instructions, LocalIdentity);

        AssertDeliveredInstructions();
    }

    [Test]
    public void Can_Deliver_Instructions_In_Batches()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        var instructions = CreateInstructions();

        sut.DeliverInstructionsInBatches(instructions, LocalIdentity);

        AssertDeliveredInstructions();
    }

    private List<RefreshInstruction> CreateInstructions() => new()
    {
        new(UserCacheRefresher.UniqueId, RefreshMethodType.RefreshByIds, Guid.Empty, 0, "[-1]", null),
        new(UserCacheRefresher.UniqueId, RefreshMethodType.RefreshByIds, Guid.Empty, 0, "[-1]", null)
    };

    private void AssertDeliveredInstructions()
    {
        List<CacheInstructionDto> cacheInstructions;
        var sqlContext = GetRequiredService<ISqlContext>();
        var sql = sqlContext.Sql()
            .Select<CacheInstructionDto>()
            .From<CacheInstructionDto>();
        using (var scope = ScopeProvider.CreateScope())
        {
            cacheInstructions = ScopeAccessor.AmbientScope.Database.Fetch<CacheInstructionDto>(sql);
            scope.Complete();
        }

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, cacheInstructions.Count);

            var firstInstruction = cacheInstructions.First();
            Assert.AreEqual(2, firstInstruction.InstructionCount);
            Assert.AreEqual(LocalIdentity, firstInstruction.OriginIdentity);
        });
    }

    [Test]
    public void Can_Process_Instructions()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        // Create three instruction records, each with two instructions.  First two records are for a different identity.
        CreateAndDeliveryMultipleInstructions(sut);

        var result = sut.ProcessAllInstructions(CacheRefreshers, CancellationToken, LocalIdentity);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, result.LastId); // 3 records found.
            Assert.AreEqual(2, result.NumberOfInstructionsProcessed); // 2 records processed (as one is for the same identity).
        });
    }

    [Test]
    public void Processes_No_Instructions_When_CancellationToken_is_Cancelled()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        CreateAndDeliveryMultipleInstructions(sut);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var result = sut.ProcessAllInstructions(CacheRefreshers, cancellationTokenSource.Token, LocalIdentity);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, result.LastId);
            Assert.AreEqual(0, result.NumberOfInstructionsProcessed);
        });
    }

    [Test]
    public void Processes_Instructions_Only_Once()
    {
        // This test shows what's happening in issue #10112
        // The DatabaseServerMessenger will run its sync operation every five seconds which calls
        // ProcessAllInstructions, which is why the CacheRefresherNotification keeps dispatching,
        // because the cache instructions gets constantly processed.
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();
        CreateAndDeliveryMultipleInstructions(sut);

        // Run once
        var result = sut.ProcessAllInstructions(CacheRefreshers, CancellationToken, LocalIdentity);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, result.LastId); // 3 records found.
            Assert.AreEqual(2, result.NumberOfInstructionsProcessed); // 2 records processed (as one is for the same identity).
        });

        // ProcessAllInstructions persists the last synced ID via ILastSyncedManager,
        // so the next call picks up where we left off.

        // The instructions has now been processed and shouldn't be processed on the next call...
        // Run again.
        var secondResult = sut.ProcessAllInstructions(CacheRefreshers, CancellationToken, LocalIdentity);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(
                0,
                secondResult
                    .LastId); // No instructions was processed so LastId is 0, this is consistent with behavior from V8
            Assert.AreEqual(0, secondResult.NumberOfInstructionsProcessed); // Nothing was processed.
        });
    }

    [Test]
    public void Processes_New_Instructions()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();
        CreateAndDeliveryMultipleInstructions(sut);

        var result = sut.ProcessAllInstructions(CacheRefreshers, CancellationToken, LocalIdentity);

        Assert.AreEqual(3, result.LastId); // Make sure LastId is 3, the rest is tested in other test.

        // Add new instruction
        var instructions = CreateInstructions();
        sut.DeliverInstructions(instructions, AlternateIdentity);

        var secondResult = sut.ProcessAllInstructions(CacheRefreshers, CancellationToken, LocalIdentity);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, secondResult.LastId);
            Assert.AreEqual(1, secondResult.NumberOfInstructionsProcessed);
        });
    }

    [Test]
    public void Correct_ID_For_Instruction_With_Same_Identity()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();
        CreateAndDeliveryMultipleInstructions(sut);

        var result = sut.ProcessAllInstructions(CacheRefreshers, CancellationToken, LocalIdentity);

        Assert.AreEqual(3, result.LastId); // Make sure LastId is 3, the rest is tested in other test.

        // Add new instruction
        var instructions = CreateInstructions();
        sut.DeliverInstructions(instructions, LocalIdentity);

        var secondResult = sut.ProcessAllInstructions(CacheRefreshers, CancellationToken, LocalIdentity);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, secondResult.LastId);
            Assert.AreEqual(0, secondResult.NumberOfInstructionsProcessed);
        });
    }

    [Test]
    public async Task Local_Instructions_Trigger_LastId_Update()
    {
        var sut = GetRequiredService<ICacheInstructionService>();
        var lastSyncedManager = GetRequiredService<ILastSyncedManager>();

        var instructions = CreateInstructions();
        sut.DeliverInstructions(instructions, LocalIdentity);
        sut.DeliverInstructions(instructions, LocalIdentity);

        var lastSynced = await lastSyncedManager.GetLastSyncedExternalAsync();
        Assert.IsNull(lastSynced);

        var result = sut.ProcessAllInstructions(CacheRefreshers, CancellationToken, LocalIdentity);

        Assert.Multiple(() =>
        {
            // 2 records found.
            Assert.AreEqual(2, result.LastId);

            // local instructions do not count against number of processed instructions.
            Assert.AreEqual(0, result.NumberOfInstructionsProcessed);
        });

        lastSynced = await lastSyncedManager.GetLastSyncedExternalAsync();
        Assert.AreEqual(2, lastSynced);
        var debug = true;
    }

    private void CreateAndDeliveryMultipleInstructions(CacheInstructionService sut)
    {
        for (var i = 0; i < 3; i++)
        {
            var instructions = CreateInstructions();
            sut.DeliverInstructions(instructions, i == 2 ? LocalIdentity : AlternateIdentity);
        }
    }
}
