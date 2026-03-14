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
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Tests.Scheduling;

[TestFixture]
internal class ContentVersionCleanupTest
{
    /// <summary>
    /// Verifies that when content version cleanup is disabled, the cleanup service is not invoked and the job is scheduled to repeat.
    /// </summary>
    /// <param name="settings">Mock options monitor for <see cref="ContentSettings"/>.</param>
    /// <param name="mainDom">Mock implementation of <see cref="IMainDom"/>.</param>
    /// <param name="serverRoleAccessor">Mock implementation of <see cref="IServerRoleAccessor"/>.</param>
    /// <param name="runtimeState">Mock implementation of <see cref="IRuntimeState"/>.</param>
    /// <param name="cleanupService">Mock implementation of <see cref="IContentVersionService"/>.</param>
    /// <param name="sut">The <see cref="ContentVersionCleanupJob"/> instance under test.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Test]
    [AutoMoqData]
    public async Task ContentVersionCleanup_WhenNotEnabled_DoesNotCleanupWillRepeat(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> settings,
        [Frozen] Mock<IMainDom> mainDom,
        [Frozen] Mock<IServerRoleAccessor> serverRoleAccessor,
        [Frozen] Mock<IRuntimeState> runtimeState,
        [Frozen] Mock<IContentVersionService> cleanupService,
        ContentVersionCleanupJob sut)
    {
        settings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new ContentVersionCleanupPolicySettings { EnableCleanup = false },
        });
        runtimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);
        mainDom.Setup(x => x.IsMainDom).Returns(true);
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.SchedulingPublisher);

        await sut.ExecuteAsync();

        cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
    }

    /// <summary>
    /// Verifies that when content version cleanup is enabled in the settings, the <see cref="IContentVersionService"/> is invoked by the <see cref="ContentVersionCleanupJob"/>.
    /// </summary>
    /// <param name="settings">A frozen mock of <see cref="IOptionsMonitor{ContentSettings}"/> providing content settings.</param>
    /// <param name="mainDom">A frozen mock of <see cref="IMainDom"/> indicating main domain status.</param>
    /// <param name="serverRoleAccessor">A frozen mock of <see cref="IServerRoleAccessor"/> providing the current server role.</param>
    /// <param name="runtimeState">A frozen mock of <see cref="IRuntimeState"/> providing the current runtime state.</param>
    /// <param name="cleanupService">A frozen mock of <see cref="IContentVersionService"/> used to verify cleanup invocation.</param>
    /// <param name="sut">The <see cref="ContentVersionCleanupJob"/> instance under test.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    [AutoMoqData]
    public async Task ContentVersionCleanup_Enabled_DelegatesToCleanupService(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> settings,
        [Frozen] Mock<IMainDom> mainDom,
        [Frozen] Mock<IServerRoleAccessor> serverRoleAccessor,
        [Frozen] Mock<IRuntimeState> runtimeState,
        [Frozen] Mock<IContentVersionService> cleanupService,
        ContentVersionCleanupJob sut)
    {
        settings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new ContentVersionCleanupPolicySettings { EnableCleanup = true },
        });

        runtimeState.Setup(x => x.Level).Returns(RuntimeLevel.Run);
        mainDom.Setup(x => x.IsMainDom).Returns(true);
        serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.SchedulingPublisher);

        await sut.ExecuteAsync();

        cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Once);
    }
}
