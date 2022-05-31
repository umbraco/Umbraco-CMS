// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices;

[TestFixture]
public class HealthCheckNotifierTests
{
    private Mock<IHealthCheckNotificationMethod> _mockNotificationMethod;

    private const string Check1Id = "00000000-0000-0000-0000-000000000001";
    private const string Check2Id = "00000000-0000-0000-0000-000000000002";
    private const string Check3Id = "00000000-0000-0000-0000-000000000003";

    [Test]
    public async Task Does_Not_Execute_When_Not_Enabled()
    {
        var sut = CreateHealthCheckNotifier(false);
        await sut.PerformExecuteAsync(null);
        VerifyNotificationsNotSent();
    }

    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Unknown)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.BootFailed)]
    public async Task Does_Not_Execute_When_Runtime_State_Is_Not_Run(RuntimeLevel runtimeLevel)
    {
        var sut = CreateHealthCheckNotifier(runtimeLevel: runtimeLevel);
        await sut.PerformExecuteAsync(null);
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Does_Not_Execute_When_Server_Role_Is_Subscriber()
    {
        var sut = CreateHealthCheckNotifier(serverRole: ServerRole.Subscriber);
        await sut.PerformExecuteAsync(null);
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Does_Not_Execute_When_Server_Role_Is_Unknown()
    {
        var sut = CreateHealthCheckNotifier(serverRole: ServerRole.Unknown);
        await sut.PerformExecuteAsync(null);
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Does_Not_Execute_When_Not_Main_Dom()
    {
        var sut = CreateHealthCheckNotifier(isMainDom: false);
        await sut.PerformExecuteAsync(null);
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Does_Not_Execute_With_No_Enabled_Notification_Methods()
    {
        var sut = CreateHealthCheckNotifier(notificationEnabled: false);
        await sut.PerformExecuteAsync(null);
        VerifyNotificationsNotSent();
    }

    [Test]
    public async Task Executes_With_Enabled_Notification_Methods()
    {
        var sut = CreateHealthCheckNotifier();
        await sut.PerformExecuteAsync(null);
        VerifyNotificationsSent();
    }

    [Test]
    public async Task Executes_Only_Enabled_Checks()
    {
        var sut = CreateHealthCheckNotifier();
        await sut.PerformExecuteAsync(null);
        _mockNotificationMethod.Verify(
            x => x.SendAsync(
                It.Is<HealthCheckResults>(y =>
                    y.ResultsAsDictionary.Count == 1 && y.ResultsAsDictionary.ContainsKey("Check1"))),
            Times.Once);
    }

    private HealthCheckNotifier CreateHealthCheckNotifier(
        bool enabled = true,
        RuntimeLevel runtimeLevel = RuntimeLevel.Run,
        ServerRole serverRole = ServerRole.Single,
        bool isMainDom = true,
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

        var mockRunTimeState = new Mock<IRuntimeState>();
        mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

        var mockServerRegistrar = new Mock<IServerRoleAccessor>();
        mockServerRegistrar.Setup(x => x.CurrentServerRole).Returns(serverRole);

        var mockMainDom = new Mock<IMainDom>();
        mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

        var mockScopeProvider = new Mock<IScopeProvider>();
        var mockLogger = new Mock<ILogger<HealthCheckNotifier>>();
        var mockProfilingLogger = new Mock<IProfilingLogger>();

        return new HealthCheckNotifier(
            new TestOptionsMonitor<HealthChecksSettings>(settings),
            checks,
            notifications,
            mockRunTimeState.Object,
            mockServerRegistrar.Object,
            mockMainDom.Object,
            mockScopeProvider.Object,
            mockLogger.Object,
            mockProfilingLogger.Object,
            Mock.Of<ICronTabParser>());
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
