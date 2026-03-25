// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Runtime;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Runtime;

[TestFixture]
public class CoreRuntimeTests
{
    /// <summary>
    /// When runtime level is BootFailed, StartAsync returns before publishing any migration notifications.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenBootFailed_ReturnsEarly()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.BootFailed);
        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(eventAggregator);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<RuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// On initial boot with Level=Upgrading, StartAsync returns early so that
    /// <see cref="UnattendedUpgradeBackgroundService"/> handles migrations after the HTTP server starts.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenUpgradingOnInitialBoot_ReturnsEarlyForBackgroundService()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Upgrading);
        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(eventAggregator);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<RuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// When premigrations report errors and a <see cref="BootFailedException"/> is registered,
    /// startup halts before post-premigrations or unattended upgrade notifications fire.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenPremigrationsHasErrors_WithBootFailedException_ReturnsEarly()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);
        runtimeState.SetupGet(x => x.BootFailedException).Returns(new BootFailedException("db error"));

        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(
            eventAggregator,
            premigrationResult: RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.HasErrors);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<PostRuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<RuntimeUnattendedUpgradeNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// When premigrations report errors but no <see cref="BootFailedException"/> has been registered,
    /// StartAsync throws an <see cref="InvalidOperationException"/> to signal an inconsistent state.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenPremigrationsHasErrors_WithoutBootFailedException_Throws()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);

        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(
            eventAggregator,
            premigrationResult: RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.HasErrors);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.StartAsync(CancellationToken.None));
    }

    /// <summary>
    /// When premigrations complete a core upgrade, DetermineRuntimeLevel is called a second time
    /// so the runtime level reflects the post-upgrade state.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenPremigrationsCoreUpgradeComplete_DeterminesRuntimeLevelAgain()
    {
        var determineCallCount = 0;
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);
        runtimeState.Setup(x => x.DetermineRuntimeLevel()).Callback(() => determineCallCount++);

        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(
            eventAggregator,
            premigrationResult: RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.CoreUpgradeComplete);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);

        Assert.That(determineCallCount, Is.GreaterThanOrEqualTo(2));
    }

    /// <summary>
    /// When premigrations are not required, the full startup notification sequence fires:
    /// premigrations, post-premigrations, and unattended upgrade.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenPremigrationsNotRequired_ContinuesToUnattendedUpgrade()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);
        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(eventAggregator);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<RuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<PostRuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<RuntimeUnattendedUpgradeNotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// When the unattended upgrade reports errors and a <see cref="BootFailedException"/> is registered,
    /// startup halts before component initialization.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenUnattendedUpgradeHasErrors_WithBootFailedException_ReturnsEarly()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);
        runtimeState.SetupGet(x => x.BootFailedException).Returns(new BootFailedException("migration error"));

        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(
            eventAggregator,
            upgradeResult: RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<UmbracoApplicationStartingNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// When the unattended upgrade reports errors but no <see cref="BootFailedException"/> has been registered,
    /// StartAsync throws an <see cref="InvalidOperationException"/> to signal an inconsistent state.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenUnattendedUpgradeHasErrors_WithoutBootFailedException_Throws()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);

        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(
            eventAggregator,
            upgradeResult: RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        Assert.ThrowsAsync<InvalidOperationException>(() => sut.StartAsync(CancellationToken.None));
    }

    /// <summary>
    /// When the unattended upgrade completes successfully (core or package migration),
    /// DetermineRuntimeLevel is called again so the level transitions to Run.
    /// </summary>
    [TestCase(RuntimeUnattendedUpgradeNotification.UpgradeResult.CoreUpgradeComplete)]
    [TestCase(RuntimeUnattendedUpgradeNotification.UpgradeResult.PackageMigrationComplete)]
    public async Task StartAsync_WhenUnattendedUpgradeSucceeds_DeterminesRuntimeLevel(
        RuntimeUnattendedUpgradeNotification.UpgradeResult upgradeResult)
    {
        var determineCallCount = 0;
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);
        runtimeState.Setup(x => x.DetermineRuntimeLevel()).Callback(() => determineCallCount++);

        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(eventAggregator, upgradeResult: upgradeResult);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);

        Assert.That(determineCallCount, Is.GreaterThanOrEqualTo(2));
    }

    /// <summary>
    /// On a normal startup at Run level with no migrations needed,
    /// <see cref="UmbracoApplicationStartingNotification"/> is published after component initialization.
    /// </summary>
    [Test]
    public async Task StartAsync_WhenRunLevel_PublishesApplicationStartingNotification()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);
        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(eventAggregator);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<UmbracoApplicationStartingNotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// When a runtime restart sets level to Upgrading (e.g. pending package migrations after a fresh install),
    /// the synchronous migration path runs instead of returning early, because the one-shot
    /// <see cref="UnattendedUpgradeBackgroundService"/> has already exited and will not re-run.
    /// This is the scenario fixed by #22202.
    /// </summary>
    [Test]
    public async Task RestartAsync_WhenUpgrading_RunsMigrationsSynchronously()
    {
        var currentLevel = RuntimeLevel.Run;
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);

        runtimeState
            .Setup(x => x.DetermineRuntimeLevel())
            .Callback(() => currentLevel = RuntimeLevel.Upgrading);
        runtimeState.SetupGet(x => x.Level).Returns(() => currentLevel);
        runtimeState.SetupGet(x => x.Reason).Returns(RuntimeLevelReason.UpgradePackageMigrations);

        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(eventAggregator);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.RestartAsync();

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<RuntimeUnattendedUpgradeNotification>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// After a restart runs package migrations synchronously and receives PackageMigrationComplete,
    /// DetermineRuntimeLevel is called again so the level transitions from Upgrading to Run.
    /// </summary>
    [Test]
    public async Task RestartAsync_WhenUpgrading_PackageMigrationComplete_DeterminesRuntimeLevel()
    {
        var currentLevel = RuntimeLevel.Run;
        var determineCallCount = 0;
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);

        runtimeState
            .Setup(x => x.DetermineRuntimeLevel())
            .Callback(() =>
            {
                determineCallCount++;
                currentLevel = determineCallCount switch
                {
                    1 => RuntimeLevel.Run,
                    2 => RuntimeLevel.Upgrading,
                    _ => RuntimeLevel.Run,
                };
            });
        runtimeState.SetupGet(x => x.Level).Returns(() => currentLevel);
        runtimeState.SetupGet(x => x.Reason).Returns(RuntimeLevelReason.UpgradePackageMigrations);

        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(
            eventAggregator,
            upgradeResult: RuntimeUnattendedUpgradeNotification.UpgradeResult.PackageMigrationComplete);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.RestartAsync();

        Assert.That(determineCallCount, Is.GreaterThanOrEqualTo(3));
    }

    /// <summary>
    /// RestartAsync terminates components via StopAsync (publishing UmbracoApplicationStoppingNotification),
    /// then re-initializes them, and finally publishes UmbracoApplicationStartedNotification.
    /// </summary>
    [Test]
    public async Task RestartAsync_TerminatesComponentsBeforeRestarting()
    {
        var runtimeState = CreateMockRuntimeState(RuntimeLevel.Run);
        var eventAggregator = new Mock<IEventAggregator>();
        SetupAllNotifications(eventAggregator);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.RestartAsync();

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<UmbracoApplicationStoppingNotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<UmbracoApplicationStartedNotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static CoreRuntime CreateSut(
        IRuntimeState runtimeState,
        IEventAggregator eventAggregator)
    {
        var loggerFactory = NullLoggerFactory.Instance;

        var components = new ComponentCollection(
            () => [],
            Mock.Of<IProfilingLogger>(),
            NullLogger<ComponentCollection>.Instance);

        var mainDom = new Mock<IMainDom>();

        var lifetime = new Mock<IHostApplicationLifetime>();
        lifetime.SetupGet(x => x.ApplicationStarted).Returns(CancellationToken.None);
        lifetime.SetupGet(x => x.ApplicationStopped).Returns(CancellationToken.None);
        lifetime.SetupGet(x => x.ApplicationStopping).Returns(CancellationToken.None);

        return new CoreRuntime(
            runtimeState,
            loggerFactory,
            components,
            Mock.Of<IApplicationShutdownRegistry>(),
            Mock.Of<IProfilingLogger>(),
            mainDom.Object,
            Mock.Of<IUmbracoDatabaseFactory>(),
            eventAggregator,
            Mock.Of<IHostingEnvironment>(),
            Mock.Of<IUmbracoVersion>(),
            null,
            lifetime.Object);
    }

    private static Mock<IRuntimeState> CreateMockRuntimeState(RuntimeLevel initialLevel)
    {
        var mock = new Mock<IRuntimeState>();
        var currentLevel = initialLevel;

        mock.SetupGet(x => x.Level).Returns(() => currentLevel);
        mock.SetupGet(x => x.Reason).Returns(RuntimeLevelReason.Run);
        mock.SetupGet(x => x.BootFailedException).Returns(() => null);
        mock.Setup(x => x.Configure(
                It.IsAny<RuntimeLevel>(),
                It.IsAny<RuntimeLevelReason>(),
                It.IsAny<Exception?>()))
            .Callback<RuntimeLevel, RuntimeLevelReason, Exception?>((level, _, _) => currentLevel = level);

        return mock;
    }

    private static void SetupAllNotifications(
        Mock<IEventAggregator> mock,
        RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult premigrationResult =
            RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.NotRequired,
        RuntimeUnattendedUpgradeNotification.UpgradeResult upgradeResult =
            RuntimeUnattendedUpgradeNotification.UpgradeResult.NotRequired)
    {
        mock.Setup(x => x.PublishAsync(It.IsAny<RuntimeUnattendedInstallNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<RuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()))
            .Callback<RuntimePremigrationsUpgradeNotification, CancellationToken>(
                (n, _) => n.UpgradeResult = premigrationResult)
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<PostRuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<RuntimeUnattendedUpgradeNotification>(), It.IsAny<CancellationToken>()))
            .Callback<RuntimeUnattendedUpgradeNotification, CancellationToken>(
                (n, _) => n.UnattendedUpgradeResult = upgradeResult)
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<UmbracoApplicationStartingNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<UmbracoApplicationStoppingNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<UmbracoApplicationStartedNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.PublishAsync(It.IsAny<UmbracoApplicationStoppedNotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.Publish(It.IsAny<INotification>()));
    }
}
