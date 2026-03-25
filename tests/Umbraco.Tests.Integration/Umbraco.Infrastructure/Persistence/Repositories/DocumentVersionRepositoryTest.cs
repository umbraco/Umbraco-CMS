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

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DocumentVersionRepositoryTest : UmbracoIntegrationTest
{
    public ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    public IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    public IContentService ContentService => GetRequiredService<IContentService>();

    [Test]
    public async Task GetDocumentVersionsEligibleForCleanup_Always_ExcludesActiveVersions()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Cms.Core.Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);

        ContentService.Publish(content, []);
        // At this point content has 2 versions, a draft version and a published version.

        ContentService.Publish(content, []);
        // At this point content has 3 versions, a historic version, a draft version and a published version.

        using (ScopeProvider.CreateScope())
        {
            var sut = new DocumentVersionRepository(ScopeAccessor);
            var results = sut.GetDocumentVersionsEligibleForCleanup(DateTime.UtcNow.AddDays(1), null);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual(1, results.First().VersionId);
            });
        }
    }

    [Test]
    public async Task GetDocumentVersionsEligibleForCleanup_Always_ExcludesPinnedVersions()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Cms.Core.Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);

        ContentService.Publish(content, []);
        // At this point content has 2 versions, a draft version and a published version.

        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
        // At this point content has 5 versions, 3 historic versions, a draft version and a published version.

        var allVersions = ContentService.GetVersions(content.Id);
        Debug.Assert(allVersions.Count() == 5, "Expected 5 versions for sanity check.");

        using (var scope = ScopeProvider.CreateScope())
        {
            int[] values = [1, 3];
            var sql = scope.SqlContext.Sql()
                .Update<ContentVersionDto>(u => u.Set(c => c.PreventCleanup, true))
                .WhereIn<ContentVersionDto>(c => c.Id, values);
            scope.Database.Execute(sql);

            var sut = new DocumentVersionRepository(ScopeAccessor);
            var results = sut.GetDocumentVersionsEligibleForCleanup(DateTime.UtcNow.AddDays(1), null);

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
    public async Task GetDocumentVersionsEligibleForCleanup_WithDateFilter_OnlyReturnsOlderVersions()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Cms.Core.Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        // Create 4 versions in total: 2 historic (1, 2), 1 current draft (3), 1 current published (4).
        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);

        using (ScopeProvider.CreateScope())
        {
            var db = ScopeAccessor.AmbientScope!.Database;
            var syntax = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax;

            // Backdate version 1 to 10 days ago, leave version 2 at today.
            db.Execute(
                $"UPDATE {syntax.GetQuotedTableName("umbracoContentVersion")} SET {syntax.GetQuotedColumnName("versionDate")} = @0 WHERE id = 1",
                DateTime.UtcNow.AddDays(-10));

            var sut = new DocumentVersionRepository(ScopeAccessor);

            // Cutoff at 5 days ago — should only return version 1.
            var results = sut.GetDocumentVersionsEligibleForCleanup(DateTime.UtcNow.AddDays(-5), null);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual(1, results.First().VersionId);
            });
        }
    }

    [Test]
    public async Task GetDocumentVersionsEligibleForCleanup_WithMaxCount_RespectsLimitAndReturnsOldestFirst()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Cms.Core.Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        // Create 5 versions in total: 3 historic (1, 2, 3), 1 current draft (4), 1 current published (5).
        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);

        using (ScopeProvider.CreateScope())
        {
            var db = ScopeAccessor.AmbientScope!.Database;
            var syntax = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax;

            // Backdate all historic versions so they pass the date filter.
            db.Execute(
                $"UPDATE {syntax.GetQuotedTableName("umbracoContentVersion")} SET {syntax.GetQuotedColumnName("versionDate")} = @0 WHERE id IN (1, 2, 3)",
                DateTime.UtcNow.AddDays(-10));

            var sut = new DocumentVersionRepository(ScopeAccessor);

            // Request at most 2 — should return the 2 oldest of the 3 eligible.
            var results = sut.GetDocumentVersionsEligibleForCleanup(DateTime.UtcNow, 2);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, results.Count);

                // Should be ordered oldest first (by versionDate ASC, and since all have same date, by id ASC).
                var ids = results.Select(x => x.VersionId).ToList();
                Assert.That(ids, Is.Ordered.Ascending);
            });
        }
    }

    [Test]
    public async Task DeleteVersions_Always_DeletesSpecifiedVersions()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Cms.Core.Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);

        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
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
    public async Task DeleteVersions_VerifiesCascadeDeletion()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Cms.Core.Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);

        ContentService.Publish(content, []);
        ContentService.Publish(content, []);
        ContentService.Publish(content, []);

        // At this point content has 4 versions: 2 historic, 1 draft, 1 published.
        using (var scope = ScopeProvider.CreateScope())
        {
            var db = ScopeAccessor.AmbientScope!.Database;
            var syntax = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax;

            // Verify initial state has rows in all relevant tables
            var beforeContentVersions = db.Single<int>($"SELECT count(1) FROM {syntax.GetQuotedTableName("umbracoContentVersion")}");
            var beforeDocumentVersions = db.Single<int>($"SELECT count(1) FROM {syntax.GetQuotedTableName("umbracoDocumentVersion")}");
            var beforePropertyData = db.Single<int>($"SELECT count(1) FROM {syntax.GetQuotedTableName("umbracoPropertyData")}");

            Assert.Greater(beforeContentVersions, 2, "Should have more than 2 content versions before delete");
            Assert.Greater(beforeDocumentVersions, 2, "Should have more than 2 document versions before delete");
            Assert.Greater(beforePropertyData, 6, "Should have more than 6 property data rows before delete");

            var sut = new DocumentVersionRepository(ScopeAccessor);
            // Delete the 2 historic versions (IDs 1 and 2)
            sut.DeleteVersions(new[] { 1, 2 });

            var afterContentVersions = db.Single<int>($"SELECT count(1) FROM {syntax.GetQuotedTableName("umbracoContentVersion")}");
            var afterDocumentVersions = db.Single<int>($"SELECT count(1) FROM {syntax.GetQuotedTableName("umbracoDocumentVersion")}");
            var afterPropertyData = db.Single<int>($"SELECT count(1) FROM {syntax.GetQuotedTableName("umbracoPropertyData")}");
            var afterCultureVariation = db.Single<int>($"SELECT count(1) FROM {syntax.GetQuotedTableName("umbracoContentVersionCultureVariation")} WHERE {syntax.GetQuotedColumnName("versionId")} IN (1, 2)");

            Assert.Multiple(() =>
            {
                // 2 versions deleted, 2 remain (current draft + current published)
                Assert.AreEqual(beforeContentVersions - 2, afterContentVersions);
                Assert.AreEqual(beforeDocumentVersions - 2, afterDocumentVersions);

                // CreateSimpleContentType has 3 properties, so 6 property data rows should be removed
                Assert.AreEqual(beforePropertyData - 6, afterPropertyData);

                // No culture variation rows for the deleted versions
                Assert.AreEqual(0, afterCultureVariation);
            });
        }
    }

    [Test]
    public async Task GetPagedItemsByContentId_WithInvariantCultureContent_ReturnsPaginatedResults()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Cms.Core.Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType);
        ContentService.Save(content);

        ContentService.Publish(content, []); // Draft + Published
        ContentService.Publish(content, []); // New Draft

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
    public async Task GetPagedItemsByContentId_WithVariantCultureContent_ReturnsPaginatedResults()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Cms.Core.Constants.Security.SuperUserKey);

        var contentType =
            ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
        contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.UpdateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        var content = ContentBuilder.CreateSimpleContent(contentType, "foo", culture: "en-US");
        content.SetCultureName("foo", "en-US");

        ContentService.Save(content);
        ContentService.Publish(content, ["en-US"]); // Draft + Published
        ContentService.Publish(content, ["en-US"]); // New Draft

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
