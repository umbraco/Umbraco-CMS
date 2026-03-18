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
    /// <summary>
    /// Verifies that <see cref="ContentVersionService.PerformContentVersionCleanup"/> always respects cancellation when delete revisions notifications are published.
    /// </summary>
    /// <param name="eventAggregator">Mocked notification publisher for publishing cancelable notifications.</param>
    /// <param name="policy">Mocked content version cleanup policy.</param>
    /// <param name="documentVersionRepository">Mocked repository for document versions.</param>
    /// <param name="someHistoricVersions">A list of historic content version metadata used as test data.</param>
    /// <param name="aDateTime">The date and time at which the cleanup is performed.</param>
    /// <param name="sut">The <see cref="ContentVersionService"/> instance under test.</param>
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

    /// <summary>
    /// Verifies that performing content version cleanup always fires a deleted versions notification for each deleted version.
    /// </summary>
    /// <param name="eventAggregator">The mock notification publisher used to publish notifications.</param>
    /// <param name="policy">The mock content version cleanup policy.</param>
    /// <param name="documentVersionRepository">The mock repository for document versions.</param>
    /// <param name="someHistoricVersions">The list of historic content version metadata to be cleaned up.</param>
    /// <param name="aDateTime">The date and time used for cleanup evaluation.</param>
    /// <param name="sut">The content version service under test.</param>
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

    /// <summary>
    /// Verifies that the <see cref="ContentVersionService.PerformContentVersionCleanup(DateTime)"/> method always returns a report containing the deleted content versions.
    /// </summary>
    /// <param name="eventAggregator">A mock event aggregator used to simulate notification publishing during cleanup.</param>
    /// <param name="policy">A mock content version cleanup policy that defines cleanup rules.</param>
    /// <param name="documentVersionRepository">A mock repository providing eligible content versions for cleanup.</param>
    /// <param name="someHistoricVersions">A list of historic content version metadata expected to be cleaned up.</param>
    /// <param name="aDateTime">The cutoff <see cref="DateTime"/> used to determine which versions are eligible for cleanup.</param>
    /// <param name="sut">The instance of <see cref="ContentVersionService"/> under test.</param>
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

    /// <summary>
    /// Verifies that <see cref="ContentVersionService.PerformContentVersionCleanup"/> always applies the provided cleanup policy to eligible content versions.
    /// </summary>
    /// <param name="eventAggregator">Mock notification publisher for publishing notifications during cleanup.</param>
    /// <param name="policy">Mock cleanup policy to determine which versions are retained or removed.</param>
    /// <param name="documentVersionRepository">Mock repository returning content versions eligible for cleanup.</param>
    /// <param name="someHistoricVersions">List of historic content version metadata used as test data.</param>
    /// <param name="aDateTime">Reference date and time for the cleanup operation.</param>
    /// <param name="sut">The <see cref="ContentVersionService"/> instance under test.</param>
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

        Debug.Assert(someHistoricVersions.Count > 1, "Test requires more than one historic version.");

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

        Debug.Assert(someHistoricVersions.Any(), "Test requires at least one historic version.");

        var expectedId = filteredSet.First().VersionId;

        documentVersionRepository.Verify(
            x => x.DeleteVersions(It.Is<IEnumerable<int>>(y => y.Single() == expectedId)),
            Times.Once);
    }
}
