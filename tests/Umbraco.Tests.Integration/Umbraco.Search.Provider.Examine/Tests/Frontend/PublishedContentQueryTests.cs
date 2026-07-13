using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.Frontend;

/// <summary>
/// Tests the Umbraco Search based template search APIs: the reimplemented
/// <see cref="IPublishedContentQuery.Search(string, int, int, out long, string, string, ISet{string}?)"/>
/// and the <c>SearchChildren</c>/<c>SearchDescendants</c> extensions.
/// </summary>
public class PublishedContentQueryTests : TestBase
{
    private static readonly Guid RootAlphaKey = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ChildBetaKey = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid ChildGammaKey = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid GrandchildDeltaKey = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private IPublishedContentQuery PublishedContentQuery => GetRequiredService<IPublishedContentQuery>();

    private IPublishedContentCache PublishedContentCache => GetRequiredService<IPublishedContentCache>();

    [Test]
    public async Task Search_FindsPublishedContentByTerm()
    {
        await CreatePublishedSiteStructure();

        PublishedSearchResult[] results = PublishedContentQuery.Search("Beta").ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(results, Has.Length.EqualTo(1));
            Assert.That(results[0].Content.Key, Is.EqualTo(ChildBetaKey));
        });
    }

    [Test]
    public async Task Search_PagesResultsAndReportsTotal()
    {
        await CreatePublishedSiteStructure();

        PublishedSearchResult[] firstPage = PublishedContentQuery.Search("Page", 0, 2, out var totalRecords).ToArray();
        PublishedSearchResult[] secondPage = PublishedContentQuery.Search("Page", 2, 2, out var totalRecordsSecondPage).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(totalRecords, Is.EqualTo(3));
            Assert.That(totalRecordsSecondPage, Is.EqualTo(3));
            Assert.That(firstPage, Has.Length.EqualTo(2));
            Assert.That(secondPage, Has.Length.EqualTo(1));
            Assert.That(firstPage.Concat(secondPage).Select(r => r.Content.Key), Is.Unique);
        });
    }

    [Test]
    public async Task Search_ScoresAreStrictlyDecreasing()
    {
        await CreatePublishedSiteStructure();

        PublishedSearchResult[] results = PublishedContentQuery.Search("Page").ToArray();

        Assert.That(results, Has.Length.EqualTo(3));
        Assert.That(results.Select(r => r.Score), Is.Ordered.Descending.And.Unique);
    }

    [Test]
    public async Task Search_WithExplicitPublishedContentIndexName_FindsPublishedContentByTerm()
    {
        await CreatePublishedSiteStructure();

        PublishedSearchResult[] results = PublishedContentQuery
            .Search("Beta", 0, 10, out var totalRecords, indexName: global::Umbraco.Cms.Core.Constants.IndexAliases.PublishedContent)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(totalRecords, Is.EqualTo(1));
            Assert.That(results[0].Content.Key, Is.EqualTo(ChildBetaKey));
        });
    }

    [Test]
    public async Task Search_UnpublishedContentIsNotFound()
    {
        await CreatePublishedSiteStructure();

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            ContentService.Unpublish(ContentService.GetById(ChildBetaKey)!);
            return Task.CompletedTask;
        });

        PublishedSearchResult[] results = PublishedContentQuery.Search("Beta").ToArray();

        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task SearchChildren_ScopesToDirectChildren()
    {
        await CreatePublishedSiteStructure();

        IPublishedContent root = PublishedContentCache.GetById(RootAlphaKey)!;

        PublishedSearchResult[] results = root.SearchChildren("Page").ToArray();

        Assert.That(results.Select(r => r.Content.Key), Is.EquivalentTo(new[] { ChildBetaKey, ChildGammaKey }));
    }

    [Test]
    public async Task SearchDescendants_FindsAllDescendants_ExcludingSelf()
    {
        await CreatePublishedSiteStructure();

        IPublishedContent root = PublishedContentCache.GetById(RootAlphaKey)!;

        // the root matches the term itself ("Alpha Site" does not, so use a term matching all nodes via document type alias fallback):
        // search for "Site" - matches the root only; then for "Page" - matches all three descendants
        PublishedSearchResult[] selfTermResults = root.SearchDescendants("Site").ToArray();
        PublishedSearchResult[] descendantResults = root.SearchDescendants("Page").ToArray();

        Assert.Multiple(() =>
        {
            // the root itself matches "Site" but must be excluded from its own descendants
            Assert.That(selfTermResults, Is.Empty);
            Assert.That(
                descendantResults.Select(r => r.Content.Key),
                Is.EquivalentTo(new[] { ChildBetaKey, ChildGammaKey, GrandchildDeltaKey }));
        });
    }

    private async Task CreatePublishedSiteStructure()
    {
        var indexAlias = GetIndexAlias(true);
        await WaitForIndexing(indexAlias, async () =>
        {
            IContentType pageType = new ContentTypeBuilder()
                .WithAlias("page")
                .WithName("Page")
                .Build();
            await ContentTypeService.CreateAsync(pageType, global::Umbraco.Cms.Core.Constants.Security.SuperUserKey);
            pageType.AllowedContentTypes = [new ContentTypeSort(pageType.Key, 0, pageType.Alias)];
            await ContentTypeService.UpdateAsync(pageType, global::Umbraco.Cms.Core.Constants.Security.SuperUserKey);

            Content rootAlpha = new ContentBuilder()
                .WithKey(RootAlphaKey)
                .WithContentType(pageType)
                .WithName("Alpha Site")
                .Build();
            SaveAndPublish(rootAlpha);

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
