using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    /// <remarks>
    /// v9 -> Tests.UnitTests
    /// Sut here is ContentService, but in v9 should be a new class
    /// </remarks>
    [TestFixture]
    public class ContentVersionCleanupService_Tests_UnitTests
    {
        [SetUp]
        public void Setup()
        {
            Current.Reset();
        }

        /// <remarks>
        /// For v9 this just needs a rewrite, no static events, no service location etc
        /// </remarks>
        [Test, AutoMoqData]
        public void PerformContentVersionCleanup_Always_RespectsDeleteRevisionsCancellation(
            [Frozen] Mock<IFactory> factory,
            [Frozen] Mock<IScope> scope,
            Mock<IDocumentVersionRepository> documentVersionRepository,
            List<TestContentVersionMeta> someHistoricVersions,
            DateTime aDateTime,
            ContentService sut)
        {
            factory.Setup(x => x.GetInstance(typeof(IDocumentVersionRepository)))
                .Returns(documentVersionRepository.Object);

            factory.Setup(x => x.GetInstance(typeof(IContentVersionCleanupPolicy)))
                .Returns(new EchoingCleanupPolicyStub());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(someHistoricVersions);

            scope.Setup(x => x.Events).Returns(new PassThroughEventDispatcher());

            // Wire up service locator
            Current.Factory = factory.Object;

            void OnDeletingVersions(IContentService sender, DeleteRevisionsEventArgs args) => args.Cancel = true;

            ContentService.DeletingVersions += OnDeletingVersions;

            // # Act
            var report = sut.PerformContentVersionCleanup(aDateTime);

            ContentService.DeletingVersions -= OnDeletingVersions;

            Assert.AreEqual(0, report.Count);
        }

        /// <remarks>
        /// For v9 this just needs a rewrite, no static events, no service location etc
        /// </remarks>
        [Test, AutoMoqData]
        public void PerformContentVersionCleanup_Always_FiresDeletedVersionsForEachDeletedVersion(
            [Frozen] Mock<IFactory> factory,
            [Frozen] Mock<IScope> scope,
            Mock<IDocumentVersionRepository> documentVersionRepository,
            List<TestContentVersionMeta> someHistoricVersions,
            DateTime aDateTime,
            ContentService sut)
        {
            factory.Setup(x => x.GetInstance(typeof(IDocumentVersionRepository)))
                .Returns(documentVersionRepository.Object);

            factory.Setup(x => x.GetInstance(typeof(IContentVersionCleanupPolicy)))
                .Returns(new EchoingCleanupPolicyStub());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(someHistoricVersions);

            scope.Setup(x => x.Events).Returns(new PassThroughEventDispatcher());

            // Wire up service locator
            Current.Factory = factory.Object;

            // v9 can Mock + Verify
            var deletedAccordingToEvents = 0;
            void OnDeletedVersions(IContentService sender, DeleteRevisionsEventArgs args) => deletedAccordingToEvents++;

            ContentService.DeletedVersions += OnDeletedVersions;

            // # Act
            sut.PerformContentVersionCleanup(aDateTime);

            ContentService.DeletedVersions -= OnDeletedVersions;

            Assert.Multiple(() =>
            {
                Assert.Greater(deletedAccordingToEvents, 0);
                Assert.AreEqual(someHistoricVersions.Count, deletedAccordingToEvents);
            });
        }

        /// <remarks>
        /// For v9 this just needs a rewrite, no static events, no service location etc
        /// </remarks>
        [Test, AutoMoqData]
        public void PerformContentVersionCleanup_Always_ReturnsReportOfDeletedItems(
            [Frozen] Mock<IFactory> factory,
            [Frozen] Mock<IScope> scope,
            Mock<IDocumentVersionRepository> documentVersionRepository,
            List<TestContentVersionMeta> someHistoricVersions,
            DateTime aDateTime,
            ContentService sut)
        {
            factory.Setup(x => x.GetInstance(typeof(IDocumentVersionRepository)))
                .Returns(documentVersionRepository.Object);

            factory.Setup(x => x.GetInstance(typeof(IContentVersionCleanupPolicy)))
                .Returns(new EchoingCleanupPolicyStub());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(someHistoricVersions);

            scope.Setup(x => x.Events).Returns(new PassThroughEventDispatcher());

            // Wire up service locator
            Current.Factory = factory.Object;

            // # Act
            var report = sut.PerformContentVersionCleanup(aDateTime);

            Assert.Multiple(() =>
            {
                Assert.Greater(report.Count, 0);
                Assert.AreEqual(someHistoricVersions.Count, report.Count);
            });
        }

        /// <remarks>
        /// For v9 this just needs a rewrite, no static events, no service location etc
        /// </remarks>
        [Test, AutoMoqData]
        public void PerformContentVersionCleanup_Always_AdheresToCleanupPolicy(
            [Frozen] Mock<IFactory> factory,
            [Frozen] Mock<IScope> scope,
            Mock<IDocumentVersionRepository> documentVersionRepository,
            Mock<IContentVersionCleanupPolicy> cleanupPolicy,
            List<TestContentVersionMeta> someHistoricVersions,
            DateTime aDateTime,
            ContentService sut)
        {
            factory.Setup(x => x.GetInstance(typeof(IDocumentVersionRepository)))
                .Returns(documentVersionRepository.Object);

            factory.Setup(x => x.GetInstance(typeof(IContentVersionCleanupPolicy)))
                .Returns(cleanupPolicy.Object);

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(someHistoricVersions);

            scope.Setup(x => x.Events).Returns(new PassThroughEventDispatcher());

            cleanupPolicy.Setup(x => x.Apply(It.IsAny<DateTime>(), It.IsAny<IEnumerable<TestContentVersionMeta>>()))
                .Returns<DateTime, IEnumerable<TestContentVersionMeta>>((_, items) => items.Take(1));

            // Wire up service locator
            Current.Factory = factory.Object;

            // # Act
            var report = sut.PerformContentVersionCleanup(aDateTime);

            Debug.Assert(someHistoricVersions.Count > 1);

            Assert.Multiple(() =>
            {
                cleanupPolicy.Verify(x => x.Apply(aDateTime, someHistoricVersions), Times.Once);
                Assert.AreEqual(someHistoricVersions.First(), report.Single());
            });
        }

        /// <remarks>
        /// For v9 this just needs a rewrite, no static events, no service location etc
        /// </remarks>
        [Test, AutoMoqData]
        public void PerformContentVersionCleanup_HasVersionsToDelete_CallsDeleteOnRepositoryWithFilteredSet(
            [Frozen] Mock<IFactory> factory,
            [Frozen] Mock<IScope> scope,
            Mock<IDocumentVersionRepository> documentVersionRepository,
            Mock<IContentVersionCleanupPolicy> cleanupPolicy,
            List<TestContentVersionMeta> someHistoricVersions,
            DateTime aDateTime,
            ContentService sut)
        {
            factory.Setup(x => x.GetInstance(typeof(IDocumentVersionRepository)))
                .Returns(documentVersionRepository.Object);

            factory.Setup(x => x.GetInstance(typeof(IContentVersionCleanupPolicy)))
                .Returns(cleanupPolicy.Object);

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(someHistoricVersions);

            scope.Setup(x => x.Events).Returns(new PassThroughEventDispatcher());

            var filteredSet = someHistoricVersions.Take(1);

            cleanupPolicy.Setup(x => x.Apply(It.IsAny<DateTime>(), It.IsAny<IEnumerable<TestContentVersionMeta>>()))
                .Returns<DateTime, IEnumerable<TestContentVersionMeta>>((_, items) => filteredSet);

            // Wire up service locator
            Current.Factory = factory.Object;

            // # Act
            var report = sut.PerformContentVersionCleanup(aDateTime);

            Debug.Assert(someHistoricVersions.Any());

            var expectedId = filteredSet.First().VersionId;

            documentVersionRepository.Verify(x => x.DeleteVersions(It.Is<IEnumerable<int>>(y => y.Single() == expectedId)), Times.Once);
        }

        class EchoingCleanupPolicyStub : IContentVersionCleanupPolicy
        {
            /// <summary>
            /// What goes in, must come out
            /// </summary>
            public EchoingCleanupPolicyStub() { }

            /* Note: Could just wire up a mock but its quite wordy.
             *
             * cleanupPolicy.Setup(x => x.Apply(It.IsAny<DateTime>(), It.IsAny<IEnumerable<TestHistoricContentVersionMeta>>()))
             *    .Returns<DateTime, IEnumerable<TestHistoricContentVersionMeta>>((date, items) => items);
             */
            public IEnumerable<ContentVersionMeta> Apply(
                DateTime asAtDate,
                IEnumerable<ContentVersionMeta> items
            ) => items;
        }

        /// <remarks>
        /// <para>NPoco &lt; 5 requires a parameter-less constructor but plays nice with get-only properties.</para>
        /// <para>Moq won't play nice with get-only properties, but doesn't require a parameter-less constructor.</para>
        ///
        /// <para>Inheritance solves this so that we get values for test data without a specimen builder</para>
        /// </remarks>
        public class TestContentVersionMeta : ContentVersionMeta
        {
            public TestContentVersionMeta(
                int contentId,
                int contentTypeId,
                int versionId,
                int userId,
                DateTime versionDate,
                bool currentPublishedVersion,
                bool currentDraftVersion,
                bool preventCleanup,
                string username)
            : base(
                contentId,
                contentTypeId,
                versionId,
                userId,
                versionDate,
                currentPublishedVersion,
                currentDraftVersion,
                preventCleanup,
                username
            )
            {
            }
        }
    }
}
