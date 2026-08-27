using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public partial class InvariantContentStructureTests
{
    [Test]
    public async Task PublishedStructure_YieldsAllPublishedDocuments()
    {
        await ContentService.SaveAsync(Root(), null, null, CancellationToken.None);
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));

            Assert.That(documents.All(d => d.ObjectType is UmbracoObjectTypes.Document), Is.True);
        });
    }

    [Test]
    public async Task PublishedRoot_YieldsOnlyRootDocument()
    {
        await ContentService.SaveAsync(Root(), null, null, CancellationToken.None);
        ContentService.Publish(Root(), ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootKey));
    }

    [Test]
    public async Task PublishedStructure_WithUnpublishedRoot_YieldsNoDocuments()
    {
        await ContentService.SaveAsync(Root(), null, null, CancellationToken.None);
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        PublishResult result = ContentService.Unpublish(Root());
        Assert.That(result.Success, Is.True);
        Assert.That(Child().Published, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Is.Empty);
    }

    [Test]
    public async Task PublishedStructure_WithUnpublishedGrandchild_YieldsNothingBelowChild()
    {
        await ContentService.SaveAsync(Root(), null, null, CancellationToken.None);
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        PublishResult result = ContentService.Unpublish(Grandchild());
        Assert.That(result.Success, Is.True);
        Assert.That(GreatGrandchild().Published, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
        });
    }

    [Test]
    public async Task PublishedStructure_WithGrandchildInRecycleBin_YieldsNothingBelowChild()
    {
        await ContentService.SaveAsync(Root(), null, null, CancellationToken.None);
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        OperationResult result = ContentService.MoveToRecycleBin(Grandchild());
        Assert.That(result.Success, Is.True);
        Assert.That(GreatGrandchild().Trashed, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
        });
    }

    [Test]
    public async Task PublishedStructure_WithGrandchildDeleted_YieldsNothingBelowChild()
    {
        await ContentService.SaveAsync(Root(), null, null, CancellationToken.None);
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        Attempt<ContentDeleteOperationStatus> result = await ContentService.DeleteAsync(Grandchild(), null, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(ContentService.GetByIdAsync(GreatGrandchildKey, CancellationToken.None).GetAwaiter().GetResult(), Is.Null);
        });

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
        });
    }
}
