// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class CacheInstructionRepositoryTest : UmbracoIntegrationTest
{
    private const string OriginIdentiy = "Test1";
    private const string Instructions = "{}";
    private readonly DateTime _date = new(2021, 7, 3, 10, 30, 0);
    private const int InstructionCount = 1;

    [Test]
    public void Can_Count_All()
    {
        const int Count = 5;

        var sp = ScopeProvider;
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = new CacheInstructionRepository((IScopeAccessor)sp);
            for (var i = 0; i < Count; i++)
            {
                repo.Add(new CacheInstruction(0, _date, Instructions, OriginIdentiy, InstructionCount));
            }

            var count = repo.CountAll();

            Assert.That(count, Is.EqualTo(Count));
        }
    }

    [Test]
    public void Can_Count_Pending_Instructions()
    {
        var sp = ScopeProvider;
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = new CacheInstructionRepository((IScopeAccessor)sp);
            for (var i = 0; i < 5; i++)
            {
                repo.Add(new CacheInstruction(0, _date, Instructions, OriginIdentiy, InstructionCount));
            }

            var count = repo.CountPendingInstructions(2);

            Assert.That(count, Is.EqualTo(3));
        }
    }

    [Test]
    public void Can_Check_Exists()
    {
        var sp = ScopeProvider;
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = new CacheInstructionRepository((IScopeAccessor)sp);

            var existsBefore = repo.Exists(1);

            repo.Add(new CacheInstruction(0, _date, Instructions, OriginIdentiy, InstructionCount));

            var existsAfter = repo.Exists(1);

            Assert.That(existsBefore, Is.False);
            Assert.That(existsAfter, Is.True);
        }
    }

    [Test]
    public void Can_Add_Cache_Instruction()
    {
        const string OriginIdentiy = "Test1";
        const string Instructions = "{}";
        var date = new DateTime(2021, 7, 3, 10, 30, 0);
        const int InstructionCount = 1;

        var sp = ScopeProvider;
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = new CacheInstructionRepository((IScopeAccessor)sp);
            repo.Add(new CacheInstruction(0, date, Instructions, OriginIdentiy, InstructionCount));

            var dtos = ScopeAccessor.AmbientScope.Database.Fetch<CacheInstructionDto>("WHERE id > -1");

            Assert.That(dtos.Any(), Is.True);

            var dto = dtos.First();
            Assert.That(dto.UtcStamp, Is.EqualTo(date));
            Assert.That(dto.Instructions, Is.EqualTo(Instructions));
            Assert.That(dto.OriginIdentity, Is.EqualTo(OriginIdentiy));
            Assert.That(dto.InstructionCount, Is.EqualTo(InstructionCount));
        }
    }

    [Test]
    public void Can_Get_Pending_Instructions()
    {
        var sp = ScopeProvider;
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = new CacheInstructionRepository((IScopeAccessor)sp);
            for (var i = 0; i < 5; i++)
            {
                repo.Add(new CacheInstruction(0, _date, Instructions, OriginIdentiy, InstructionCount));
            }

            var instructions = repo.GetPendingInstructions(2, 2);

            Assert.That(instructions.Count(), Is.EqualTo(2));

            Assert.That(string.Join(",", instructions.Select(x => x.Id)), Is.EqualTo("3,4"));
        }
    }

    [Test]
    public void Can_Delete_Old_Instructions()
    {
        var sp = ScopeProvider;
        using (var scope = ScopeProvider.CreateScope())
        {
            var repo = new CacheInstructionRepository((IScopeAccessor)sp);
            for (var i = 0; i < 5; i++)
            {
                var date = i == 0 ? DateTime.UtcNow.AddDays(-2) : DateTime.UtcNow;
                repo.Add(new CacheInstruction(0, date, Instructions, OriginIdentiy, InstructionCount));
            }

            repo.DeleteInstructionsOlderThan(DateTime.UtcNow.AddDays(-1));

            var count = repo.CountAll();
            Assert.That(count, Is.EqualTo(4)); // 5 have been added, 1 is older and deleted.
        }
    }
}
