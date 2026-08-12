// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Runtime;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Install;

[TestFixture]
public class UnattendedUpgraderTests
{
    [Test]
    public async Task RunPackageMigrations_WhenSinglePlanFails_SetsBootFailedWithThePlanException()
    {
        var planException = new InvalidOperationException("migration step exploded");
        (UnattendedUpgrader sut, Mock<IRuntimeState> runtimeState) =
            CreateScenario(new PlanSpec("Failing Package", Successful: false, planException));
        var notification = new RuntimeUnattendedUpgradeNotification();

        await sut.HandleAsync(notification, CancellationToken.None);

        Assert.Multiple(() =>
        {
            // The failed (but not thrown) package plan must surface as a boot failure, mirroring the core upgrade path,
            // forwarding the original migration exception rather than swallowing it.
            runtimeState.Verify(
                x => x.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, planException),
                Times.Once);
            Assert.That(notification.UnattendedUpgradeResult, Is.EqualTo(RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors));
        });
    }

    [Test]
    public async Task RunPackageMigrations_WhenMultiplePlansFail_SetsBootFailedWithAllFailures()
    {
        var firstException = new InvalidOperationException("first plan exploded");
        var secondException = new InvalidOperationException("second plan exploded");

        (UnattendedUpgrader sut, Mock<IRuntimeState> runtimeState) = CreateScenario(
            new PlanSpec("First Failing Package", Successful: false, firstException),
            new PlanSpec("Second Failing Package", Successful: false, secondException));
        var notification = new RuntimeUnattendedUpgradeNotification();

        Func<Exception?> capturedError = CaptureBootFailedException(runtimeState);

        await sut.HandleAsync(notification, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(notification.UnattendedUpgradeResult, Is.EqualTo(RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors));

            // Both failures must be reported together so the operator can address them in one pass.
            var aggregate = capturedError() as AggregateException;
            Assert.That(aggregate, Is.Not.Null, "Expected the boot failure to aggregate all failed plans.");
            Assert.That(aggregate!.InnerExceptions, Does.Contain(firstException));
            Assert.That(aggregate.InnerExceptions, Does.Contain(secondException));
        });
    }

    [Test]
    public async Task RunPackageMigrations_WhenPlanFailsWithoutException_SetsBootFailedWithFallbackException()
    {
        const string planName = "Failing Package Without Exception";
        (UnattendedUpgrader sut, Mock<IRuntimeState> runtimeState) =
            CreateScenario(new PlanSpec(planName, Successful: false, Exception: null));
        var notification = new RuntimeUnattendedUpgradeNotification();

        Func<Exception?> capturedError = CaptureBootFailedException(runtimeState);

        await sut.HandleAsync(notification, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(notification.UnattendedUpgradeResult, Is.EqualTo(RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors));

            // A plan can fail without carrying an exception; fall back to a descriptive one naming the plan.
            Exception? error = capturedError();
            Assert.That(error, Is.InstanceOf<UnattendedInstallException>());
            Assert.That(error!.Message, Does.Contain(planName));
        });
    }

    [Test]
    public async Task RunPackageMigrations_WhenPlanSucceeds_DoesNotSetBootFailed()
    {
        (UnattendedUpgrader sut, Mock<IRuntimeState> runtimeState) =
            CreateScenario(new PlanSpec("Succeeding Package", Successful: true, Exception: null));
        var notification = new RuntimeUnattendedUpgradeNotification();

        await sut.HandleAsync(notification, CancellationToken.None);

        Assert.Multiple(() =>
        {
            runtimeState.Verify(
                x => x.Configure(RuntimeLevel.BootFailed, It.IsAny<RuntimeLevelReason>(), It.IsAny<Exception?>()),
                Times.Never);
            Assert.That(notification.UnattendedUpgradeResult, Is.EqualTo(RuntimeUnattendedUpgradeNotification.UpgradeResult.PackageMigrationComplete));
        });
    }

    private static (UnattendedUpgrader Sut, Mock<IRuntimeState> RuntimeState) CreateScenario(params PlanSpec[] specs)
    {
        var plans = specs.Select(spec => new TestPackageMigrationPlan(spec.Name)).ToArray();
        var planCollection = new PackageMigrationPlanCollection(() => plans);
        Dictionary<string, PlanSpec> specsByName = specs.ToDictionary(spec => spec.Name);

        var migrationExecutor = new Mock<IMigrationPlanExecutor>();
        migrationExecutor
            .Setup(x => x.ExecutePlanAsync(It.IsAny<MigrationPlan>(), It.IsAny<string>()))
            .ReturnsAsync((MigrationPlan executedPlan, string initialState) =>
            {
                PlanSpec spec = specsByName[executedPlan.Name];
                return new ExecutedMigrationPlan
                {
                    Plan = executedPlan,
                    InitialState = initialState,
                    FinalState = spec.Successful ? "done" : initialState,
                    Successful = spec.Successful,
                    Exception = spec.Exception,
                    CompletedTransitions = Array.Empty<MigrationPlan.Transition>(),
                };
            });

        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        var packageMigrationRunner = new PackageMigrationRunner(
            Mock.Of<IProfilingLogger>(),
            scopeProvider.Object,
            new PendingPackageMigrations(NullLogger<PendingPackageMigrations>.Instance, planCollection),
            planCollection,
            migrationExecutor.Object,
            Mock.Of<IKeyValueService>(),
            Mock.Of<IEventAggregator>(),
            NullLogger<PackageMigrationRunner>.Instance);

        Mock<IRuntimeState> runtimeState = CreateRuntimeState(specs.Select(spec => spec.Name).ToArray());

        var sut = new UnattendedUpgrader(
            Mock.Of<IProfilingLogger>(),
            Mock.Of<IUmbracoVersion>(),
            databaseBuilder: null!, // Unused on the package-migration branch exercised by these tests.
            runtimeState.Object,
            packageMigrationRunner,
            Options.Create(new UnattendedSettings()),
            CreateDistributedCache(),
            NullLogger<UnattendedUpgrader>.Instance);

        return (sut, runtimeState);
    }

    private static Mock<IRuntimeState> CreateRuntimeState(IReadOnlyList<string> pendingPlanNames)
    {
        var mock = new Mock<IRuntimeState>();
        mock.SetupGet(x => x.Level).Returns(RuntimeLevel.Upgrading);
        mock.SetupGet(x => x.Reason).Returns(RuntimeLevelReason.UpgradePackageMigrations);
        mock.SetupGet(x => x.StartupState).Returns(new Dictionary<string, object>
        {
            [RuntimeState.PendingPackageMigrationsStateKey] = pendingPlanNames,
        });
        return mock;
    }

    private static Func<Exception?> CaptureBootFailedException(Mock<IRuntimeState> runtimeState)
    {
        Exception? captured = null;
        runtimeState
            .Setup(x => x.Configure(RuntimeLevel.BootFailed, It.IsAny<RuntimeLevelReason>(), It.IsAny<Exception?>()))
            .Callback<RuntimeLevel, RuntimeLevelReason, Exception?>((_, _, exception) => captured = exception);
        return () => captured;
    }

    private static DistributedCache CreateDistributedCache()
    {
        static ICacheRefresher Refresher(Guid id)
        {
            var mock = new Mock<ICacheRefresher>();
            mock.SetupGet(x => x.RefresherUniqueId).Returns(id);
            return mock.Object;
        }

        var refreshers = new CacheRefresherCollection(() => new[]
        {
            Refresher(ContentCacheRefresher.UniqueId),
            Refresher(MediaCacheRefresher.UniqueId),
            Refresher(ElementCacheRefresher.UniqueId),
            Refresher(DomainCacheRefresher.UniqueId),
        });

        return new DistributedCache(Mock.Of<IServerMessenger>(), refreshers);
    }

    private sealed record PlanSpec(string Name, bool Successful, Exception? Exception);

    private sealed class TestPackageMigrationPlan : PackageMigrationPlan
    {
        public TestPackageMigrationPlan(string planName)
            : base(planName)
        {
        }

        protected override void DefinePlan() => To<NoOpMigration>("done");
    }

    private sealed class NoOpMigration : AsyncMigrationBase
    {
        public NoOpMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override async Task MigrateAsync()
        {
            await Task.CompletedTask;
        }
    }
}
