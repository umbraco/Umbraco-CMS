using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Tests.Scheduling;

[TestFixture]
internal class ContentVersionCleanupTest
{
    [Test]
    [AutoMoqData]
    public async Task ContentVersionCleanup_WhenNotEnabled_DoesNotCleanupWillRepeat(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> settings,
        [Frozen] Mock<IMainDom> mainDom,
        [Frozen] Mock<IServerRoleAccessor> serverRoleAccessor,
        [Frozen] Mock<IRuntimeState> runtimeState,
        [Frozen] Mock<IContentVersionService> cleanupService,
        ContentVersionCleanup sut)
    {
        settings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new ContentVersionCleanupPolicySettings { EnableCleanup = false },
        });
        runtimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);
        mainDom.Setup(x => x.IsMainDom).Returns(true);
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.SchedulingPublisher);

        await sut.PerformExecuteAsync(null);

        cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    [AutoMoqData]
    public async Task ContentVersionCleanup_RuntimeLevelNotRun_DoesNotCleanupWillRepeat(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> settings,
        [Frozen] Mock<IMainDom> mainDom,
        [Frozen] Mock<IServerRoleAccessor> serverRoleAccessor,
        [Frozen] Mock<IRuntimeState> runtimeState,
        [Frozen] Mock<IContentVersionService> cleanupService,
        ContentVersionCleanup sut)
    {
        settings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new ContentVersionCleanupPolicySettings { EnableCleanup = true },
        });
        runtimeState.Setup(x => x.Level).Returns(RuntimeLevel.Unknown);
        mainDom.Setup(x => x.IsMainDom).Returns(true);
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.SchedulingPublisher);

        await sut.PerformExecuteAsync(null);

        cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    [AutoMoqData]
    public async Task ContentVersionCleanup_ServerRoleUnknown_DoesNotCleanupWillRepeat(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> settings,
        [Frozen] Mock<IMainDom> mainDom,
        [Frozen] Mock<IServerRoleAccessor> serverRoleAccessor,
        [Frozen] Mock<IRuntimeState> runtimeState,
        [Frozen] Mock<IContentVersionService> cleanupService,
        ContentVersionCleanup sut)
    {
        settings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new ContentVersionCleanupPolicySettings { EnableCleanup = true },
        });
        runtimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);
        mainDom.Setup(x => x.IsMainDom).Returns(true);
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.Unknown);

        await sut.PerformExecuteAsync(null);

        cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    [AutoMoqData]
    public async Task ContentVersionCleanup_NotMainDom_DoesNotCleanupWillNotRepeat(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> settings,
        [Frozen] Mock<IMainDom> mainDom,
        [Frozen] Mock<IServerRoleAccessor> serverRoleAccessor,
        [Frozen] Mock<IRuntimeState> runtimeState,
        [Frozen] Mock<IContentVersionService> cleanupService,
        ContentVersionCleanup sut)
    {
        settings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new ContentVersionCleanupPolicySettings { EnableCleanup = true },
        });

        runtimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);
        mainDom.Setup(x => x.IsMainDom).Returns(false);
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.SchedulingPublisher);

        await sut.PerformExecuteAsync(null);

        cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
    }

    [Test]
    [AutoMoqData]
    public async Task ContentVersionCleanup_Enabled_DelegatesToCleanupService(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> settings,
        [Frozen] Mock<IMainDom> mainDom,
        [Frozen] Mock<IServerRoleAccessor> serverRoleAccessor,
        [Frozen] Mock<IRuntimeState> runtimeState,
        [Frozen] Mock<IContentVersionService> cleanupService,
        ContentVersionCleanup sut)
    {
        settings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new ContentVersionCleanupPolicySettings { EnableCleanup = true },
        });

        runtimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);
        mainDom.Setup(x => x.IsMainDom).Returns(true);
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.SchedulingPublisher);

        await sut.PerformExecuteAsync(null);

        cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Once);
    }
}
