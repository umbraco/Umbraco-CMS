using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;

namespace Umbraco.Tests.Services;

[TestFixture]
internal class ContentVersionCleanupServiceTest
{
    [Test]
    [AutoMoqData]
    public void PerformContentVersionCleanup_Always_RespectsDeleteRevisionsCancellation(
        [Frozen] Mock<IScopedNotificationPublisher> eventAggregator,
        [Frozen] Mock<IContentVersionCleanupPolicy> policy,
        [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
        List<ContentVersionMeta> someHistoricVersions,
        DateTime aDateTime,
        ContentVersionService sut)
    {
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
            .Returns(someHistoricVersions);

        eventAggregator.Setup(x => x.PublishCancelable(It.IsAny<ContentDeletingVersionsNotification>()))
            .Returns(true);

        policy.Setup(x => x.Apply(aDateTime, someHistoricVersions))
            .Returns(someHistoricVersions);

        // # Act
        var report = sut.PerformContentVersionCleanup(aDateTime);

        Assert.Multiple(() =>
        {
            eventAggregator.Verify(
                x => x.PublishCancelable(
                    It.IsAny<ContentDeletingVersionsNotification>()),
                Times.Exactly(someHistoricVersions.Count));
            Assert.AreEqual(0, report.Count);
        });
    }

    [Test]
    [AutoMoqData]
    public void PerformContentVersionCleanup_Always_FiresDeletedVersionsForEachDeletedVersion(
        [Frozen] Mock<IScopedNotificationPublisher> eventAggregator,
        [Frozen] Mock<IContentVersionCleanupPolicy> policy,
        [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
        List<ContentVersionMeta> someHistoricVersions,
        DateTime aDateTime,
        ContentVersionService sut)
    {
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
            .Returns(someHistoricVersions);

        eventAggregator
            .Setup(x => x.PublishCancelable(It.IsAny<ICancelableNotification>()))
            .Returns(false);

        policy.Setup(x => x.Apply(aDateTime, someHistoricVersions))
            .Returns(someHistoricVersions);

        // # Act
        sut.PerformContentVersionCleanup(aDateTime);

        eventAggregator.Verify(x => x.Publish(It.IsAny<ContentDeletedVersionsNotification>()), Times.Exactly(someHistoricVersions.Count));
    }

    [Test]
    [AutoMoqData]
    public void PerformContentVersionCleanup_Always_ReturnsReportOfDeletedItems(
        [Frozen] Mock<IScopedNotificationPublisher> eventAggregator,
        [Frozen] Mock<IContentVersionCleanupPolicy> policy,
        [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
        List<ContentVersionMeta> someHistoricVersions,
        DateTime aDateTime,
        ContentVersionService sut)
    {
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
            .Returns(someHistoricVersions);

        eventAggregator
            .Setup(x => x.PublishCancelable(It.IsAny<ICancelableNotification>()))
            .Returns(false);

        // # Act
        var report = sut.PerformContentVersionCleanup(aDateTime);

        Assert.Multiple(() =>
        {
            Assert.Greater(report.Count, 0);
            Assert.AreEqual(someHistoricVersions.Count, report.Count);
        });
    }

    [Test]
    [AutoMoqData]
    public void PerformContentVersionCleanup_Always_AdheresToCleanupPolicy(
        [Frozen] Mock<IScopedNotificationPublisher> eventAggregator,
        [Frozen] Mock<IContentVersionCleanupPolicy> policy,
        [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
        List<ContentVersionMeta> someHistoricVersions,
        DateTime aDateTime,
        ContentVersionService sut)
    {
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
            .Returns(someHistoricVersions);

        eventAggregator
            .Setup(x => x.PublishCancelable(It.IsAny<ICancelableNotification>()))
            .Returns(false);

        policy.Setup(x => x.Apply(It.IsAny<DateTime>(), It.IsAny<IEnumerable<ContentVersionMeta>>()))
            .Returns<DateTime, IEnumerable<ContentVersionMeta>>((_, items) => items.Take(1));

        // # Act
        var report = sut.PerformContentVersionCleanup(aDateTime);

        Debug.Assert(someHistoricVersions.Count > 1);

        Assert.Multiple(() =>
        {
            policy.Verify(x => x.Apply(aDateTime, someHistoricVersions), Times.Once);
            Assert.AreEqual(someHistoricVersions.First(), report.Single());
        });
    }

    /// <remarks>
    ///     For v9 this just needs a rewrite, no static events, no service location etc
    /// </remarks>
    [Test]
    [AutoMoqData]
    public void PerformContentVersionCleanup_HasVersionsToDelete_CallsDeleteOnRepositoryWithFilteredSet(
        [Frozen] Mock<IScopedNotificationPublisher> eventAggregator,
        [Frozen] Mock<IContentVersionCleanupPolicy> policy,
        [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
        List<ContentVersionMeta> someHistoricVersions,
        DateTime aDateTime,
        ContentVersionService sut)
    {
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
            .Returns(someHistoricVersions);

        eventAggregator
            .Setup(x => x.PublishCancelable(It.IsAny<ICancelableNotification>()))
            .Returns(false);

        var filteredSet = someHistoricVersions.Take(1);

        policy.Setup(x => x.Apply(It.IsAny<DateTime>(), It.IsAny<IEnumerable<ContentVersionMeta>>()))
            .Returns<DateTime, IEnumerable<ContentVersionMeta>>((_, items) => filteredSet);

        // # Act
        sut.PerformContentVersionCleanup(aDateTime);

        Debug.Assert(someHistoricVersions.Any());

        var expectedId = filteredSet.First().VersionId;

        documentVersionRepository.Verify(
            x => x.DeleteVersions(It.Is<IEnumerable<int>>(y => y.Single() == expectedId)),
            Times.Once);
    }
}
