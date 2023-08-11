// Copyright (c) Umbraco.
// See LICENSE for more details.

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
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;
using Umbraco.Cms.Infrastructure.HostedServices;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

[TestFixture]
public class ScheduledPublishingJobTests
{
    private Mock<IContentService> _mockContentService;
    private Mock<ILogger<ScheduledPublishingJob>> _mockLogger;

    [Test]
    public async Task Does_Not_Execute_When_Not_Enabled()
    {
        var sut = CreateScheduledPublishing(enabled: false);
        await sut.RunJobAsync();
        VerifyScheduledPublishingNotPerformed();
    }

    [Test]
    public async Task Executes_And_Performs_Scheduled_Publishing()
    {
        var sut = CreateScheduledPublishing();
        await sut.RunJobAsync();
        VerifyScheduledPublishingPerformed();
    }

    private ScheduledPublishingJob CreateScheduledPublishing(
        bool enabled = true)
    {
        if (enabled)
        {
            Suspendable.ScheduledPublishing.Resume();
        }
        else
        {
            Suspendable.ScheduledPublishing.Suspend();
        }

        _mockContentService = new Mock<IContentService>();

        var mockUmbracoContextFactory = new Mock<IUmbracoContextFactory>();
        mockUmbracoContextFactory.Setup(x => x.EnsureUmbracoContext())
            .Returns(new UmbracoContextReference(null, false, null));

        _mockLogger = new Mock<ILogger<ScheduledPublishingJob>>();

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

        return new ScheduledPublishingJob(
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
