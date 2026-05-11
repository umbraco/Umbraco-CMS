// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Install;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Install;

[TestFixture]
public class MigrationCoordinatorTests
{
    private const string TestMachineIdentifier = "test-machine";
    private const string OtherMachineIdentifier = "other-machine";

    private Mock<ICoreScopeProvider> _scopeProviderMock = null!;
    private Mock<IKeyValueService> _keyValueServiceMock = null!;
    private Mock<IRuntimeState> _runtimeStateMock = null!;

    [SetUp]
    public void SetUp()
    {
        _scopeProviderMock = new Mock<ICoreScopeProvider>();
        _keyValueServiceMock = new Mock<IKeyValueService>();
        _runtimeStateMock = new Mock<IRuntimeState>();

        SetupScopeProviderMock();
    }

    // TryBecomeLeaderAsync — claim acquisition

    [Test]
    public async Task TryBecomeLeaderAsync_WhenClaimKeyIsEmpty_ClaimsLeadershipAndReturnsTrue()
    {
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns((string?)null);

        var sut = CreateSut();
        var result = await sut.TryBecomeLeaderAsync(CancellationToken.None);

        Assert.IsTrue(result);
        _keyValueServiceMock.Verify(
            x => x.SetValue(
                Constants.Conventions.Migrations.UpgradeLockKey,
                It.Is<string>(v => v.StartsWith(TestMachineIdentifier + "|"))),
            Times.Once);
    }

    [Test]
    public async Task TryBecomeLeaderAsync_WhenClaimIsStale_ClaimsLeadershipAndReturnsTrue()
    {
        var staleTimestamp = DateTimeOffset.UtcNow.AddHours(-3).ToString("O");
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns($"{OtherMachineIdentifier}|{staleTimestamp}");

        var sut = CreateSut(claimTimeout: TimeSpan.FromHours(2));
        var result = await sut.TryBecomeLeaderAsync(CancellationToken.None);

        Assert.IsTrue(result);
        _keyValueServiceMock.Verify(
            x => x.SetValue(
                Constants.Conventions.Migrations.UpgradeLockKey,
                It.Is<string>(v => v.StartsWith(TestMachineIdentifier + "|"))),
            Times.Once);
    }

    [Test]
    public async Task TryBecomeLeaderAsync_WhenClaimBelongsToSameMachine_ReclaimsAndReturnsTrue()
    {
        var recentTimestamp = DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns($"{TestMachineIdentifier}|{recentTimestamp}");

        var sut = CreateSut();
        var result = await sut.TryBecomeLeaderAsync(CancellationToken.None);

        Assert.IsTrue(result);
        _keyValueServiceMock.Verify(
            x => x.SetValue(
                Constants.Conventions.Migrations.UpgradeLockKey,
                It.Is<string>(v => v.StartsWith(TestMachineIdentifier + "|"))),
            Times.Once);
    }

    // TryBecomeLeaderAsync — follower path

    [Test]
    public async Task TryBecomeLeaderAsync_WhenOtherMachineHoldsClaim_PollsUntilRunLevelAndReturnsFalse()
    {
        var recentTimestamp = DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns($"{OtherMachineIdentifier}|{recentTimestamp}");

        _runtimeStateMock.SetupGet(x => x.Level).Returns(RuntimeLevel.Upgrading);
        _runtimeStateMock
            .Setup(x => x.DetermineRuntimeLevel())
            .Callback(() => _runtimeStateMock.SetupGet(x => x.Level).Returns(RuntimeLevel.Run));

        var sut = CreateSut();
        var result = await sut.TryBecomeLeaderAsync(CancellationToken.None);

        Assert.IsFalse(result);
    }

    [Test]
    public async Task TryBecomeLeaderAsync_WhenOtherMachineHoldsClaim_PollsUntilBootFailedAndReturnsFalse()
    {
        var recentTimestamp = DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns($"{OtherMachineIdentifier}|{recentTimestamp}");

        _runtimeStateMock.SetupGet(x => x.Level).Returns(RuntimeLevel.Upgrading);
        _runtimeStateMock
            .Setup(x => x.DetermineRuntimeLevel())
            .Callback(() => _runtimeStateMock.SetupGet(x => x.Level).Returns(RuntimeLevel.BootFailed));

        var sut = CreateSut();
        var result = await sut.TryBecomeLeaderAsync(CancellationToken.None);

        Assert.IsFalse(result);
    }

    [Test]
    public async Task TryBecomeLeaderAsync_WhenDetermineRuntimeLevelThrows_LogsWarningAndReturnsFalseWhenRunDetected()
    {
        var recentTimestamp = DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns($"{OtherMachineIdentifier}|{recentTimestamp}");

        // Level returns Run so the switch exits without sleeping; DetermineRuntimeLevel still throws.
        _runtimeStateMock.SetupGet(x => x.Level).Returns(RuntimeLevel.Run);
        _runtimeStateMock
            .Setup(x => x.DetermineRuntimeLevel())
            .Throws(new InvalidOperationException("db gone"));

        var sut = CreateSut();
        var result = await sut.TryBecomeLeaderAsync(CancellationToken.None);

        Assert.IsFalse(result);
    }

    // TryBecomeLeaderAsync — cancellation

    [Test]
    public async Task TryBecomeLeaderAsync_WhenCancelledBeforeFirstIteration_ReturnsFalse()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var sut = CreateSut();
        var result = await sut.TryBecomeLeaderAsync(cts.Token);

        Assert.IsFalse(result);
        _scopeProviderMock.Verify(
            x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    // ReleaseLeadership

    [Test]
    public async Task ReleaseLeadership_WhenClaimMatchesDatabaseValue_ClearsKey()
    {
        string? capturedClaim = null;
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns(() => capturedClaim);
        _keyValueServiceMock
            .Setup(x => x.SetValue(Constants.Conventions.Migrations.UpgradeLockKey, It.IsAny<string>()))
            .Callback<string, string>((_, v) => capturedClaim = v);

        var sut = CreateSut();
        await sut.TryBecomeLeaderAsync(CancellationToken.None);

        sut.ReleaseLeadership();

        _keyValueServiceMock.Verify(
            x => x.SetValue(Constants.Conventions.Migrations.UpgradeLockKey, string.Empty),
            Times.Once);
    }

    [Test]
    public async Task ReleaseLeadership_WhenDatabaseValueDiffersFromLeaderClaim_DoesNotClearKey()
    {
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns((string?)null);

        var sut = CreateSut();
        await sut.TryBecomeLeaderAsync(CancellationToken.None);

        // Another server has since taken over the claim.
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns($"{OtherMachineIdentifier}|{DateTimeOffset.UtcNow:O}");

        sut.ReleaseLeadership();

        _keyValueServiceMock.Verify(
            x => x.SetValue(Constants.Conventions.Migrations.UpgradeLockKey, string.Empty),
            Times.Never);
    }

    [Test]
    public void ReleaseLeadership_WhenCalledBeforeTryBecomeLeaderAsync_DoesNothing()
    {
        var sut = CreateSut();
        sut.ReleaseLeadership();

        _scopeProviderMock.Verify(
            x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    [Test]
    public async Task ReleaseLeadership_WhenCalledTwice_IsIdempotent()
    {
        string? capturedClaim = null;
        _keyValueServiceMock
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UpgradeLockKey))
            .Returns(() => capturedClaim);
        _keyValueServiceMock
            .Setup(x => x.SetValue(Constants.Conventions.Migrations.UpgradeLockKey, It.IsAny<string>()))
            .Callback<string, string>((_, v) => capturedClaim = v);

        var sut = CreateSut();
        await sut.TryBecomeLeaderAsync(CancellationToken.None);

        sut.ReleaseLeadership(); // Clears claim, sets _leaderClaim = null.
        sut.ReleaseLeadership(); // _leaderClaim is null — returns immediately, no scope created.

        // TryBecomeLeaderAsync: 1 scope, first ReleaseLeadership: 1 scope, second: 0 scopes.
        _scopeProviderMock.Verify(
            x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Exactly(2));
    }

    private MigrationCoordinator CreateSut(
        string machineIdentifier = TestMachineIdentifier,
        TimeSpan? claimTimeout = null)
    {
        var machineInfoFactory = Mock.Of<IMachineInfoFactory>(
            f => f.GetMachineIdentifier() == machineIdentifier);

        var settings = Options.Create(new UnattendedSettings
        {
            MigrationClaimTimeout = claimTimeout ?? TimeSpan.FromMinutes(5),
        });

        return new MigrationCoordinator(
            _scopeProviderMock.Object,
            _keyValueServiceMock.Object,
            _runtimeStateMock.Object,
            machineInfoFactory,
            settings,
            NullLogger<MigrationCoordinator>.Instance);
    }

    private void SetupScopeProviderMock() =>
        _scopeProviderMock
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());
}
