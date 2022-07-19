// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class CacheInstructionServiceTests : UmbracoIntegrationTest
{
    private const string LocalIdentity = "localIdentity";
    private const string AlternateIdentity = "alternateIdentity";

    private CancellationToken CancellationToken => new();
    private CacheRefresherCollection CacheRefreshers => GetRequiredService<CacheRefresherCollection>();
    private IServerRoleAccessor ServerRoleAccessor => GetRequiredService<IServerRoleAccessor>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddNuCache();
    }

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

        var result = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole, CancellationToken,
            LocalIdentity, DateTime.UtcNow.AddSeconds(-1), -1);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, result.LastId); // 3 records found.
            Assert.AreEqual(2,
                result.NumberOfInstructionsProcessed); // 2 records processed (as one is for the same identity).
            Assert.IsFalse(result.InstructionsWerePruned);
        });
    }

    [Test]
    public void Can_Process_And_Purge_Instructions()
    {
        // Purging of instructions only occurs on single or master servers, so we need to ensure this is set before running the test.
        EnsureServerRegistered();
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        CreateAndDeliveryMultipleInstructions(sut);

        var result = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole, CancellationToken,
            LocalIdentity, DateTime.UtcNow.AddHours(-1), -1);

        Assert.IsTrue(result.InstructionsWerePruned);
    }

    [Test]
    public void Processes_No_Instructions_When_CancellationToken_is_Cancelled()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

        CreateAndDeliveryMultipleInstructions(sut);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var result = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole,
            cancellationTokenSource.Token, LocalIdentity, DateTime.UtcNow.AddSeconds(-1), -1);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, result.LastId);
            Assert.AreEqual(0, result.NumberOfInstructionsProcessed);
            Assert.IsFalse(result.InstructionsWerePruned);
        });
    }

    [Test]
    public void Processes_Instructions_Only_Once()
    {
        // This test shows what's happening in issue #10112
        // The DatabaseServerMessenger will run its sync operation every five seconds which calls CacheInstructionService.ProcessInstructions,
        // which is why the CacheRefresherNotification keeps dispatching, because the cache instructions gets constantly processed.
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();
        CreateAndDeliveryMultipleInstructions(sut);

        var lastId = -1;
        // Run once
        var result = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole, CancellationToken,
            LocalIdentity, DateTime.UtcNow.AddSeconds(-1), lastId);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, result.LastId); // 3 records found.
            Assert.AreEqual(2,
                result.NumberOfInstructionsProcessed); // 2 records processed (as one is for the same identity).
            Assert.IsFalse(result.InstructionsWerePruned);
        });

        // DatabaseServerMessenger stores the LastID after ProcessInstructions has been run.
        lastId = result.LastId;

        // The instructions has now been processed and shouldn't be processed on the next call...
        // Run again.
        var secondResult = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole,
            CancellationToken, LocalIdentity, DateTime.UtcNow.AddSeconds(-1), lastId);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(0,
                secondResult
                    .LastId); // No instructions was processed so LastId is 0, this is consistent with behavior from V8
            Assert.AreEqual(0, secondResult.NumberOfInstructionsProcessed); // Nothing was processed.
            Assert.IsFalse(secondResult.InstructionsWerePruned);
        });
    }

    [Test]
    public void Processes_New_Instructions()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();
        CreateAndDeliveryMultipleInstructions(sut);

        var lastId = -1;
        var result = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole, CancellationToken,
            LocalIdentity, DateTime.UtcNow.AddSeconds(-1), lastId);

        Assert.AreEqual(3, result.LastId); // Make sure LastId is 3, the rest is tested in other test.
        lastId = result.LastId;

        // Add new instruction
        var instructions = CreateInstructions();
        sut.DeliverInstructions(instructions, AlternateIdentity);

        var secondResult = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole,
            CancellationToken, LocalIdentity, DateTime.UtcNow.AddSeconds(-1), lastId);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, secondResult.LastId);
            Assert.AreEqual(1, secondResult.NumberOfInstructionsProcessed);
            Assert.IsFalse(secondResult.InstructionsWerePruned);
        });
    }

    [Test]
    public void Correct_ID_For_Instruction_With_Same_Identity()
    {
        var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();
        CreateAndDeliveryMultipleInstructions(sut);

        var lastId = -1;
        var result = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole, CancellationToken,
            LocalIdentity, DateTime.UtcNow.AddSeconds(-1), lastId);

        Assert.AreEqual(3, result.LastId); // Make sure LastId is 3, the rest is tested in other test.
        lastId = result.LastId;

        // Add new instruction
        var instructions = CreateInstructions();
        sut.DeliverInstructions(instructions, LocalIdentity);

        var secondResult = sut.ProcessInstructions(CacheRefreshers, ServerRoleAccessor.CurrentServerRole,
            CancellationToken, LocalIdentity, DateTime.UtcNow.AddSeconds(-1), lastId);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, secondResult.LastId);
            Assert.AreEqual(0, secondResult.NumberOfInstructionsProcessed);
            Assert.IsFalse(secondResult.InstructionsWerePruned);
        });
    }

    private void CreateAndDeliveryMultipleInstructions(CacheInstructionService sut)
    {
        for (var i = 0; i < 3; i++)
        {
            var instructions = CreateInstructions();
            sut.DeliverInstructions(instructions, i == 2 ? LocalIdentity : AlternateIdentity);
        }
    }

    private void EnsureServerRegistered()
    {
        var serverRegistrationService = GetRequiredService<IServerRegistrationService>();
        serverRegistrationService.TouchServer("http://localhost", TimeSpan.FromMinutes(10));
    }
}
