using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

public partial class InvariantDocumentTreeTests : IndexTestBase
{
    [Test]
    public async Task PublishedStructure_YieldsAllPublishedDocuments()
    {
        await CreateInvariantDocumentTree(true);
        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(3));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
            Assert.That(results[1].Id, Is.EqualTo(ChildKey.ToString()));
            Assert.That(results[2].Id, Is.EqualTo(GrandchildKey.ToString()));
        });
    }

    [Test]
    public async Task PublishedStructure_AlsoIndexesDraftStructure()
    {
        await CreateInvariantDocumentTree(true);
        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(3));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
            Assert.That(results[1].Id, Is.EqualTo(ChildKey.ToString()));
            Assert.That(results[2].Id, Is.EqualTo(GrandchildKey.ToString()));
        });
    }

    [Test]
    public async Task PublishedStructure_WithUnpublishedRoot_YieldsNoDocuments()
    {
        await CreateInvariantDocumentTree(true);
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent root = ContentService.GetById(RootKey)!;
            ContentService.Unpublish(root);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults publishedResultsRoot = index.Searcher.CreateQuery().All().Execute();
        Assert.That(publishedResultsRoot.TotalItemCount, Is.EqualTo(0));
    }

    [Test]
    public async Task PublishedStructure_WithUnpublishedChild_YieldsNothingBelowRoot()
    {
        await CreateInvariantDocumentTree(true);
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent child = ContentService.GetById(ChildKey)!;
            ContentService.Unpublish(child);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(1));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
        });
    }

    [Test]
    public async Task PublishedStructure_WithUnpublishedGrandchild_YieldsNothingBelowChild()
    {
        await CreateInvariantDocumentTree(true);
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent grandChild = ContentService.GetById(GrandchildKey)!;
            ContentService.Unpublish(grandChild);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(2));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
            Assert.That(results[1].Id, Is.EqualTo(ChildKey.ToString()));
        });
    }

    [Test]
    public async Task PublishedStructure_WithRootInRecycleBin_YieldsNoDocuments()
    {
        await CreateInvariantDocumentTree(true);
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent root = ContentService.GetById(RootKey)!;
            ContentService.MoveToRecycleBin(root);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResults publishedResultsRoot = index.Searcher.CreateQuery().All().Execute();
        Assert.That(publishedResultsRoot.TotalItemCount, Is.EqualTo(0));
    }

    [Test]
    public async Task PublishedStructure_WithChildInRecycleBin_YieldsNothingBelowRoot()
    {
        await CreateInvariantDocumentTree(true);
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent child = ContentService.GetById(ChildKey)!;
            ContentService.MoveToRecycleBin(child);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(1));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
        });
    }

    [Test]
    public async Task PublishedStructure_WithUGrandchildInRecycleBin_YieldsNothingBelowChild()
    {
        await CreateInvariantDocumentTree(true);
        await WaitForIndexing(Cms.Core.Constants.IndexAliases.PublishedContent, () =>
        {
            IContent grandChild = ContentService.GetById(GrandchildKey)!;
            ContentService.MoveToRecycleBin(grandChild);
            return Task.CompletedTask;
        });

        IIndex index = GetIndex(Cms.Core.Constants.IndexAliases.PublishedContent);
        ISearchResult[] results = index.Searcher.CreateQuery().All().Execute().ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(results.Length, Is.EqualTo(2));
            Assert.That(results[0].Id, Is.EqualTo(RootKey.ToString()));
            Assert.That(results[1].Id, Is.EqualTo(ChildKey.ToString()));
        });
    }
}
