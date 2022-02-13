using System;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Tests.Testing;
using Umbraco.Web.Scheduling;

namespace Umbraco.Tests.Scheduling
{
    [TestFixture]
    class ContentVersionCleanup_Tests_UnitTests
    {
        [Test, AutoMoqData]
        public void ContentVersionCleanup_WhenNotEnabled_DoesNotCleanupWillRepeat(
            [Frozen] Mock<IContentVersionCleanupPolicyGlobalSettings> settings,
            [Frozen] Mock<IRuntimeState> state,
            [Frozen] Mock<IContentVersionService> cleanupService,
            ContentVersionCleanup sut)
        {
            settings.Setup(x => x.EnableCleanup).Returns(false);

            state.Setup(x => x.Level).Returns(RuntimeLevel.Run);
            state.Setup(x => x.IsMainDom).Returns(true);
            state.Setup(x => x.ServerRole).Returns(ServerRole.Master);

            var result = sut.PerformRun();

            Assert.Multiple(() =>
            {
                Assert.False(result);
                cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
            });
        }

        [Test, AutoMoqData]
        public void ContentVersionCleanup_RuntimeLevelNotRun_DoesNotCleanupWillRepeat(
            [Frozen] Mock<IContentVersionCleanupPolicyGlobalSettings> settings,
            [Frozen] Mock<IRuntimeState> state,
            [Frozen] Mock<IContentVersionService> cleanupService,
            ContentVersionCleanup sut)
        {
            settings.Setup(x => x.EnableCleanup).Returns(true);

            state.Setup(x => x.Level).Returns(RuntimeLevel.Unknown);
            state.Setup(x => x.IsMainDom).Returns(true);
            state.Setup(x => x.ServerRole).Returns(ServerRole.Master);

            var result = sut.PerformRun();

            Assert.Multiple(() =>
            {
                Assert.True(result);
                cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
            });
        }

        [Test, AutoMoqData]
        public void ContentVersionCleanup_ServerRoleUnknown_DoesNotCleanupWillRepeat(
            [Frozen] Mock<IContentVersionCleanupPolicyGlobalSettings> settings,
            [Frozen] Mock<IRuntimeState> state,
            [Frozen] Mock<IContentVersionService> cleanupService,
            ContentVersionCleanup sut)
        {
            settings.Setup(x => x.EnableCleanup).Returns(true);

            state.Setup(x => x.Level).Returns(RuntimeLevel.Run);
            state.Setup(x => x.IsMainDom).Returns(true);
            state.Setup(x => x.ServerRole).Returns(ServerRole.Unknown);

            var result = sut.PerformRun();

            Assert.Multiple(() =>
            {
                Assert.True(result);
                cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
            });
        }

        [Test, AutoMoqData]
        public void ContentVersionCleanup_NotMainDom_DoesNotCleanupWillNotRepeat(
            [Frozen] Mock<IContentVersionCleanupPolicyGlobalSettings> settings,
            [Frozen] Mock<IRuntimeState> state,
            [Frozen] Mock<IContentVersionService> cleanupService,
            ContentVersionCleanup sut)
        {
            settings.Setup(x => x.EnableCleanup).Returns(true);

            state.Setup(x => x.Level).Returns(RuntimeLevel.Run);
            state.Setup(x => x.IsMainDom).Returns(false);
            state.Setup(x => x.ServerRole).Returns(ServerRole.Master);

            var result = sut.PerformRun();

            Assert.Multiple(() =>
            {
                Assert.False(result);
                cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Never);
            });
        }

        [Test, AutoMoqData]
        public void ContentVersionCleanup_Enabled_DelegatesToCleanupService(
            [Frozen] Mock<IContentVersionCleanupPolicyGlobalSettings> settings,
            [Frozen] Mock<IRuntimeState> state,
            [Frozen] Mock<IContentVersionService> cleanupService,
            ContentVersionCleanup sut)
        {
            settings.Setup(x => x.EnableCleanup).Returns(true);

            state.Setup(x => x.Level).Returns(RuntimeLevel.Run);
            state.Setup(x => x.IsMainDom).Returns(true);
            state.Setup(x => x.ServerRole).Returns(ServerRole.Master);

            var result = sut.PerformRun();

            Assert.Multiple(() =>
            {
                Assert.True(result);
                cleanupService.Verify(x => x.PerformContentVersionCleanup(It.IsAny<DateTime>()), Times.Once);
            });
        }
    }
}
