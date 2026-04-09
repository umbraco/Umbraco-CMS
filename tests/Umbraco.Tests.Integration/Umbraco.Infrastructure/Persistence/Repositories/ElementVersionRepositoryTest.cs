using System.Diagnostics;
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
internal sealed class ElementVersionRepositoryTest : UmbracoIntegrationTest
{
    public IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    public IElementService ElementService => GetRequiredService<IElementService>();

    [Test]
    public void GetElementVersionsEligibleForCleanup_Always_ExcludesActiveVersions()
    {
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        ContentTypeService.Save(contentType);

        var content = ElementBuilder.CreateSimpleElement(contentType);
        ElementService.Save(content);

        ElementService.Publish(content, Array.Empty<string>());
        // At this point content has 2 versions, a draft version and a published version.

        ElementService.Publish(content, Array.Empty<string>());
        // At this point content has 3 versions, a historic version, a draft version and a published version.

        using (ScopeProvider.CreateScope())
        {
            var sut = new ElementVersionRepository(ScopeAccessor);
            var results = sut.GetContentVersionsEligibleForCleanup();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual(1, results.First().VersionId);
            });
        }
    }

    [Test]
    public void GetElementVersionsEligibleForCleanup_Always_ExcludesPinnedVersions()
    {
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        ContentTypeService.Save(contentType);

        var content = ElementBuilder.CreateSimpleElement(contentType);
        ElementService.Save(content);

        ElementService.Publish(content, Array.Empty<string>());
        // At this point content has 2 versions, a draft version and a published version.
        ElementService.Publish(content, Array.Empty<string>());
        ElementService.Publish(content, Array.Empty<string>());
        ElementService.Publish(content, Array.Empty<string>());
        // At this point content has 5 versions, 3 historic versions, a draft version and a published version.

        var allVersions = ElementService.GetVersions(content.Id);
        Debug.Assert(allVersions.Count() == 5); // Sanity check

        using (var scope = ScopeProvider.CreateScope())
        {
            ScopeAccessor.AmbientScope.Database.Update<ContentVersionDto>("set preventCleanup = 1 where id in (1,3)");

            var sut = new ElementVersionRepository(ScopeAccessor);
            var results = sut.GetContentVersionsEligibleForCleanup();

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
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        ContentTypeService.Save(contentType);

        var content = ElementBuilder.CreateSimpleElement(contentType);
        ElementService.Save(content);

        ElementService.Publish(content, Array.Empty<string>());
        ElementService.Publish(content, Array.Empty<string>());
        ElementService.Publish(content, Array.Empty<string>());
        ElementService.Publish(content, Array.Empty<string>());
        using (var scope = ScopeProvider.CreateScope())
        {
            var query = ScopeAccessor.AmbientScope.SqlContext.Sql();

            query.Select<ContentVersionDto>()
                .From<ContentVersionDto>();

            var sut = new ElementVersionRepository(ScopeAccessor);
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
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        ContentTypeService.Save(contentType);

        var content = ElementBuilder.CreateSimpleElement(contentType);
        ElementService.Save(content);

        ElementService.Publish(content, Array.Empty<string>()); // Draft + Published
        ElementService.Publish(content, Array.Empty<string>()); // New Draft

        using (ScopeProvider.CreateScope())
        {
            var sut = new ElementVersionRepository((IScopeAccessor)ScopeProvider);
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
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }
        ContentTypeService.Save(contentType);

        var content = ElementBuilder.CreateSimpleElement(contentType, "foo", culture: "en-US");
        content.SetCultureName("foo", "en-US");

        ElementService.Save(content);
        ElementService.Publish(content, new[] { "en-US" }); // Draft + Published
        ElementService.Publish(content, new[] { "en-US" }); // New Draft

        using (ScopeProvider.CreateScope())
        {
            var sut = new ElementVersionRepository((IScopeAccessor)ScopeProvider);
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
