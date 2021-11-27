﻿using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Repositories.Implement;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence.Repositories
{
    /// <remarks>
    /// v9 -> Tests.Integration
    /// </remarks>
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class DocumentVersionRepository_Tests_Integration : TestWithDatabaseBase
    {
        [Test]
        public void GetDocumentVersionsEligibleForCleanup_Always_ExcludesActiveVersions()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateSimpleContent(contentType);

            ServiceContext.ContentService.SaveAndPublish(content);
            // At this point content has 2 versions, a draft version and a published version.

            ServiceContext.ContentService.SaveAndPublish(content);
            // At this point content has 3 versions, a historic version, a draft version and a published version.

            var scopeProvider = TestObjects.GetScopeProvider(Logger);
            using (scopeProvider.CreateScope())
            {
                var sut = new DocumentVersionRepository((IScopeAccessor)scopeProvider);
                var results = sut.GetDocumentVersionsEligibleForCleanup();

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(1, results.Count);
                    Assert.AreEqual(1, results.First().VersionId);
                });
            }
        }

        [Test]
        public void GetDocumentVersionsEligibleForCleanup_Always_ExcludesPinnedVersions()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateSimpleContent(contentType);

            ServiceContext.ContentService.SaveAndPublish(content);
            // At this point content has 2 versions, a draft version and a published version.
            ServiceContext.ContentService.SaveAndPublish(content);
            ServiceContext.ContentService.SaveAndPublish(content);
            ServiceContext.ContentService.SaveAndPublish(content);
            // At this point content has 5 versions, 3 historic versions, a draft version and a published version.

            var allVersions = ServiceContext.ContentService.GetVersions(content.Id);
            Debug.Assert(allVersions.Count() == 5); // Sanity check

            var scopeProvider = TestObjects.GetScopeProvider(Logger);
            using (var scope = scopeProvider.CreateScope())
            {
                scope.Database.Update<ContentVersionDto>("set preventCleanup = 1 where id in (1,3)");

                var sut = new DocumentVersionRepository((IScopeAccessor)scopeProvider);
                var results = sut.GetDocumentVersionsEligibleForCleanup();

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(1, results.Count);

                    // We pinned 1 & 3
                    // 4 is current
                    // 5 is published
                    // So all that is left is 2
                    Assert.AreEqual(2, results.First().VersionId);
                });
            }
        }

        [Test]
        public void DeleteVersions_Always_DeletesSpecifiedVersions()
        {
            var contentType = MockedContentTypes.CreateSimpleContentType("umbTextpage", "Textpage");
            ServiceContext.FileService.SaveTemplate(contentType.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType);

            var content = MockedContent.CreateSimpleContent(contentType);

            ServiceContext.ContentService.SaveAndPublish(content);
            ServiceContext.ContentService.SaveAndPublish(content);
            ServiceContext.ContentService.SaveAndPublish(content);
            ServiceContext.ContentService.SaveAndPublish(content);

            var scopeProvider = TestObjects.GetScopeProvider(Logger);
            using (var scope = scopeProvider.CreateScope())
            {
                var query = scope.SqlContext.Sql();

                query.Select<ContentVersionDto>()
                    .From<ContentVersionDto>();

                var sut = new DocumentVersionRepository((IScopeAccessor)scopeProvider);
                sut.DeleteVersions(new []{1,2,3});

                var after = scope.Database.Fetch<ContentVersionDto>(query);

                Assert.Multiple(() =>
                {
                    Assert.AreEqual(2, after.Count);
                    Assert.True(after.All(x => x.Id > 3));
                });
            }
        }
    }
}
