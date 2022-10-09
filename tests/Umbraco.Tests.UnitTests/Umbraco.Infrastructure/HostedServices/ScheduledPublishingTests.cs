// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HostedServices;

[TestFixture]
public class ScheduledPublishingTests
{
    private Mock<IContentService> _mockContentService;
    private Mock<ILogger<ScheduledPublishing>> _mockLogger;

    [Test]
    public async Task Does_Not_Execute_When_Not_Enabled()
    {
        var sut = CreateScheduledPublishing(false);
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
    public async Task Does_Not_Execute_When_Server_Role_Is_Subscriber()
    {
        var sut = CreateScheduledPublishing(serverRole: ServerRole.Subscriber);
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

        var mockServerRegistrar = new Mock<IServerRoleAccessor>();
        mockServerRegistrar.Setup(x => x.CurrentServerRole).Returns(serverRole);

        var mockMainDom = new Mock<IMainDom>();
        mockMainDom.SetupGet(x => x.IsMainDom).Returns(isMainDom);

        _mockContentService = new Mock<IContentService>();

        var mockUmbracoContextFactory = new Mock<IUmbracoContextFactory>();
        mockUmbracoContextFactory.Setup(x => x.EnsureUmbracoContext())
            .Returns(new UmbracoContextReference(null, false, null));

        _mockLogger = new Mock<ILogger<ScheduledPublishing>>();

        var mockServerMessenger = new Mock<IServerMessenger>();

        var mockScopeProvider = new Mock<ICoreScopeProvider>();
        mockScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<IScope>());

        return new ScheduledPublishing(
            mockRunTimeState.Object,
            mockMainDom.Object,
            mockServerRegistrar.Object,
            _mockContentService.Object,
            mockUmbracoContextFactory.Object,
            _mockLogger.Object,
            mockServerMessenger.Object,
            mockScopeProvider.Object);
    }

    private void VerifyScheduledPublishingNotPerformed() => VerifyScheduledPublishingPerformed(Times.Never());

    private void VerifyScheduledPublishingPerformed() => VerifyScheduledPublishingPerformed(Times.Once());

    private void VerifyScheduledPublishingPerformed(Times times) =>
        _mockContentService.Verify(x => x.PerformScheduledPublish(It.IsAny<DateTime>()), times);
}
