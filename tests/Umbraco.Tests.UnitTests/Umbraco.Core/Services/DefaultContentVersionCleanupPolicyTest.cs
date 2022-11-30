using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using ContentVersionCleanupPolicySettings = Umbraco.Cms.Core.Models.ContentVersionCleanupPolicySettings;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class DefaultContentVersionCleanupPolicyTest
    {
        [Test]
        [AutoMoqData]
        public void Apply_AllOlderThanKeepSettings_AllVersionsReturned(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var versionId = 0;

            var historicItems = new List<ContentVersionMeta>
            {
                new ContentVersionMeta(versionId: ++versionId, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),
                new ContentVersionMeta(versionId: ++versionId, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 0,
                },
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(Array.Empty<ContentVersionCleanupPolicySettings>());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.AreEqual(2, results.Count);
        }

        [Test]
        [AutoMoqData]
        public void Apply_OverlappingKeepSettings_KeepAllVersionsNewerThanDaysTakesPriority(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var versionId = 0;

            var historicItems = new List<ContentVersionMeta>
            {
                new ContentVersionMeta(versionId: ++versionId, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),
                new ContentVersionMeta(versionId: ++versionId, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 2,
                    KeepLatestVersionPerDayForDays = 2,
                },
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(Array.Empty<ContentVersionCleanupPolicySettings>());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.AreEqual(0, results.Count);
        }

        [Test]
        [AutoMoqData]
        public void Apply_WithinInKeepLatestPerDay_ReturnsSinglePerContentPerDay(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var historicItems = new List<ContentVersionMeta>
            {
                new ContentVersionMeta(versionId: 1, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 2, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 3, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),

                new ContentVersionMeta(versionId: 4, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddDays(-1).AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 5, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddDays(-1).AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 6, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddDays(-1).AddHours(-1), false, false, false, null),

                // another content
                new ContentVersionMeta(versionId: 7, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 8, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 9, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 3,
                },
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(Array.Empty<ContentVersionCleanupPolicySettings>());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            // Keep latest per day for 3 days per content type
            // 2 content types, one of which has 2 days of entries, the other only a single day
            Assert.Multiple(() =>
            {
                Assert.AreEqual(6, results.Count);
                Assert.AreEqual(4, results.Count(x => x.ContentTypeId == 1));
                Assert.AreEqual(2, results.Count(x => x.ContentTypeId == 2));
                Assert.False(results.Any(x => x.VersionId == 9)); // Most recent for content type 2
                Assert.False(results.Any(x => x.VersionId == 3)); // Most recent for content type 1 today
                Assert.False(results.Any(x => x.VersionId == 6)); // Most recent for content type 1 yesterday
            });
        }

        [Test]
        [AutoMoqData]
        public void Apply_HasOverridePolicy_RespectsPreventCleanup(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var historicItems = new List<ContentVersionMeta>
            {
                new ContentVersionMeta(versionId: 1, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 2, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 3, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),

                // another content & type
                new ContentVersionMeta(versionId: 4, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 5, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 6, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 0,
                },
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(new ContentVersionCleanupPolicySettings[]
                {
                    new() { ContentTypeId = 2, PreventCleanup = true },
                });

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.True(results.All(x => x.ContentTypeId == 1));
        }

        [Test]
        [AutoMoqData]
        public void Apply_HasOverridePolicy_RespectsKeepAll(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var historicItems = new List<ContentVersionMeta>
            {
                new ContentVersionMeta(versionId: 1, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 2, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 3, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),

                // another content & type
                new ContentVersionMeta(versionId: 4, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 5, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 6, contentId: 2, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 0,
                },
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(new ContentVersionCleanupPolicySettings[]
                {
                    new() { ContentTypeId = 2, PreventCleanup = false, KeepAllVersionsNewerThanDays = 3 },
                });

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.True(results.All(x => x.ContentTypeId == 1));
        }

        [Test]
        [AutoMoqData]
        public void Apply_HasOverridePolicy_RespectsKeepLatest(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var historicItems = new List<ContentVersionMeta>
            {
                new ContentVersionMeta(versionId: 1, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 2, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 3, contentId: 1, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),

                // another content
                new ContentVersionMeta(versionId: 4, contentId: 2, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 5, contentId: 2, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 6, contentId: 2, contentTypeId: 1, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),

                // another content & type
                new ContentVersionMeta(versionId: 7, contentId: 3, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-3), false, false, false, null),
                new ContentVersionMeta(versionId: 8, contentId: 3, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-2), false, false, false, null),
                new ContentVersionMeta(versionId: 9, contentId: 3, contentTypeId: 2, -1, versionDate: DateTime.Today.AddHours(-1), false, false, false, null),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 0,
                },
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(new ContentVersionCleanupPolicySettings[]
                {
                    new() { ContentTypeId = 2, PreventCleanup = false, KeepLatestVersionPerDayForDays = 3 },
                });

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            // By default no historic versions are kept
            // Override policy for content type 2 keeps latest per day for 3 days, no versions retained for content type with id 1
            // There were 3 entries for content type 2 all on the same day
            // version id 9 is most recent for content type 2, and should be filtered, all the rest should be present.
            Assert.Multiple(() =>
            {
                Assert.AreEqual(8, results.Count);
                Assert.AreEqual(2, results.Count(x => x.ContentTypeId == 2));
                Assert.False(results.Any(x => x.VersionId == 9));
            });
        }
    }
}
