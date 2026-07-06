using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

public partial class InvariantContentStructureTests
{
    [Test]
    public void PublishedStructure_YieldsAllPublishedDocuments()
    {
        ContentService.Save(Root());
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
    public void PublishedRoot_YieldsOnlyRootDocument()
    {
        ContentService.Save(Root());
        ContentService.Publish(Root(), ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootKey));
    }

    [Test]
    public void PublishedStructure_WithUnpublishedRoot_YieldsNoDocuments()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        PublishResult result = ContentService.Unpublish(Root());
        Assert.That(result.Success, Is.True);
        Assert.That(Child().Published, Is.True);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Is.Empty);
    }

    [Test]
    public void PublishedStructure_WithUnpublishedGrandchild_YieldsNothingBelowChild()
    {
        ContentService.Save(Root());
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
    public void PublishedStructure_WithGrandchildInRecycleBin_YieldsNothingBelowChild()
    {
        ContentService.Save(Root());
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
    public void PublishedStructure_WithGrandchildDeleted_YieldsNothingBelowChild()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        OperationResult result = ContentService.Delete(Grandchild());
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(ContentService.GetById(GreatGrandchildKey), Is.Null);
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
