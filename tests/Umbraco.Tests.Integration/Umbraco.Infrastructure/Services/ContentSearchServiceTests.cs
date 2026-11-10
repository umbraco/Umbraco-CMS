using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

// NOTE: this is not an exhaustive test suite for IContentSearchService, because the core implementation simply
//       wraps IContentService to search children (IContentService.GetPagedChildren), and that's tested elsewhere.
//       instead, these tests aim at testing the logic of the shared base between the core implementations of
//       IContentSearchService and IMediaSearchService (which does the same thing using IMediaService).
[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerFixture,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public class ContentSearchServiceTests : UmbracoIntegrationTest
{
    private Dictionary<string, IContent> _contentByName = new ();

    protected IContentSearchService ContentSearchService => GetRequiredService<IContentSearchService>();

    protected IContentService ContentService => GetRequiredService<IContentService>();

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    [Test]
    public async Task Can_Search_Children_Of_System_Root()
    {
        var result = await ContentSearchService.SearchChildrenAsync(null, null, null, 0, 1000);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(3, result.Total);
            Assert.AreEqual(3, result.Items.Count());
        });

        var resultKeys = result.Items.Select(item => item.Key).ToArray();
        var expectedKeys = new[]
        {
            _contentByName["Root 1"].Key,
            _contentByName["Root 2"].Key,
            _contentByName["Root 3"].Key
        };
        CollectionAssert.AreEqual(expectedKeys, resultKeys);
    }

    [Test]
    public async Task Can_Search_Children_Of_Specified_Parent()
    {
        var result = await ContentSearchService.SearchChildrenAsync(null, _contentByName["Root 1"].Key, null, 0, 1000);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(5, result.Total);
            Assert.AreEqual(5, result.Items.Count());
        });

        var resultKeys = result.Items.Select(item => item.Key).ToArray();
        var expectedKeys = new[]
        {
            _contentByName["Root 1/Child 1"].Key,
            _contentByName["Root 1/Child 2"].Key,
            _contentByName["Root 1/Child 3"].Key,
            _contentByName["Root 1/Child 4"].Key,
            _contentByName["Root 1/Child 5"].Key
        };
        CollectionAssert.AreEqual(expectedKeys, resultKeys);
    }

    [Test]
    public async Task Can_Apply_Pagination_With_Skip_Take()
    {
        var result = await ContentSearchService.SearchChildrenAsync(null, _contentByName["Root 2"].Key, Ordering.By("name"), 2, 2);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(5, result.Total);
            Assert.AreEqual(2, result.Items.Count());
        });

        var resultKeys = result.Items.Select(item => item.Key).ToArray();
        var expectedKeys = new[]
        {
            _contentByName["Root 2/Child 3"].Key,
            _contentByName["Root 2/Child 4"].Key
        };
        CollectionAssert.AreEqual(expectedKeys, resultKeys);
    }

    [Test]
    public async Task Can_Filter_By_Name()
    {
        var result = await ContentSearchService.SearchChildrenAsync("2", _contentByName["Root 3"].Key, null);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Total);
            Assert.AreEqual(1, result.Items.Count());
        });

        Assert.AreEqual(_contentByName["Root 3/Child 2"].Key, result.Items.First().Key);
    }

    [Test]
    public async Task Can_Order_By_Published_With_Culture()
    {
        // Ordering by "published" with a culture builds a CASE over the published flag and a 0/1 literal pair,
        // which must use the provider's boolean literals (providers with a native boolean type, e.g. PostgreSQL,
        // need both CASE branches to be boolean).
        // Its own parent and content types, so publishing here doesn't change what the fixture's other tests see.
        // An invariant child takes the THEN branch (the document's published flag); a culture-variant child takes
        // the ELSE branch (whether the culture has a published version).
        var invariantType = new ContentTypeBuilder()
            .WithAlias("publishedOrderingInvariant")
            .Build();
        await ContentTypeService.CreateAsync(invariantType, Constants.Security.SuperUserKey);

        var variantType = new ContentTypeBuilder()
            .WithAlias("publishedOrderingVariant")
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        await ContentTypeService.CreateAsync(variantType, Constants.Security.SuperUserKey);

        var parentType = new ContentTypeBuilder()
            .WithAlias("publishedOrderingParent")
            .Build();
        parentType.AllowedAsRoot = true;
        parentType.AllowedContentTypes =
        [
            new ContentTypeSort(invariantType.Key, 1, invariantType.Alias),
            new ContentTypeSort(variantType.Key, 2, variantType.Alias),
        ];
        await ContentTypeService.CreateAsync(parentType, Constants.Security.SuperUserKey);

        var parent = new ContentBuilder()
            .WithContentType(parentType)
            .WithName("Published ordering")
            .Build();
        ContentService.Save(parent);

        try
        {
            await AssertPublishedOrderingAsync(parent, invariantType, variantType);
        }
        finally
        {
            // Removed again so the fixture's other tests (the system root's children among them) see only their own content
            ContentService.Delete(parent);
            foreach (var type in new[] { parentType, invariantType, variantType })
            {
                await ContentTypeService.DeleteAsync(type.Key, Constants.Security.SuperUserKey);
            }
        }
    }

    private async Task AssertPublishedOrderingAsync(IContent parent, IContentType invariantType, IContentType variantType)
    {
        Assert.IsTrue(ContentService.Publish(parent, ["*"]).Success);

        IContent Invariant(string name)
        {
            var child = new ContentBuilder().WithContentType(invariantType).WithParent(parent).WithName(name).Build();
            ContentService.Save(child);
            return child;
        }

        IContent Variant(string name)
        {
            var child = new ContentBuilder().WithContentType(variantType).WithParent(parent).WithCultureName("en-US", name).Build();
            ContentService.Save(child);
            return child;
        }

        var publishedInvariant = Invariant("Invariant published");
        Invariant("Invariant draft");
        var publishedVariant = Variant("Variant published");
        Variant("Variant draft");
        Assert.IsTrue(ContentService.Publish(publishedInvariant, ["*"]).Success);
        Assert.IsTrue(ContentService.Publish(publishedVariant, ["en-US"]).Success);

        var result = await ContentSearchService.SearchChildrenAsync(
            null,
            parent.Key,
            Ordering.By("published", Direction.Descending, culture: "en-US"),
            0,
            1000);
        Assert.AreEqual(4, result.Total);

        // The published invariant and published variant children come first
        CollectionAssert.AreEquivalent(
            new[] { publishedInvariant.Key, publishedVariant.Key },
            result.Items.Take(2).Select(item => item.Key).ToArray());
    }

    [SetUp]
    public async Task SetUpTest()
    {
        if (_contentByName.Any())
        {
            return;
        }

        var contentType = new ContentTypeBuilder()
            .WithAlias("theContentType")
            .Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new() { Alias = contentType.Alias, Key = contentType.Key }];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);
        foreach (var rootNumber in Enumerable.Range(1, 3))
        {
            var root = new ContentBuilder()
                .WithContentType(contentType)
                .WithName($"Root {rootNumber}")
                .Build();
            ContentService.Save(root);
            _contentByName[root.Name!] = root;

            foreach (var childNumber in Enumerable.Range(1, 5))
            {
                var child = new ContentBuilder()
                    .WithContentType(contentType)
                    .WithParent(root)
                    .WithName($"Child {childNumber}")
                    .Build();
                ContentService.Save(child);
                _contentByName[$"{root.Name!}/{child.Name!}"] = child;
            }
        }
    }
}
