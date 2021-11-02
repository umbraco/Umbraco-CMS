using System;
using System.Data;
using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ContentVersionCleanupService_Tests_Integration : TestWithDatabaseBase
    {
        /// <remarks>
        /// This is covered by the unit tests, but nice to know it deletes on infra.
        /// And proves implementation is compatible with SQL CE.
        /// </remarks>
        [Test]
        public void PerformContentVersionCleanup_WithNoKeepPeriods_DeletesEverythingExceptActive()
        {
            // For reference currently has
            // 5000 Documents
            // With 200K Versions
            // With 11M Property data

            var contentTypeA = MockedContentTypes.CreateSimpleContentType("contentTypeA", "contentTypeA");
            // Kill all historic
            contentTypeA.HistoryCleanup.PreventCleanup = false;
            contentTypeA.HistoryCleanup.KeepAllVersionsNewerThanDays = 0;
            contentTypeA.HistoryCleanup.KeepLatestVersionPerDayForDays = 0;

            ServiceContext.FileService.SaveTemplate(contentTypeA.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentTypeA);

            var content = MockedContent.CreateSimpleContent(contentTypeA);
            ServiceContext.ContentService.SaveAndPublish(content, raiseEvents: false);

            for (var i = 0; i < 10; i++)
            {
                ServiceContext.ContentService.SaveAndPublish(content, raiseEvents: false);
            }

            var before = GetReport();

            Debug.Assert(before.ContentVersions == 12); // 10 historic + current draft + current published
            Debug.Assert(before.PropertyData == 12 * 3); // CreateSimpleContentType = 3 props



            ((IContentVersionService)ServiceContext.ContentService).PerformContentVersionCleanup(DateTime.Now.AddHours(1));

            var after = GetReport();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, after.ContentVersions); // current draft, current published
                Assert.AreEqual(2, after.DocumentVersions);
                Assert.AreEqual(6, after.PropertyData); // CreateSimpleContentType = 3 props
            });
        }

        private Report GetReport()
        {
            var scopeProvider = TestObjects.GetScopeProvider(Logger);
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                // SQL CE is fun!
                var contentVersions = scope.Database.Single<int>(@"select count(1) from umbracoContentVersion");
                var documentVersions = scope.Database.Single<int>(@"select count(1) from umbracoDocumentVersion");
                var propertyData = scope.Database.Single<int>(@"select count(1) from umbracoPropertyData");

                return new Report
                {
                    ContentVersions = contentVersions,
                    DocumentVersions = documentVersions,
                    PropertyData = propertyData
                };
            }
        }

        private void InsertCleanupPolicy(IContentType contentType, int daysToKeepAll, int daysToRollupAll, bool preventCleanup = false)
        {
            var scopeProvider = TestObjects.GetScopeProvider(Logger);
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var entity = new ContentVersionCleanupPolicyDto
                {
                    ContentTypeId = contentType.Id,
                    KeepAllVersionsNewerThanDays = daysToKeepAll,
                    KeepLatestVersionPerDayForDays = daysToRollupAll,
                    PreventCleanup = preventCleanup,
                    Updated = DateTime.Today
                };

                scope.Database.Insert(entity);
            }
        }

        class Report
        {
            public int ContentVersions { get; set; }
            public int DocumentVersions { get; set; }
            public int PropertyData { get; set; }
        }
    }
}
