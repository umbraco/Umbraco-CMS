using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

/// <remarks>
///     v9 -> Tests.Integration
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class DocumentVersionRepositoryTest : UmbracoIntegrationTest
{
    public IFileService FileService => GetRequiredService<IFileService>();
    public IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
    public IContentService ContentService => GetRequiredService<IContentService>();

    [Test]
    public void GetDocumentVersionsEligibleForCleanup_Always_ExcludesActiveVersions()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType);

        ContentService.SaveAndPublish(content);
        // At this point content has 2 versions, a draft version and a published version.

        ContentService.SaveAndPublish(content);
        // At this point content has 3 versions, a historic version, a draft version and a published version.

        using (ScopeProvider.CreateScope())
        {
            var sut = new DocumentVersionRepository(ScopeAccessor);
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
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType);

        ContentService.SaveAndPublish(content);
        // At this point content has 2 versions, a draft version and a published version.
        ContentService.SaveAndPublish(content);
        ContentService.SaveAndPublish(content);
        ContentService.SaveAndPublish(content);
        // At this point content has 5 versions, 3 historic versions, a draft version and a published version.

        var allVersions = ContentService.GetVersions(content.Id);
        Debug.Assert(allVersions.Count() == 5); // Sanity check

        using (var scope = ScopeProvider.CreateScope())
        {
            ScopeAccessor.AmbientScope.Database.Update<ContentVersionDto>("set preventCleanup = 1 where id in (1,3)");

            var sut = new DocumentVersionRepository(ScopeAccessor);
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
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType);

        ContentService.SaveAndPublish(content);
        ContentService.SaveAndPublish(content);
        ContentService.SaveAndPublish(content);
        ContentService.SaveAndPublish(content);
        using (var scope = ScopeProvider.CreateScope())
        {
            var query = ScopeAccessor.AmbientScope.SqlContext.Sql();

            query.Select<ContentVersionDto>()
                .From<ContentVersionDto>();

            var sut = new DocumentVersionRepository(ScopeAccessor);
            sut.DeleteVersions(new[] { 1, 2, 3 });

            var after = ScopeAccessor.AmbientScope.Database.Fetch<ContentVersionDto>(query);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, after.Count);
                Assert.True(after.All(x => x.Id > 3));
            });
        }
    }


    [Test]
    public void GetPagedItemsByContentId_WithInvariantCultureContent_ReturnsPaginatedResults()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType);

        ContentService.SaveAndPublish(content); // Draft + Published
        ContentService.SaveAndPublish(content); // New Draft

        using (ScopeProvider.CreateScope())
        {
            var sut = new DocumentVersionRepository((IScopeAccessor)ScopeProvider);
            var page1 = sut.GetPagedItemsByContentId(content.Id, 0, 2, out var page1Total);
            var page2 = sut.GetPagedItemsByContentId(content.Id, 1, 2, out var page2Total);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, page1.Count());
                Assert.AreEqual(3, page1Total);

                Assert.AreEqual(1, page2.Count());
                Assert.AreEqual(3, page2Total);
            });
        }
    }

    [Test]
    public void GetPagedItemsByContentId_WithVariantCultureContent_ReturnsPaginatedResults()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        FileService.SaveTemplate(contentType.DefaultTemplate);
        ContentTypeService.Save(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType, "foo", culture: "en-US");
        content.SetCultureName("foo", "en-US");

        ContentService.SaveAndPublish(content, "en-US"); // Draft + Published
        ContentService.SaveAndPublish(content, "en-US"); // New Draft

        using (ScopeProvider.CreateScope())
        {
            var sut = new DocumentVersionRepository((IScopeAccessor)ScopeProvider);
            var page1 = sut.GetPagedItemsByContentId(content.Id, 0, 2, out var page1Total, 1);
            var page2 = sut.GetPagedItemsByContentId(content.Id, 1, 2, out var page2Total, 1);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, page1.Count());
                Assert.AreEqual(3, page1Total);

                Assert.AreEqual(1, page2.Count());
                Assert.AreEqual(3, page2Total);
            });
        }
    }
}
