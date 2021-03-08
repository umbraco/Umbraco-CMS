// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class CacheInstructionServiceTests : UmbracoIntegrationTest
    {
        private const string LocalIdentity = "localIdentity";
        private const string AlternateIdentity = "alternateIdentity";

        [Test]
        public void Can_Ensure_Initialized_With_No_Instructions()
        {
            var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

            CacheInstructionServiceInitializationResult result = sut.EnsureInitialized(false, 0);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.ColdBootRequired);
                Assert.AreEqual(0, result.MaxId);
                Assert.AreEqual(0, result.LastId);
            });
        }

        [Test]
        public void Can_Ensure_Initialized_With_UnSynced_Instructions()
        {
            var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

            List<RefreshInstruction> instructions = CreateInstructions();
            sut.DeliverInstructions(instructions, LocalIdentity);

            CacheInstructionServiceInitializationResult result = sut.EnsureInitialized(false, 0);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.ColdBootRequired);
                Assert.AreEqual(1, result.MaxId);
                Assert.AreEqual(-1, result.LastId);
            });
        }

        [Test]
        public void Can_Ensure_Initialized_With_Synced_Instructions()
        {
            var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

            List<RefreshInstruction> instructions = CreateInstructions();
            sut.DeliverInstructions(instructions, LocalIdentity);

            CacheInstructionServiceInitializationResult result = sut.EnsureInitialized(false, 1);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.ColdBootRequired);
                Assert.AreEqual(1, result.LastId);
            });
        }

        [Test]
        public void Can_Deliver_Instructions()
        {
            var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

            List<RefreshInstruction> instructions = CreateInstructions();

            sut.DeliverInstructions(instructions, LocalIdentity);

            AssertDeliveredInstructions();
        }

        [Test]
        public void Can_Deliver_Instructions_In_Batches()
        {
            var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

            List<RefreshInstruction> instructions = CreateInstructions();

            sut.DeliverInstructionsInBatches(instructions, LocalIdentity);

            AssertDeliveredInstructions();
        }

        private List<RefreshInstruction> CreateInstructions() => new List<RefreshInstruction>
            {
                new RefreshInstruction(UserCacheRefresher.UniqueId, RefreshMethodType.RefreshByIds, Guid.Empty, 0, "[-1]", null),
                new RefreshInstruction(UserCacheRefresher.UniqueId, RefreshMethodType.RefreshByIds, Guid.Empty, 0, "[-1]", null),
            };

        private void AssertDeliveredInstructions()
        {
            List<CacheInstructionDto> cacheInstructions;
            ISqlContext sqlContext = GetRequiredService<ISqlContext>();
            Sql<ISqlContext> sql = sqlContext.Sql()
                .Select<CacheInstructionDto>()
                .From<CacheInstructionDto>();
            using (IScope scope = ScopeProvider.CreateScope())
            {
                cacheInstructions = scope.Database.Fetch<CacheInstructionDto>(sql);
                scope.Complete();
            }

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, cacheInstructions.Count);

                CacheInstructionDto firstInstruction = cacheInstructions.First();
                Assert.AreEqual(2, firstInstruction.InstructionCount);
                Assert.AreEqual(LocalIdentity, firstInstruction.OriginIdentity);
            });
        }

        [Test]
        public void Can_Process_Instructions()
        {
            var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

            // Create three instruction records, each with two instructions.  First two records are for a different identity.
            CreateMultipleInstructions(sut);

            CacheInstructionServiceProcessInstructionsResult result = sut.ProcessInstructions(false, LocalIdentity, DateTime.UtcNow.AddSeconds(-1));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, result.LastId);                          // 3 records found.
                Assert.AreEqual(2, result.NumberOfInstructionsProcessed);   // 2 records processed (as one is for the same identity).
                Assert.IsFalse(result.InstructionsWerePruned);
            });
        }

        [Test]
        public void Can_Process_And_Purge_Instructions()
        {
            // Purging of instructions only occurs on single or master servers, so we need to ensure this is set before running the test.
            EnsureServerRegistered();
            var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

            CreateMultipleInstructions(sut);

            CacheInstructionServiceProcessInstructionsResult result = sut.ProcessInstructions(false, LocalIdentity, DateTime.UtcNow.AddHours(-1));

            Assert.IsTrue(result.InstructionsWerePruned);
        }

        [Test]
        public void Processes_No_Instructions_When_Released()
        {
            var sut = (CacheInstructionService)GetRequiredService<ICacheInstructionService>();

            CreateMultipleInstructions(sut);

            CacheInstructionServiceProcessInstructionsResult result = sut.ProcessInstructions(true, LocalIdentity, DateTime.UtcNow.AddSeconds(-1));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(0, result.LastId);
                Assert.AreEqual(0, result.NumberOfInstructionsProcessed);
                Assert.IsFalse(result.InstructionsWerePruned);
            });
        }

        private void CreateMultipleInstructions(CacheInstructionService sut)
        {
            for (int i = 0; i < 3; i++)
            {
                List<RefreshInstruction> instructions = CreateInstructions();
                sut.DeliverInstructions(instructions, i == 2 ? LocalIdentity : AlternateIdentity);
            }
        }

        private void EnsureServerRegistered()
        {
            IServerRegistrationService serverRegistrationService = GetRequiredService<IServerRegistrationService>();
            serverRegistrationService.TouchServer("http://localhost", TimeSpan.FromMinutes(10));
        }
    }
}
