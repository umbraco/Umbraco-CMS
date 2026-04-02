// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Install;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Install;

[TestFixture]
public class UnattendedUpgradeBackgroundServiceTests
{
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.BootFailed)]
    [TestCase(RuntimeLevel.BootFailed)]
    public async Task ExecuteAsync_WhenLevelIsNotUpgrading_DoesNothing(RuntimeLevel level)
    {
        var runtimeState = CreateMockRuntimeState(level);
        var eventAggregator = new Mock<IEventAggregator>();
        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        eventAggregator.Verify(
            x => x.PublishAsync(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenAllNotificationsAreNotRequired_InitializesComponentsAndRegistersLifetimeCallbacks()
    {
        // Simulates the multi-instance scenario where a second instance finds no migrations to run
        // (because the first instance already completed them). Both notifications return NotRequired,
        // but DetermineRuntimeLevel() must still be called so the level transitions from Upgrading to Run.
        var runtimeState = CreateMockRuntimeState();
        var eventAggregator = new Mock<IEventAggregator>();
        SetupPublishAsync(eventAggregator);

        var lifetime = CreateMockLifetime();
        var sut = CreateSut(runtimeState.Object, eventAggregator.Object, hostApplicationLifetime: lifetime.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        // All three notification steps must have fired.
        eventAggregator.Verify(x => x.PublishAsync(It.IsAny<RuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        eventAggregator.Verify(x => x.PublishAsync(It.IsAny<PostRuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        eventAggregator.Verify(x => x.PublishAsync(It.IsAny<RuntimeUnattendedUpgradeNotification>(), It.IsAny<CancellationToken>()), Times.Once);
        eventAggregator.Verify(x => x.PublishAsync(It.IsAny<UmbracoApplicationStartingNotification>(), It.IsAny<CancellationToken>()), Times.Once);

        // DetermineRuntimeLevel must be called unconditionally after migrations complete.
        runtimeState.Verify(x => x.DetermineRuntimeLevel(), Times.Once);

        // BootFailed must NOT be set.
        runtimeState.Verify(x => x.Configure(RuntimeLevel.BootFailed, It.IsAny<RuntimeLevelReason>(), It.IsAny<Exception?>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenPremigrationsReturnsCorUpgradeComplete_CallsDetermineRuntimeLevel()
    {
        var runtimeState = CreateMockRuntimeState();
        var eventAggregator = new Mock<IEventAggregator>();

        SetupPublishAsync(eventAggregator, premigrationResult: RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.CoreUpgradeComplete);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        runtimeState.Verify(x => x.DetermineRuntimeLevel(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenPremigrationsHasErrorsAndBootFailedExceptionExists_SetsBootFailed()
    {
        var bootEx = new BootFailedException("db error");
        var runtimeState = CreateMockRuntimeState(initialBootFailedException: bootEx);
        var eventAggregator = new Mock<IEventAggregator>();

        SetupPublishAsync(eventAggregator, premigrationResult: RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.HasErrors);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        runtimeState.Verify(x => x.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, null), Times.Once);

        // Execution stops after premigrations: post-premigrations and main upgrade must NOT fire.
        eventAggregator.Verify(x => x.PublishAsync(It.IsAny<PostRuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()), Times.Never);
        eventAggregator.Verify(x => x.PublishAsync(It.IsAny<RuntimeUnattendedUpgradeNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenPremigrationsHasErrorsAndBootFailedExceptionIsNull_SetsBootFailed()
    {
        var runtimeState = CreateMockRuntimeState(); // BootFailedException starts null
        var eventAggregator = new Mock<IEventAggregator>();

        SetupPublishAsync(eventAggregator, premigrationResult: RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.HasErrors);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        // The InvalidOperationException thrown inside RunMigrationsAsync is caught by ExecuteAsync,
        // which then calls Configure with the caught exception.
        runtimeState.Verify(
            x => x.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, It.IsNotNull<Exception>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenUnattendedUpgradeHasErrorsAndBootFailedExceptionExists_SetsBootFailed()
    {
        var bootEx = new BootFailedException("migration error");
        var runtimeState = CreateMockRuntimeState(initialBootFailedException: bootEx);
        var eventAggregator = new Mock<IEventAggregator>();

        SetupPublishAsync(eventAggregator, upgradeResult: RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        runtimeState.Verify(x => x.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, null), Times.Once);

        // Components must NOT be initialized.
        eventAggregator.Verify(x => x.PublishAsync(It.IsAny<UmbracoApplicationStartingNotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase(RuntimeUnattendedUpgradeNotification.UpgradeResult.CoreUpgradeComplete)]
    [TestCase(RuntimeUnattendedUpgradeNotification.UpgradeResult.PackageMigrationComplete)]
    public async Task ExecuteAsync_WhenUnattendedUpgradeSucceeds_CallsDetermineRuntimeLevel(
        RuntimeUnattendedUpgradeNotification.UpgradeResult result)
    {
        var runtimeState = CreateMockRuntimeState();
        var eventAggregator = new Mock<IEventAggregator>();

        SetupPublishAsync(eventAggregator, upgradeResult: result);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        runtimeState.Verify(x => x.DetermineRuntimeLevel(), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenPublishAsyncThrows_SetsBootFailed()
    {
        var runtimeState = CreateMockRuntimeState();
        var eventAggregator = new Mock<IEventAggregator>();

        eventAggregator
            .Setup(x => x.PublishAsync(It.IsAny<RuntimePremigrationsUpgradeNotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("db gone"));

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        runtimeState.Verify(
            x => x.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, It.IsNotNull<Exception>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_WhenBootFailedExceptionAlreadySetBeforeDetermineRuntimeLevel_SkipsDetermineRuntimeLevel()
    {
        // The BootFailedException is set before CoreRuntime calls DetermineRuntimeLevel.
        // The guard in UnattendedUpgradeBackgroundService.DetermineRuntimeLevel() should return early.
        var bootEx = new BootFailedException("pre-existing error");
        var runtimeState = CreateMockRuntimeState(
            initialBootFailedException: bootEx);

        var eventAggregator = new Mock<IEventAggregator>();
        SetupPublishAsync(
            eventAggregator,
            premigrationResult: RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.CoreUpgradeComplete);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        // DetermineRuntimeLevel on the state object must NOT be called because the guard returned early.
        runtimeState.Verify(x => x.DetermineRuntimeLevel(), Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenDetermineRuntimeLevelThrows_SetsBootFailed()
    {
        var runtimeState = CreateMockRuntimeState();
        runtimeState
            .Setup(x => x.DetermineRuntimeLevel())
            .Throws(new InvalidOperationException("db gone"));

        var eventAggregator = new Mock<IEventAggregator>();
        SetupPublishAsync(
            eventAggregator,
            premigrationResult: RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.CoreUpgradeComplete);

        var sut = CreateSut(runtimeState.Object, eventAggregator.Object);

        await sut.StartAsync(CancellationToken.None);
        await sut.ExecuteTask!;

        runtimeState.Verify(
            x => x.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException, It.IsNotNull<Exception>()),
            Times.Once);
    }

    private static UnattendedUpgradeBackgroundService CreateSut(
        IRuntimeState runtimeState,
        IEventAggregator eventAggregator,
        IHostApplicationLifetime? hostApplicationLifetime = null)
    {
        var components = new ComponentCollection(
            () => Enumerable.Empty<IAsyncComponent>(),
            Mock.Of<IProfilingLogger>(),
            NullLogger<ComponentCollection>.Instance);

        return new UnattendedUpgradeBackgroundService(
            runtimeState,
            eventAggregator,
            components,
            hostApplicationLifetime ?? CreateMockLifetime().Object,
            NullLogger<UnattendedUpgradeBackgroundService>.Instance);
    }

    private static Mock<IRuntimeState> CreateMockRuntimeState(
        RuntimeLevel initialLevel = RuntimeLevel.Upgrading,
        BootFailedException? initialBootFailedException = null)
    {
        var mock = new Mock<IRuntimeState>();
        var currentLevel = initialLevel;
        var bootFailedException = initialBootFailedException;

        mock.SetupGet(x => x.Level).Returns(() => currentLevel);
        mock.SetupGet(x => x.BootFailedException).Returns(() => bootFailedException);
        mock.Setup(x => x.Configure(
                It.IsAny<RuntimeLevel>(),
                It.IsAny<RuntimeLevelReason>(),
                It.IsAny<Exception?>()))
            .Callback<RuntimeLevel, RuntimeLevelReason, Exception?>((level, _, ex) =>
            {
                currentLevel = level;
                if (ex is not null)
                {
                    bootFailedException = new BootFailedException(ex.Message, ex);
                }
            });

        return mock;
    }

    private static Mock<IHostApplicationLifetime> CreateMockLifetime()
    {
        var mock = new Mock<IHostApplicationLifetime>();
        mock.SetupGet(x => x.ApplicationStarted).Returns(CancellationToken.None);
        mock.SetupGet(x => x.ApplicationStopped).Returns(CancellationToken.None);
        return mock;
    }

    /// <summary>
    /// Sets up IEventAggregator.PublishAsync to complete successfully for all notification types
    /// used by <see cref="UnattendedUpgradeBackgroundService"/>.
    /// Optional parameters control the result written back on to the mutable notification objects.
    /// </summary>
    private static void SetupPublishAsync(
        Mock<IEventAggregator> mock,
        RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult premigrationResult =
            RuntimePremigrationsUpgradeNotification.PremigrationUpgradeResult.NotRequired,
        RuntimeUnattendedUpgradeNotification.UpgradeResult upgradeResult =
            RuntimeUnattendedUpgradeNotification.UpgradeResult.NotRequired)
    {
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
    }
}
