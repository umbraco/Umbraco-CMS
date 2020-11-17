using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Infrastructure.HostedServices;
using Umbraco.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Umbraco.Tests.UnitTests.Umbraco.Infrastructure.HostedServices
{
    [TestFixture]
    public class ScheduledPublishingTests
    {
        private Mock<IContentService> _mockContentService;
        private Mock<ILogger<ScheduledPublishing>> _mockLogger;

        [Test]
        public async Task Does_Not_Execute_When_Not_Enabled()
        {
            var sut = CreateScheduledPublishing(enabled: false);
            await sut.PerformExecuteAsync(null);
            VerifyScheduledPublishingNotPerformed();
        }

        [TestCase(RuntimeLevel.Boot)]
        [TestCase(RuntimeLevel.Install)]
        [TestCase(RuntimeLevel.Unknown)]
        [TestCase(RuntimeLevel.Upgrade)]
        [TestCase(RuntimeLevel.BootFailed)]
        public async Task Does_Not_Execute_When_Runtime_State_Is_Not_Run(RuntimeLevel runtimeLevel)
        {
            var sut = CreateScheduledPublishing(runtimeLevel: runtimeLevel);
            await sut.PerformExecuteAsync(null);
            VerifyScheduledPublishingNotPerformed();
        }

        [Test]
        public async Task Does_Not_Execute_When_Server_Role_Is_Replica()
        {
            var sut = CreateScheduledPublishing(serverRole: ServerRole.Replica);
            await sut.PerformExecuteAsync(null);
            VerifyScheduledPublishingNotPerformed();
        }

        [Test]
        public async Task Does_Not_Execute_When_Server_Role_Is_Unknown()
        {
            var sut = CreateScheduledPublishing(serverRole: ServerRole.Unknown);
            await sut.PerformExecuteAsync(null);
            VerifyScheduledPublishingNotPerformed();
        }

        [Test]
        public async Task Does_Not_Execute_When_Not_Main_Dom()
        {
            var sut = CreateScheduledPublishing(isMainDom: false);
            await sut.PerformExecuteAsync(null);
            VerifyScheduledPublishingNotPerformed();
        }

        [Test]
        public async Task Executes_And_Performs_Scheduled_Publishing()
        {
            var sut = CreateScheduledPublishing();
            await sut.PerformExecuteAsync(null);
            VerifyScheduledPublishingPerformed();
        }

        private ScheduledPublishing CreateScheduledPublishing(
            bool enabled = true,
            RuntimeLevel runtimeLevel = RuntimeLevel.Run,
            ServerRole serverRole = ServerRole.Single,
            bool isMainDom = true)
        {
            if (enabled)
            {
                Suspendable.ScheduledPublishing.Resume();
            }
            else
            {
                Suspendable.ScheduledPublishing.Suspend();
            }

            var mockRunTimeState = new Mock<IRuntimeState>();
            mockRunTimeState.SetupGet(x => x.Level).Returns(runtimeLevel);

            var mockServerRegistrar = new Mock<IServerRegistrar>();
            mockServerRegistrar.Setup(x => x.GetCurrentServerRole()).Returns(serverRole);

            var mockMainDom = new Mock<IMainDom>();
            mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

            _mockContentService = new Mock<IContentService>();

            var mockUmbracoContextFactory = new Mock<IUmbracoContextFactory>();
            mockUmbracoContextFactory.Setup(x => x.EnsureUmbracoContext()).Returns(new UmbracoContextReference(null, false, null));

            _mockLogger = new Mock<ILogger<ScheduledPublishing>>();

            var mockServerMessenger = new Mock<IServerMessenger>();

            var mockBackOfficeSecurityFactory = new Mock<IBackofficeSecurityFactory>();

            return new ScheduledPublishing(mockRunTimeState.Object, mockMainDom.Object, mockServerRegistrar.Object, _mockContentService.Object,
                mockUmbracoContextFactory.Object, _mockLogger.Object, mockServerMessenger.Object, mockBackOfficeSecurityFactory.Object);
        }

        private void VerifyScheduledPublishingNotPerformed()
        {
            VerifyScheduledPublishingPerformed(Times.Never());
        }

        private void VerifyScheduledPublishingPerformed()
        {
            VerifyScheduledPublishingPerformed(Times.Once());
        }

        private void VerifyScheduledPublishingPerformed(Times times)
        {
            _mockContentService.Verify(x => x.PerformScheduledPublish(It.IsAny<DateTime>()), times);
        }
    }
}
