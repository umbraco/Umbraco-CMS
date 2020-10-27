using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.HealthCheck.Checks;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Sync;
using Umbraco.Infrastructure.HealthCheck;
using Umbraco.Infrastructure.HostedServices;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.HealthCheck.NotificationMethods;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.HostedServices
{
    [TestFixture]
    public class HealthCheckNotifierTests
    {
        private Mock<IHealthCheckNotificationMethod> _mockNotificationMethod;

        private const string Check1Id = "00000000-0000-0000-0000-000000000001";
        private const string Check2Id = "00000000-0000-0000-0000-000000000002";
        private const string Check3Id = "00000000-0000-0000-0000-000000000003";

        [Test]
        public void Does_Not_Execute_When_Not_Enabled()
        {
            var sut = CreateHealthCheckNotifier(enabled: false);
            sut.ExecuteAsync(null);
            _mockNotificationMethod.Verify(x => x.SendAsync(It.IsAny<HealthCheckResults>()), Times.Never);
        }

        [Test]
        public void Does_Not_Execute_When_Runtime_State_Is_Not_Run()
        {
            var sut = CreateHealthCheckNotifier(runtimeLevel: RuntimeLevel.Boot);
            sut.ExecuteAsync(null);
            _mockNotificationMethod.Verify(x => x.SendAsync(It.IsAny<HealthCheckResults>()), Times.Never);
        }

        [Test]
        public void Does_Not_Execute_When_Server_Role_Is_Replica()
        {
            var sut = CreateHealthCheckNotifier(serverRole: ServerRole.Replica);
            sut.ExecuteAsync(null);
            _mockNotificationMethod.Verify(x => x.SendAsync(It.IsAny<HealthCheckResults>()), Times.Never);
        }

        [Test]
        public void Does_Not_Execute_When_Server_Role_Is_Unknown()
        {
            var sut = CreateHealthCheckNotifier(serverRole: ServerRole.Unknown);
            sut.ExecuteAsync(null);
            _mockNotificationMethod.Verify(x => x.SendAsync(It.IsAny<HealthCheckResults>()), Times.Never);
        }

        [Test]
        public void Does_Not_Execute_When_Not_Main_Dom()
        {
            var sut = CreateHealthCheckNotifier(isMainDom: false);
            sut.ExecuteAsync(null);
            _mockNotificationMethod.Verify(x => x.SendAsync(It.IsAny<HealthCheckResults>()), Times.Never);
        }

        [Test]
        public void Does_Not_Execute_With_No_Enabled_Notification_Methods()
        {
            var sut = CreateHealthCheckNotifier(notificationEnabled: false);
            sut.ExecuteAsync(null);
            _mockNotificationMethod.Verify(x => x.SendAsync(It.IsAny<HealthCheckResults>()), Times.Never);
        }

        [Test]
        public void Executes_With_Enabled_Notification_Methods()
        {
            var sut = CreateHealthCheckNotifier();
            sut.ExecuteAsync(null);
            _mockNotificationMethod.Verify(x => x.SendAsync(It.IsAny<HealthCheckResults>()), Times.Once);
        }

        [Test]
        public void Executes_Only_Enabled_Checks()
        {
            var sut = CreateHealthCheckNotifier();
            sut.ExecuteAsync(null);
            _mockNotificationMethod.Verify(x => x.SendAsync(It.Is<HealthCheckResults>(
                y => y.ResultsAsDictionary.Count == 1 && y.ResultsAsDictionary.ContainsKey("Check1"))), Times.Once);
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
                Notification = new HealthChecksSettings.HealthCheckNotificationSettings
                {
                    Enabled = enabled,
                    DisabledChecks = new List<DisabledHealthCheck>
                {
                    new DisabledHealthCheck { Id = Guid.Parse(Check3Id) }
                }
                },
                DisabledChecks = new List<DisabledHealthCheck>
                {
                    new DisabledHealthCheck { Id = Guid.Parse(Check2Id) }
                }
            };
            var checks = new HealthCheckCollection(new List<HealthCheck>
            {
                new TestHealthCheck1(),
                new TestHealthCheck2(),
                new TestHealthCheck3(),
            });

            _mockNotificationMethod = new Mock<IHealthCheckNotificationMethod>();
            _mockNotificationMethod.SetupGet(x => x.Enabled).Returns(notificationEnabled);
            var notifications = new HealthCheckNotificationMethodCollection(new List<IHealthCheckNotificationMethod> { _mockNotificationMethod.Object });

            var mockRunTimeState = new Mock<IRuntimeState>();
            mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

            var mockServerRegistrar = new Mock<IServerRegistrar>();
            mockServerRegistrar.Setup(x => x.GetCurrentServerRole()).Returns(serverRole);

            var mockMainDom = new Mock<IMainDom>();
            mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

            var mockScopeProvider = new Mock<IScopeProvider>();
            var mockLogger = new Mock<ILogger<HealthCheckNotifier>>();
            var mockProfilingLogger = new Mock<IProfilingLogger>();

            return new HealthCheckNotifier(Options.Create(settings), checks, notifications,
                mockRunTimeState.Object, mockServerRegistrar.Object, mockMainDom.Object, mockScopeProvider.Object,
                mockLogger.Object, mockProfilingLogger.Object);
        }

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
            public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
            {
                return new HealthCheckStatus("Check message");
            }

            public override IEnumerable<HealthCheckStatus> GetStatus()
            {
                return Enumerable.Empty<HealthCheckStatus>();
            }
        }
    }
}
