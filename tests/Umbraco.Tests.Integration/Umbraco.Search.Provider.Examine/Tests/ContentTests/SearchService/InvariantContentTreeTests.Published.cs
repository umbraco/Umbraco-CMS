using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Searching;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.SearchService;

public partial class InvariantContentTreeTests
{
    [Test]
    public async Task PublishStructure_WithRootInRecycleBin_YieldsNoDocuments()
    {
        var indexAlias = GetIndexAlias(true);
        await WaitForIndexing(indexAlias, async () =>
        {
            await CreateInvariantDocumentTree(true);
        });

        await WaitForIndexing(indexAlias, () =>
        {
            IContent root = ContentService.GetById(RootKey)!;
            ContentService.MoveToRecycleBin(root);
            return Task.CompletedTask;
        });


        SearchResult rootResult = await Searcher.SearchAsync(indexAlias, "Root", null, null, null, null, null, null, 0, 100);
        SearchResult childResult = await Searcher.SearchAsync(indexAlias, "Child", null, null, null, null, null, null, 0, 100);
        SearchResult grandChildResult = await Searcher.SearchAsync(indexAlias, "Grandchild", null, null, null, null, null, null, 0, 100);

        Assert.Multiple(() =>
        {
            Assert.That(rootResult.Total, Is.EqualTo(0));
            Assert.That(childResult.Total, Is.EqualTo(0));
            Assert.That(grandChildResult.Total, Is.EqualTo(0));
        });
    }


    [Test]
    public async Task PublishStructure_WithChildUnpublished_YieldsNothingBelowRoot()
    {
        var indexAlias = GetIndexAlias(true);
        await WaitForIndexing(indexAlias, async () =>
        {
            await CreateInvariantDocumentTree(true);
        });

        await WaitForIndexing(indexAlias, () =>
        {
            IContent child = ContentService.GetById(ChildKey)!;
            ContentService.Unpublish(child);
            return Task.CompletedTask;
        });

        SearchResult rootResult = await Searcher.SearchAsync(indexAlias, "Root", null, null, null, null, null, null, 0, 100);
        SearchResult childResult = await Searcher.SearchAsync(indexAlias, "Child", null, null, null, null, null, null, 0, 100);
        SearchResult grandChildResult = await Searcher.SearchAsync(indexAlias, "Grandchild", null, null, null, null, null, null, 0, 100);

        Assert.Multiple(() =>
        {
            Assert.That(rootResult.Total, Is.EqualTo(1));
            Assert.That(childResult.Total, Is.EqualTo(0));
            Assert.That(grandChildResult.Total, Is.EqualTo(0));
            Assert.That(rootResult.Documents.First().Id, Is.EqualTo(RootKey));
        });
    }

    [Test]
    public async Task PublishStructure_WithGrandchildUnpublished_YieldsNothingBelowChild()
    {
        var indexAlias = GetIndexAlias(true);
        await WaitForIndexing(indexAlias, async () =>
        {
            await CreateInvariantDocumentTree(true);
        });

        await WaitForIndexing(indexAlias, () =>
        {
            IContent grandchild = ContentService.GetById(GrandchildKey)!;
            ContentService.Unpublish(grandchild);
            return Task.CompletedTask;
        });

        SearchResult rootResult = await Searcher.SearchAsync(indexAlias, "Root", null, null, null, null, null, null, 0, 100);
        SearchResult childResult = await Searcher.SearchAsync(indexAlias, "Child", null, null, null, null, null, null, 0, 100);
        SearchResult grandChildResult = await Searcher.SearchAsync(indexAlias, "Grandchild", null, null, null, null, null, null, 0, 100);

        Assert.Multiple(() =>
        {
            Assert.That(rootResult.Total, Is.EqualTo(1));
            Assert.That(childResult.Total, Is.EqualTo(1));
            Assert.That(grandChildResult.Total, Is.EqualTo(0));
            Assert.That(rootResult.Documents.First().Id, Is.EqualTo(RootKey));
            Assert.That(childResult.Documents.First().Id, Is.EqualTo(ChildKey));
        });
    }
}
