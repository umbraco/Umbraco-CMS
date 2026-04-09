using System.Diagnostics;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
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
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup(It.IsAny<DateTime>(), It.IsAny<int?>()))
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
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup(It.IsAny<DateTime>(), It.IsAny<int?>()))
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
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup(It.IsAny<DateTime>(), It.IsAny<int?>()))
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
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup(It.IsAny<DateTime>(), It.IsAny<int?>()))
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
        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup(It.IsAny<DateTime>(), It.IsAny<int?>()))
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

    [Test]
    [AutoMoqData]
    public void PerformContentVersionCleanup_WithOverrideSmallerKeepAllDays_UsesSmallestAsCutoff(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> contentSettings,
        [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
        ContentVersionService sut)
    {
        var asAtDate = new DateTime(2026, 3, 24, 12, 0, 0, DateTimeKind.Utc);

        contentSettings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new Umbraco.Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings
            {
                EnableCleanup = true,
                KeepAllVersionsNewerThanDays = 7,
                KeepLatestVersionPerDayForDays = 0,
            },
        });

        // Override with a smaller KeepAllVersionsNewerThanDays (2 < 7).
        documentVersionRepository.Setup(x => x.GetCleanupPolicies())
            .Returns(
            [
                new() { ContentTypeId = 1, KeepAllVersionsNewerThanDays = 2 },
            ]);

        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup(It.IsAny<DateTime>(), It.IsAny<int?>()))
            .Returns(Array.Empty<ContentVersionMeta>());

        sut.PerformContentVersionCleanup(asAtDate);

        // The effective cutoff should be 2 days (the override's value, not the global 7).
        DateTime expectedCutoff = asAtDate.AddDays(-2);
        documentVersionRepository.Verify(
            x => x.GetDocumentVersionsEligibleForCleanup(expectedCutoff, It.IsAny<int?>()),
            Times.Once);
    }

    [Test]
    [AutoMoqData]
    public void PerformContentVersionCleanup_WithOverrideNullKeepAllDays_UsesGlobalValue(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> contentSettings,
        [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
        ContentVersionService sut)
    {
        var asAtDate = new DateTime(2026, 3, 24, 12, 0, 0, DateTimeKind.Utc);

        contentSettings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new Umbraco.Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings
            {
                EnableCleanup = true,
                KeepAllVersionsNewerThanDays = 7,
                KeepLatestVersionPerDayForDays = 0,
            },
        });

        // Override with null KeepAllVersionsNewerThanDays — should not affect global.
        documentVersionRepository.Setup(x => x.GetCleanupPolicies())
            .Returns(
            [
                new() { ContentTypeId = 1, KeepAllVersionsNewerThanDays = null, PreventCleanup = true },
            ]);

        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup(It.IsAny<DateTime>(), It.IsAny<int?>()))
            .Returns(Array.Empty<ContentVersionMeta>());

        sut.PerformContentVersionCleanup(asAtDate);

        // The effective cutoff should remain 7 days (the global value).
        DateTime expectedCutoff = asAtDate.AddDays(-7);
        documentVersionRepository.Verify(
            x => x.GetDocumentVersionsEligibleForCleanup(expectedCutoff, It.IsAny<int?>()),
            Times.Once);
    }

    [Test]
    [AutoMoqData]
    public void PerformContentVersionCleanup_WithMultipleOverrides_UsesSmallestKeepAllDays(
        [Frozen] Mock<IOptionsMonitor<ContentSettings>> contentSettings,
        [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
        ContentVersionService sut)
    {
        var asAtDate = new DateTime(2026, 3, 24, 12, 0, 0, DateTimeKind.Utc);

        contentSettings.Setup(x => x.CurrentValue).Returns(new ContentSettings
        {
            ContentVersionCleanupPolicy = new Umbraco.Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings
            {
                EnableCleanup = true,
                KeepAllVersionsNewerThanDays = 7,
                KeepLatestVersionPerDayForDays = 0,
            },
        });

        // Multiple overrides — smallest (1) should win over global (7) and the other override (5).
        documentVersionRepository.Setup(x => x.GetCleanupPolicies())
            .Returns(
            [
                new() { ContentTypeId = 1, KeepAllVersionsNewerThanDays = 5 },
                new() { ContentTypeId = 2, KeepAllVersionsNewerThanDays = 1 },
            ]);

        documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup(It.IsAny<DateTime>(), It.IsAny<int?>()))
            .Returns(Array.Empty<ContentVersionMeta>());

        sut.PerformContentVersionCleanup(asAtDate);

        DateTime expectedCutoff = asAtDate.AddDays(-1);
        documentVersionRepository.Verify(
            x => x.GetDocumentVersionsEligibleForCleanup(expectedCutoff, It.IsAny<int?>()),
            Times.Once);
    }
}
