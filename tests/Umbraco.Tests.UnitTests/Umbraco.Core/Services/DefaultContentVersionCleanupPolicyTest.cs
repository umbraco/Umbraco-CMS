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
using Umbraco.Cms.Infrastructure.Services.Implement;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using ContentVersionCleanupPolicySettings = Umbraco.Cms.Core.Models.ContentVersionCleanupPolicySettings;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    public class DefaultContentVersionCleanupPolicyTest
    {
        [Test, AutoMoqData]
        public void Apply_AllOlderThanKeepSettings_AllVersionsReturned(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var versionId = 0;

            var historicItems = new List<HistoricContentVersionMeta>
            {
                new HistoricContentVersionMeta(versionId: ++versionId, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),
                new HistoricContentVersionMeta(versionId: ++versionId, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 0
                }
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(Array.Empty<ContentVersionCleanupPolicySettings>());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.AreEqual(2, results.Count);
        }

        [Test, AutoMoqData]
        public void Apply_OverlappingKeepSettings_KeepAllVersionsNewerThanDaysTakesPriority(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var versionId = 0;

            var historicItems = new List<HistoricContentVersionMeta>
            {
                new HistoricContentVersionMeta(versionId: ++versionId, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),
                new HistoricContentVersionMeta(versionId: ++versionId, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 2,
                    KeepLatestVersionPerDayForDays = 2
                }
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(Array.Empty<ContentVersionCleanupPolicySettings>());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.AreEqual(0, results.Count);
        }

        [Test, AutoMoqData]
        public void Apply_WithinInKeepLatestPerDay_ReturnsSinglePerContentPerDay(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var historicItems = new List<HistoricContentVersionMeta>
            {
                new HistoricContentVersionMeta(versionId: 1, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 2, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 3, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),

                new HistoricContentVersionMeta(versionId: 4, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddDays(-1).AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 5, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddDays(-1).AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 6, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddDays(-1).AddHours(-1)),
                // another content
                new HistoricContentVersionMeta(versionId: 7, contentId: 2, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 8, contentId: 2, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 9, contentId: 2, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 3
                }
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(Array.Empty<ContentVersionCleanupPolicySettings>());

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, results.Count);
                Assert.True(results.Exists(x => x.VersionId == 3));
                Assert.True(results.Exists(x => x.VersionId == 6));
                Assert.True(results.Exists(x => x.VersionId == 9));
            });
        }

        [Test, AutoMoqData]
        public void Apply_HasOverridePolicy_RespectsPreventCleanup(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var historicItems = new List<HistoricContentVersionMeta>
            {
                new HistoricContentVersionMeta(versionId: 1, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 2, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 3, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),
                // another content & type
                new HistoricContentVersionMeta(versionId: 4, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 5, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 6, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-1)),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 0
                }
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(new ContentVersionCleanupPolicySettings[]
                {
                    new ContentVersionCleanupPolicySettings{ ContentTypeId = 2, PreventCleanup = true }
                });

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.True(results.All(x => x.ContentTypeId == 1));
        }

        [Test, AutoMoqData]
        public void Apply_HasOverridePolicy_RespectsKeepAll(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var historicItems = new List<HistoricContentVersionMeta>
            {
                new HistoricContentVersionMeta(versionId: 1, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 2, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 3, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),
                // another content & type
                new HistoricContentVersionMeta(versionId: 4, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 5, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 6, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-1)),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 0
                }
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(new ContentVersionCleanupPolicySettings[]
                {
                    new ContentVersionCleanupPolicySettings{ ContentTypeId = 2, PreventCleanup = false, KeepAllVersionsNewerThanDays = 3 }
                });

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.True(results.All(x => x.ContentTypeId == 1));
        }

        [Test, AutoMoqData]
        public void Apply_HasOverridePolicy_RespectsKeepLatest(
            [Frozen] Mock<IDocumentVersionRepository> documentVersionRepository,
            [Frozen] Mock<IOptions<ContentSettings>> contentSettings,
            DefaultContentVersionCleanupPolicy sut)
        {
            var historicItems = new List<HistoricContentVersionMeta>
            {
                new HistoricContentVersionMeta(versionId: 1, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 2, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 3, contentId: 1, contentTypeId: 1, versionDate: DateTime.Today.AddHours(-1)),
                // another content & type
                new HistoricContentVersionMeta(versionId: 4, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-3)),
                new HistoricContentVersionMeta(versionId: 5, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-2)),
                new HistoricContentVersionMeta(versionId: 6, contentId: 2, contentTypeId: 2, versionDate: DateTime.Today.AddHours(-1)),
            };

            contentSettings.Setup(x => x.Value).Returns(new ContentSettings()
            {
                ContentVersionCleanupPolicy = new Cms.Core.Configuration.Models.ContentVersionCleanupPolicySettings()
                {
                    EnableCleanup = true,
                    KeepAllVersionsNewerThanDays = 0,
                    KeepLatestVersionPerDayForDays = 0
                }
            });

            documentVersionRepository.Setup(x => x.GetCleanupPolicies())
                .Returns(new ContentVersionCleanupPolicySettings[]
                {
                    new ContentVersionCleanupPolicySettings{ ContentTypeId = 2, PreventCleanup = false, KeepLatestVersionPerDayForDays = 3 }
                });

            documentVersionRepository.Setup(x => x.GetDocumentVersionsEligibleForCleanup())
                .Returns(historicItems);

            var results = sut.Apply(DateTime.Today, historicItems).ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(3, results.Count(x => x.ContentTypeId == 1));
                Assert.AreEqual(6, results.Single(x => x.ContentTypeId == 2).VersionId);
            });
        }
    }
}
