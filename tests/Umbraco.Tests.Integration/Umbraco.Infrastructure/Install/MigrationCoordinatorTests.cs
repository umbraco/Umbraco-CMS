// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Install;

[TestFixture]
[CancelAfter(60000)]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MigrationCoordinatorTests : UmbracoIntegrationTest
{
    private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

    // Persistence

    [Test]
    public async Task TryBecomeLeaderAsync_WhenNoClaim_WritesClaimToDatabase()
    {
        var coordinator = CreateCoordinator("machine-a", CreateUpgradingRuntimeState().Object);

        await coordinator.TryBecomeLeaderAsync(CancellationToken.None);

        var claim = KeyValueService.GetValue(Constants.Conventions.Migrations.UpgradeLockKey);
        Assert.That(claim, Does.StartWith("machine-a|"));
    }

    [Test]
    public async Task ReleaseLeadership_AfterClaimingLeadership_ClearsKeyInDatabase()
    {
        var coordinator = CreateCoordinator("machine-a", CreateUpgradingRuntimeState().Object);
        await coordinator.TryBecomeLeaderAsync(CancellationToken.None);

        coordinator.ReleaseLeadership();

        var claim = KeyValueService.GetValue(Constants.Conventions.Migrations.UpgradeLockKey);
        Assert.That(claim, Is.Null.Or.Empty);
    }

    // Stale claim takeover

    [Test]
    public async Task TryBecomeLeaderAsync_WhenExistingClaimIsStale_TakesOverLeadership()
    {
        var staleTimestamp = DateTimeOffset.UtcNow.AddHours(-3).ToString("O");
        KeyValueService.SetValue(
            Constants.Conventions.Migrations.UpgradeLockKey,
            $"crashed-server|{staleTimestamp}");

        var coordinator = CreateCoordinator("machine-a", CreateUpgradingRuntimeState().Object, claimTimeout: TimeSpan.FromHours(2));
        var result = await coordinator.TryBecomeLeaderAsync(CancellationToken.None);

        Assert.That(result, Is.True);
        var claim = KeyValueService.GetValue(Constants.Conventions.Migrations.UpgradeLockKey);
        Assert.That(claim, Does.StartWith("machine-a|"));
    }

    // Concurrent race — the core guarantee

    [Test]
    public void TryBecomeLeaderAsync_WhenTwoCoordinatorsRaceConcurrently_ExactlyOneWins()
    {
        // Both mocks start Upgrading. The winner calls DetermineRuntimeLevel once (post-claim check)
        // and sees Upgrading — migrations haven't run yet, so it continues as leader and returns true.
        // The loser calls DetermineRuntimeLevel twice: the first poll keeps Upgrading (5 s sleep),
        // the second transitions to Run, and the loser returns false.
        var runtimeState1 = CreateTransitioningRuntimeState();
        var runtimeState2 = CreateTransitioningRuntimeState();

        var coordinator1 = CreateCoordinator("machine-a", runtimeState1.Object);
        var coordinator2 = CreateCoordinator("machine-b", runtimeState2.Object);

        bool? result1 = null;
        bool? result2 = null;
        Exception? ex1 = null;
        Exception? ex2 = null;

        var gate = new ManualResetEventSlim(false);

        var t1 = new Thread(() =>
        {
            try
            {
                gate.Wait();
                result1 = coordinator1.TryBecomeLeaderAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                ex1 = ex;
            }
        });

        var t2 = new Thread(() =>
        {
            try
            {
                gate.Wait();
                result2 = coordinator2.TryBecomeLeaderAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                ex2 = ex;
            }
        });

        // Suppress ambient scope from leaking into worker threads (matches LocksTests pattern).
        using (ExecutionContext.SuppressFlow())
        {
            t1.Start();
            t2.Start();
        }

        gate.Set();
        t1.Join();
        t2.Join();

        Assert.That(ex1, Is.Null, $"Coordinator 1 threw: {ex1}");
        Assert.That(ex2, Is.Null, $"Coordinator 2 threw: {ex2}");
        Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Not.Null);
        Assert.That(result2, Is.Not.EqualTo(result1), "Exactly one coordinator should win leadership");

        var winnerId = result1 == true ? "machine-a" : "machine-b";
        var claim = KeyValueService.GetValue(Constants.Conventions.Migrations.UpgradeLockKey);
        Assert.That(claim, Does.StartWith(winnerId + "|"));
    }

    private MigrationCoordinator CreateCoordinator(
        string machineId,
        IRuntimeState runtimeState,
        TimeSpan? claimTimeout = null)
    {
        var machineInfoFactory = Mock.Of<IMachineInfoFactory>(
            f => f.GetMachineIdentifier() == machineId);

        var settings = Options.Create(new UnattendedSettings
        {
            MigrationClaimTimeout = claimTimeout ?? TimeSpan.FromHours(2),
        });

        return new MigrationCoordinator(
            GetRequiredService<ICoreScopeProvider>(),
            GetRequiredService<IKeyValueService>(),
            runtimeState,
            machineInfoFactory,
            settings,
            NullLogger<MigrationCoordinator>.Instance);
    }

    private static Mock<IRuntimeState> CreateUpgradingRuntimeState()
    {
        var mock = new Mock<IRuntimeState>();
        mock.SetupGet(x => x.Level).Returns(RuntimeLevel.Upgrading);
        return mock;
    }

    private static Mock<IRuntimeState> CreateTransitioningRuntimeState()
    {
        var mock = new Mock<IRuntimeState>();
        mock.SetupGet(x => x.Level).Returns(RuntimeLevel.Upgrading);

        // The winner calls DetermineRuntimeLevel() once from the post-claim check and should
        // still see Upgrading (migrations haven't run yet). The loser calls it from the poll
        // loop — first call keeps Upgrading, so it sleeps once; second call transitions to Run.
        int callCount = 0;
        mock.Setup(x => x.DetermineRuntimeLevel())
            .Callback(() =>
            {
                if (Interlocked.Increment(ref callCount) >= 2)
                {
                    mock.SetupGet(x => x.Level).Returns(RuntimeLevel.Run);
                }
            });
        return mock;
    }
}
