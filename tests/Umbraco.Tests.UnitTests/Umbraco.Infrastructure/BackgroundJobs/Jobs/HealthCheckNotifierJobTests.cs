// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

[TestFixture]
public class HealthCheckNotifierJobTests
{
    private Mock<IHealthCheckNotificationMethod> _mockNotificationMethod;

    private const string Check1Id = "00000000-0000-0000-0000-000000000001";
    private const string Check2Id = "00000000-0000-0000-0000-000000000002";
    private const string Check3Id = "00000000-0000-0000-0000-000000000003";

    [Test]
    public async Task Does_Not_Execute_When_Not_Enabled()
    {
        var sut = CreateHealthCheckNotifier(false);
        await sut.RunJobAsync();
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Does_Not_Execute_With_No_Enabled_Notification_Methods()
    {
        var sut = CreateHealthCheckNotifier(notificationEnabled: false);
        await sut.RunJobAsync();
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Executes_With_Enabled_Notification_Methods()
    {
        var sut = CreateHealthCheckNotifier();
        await sut.RunJobAsync();
        VerifyNotificationsSent();
    }

    [Test]
    public async Task Executes_Only_Enabled_Checks()
    {
        var sut = CreateHealthCheckNotifier();
        await sut.RunJobAsync();
        _mockNotificationMethod.Verify(
            x => x.SendAsync(
                It.Is<HealthCheckResults>(y =>
                    y.ResultsAsDictionary.Count == 1 && y.ResultsAsDictionary.ContainsKey("Check1"))),
            Times.Once);
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


        var mockScopeProvider = new Mock<IScopeProvider>();
        var mockLogger = new Mock<ILogger<HealthCheckNotifierJob>>();
        var mockProfilingLogger = new Mock<IProfilingLogger>();

        return new HealthCheckNotifierJob(
            new TestOptionsMonitor<HealthChecksSettings>(settings),
            checks,
            notifications,
            mockScopeProvider.Object,
            mockLogger.Object,
            mockProfilingLogger.Object,
            Mock.Of<ICronTabParser>(),
            Mock.Of<IEventAggregator>());
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

        public override async Task<IEnumerable<HealthCheckStatus>> GetStatus() => Enumerable.Empty<HealthCheckStatus>();
    }
}
