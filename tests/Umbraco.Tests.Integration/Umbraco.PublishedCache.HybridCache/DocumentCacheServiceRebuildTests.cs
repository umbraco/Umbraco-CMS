// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.PublishedCache.HybridCache;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
internal sealed class DocumentCacheServiceRebuildTests : UmbracoIntegrationTestWithContent
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddUmbracoHybridCache();
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();

        // Use JSON to allow easier verification of data.
        builder.Services.PostConfigure<NuCacheSettings>(options => options.NuCacheSerializerType = NuCacheSerializerType.JSON);
    }

    private ISqlContext SqlContext => GetRequiredService<ISqlContext>();

    private IDocumentCacheService DocumentCacheService => GetRequiredService<IDocumentCacheService>();

    [Test]
    public void Rebuild_Creates_CmsContentNu_Records_For_ContentType()
    {
        // Arrange - Content is created in base class Setup()
        // The base class creates: Textpage, Subpage, Subpage2, Subpage3 (all using ContentType)

        // - publish the root page to ensure we have published and draft content
        ContentService.Publish(Textpage, ["*"]);

        // Act - Call Rebuild for the ContentType
        DocumentCacheService.Rebuild([ContentType.Id]);

        // Assert - Verify cmsContentNu table has records for the content items
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>();

            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            var draftDtos = dtos.Where(d => !d.Published).ToList();
            var publishedDtos = dtos.Where(d => d.Published).ToList();

            // Verify we have draft records for non-trashed content
            // Textpage, Subpage, Subpage2, Subpage3 should have draft cache entries
            Assert.That(draftDtos, Has.Count.GreaterThanOrEqualTo(4), "Expected at least 4 draft cache records");

            // Verify we have published records for published content
            // Textpage, Subpage, Subpage2, Subpage3 should have draft cache entries
            Assert.AreEqual(1, publishedDtos.Count, "Expected 1 published cache record");

            // Verify specific content items have cache entries
            var nodeIds = draftDtos.Select(d => d.NodeId).ToList();
            Assert.That(nodeIds, Does.Contain(Textpage.Id), "Textpage should have draft cache entry");
            Assert.That(nodeIds, Does.Contain(Subpage.Id), "Subpage should have draft cache entry");
            Assert.That(nodeIds, Does.Contain(Subpage2.Id), "Subpage2 should have draft cache entry");
            Assert.That(nodeIds, Does.Contain(Subpage3.Id), "Subpage3 should have draft cache entry");

            nodeIds = [.. publishedDtos.Select(d => d.NodeId)];
            Assert.That(nodeIds, Does.Contain(Textpage.Id), "Textpage should have published cache entry");
            Assert.That(nodeIds, Has.No.Member(Subpage.Id), "Subpage should have not have published cache entry");

            // Verify cache data is not empty
            var textpageDto = dtos.First(d => d.NodeId == Textpage.Id);
            Assert.That(textpageDto.Data, Is.Not.Null.And.Not.Empty, "Cache data should not be empty");
        }
    }

    [Test]
    public void Rebuild_Replaces_Existing_CmsContentNu_Records()
    {
        // Arrange - First rebuild to create initial records
        DocumentCacheService.Rebuild([ContentType.Id]);

        // Get initial data
        string initialData;
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == Textpage.Id && !x.Published);
            var dto = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql).FirstOrDefault();
            Assert.That(dto, Is.Not.Null);
            initialData = dto!.Data!;
        }

        // Modify content
        Textpage.SetValue("title", "Modified Title For Rebuild Test");
        ContentService.Save(Textpage);

        // Act - Rebuild again
        DocumentCacheService.Rebuild([ContentType.Id]);

        // Assert - Verify record was updated (not duplicated)
        using (var scope = ScopeProvider.CreateScope(autoComplete: true))
        {
            var selectSql = SqlContext.Sql()
                .Select<ContentNuDto>()
                .From<ContentNuDto>()
                .Where<ContentNuDto>(x => x.NodeId == Textpage.Id && !x.Published);
            var dtos = ScopeAccessor.AmbientScope!.Database.Fetch<ContentNuDto>(selectSql);

            // Should have exactly one draft record (no duplicates)
            Assert.That(dtos, Has.Count.EqualTo(1), "Should have exactly one draft cache record");

            // Data should be different (updated)
            var updatedData = dtos[0].Data;
            Assert.That(updatedData, Does.Contain("Modified Title For Rebuild Test"), "Cache data should contain the modified title");
        }
    }
}
