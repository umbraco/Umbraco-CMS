// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

[TestFixture]
public class HealthCheckNotifierJobTests
{
    private Mock<IHealthCheckNotificationMethod> _mockNotificationMethod;
    private Mock<IEventAggregator> _mockEventAggregator;
    private bool _scopeDisposed;

    private const string Check1Id = "00000000-0000-0000-0000-000000000001";
    private const string Check2Id = "00000000-0000-0000-0000-000000000002";
    private const string Check3Id = "00000000-0000-0000-0000-000000000003";

    [Test]
    public async Task Does_Not_Execute_When_Not_Enabled()
    {
        var sut = CreateHealthCheckNotifier(false);
        await sut.ExecuteAsync();
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Does_Not_Execute_With_No_Enabled_Notification_Methods()
    {
        var sut = CreateHealthCheckNotifier(notificationEnabled: false);
        await sut.ExecuteAsync();
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Executes_With_Enabled_Notification_Methods()
    {
        var sut = CreateHealthCheckNotifier();
        await sut.ExecuteAsync();
        VerifyNotificationsSent();
    }

    [Test]
    public async Task Executes_Only_Enabled_Checks()
    {
        var sut = CreateHealthCheckNotifier();
        await sut.ExecuteAsync();
        _mockNotificationMethod.Verify(
            x => x.SendAsync(
                It.Is<HealthCheckResults>(y =>
                    y.ResultsAsDictionary.Count == 1 && y.ResultsAsDictionary.ContainsKey("Check1"))),
            Times.Once);
    }

    [Test]
    public async Task Publishes_And_Sends_Results_After_The_Checks_Scope_Has_Ended()
    {
        var sut = CreateHealthCheckNotifier();
        bool? publishedAfterScopeEnded = null;
        bool? sentAfterScopeEnded = null;
        _mockEventAggregator
            .Setup(x => x.Publish(It.IsAny<HealthCheckCompletedNotification>()))
            .Callback(() => publishedAfterScopeEnded = _scopeDisposed);
        _mockNotificationMethod
            .Setup(x => x.SendAsync(It.IsAny<HealthCheckResults>()))
            .Callback(() => sentAfterScopeEnded = _scopeDisposed)
            .Returns(Task.CompletedTask);

        await sut.ExecuteAsync();

        Assert.IsTrue(publishedAfterScopeEnded);
        Assert.IsTrue(sentAfterScopeEnded);
    }

    private HealthCheckNotifierJob CreateHealthCheckNotifier(
        bool enabled = true,
        bool notificationEnabled = true)
    {
        var settings = new HealthChecksSettings
        {
            Notification = new HealthChecksNotificationSettings
            {
                Enabled = enabled,
                DisabledChecks = new List<DisabledHealthCheckSettings> { new() { Id = Guid.Parse(Check3Id) } },
            },
            DisabledChecks = new List<DisabledHealthCheckSettings> { new() { Id = Guid.Parse(Check2Id) } },
        };
        var checks = new HealthCheckCollection(() => new List<HealthCheck>
        {
            new TestHealthCheck1(),
            new TestHealthCheck2(),
            new TestHealthCheck3(),
        });

        _mockNotificationMethod = new Mock<IHealthCheckNotificationMethod>();
        _mockNotificationMethod.SetupGet(x => x.Enabled).Returns(notificationEnabled);
        var notifications = new HealthCheckNotificationMethodCollection(() =>
            new List<IHealthCheckNotificationMethod> { _mockNotificationMethod.Object });


        _scopeDisposed = false;
        var mockScope = new Mock<ICoreScope>();
        mockScope.Setup(x => x.Dispose()).Callback(() => _scopeDisposed = true);
        var mockScopeProvider = new Mock<ICoreScopeProvider>();
        mockScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(mockScope.Object);
        var mockProfilingLogger = new Mock<IProfilingLogger>();
        _mockEventAggregator = new Mock<IEventAggregator>();

        return new HealthCheckNotifierJob(
            new TestOptionsMonitor<HealthChecksSettings>(settings),
            checks,
            notifications,
            mockScopeProvider.Object,
            mockProfilingLogger.Object,
            _mockEventAggregator.Object);
    }

    private void VerifyNotificationsNotSent() => VerifyNotificationsSentTimes(Times.Never());

    private void VerifyNotificationsSent() => VerifyNotificationsSentTimes(Times.Once());

    private void VerifyNotificationsSentTimes(Times times) =>
        _mockNotificationMethod.Verify(x => x.SendAsync(It.IsAny<HealthCheckResults>()), times);

    [HealthCheck(Check1Id, "Check1")]
    private class TestHealthCheck1 : TestHealthCheck
    {
    }

    [HealthCheck(Check2Id, "Check2")]
    private class TestHealthCheck2 : TestHealthCheck
    {
    }

    [HealthCheck(Check3Id, "Check3")]
    private class TestHealthCheck3 : TestHealthCheck
    {
    }

    private class TestHealthCheck : HealthCheck
    {
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action) => new("Check message");

        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync() => Task.FromResult(Enumerable.Empty<HealthCheckStatus>());
    }
}
