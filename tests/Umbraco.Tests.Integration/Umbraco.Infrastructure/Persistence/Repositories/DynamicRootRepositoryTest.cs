using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class DynamicRootRepositoryTest : UmbracoIntegrationTest
{
    // More origins than one query can bind alongside a single document type, so they are resolved in several batches.
    private const int OriginCount = 1000;

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IDynamicRootRepository DynamicRootRepository => GetRequiredService<IDynamicRootRepository>();

    private IContentType ParentContentType { get; set; } = null!;

    private IContentType ItemContentType { get; set; } = null!;

    private IContent Parent { get; set; } = null!;

    private Guid[] ItemKeys { get; set; } = [];

    [SetUp]
    public async Task Setup()
    {
        ITemplate template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        ParentContentType = ContentTypeBuilder.CreateSimpleContentType("parent", "Parent", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(ParentContentType, Constants.Security.SuperUserKey);
        ItemContentType = ContentTypeBuilder.CreateSimpleContentType("item", "Item", defaultTemplateId: template.Id);
        await ContentTypeService.CreateAsync(ItemContentType, Constants.Security.SuperUserKey);

        Parent = ContentBuilder.CreateSimpleContent(ParentContentType, "Parent");
        ContentService.Save(Parent);

        Content[] items = Enumerable.Range(0, OriginCount)
            .Select(index => ContentBuilder.CreateSimpleContent(ItemContentType, $"Item {index}", Parent.Id))
            .ToArray();
        ContentService.Save(items);
        ItemKeys = items.Select(item => item.Key).ToArray();
    }

    [Test]
    public async Task NearestDescendantOrSelfAsync_Resolves_Every_Origin_Across_Batches()
    {
        var queryStep = new DynamicRootQueryStep { Alias = "NearestDescendantOrSelf", AnyOfDocTypeKeys = [ItemContentType.Key] };

        ICollection<Guid> result;
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            result = await DynamicRootRepository.NearestDescendantOrSelfAsync(ItemKeys, queryStep);
        }

        Assert.That(result, Is.EquivalentTo(ItemKeys));
    }

    [Test]
    public async Task NearestAncestorsOrSelfAsync_Returns_A_Shared_Ancestor_Once_Across_Batches()
    {
        var queryStep = new DynamicRootQueryStep { Alias = "NearestAncestorOrSelf", AnyOfDocTypeKeys = [ParentContentType.Key] };

        ICollection<Guid> result;
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            result = await DynamicRootRepository.NearestAncestorsOrSelfAsync(ItemKeys, queryStep);
        }

        Assert.That(result, Is.EquivalentTo(new[] { Parent.Key }));
    }
}
