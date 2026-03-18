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
/// <summary>
/// Contains unit tests for the <see cref="DefaultContentVersionCleanupPolicy"/> class, verifying its behavior and functionality.
/// </summary>
    [TestFixture]
    public class DefaultContentVersionCleanupPolicyTest
    {
    /// <summary>
    /// Verifies that the <see cref="DefaultContentVersionCleanupPolicy.Apply"/> method returns all content versions
    /// when all available versions are older than the configured keep settings, meaning none are excluded from cleanup.
    /// </summary>
    /// <param name="documentVersionRepository">A frozen mock of <see cref="IDocumentVersionRepository"/> used to provide version data for the test.</param>
    /// <param name="contentSettings">A frozen mock of <see cref="IOptions{ContentSettings}"/> used to configure cleanup policy settings.</param>
    /// <param name="sut">The system under test: an instance of <see cref="DefaultContentVersionCleanupPolicy"/>.</param>
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

    /// <summary>
    /// Tests that when overlapping keep settings are applied, the setting to keep all versions newer than a specified number of days takes priority.
    /// </summary>
    /// <param name="documentVersionRepository">The mock repository for document versions.</param>
    /// <param name="contentSettings">The mock options for content settings.</param>
    /// <param name="sut">The system under test, the DefaultContentVersionCleanupPolicy instance.</param>
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

    /// <summary>
    /// Verifies that the <see cref="DefaultContentVersionCleanupPolicy.Apply"/> method retains only the latest content version per content item per day
    /// when the <c>KeepLatestVersionPerDayForDays</c> setting is enabled, ensuring that for each content item and each day, only one version is kept.
    /// </summary>
    /// <param name="documentVersionRepository">A mock repository providing content version metadata for testing.</param>
    /// <param name="contentSettings">A mock options object specifying content cleanup policy settings.</param>
    /// <param name="sut">The instance of <see cref="DefaultContentVersionCleanupPolicy"/> under test.</param>
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

    /// <summary>
    /// Verifies that the <c>Apply</c> method of <see cref="DefaultContentVersionCleanupPolicy"/> respects override policies by preventing cleanup
    /// for content types that have the <c>PreventCleanup</c> flag set in their policy. Ensures that only content versions for types without this flag are eligible for cleanup.
    /// </summary>
    /// <param name="documentVersionRepository">A mock repository used to provide content version data and cleanup policies.</param>
    /// <param name="contentSettings">A mock options object supplying content settings, including the global cleanup policy.</param>
    /// <param name="sut">The <see cref="DefaultContentVersionCleanupPolicy"/> instance under test.</param>
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

    /// <summary>
    /// Verifies that the <c>Apply</c> method of <see cref="DefaultContentVersionCleanupPolicy"/> respects an override policy that keeps all versions newer than a specified number of days for a given content type.
    /// Ensures that versions for content types with an override policy are excluded from cleanup if they meet the policy's criteria.
    /// </summary>
    /// <param name="documentVersionRepository">A mock repository providing document version data and cleanup policies.</param>
    /// <param name="contentSettings">A mock options object providing global content settings.</param>
    /// <param name="sut">The instance of <see cref="DefaultContentVersionCleanupPolicy"/> under test.</param>
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

    /// <summary>
    /// Verifies that the <c>Apply</c> method of <see cref="DefaultContentVersionCleanupPolicy"/> correctly respects an override cleanup policy,
    /// specifically ensuring that the latest version per day is kept for content types with an override, while other content types are not retained.
    /// </summary>
    /// <param name="documentVersionRepository">A mock repository providing document version data and cleanup policies.</param>
    /// <param name="contentSettings">A mock providing content settings options, including the default cleanup policy.</param>
    /// <param name="sut">The <see cref="DefaultContentVersionCleanupPolicy"/> instance under test.</param>
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
