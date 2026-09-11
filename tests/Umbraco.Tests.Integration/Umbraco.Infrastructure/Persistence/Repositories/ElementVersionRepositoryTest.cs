using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core;
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
    public async Task GetElementVersionsEligibleForCleanup_Always_ExcludesActiveVersions()
    {
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        var content = ElementBuilder.CreateSimpleElement(contentType);
        ElementService.Save(content);

        ElementService.Publish(content, Array.Empty<string>());
        // At this point content has 2 versions, a draft version and a published version.

        ElementService.Publish(content, Array.Empty<string>());
        // At this point content has 3 versions, a historic version, a draft version and a published version.

        using (ScopeProvider.CreateScope())
        {
            var sut = new ElementVersionRepository(ScopeAccessor);
            var results = sut.GetContentVersionsEligibleForCleanup(DateTime.UtcNow.AddDays(1), null);

            Assert.Multiple(() =>
            {
                Assert.That(results, Has.Count.EqualTo(1));
                Assert.That(results.First().VersionId, Is.EqualTo(1));
            });
        }
    }

    [Test]
    public async Task GetElementVersionsEligibleForCleanup_Always_ExcludesPinnedVersions()
    {
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

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
            var results = sut.GetContentVersionsEligibleForCleanup(DateTime.UtcNow.AddDays(1), null);

            Assert.Multiple(() =>
            {
                Assert.That(results, Has.Count.EqualTo(1));

                // We pinned 1 & 3
                // 4 is current
                // 5 is published
                // So all that is left is 2
                Assert.That(results.First().VersionId, Is.EqualTo(2));
            });
        }
    }

    [Test]
    public async Task DeleteVersions_Always_DeletesSpecifiedVersions()
    {
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

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
                Assert.That(after, Has.Count.EqualTo(2));
                Assert.That(after.All(x => x.Id > 3), Is.True);
            });
        }
    }


    [Test]
    public async Task GetPagedItemsByContentId_WithInvariantCultureContent_ReturnsPaginatedResults()
    {
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

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
                Assert.That(page1.Count(), Is.EqualTo(2));
                Assert.That(page1Total, Is.EqualTo(3));

                Assert.That(page2.Count(), Is.EqualTo(1));
                Assert.That(page2Total, Is.EqualTo(3));
            });
        }
    }

    [Test]
    public async Task GetPagedItemsByContentId_WithVariantCultureContent_ReturnsPaginatedResults()
    {
        var contentType = ContentTypeBuilder.CreateSimpleElementType();
        contentType.Variations = ContentVariation.Culture;
        foreach (var propertyType in contentType.PropertyTypes)
        {
            propertyType.Variations = ContentVariation.Culture;
        }
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

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
                Assert.That(page1.Count(), Is.EqualTo(2));
                Assert.That(page1Total, Is.EqualTo(3));

                Assert.That(page2.Count(), Is.EqualTo(1));
                Assert.That(page2Total, Is.EqualTo(3));
            });
        }
    }
}
