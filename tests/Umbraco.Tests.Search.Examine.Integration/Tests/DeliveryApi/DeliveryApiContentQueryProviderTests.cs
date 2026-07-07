using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Search.DeliveryApi.DependencyInjection;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.DeliveryApi;

/// <summary>
/// Tests the Umbraco Search based implementation of <see cref="IApiContentQueryProvider"/>
/// (<c>DeliveryApiContentQueryProvider</c>), which replaces the Examine based core implementation
/// when Delivery API querying is flipped to Umbraco Search.
/// </summary>
public class DeliveryApiContentQueryProviderTests : TestBase
{
    private static readonly Guid RootAlphaKey = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid RootZuluKey = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ChildBetaKey = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ChildGammaKey = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid GrandchildDeltaKey = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private const string DefaultCulture = "en-US";

    private IApiContentQueryProvider Provider => GetRequiredService<IApiContentQueryProvider>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddDeliveryApiSearch();
    }

    [Test]
    public async Task AllContentSelector_ReturnsAllPublishedContent()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(Provider.AllContentSelectorOption());

        Assert.That(result.Total, Is.EqualTo(5));
    }

    [Test]
    public async Task ChildrenSelector_ReturnsDirectChildrenOnly()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(new SelectorOption
        {
            FieldName = "parentId",
            Values = [RootAlphaKey.ToString("D")],
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Items, Is.EquivalentTo(new[] { ChildBetaKey, ChildGammaKey }));
        });
    }

    [Test]
    public async Task ChildrenSelector_ForLeafNode_ReturnsNothing()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(new SelectorOption
        {
            FieldName = "parentId",
            Values = [GrandchildDeltaKey.ToString("D")],
        });

        Assert.That(result.Total, Is.EqualTo(0));
    }

    [Test]
    public async Task DescendantsSelector_ReturnsDescendants_CurrentlyIncludingSelf()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(new SelectorOption
        {
            FieldName = "ancestorIds",
            Values = [RootAlphaKey.ToString("D")],
        });

        // NOTE: "ancestorIds" maps to the PathIds system field, which is ancestors-OR-SELF - so the root
        //       itself is currently included. This pins the known TODO in DeliveryApiContentQueryProvider
        //       ("PathIds equals ancestors-or-self, but the Delivery API queries for ancestors only").
        //       If that TODO gets fixed, this test should change to expect only the three descendants.
        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(4));
            Assert.That(result.Items, Is.EquivalentTo(new[] { RootAlphaKey, ChildBetaKey, ChildGammaKey, GrandchildDeltaKey }));
        });
    }

    [Test]
    public async Task AncestorsSelector_ReturnsRequestedItems()
    {
        await CreatePublishedSiteStructure();

        // the AncestorsSelector handler resolves the ancestor keys of the requested item up front;
        // the provider is only asked to fetch documents by those ids ("itemId" maps to the Id system field)
        PagedModel<Guid> result = ExecuteQuery(new SelectorOption
        {
            FieldName = "itemId",
            Values = [RootAlphaKey.ToString("D"), ChildBetaKey.ToString("D")],
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Items, Is.EquivalentTo(new[] { RootAlphaKey, ChildBetaKey }));
        });
    }

    [Test]
    public async Task ContentTypeFilter_Is_FiltersByAlias()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(
            Provider.AllContentSelectorOption(),
            new FilterOption { FieldName = "contentType", Values = ["landing"], Operator = FilterOperation.Is });

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Items.Single(), Is.EqualTo(RootZuluKey));
        });
    }

    [Test]
    public async Task ContentTypeFilter_IsNot_ExcludesByAlias()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(
            Provider.AllContentSelectorOption(),
            new FilterOption { FieldName = "contentType", Values = ["landing"], Operator = FilterOperation.IsNot });

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(4));
            Assert.That(result.Items, Does.Not.Contain(RootZuluKey));
        });
    }

    [Test]
    public async Task NameFilter_Contains_MatchesAnalyzedText()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(
            Provider.AllContentSelectorOption(),
            new FilterOption { FieldName = "name", Values = ["Page"], Operator = FilterOperation.Contains });

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(3));
            Assert.That(result.Items, Is.EquivalentTo(new[] { ChildBetaKey, ChildGammaKey, GrandchildDeltaKey }));
        });
    }

    [Test]
    public async Task NameFilter_DoesNotContain_ExcludesAnalyzedText()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(
            Provider.AllContentSelectorOption(),
            new FilterOption { FieldName = "name", Values = ["Page"], Operator = FilterOperation.DoesNotContain });

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(2));
            Assert.That(result.Items, Is.EquivalentTo(new[] { RootAlphaKey, RootZuluKey }));
        });
    }

    [TestCase(Direction.Ascending)]
    [TestCase(Direction.Descending)]
    public async Task SortOrderSort_OrdersChildren(Direction direction)
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(
            new SelectorOption { FieldName = "parentId", Values = [RootAlphaKey.ToString("D")] },
            sortOption: new SortOption { FieldName = "sortOrder", Direction = direction });

        Guid[] expected = direction is Direction.Ascending
            ? [ChildBetaKey, ChildGammaKey]
            : [ChildGammaKey, ChildBetaKey];

        Assert.That(result.Items, Is.EqualTo(expected).AsCollection);
    }

    [Test]
    public async Task Pagination_SkipTake_ReturnsSliceAndKeepsTotal()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> firstPage = ExecuteQuery(
            new SelectorOption { FieldName = "ancestorIds", Values = [RootAlphaKey.ToString("D")] },
            sortOption: new SortOption { FieldName = "sortOrder", Direction = Direction.Ascending },
            skip: 0,
            take: 2);
        PagedModel<Guid> secondPage = ExecuteQuery(
            new SelectorOption { FieldName = "ancestorIds", Values = [RootAlphaKey.ToString("D")] },
            sortOption: new SortOption { FieldName = "sortOrder", Direction = Direction.Ascending },
            skip: 2,
            take: 2);

        Assert.Multiple(() =>
        {
            Assert.That(firstPage.Total, Is.EqualTo(4));
            Assert.That(secondPage.Total, Is.EqualTo(4));
            Assert.That(firstPage.Items.Count(), Is.EqualTo(2));
            Assert.That(secondPage.Items.Count(), Is.EqualTo(2));
            Assert.That(firstPage.Items.Concat(secondPage.Items), Is.Unique);
        });
    }

    [Test]
    public async Task UnknownFieldFilter_IsIgnored()
    {
        await CreatePublishedSiteStructure();

        PagedModel<Guid> result = ExecuteQuery(
            Provider.AllContentSelectorOption(),
            new FilterOption { FieldName = "noSuchField", Values = ["whatever"], Operator = FilterOperation.Is });

        // filters for unknown fields cannot be resolved to an index field type and are skipped (with a logged warning)
        Assert.That(result.Total, Is.EqualTo(5));
    }

    private PagedModel<Guid> ExecuteQuery(
        SelectorOption selectorOption,
        FilterOption? filterOption = null,
        SortOption? sortOption = null,
        int skip = 0,
        int take = 100)
        => Provider.ExecuteQuery(
            selectorOption,
            filterOption is not null ? [filterOption] : [],
            sortOption is not null ? [sortOption] : [],
            DefaultCulture,
            ProtectedAccess.None,
            preview: false,
            skip: skip,
            take: take);

    private async Task CreatePublishedSiteStructure()
    {
        var indexAlias = GetIndexAlias(true);
        await WaitForIndexing(indexAlias, async () =>
        {
            IContentType pageType = new ContentTypeBuilder()
                .WithAlias("page")
                .WithName("Page")
                .Build();
            await ContentTypeService.CreateAsync(pageType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);
            pageType.AllowedContentTypes = [new ContentTypeSort(pageType.Key, 0, pageType.Alias)];
            await ContentTypeService.UpdateAsync(pageType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);

            IContentType landingType = new ContentTypeBuilder()
                .WithAlias("landing")
                .WithName("Landing")
                .Build();
            await ContentTypeService.CreateAsync(landingType, Umbraco.Cms.Core.Constants.Security.SuperUserKey);

            Content rootAlpha = new ContentBuilder()
                .WithKey(RootAlphaKey)
                .WithContentType(pageType)
                .WithName("Alpha Site")
                .Build();
            SaveAndPublish(rootAlpha);

            Content rootZulu = new ContentBuilder()
                .WithKey(RootZuluKey)
                .WithContentType(landingType)
                .WithName("Zulu Site")
                .Build();
            SaveAndPublish(rootZulu);

            Content childBeta = new ContentBuilder()
                .WithKey(ChildBetaKey)
                .WithContentType(pageType)
                .WithParent(rootAlpha)
                .WithName("Beta Page")
                .Build();
            SaveAndPublish(childBeta);

            Content childGamma = new ContentBuilder()
                .WithKey(ChildGammaKey)
                .WithContentType(pageType)
                .WithParent(rootAlpha)
                .WithName("Gamma Page")
                .Build();
            SaveAndPublish(childGamma);

            Content grandchildDelta = new ContentBuilder()
                .WithKey(GrandchildDeltaKey)
                .WithContentType(pageType)
                .WithParent(childBeta)
                .WithName("Delta Page")
                .Build();
            SaveAndPublish(grandchildDelta);
        });
    }
}
