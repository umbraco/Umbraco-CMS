using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

public partial class InvariantContentTests
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
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], "The root title", 12);
            VerifyDocumentPropertyValues(documents[1], "The child title", 34);
            VerifyDocumentPropertyValues(documents[2], "The grandchild title", 56);
            VerifyDocumentPropertyValues(documents[3], "The great grandchild title", 78);
        });
    }

    [Test]
    public void PublishedStructure_CanRefreshChild()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IContent child = Child();
        child.SetValue("title", "The updated child title");
        child.SetValue("count", 123456);
        ContentService.Save(child);
        ContentService.Publish(child, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
        });

        VerifyDocumentPropertyValues(documents[1], "The updated child title", 123456);
    }

    [Test]
    public void PublishedStructure_YieldsStructuralFields()
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
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentStructureValues(documents[0], RootKey, Guid.Empty, [RootKey]);
            VerifyDocumentStructureValues(documents[1], ChildKey, RootKey, [RootKey, ChildKey]);
            VerifyDocumentStructureValues(documents[2], GrandchildKey, ChildKey, [RootKey, ChildKey, GrandchildKey]);
            VerifyDocumentStructureValues(documents[3], GreatGrandchildKey, GrandchildKey, [RootKey, ChildKey, GrandchildKey, GreatGrandchildKey]);
        });
    }

    [Test]
    public void PublishedStructure_YieldsSystemFields()
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
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentSystemValues(documents[0], Root(), ["tag1", "tag2"]);
            VerifyDocumentSystemValues(documents[1], Child(), ["tag3", "tag4"]);
            VerifyDocumentSystemValues(documents[2], Grandchild(), ["tag5", "tag6"]);
            VerifyDocumentSystemValues(documents[3], GreatGrandchild(), ["tag7", "tag8"]);
        });
    }

    [Test]
    public void PublishedStructure_CanUpdateEditableSystemFields()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        IContent child = Child();
        child.Name = "The updated child name";
        child.SetValue("tags", "[\"updated-tag1\",\"updated-tag2\",\"updated-tag3\"]");
        ContentService.Save(child);
        ContentService.Publish(child, ["*"]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
        VerifyDocumentSystemValues(documents[1], Child(), ["updated-tag1", "updated-tag2", "updated-tag3"]);
    }

    [Test]
    public void PublishedStructure_RemovesAllPublishedDocumentsWhenRootIsTrashed()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentService.MoveToRecycleBin(Root());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(0));
    }

    [Test]
    public void PublishedStructure_RemovesAllPublishedDescendantDocumentsWhenChildIsTrashed()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentService.MoveToRecycleBin(Child());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootKey));
    }

    [Test]
    public void PublishedStructure_UpdatesStructuralFieldsWhenRootIsMoved()
    {
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        var secondRootKey = Guid.NewGuid();
        Content secondRoot = new ContentBuilder()
            .WithKey(secondRootKey)
            .WithContentType(ContentTypeService.Get(Root().ContentType.Key)!)
            .WithName("Second Root")
            .Build();
        ContentService.Save(secondRoot);
        ContentService.Publish(secondRoot, ["*"]);

        OperationResult moveResult = ContentService.Move(Root(), secondRoot.Id);
        Assert.That(moveResult.Result, Is.EqualTo(OperationResultType.Success));

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
            Assert.That(documents[4].Id, Is.EqualTo(secondRootKey));
        });

        Assert.Multiple(() =>
        {
            // all items were moved; previous root is now a child to second root, and all paths should be updated accordingly
            VerifyDocumentStructureValues(documents[0], RootKey, secondRootKey, [secondRootKey, RootKey]);
            VerifyDocumentStructureValues(documents[1], ChildKey, RootKey, [secondRootKey, RootKey, ChildKey]);
            VerifyDocumentStructureValues(documents[2], GrandchildKey, ChildKey, [secondRootKey, RootKey, ChildKey, GrandchildKey]);
            VerifyDocumentStructureValues(documents[3], GreatGrandchildKey, GrandchildKey, [secondRootKey, RootKey, ChildKey, GrandchildKey, GreatGrandchildKey]);
            // second root is the only one at tree root level
            VerifyDocumentStructureValues(documents[4], secondRootKey, Guid.Empty, [secondRootKey]);
        });
    }

    [Test]
    public void PublishedStructure_UpdatesStructuralFieldsWhenChildIsMoved()
    {
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        var secondRootKey = Guid.NewGuid();
        Content secondRoot = new ContentBuilder()
            .WithKey(secondRootKey)
            .WithContentType(ContentTypeService.Get(Root().ContentType.Key)!)
            .WithName("Second Root")
            .Build();
        ContentService.Save(secondRoot);
        ContentService.Publish(secondRoot, ["*"]);

        OperationResult moveResult = ContentService.Move(Child(), secondRoot.Id);
        Assert.That(moveResult.Result, Is.EqualTo(OperationResultType.Success));

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
            Assert.That(documents[4].Id, Is.EqualTo(secondRootKey));
        });

        Assert.Multiple(() =>
        {
            // first root did not move (still at tree root level)
            VerifyDocumentStructureValues(documents[0], RootKey, Guid.Empty, [RootKey]);
            // child and all descendants moved under second root, and all paths should be updated accordingly
            VerifyDocumentStructureValues(documents[1], ChildKey, secondRootKey, [secondRootKey, ChildKey]);
            VerifyDocumentStructureValues(documents[2], GrandchildKey, ChildKey, [secondRootKey, ChildKey, GrandchildKey]);
            VerifyDocumentStructureValues(documents[3], GreatGrandchildKey, GrandchildKey, [secondRootKey, ChildKey, GrandchildKey, GreatGrandchildKey]);
            // second root is also at tree root level
            VerifyDocumentStructureValues(documents[4], secondRootKey, Guid.Empty, [secondRootKey]);
        });
    }

    [Test]
    public void PublishedStructure_RemovesChildAndDescendantsWhenMovedToAnUnpublishedParent()
    {
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        var secondRootKey = Guid.NewGuid();
        Content secondRoot = new ContentBuilder()
            .WithKey(secondRootKey)
            .WithContentType(ContentTypeService.Get(Root().ContentType.Key)!)
            .WithName("Second Root")
            .Build();
        ContentService.Save(secondRoot);

        OperationResult moveResult = ContentService.Move(Child(), secondRoot.Id);
        Assert.That(moveResult.Result, Is.EqualTo(OperationResultType.Success));

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootKey));

        VerifyDocumentStructureValues(documents[0], RootKey, Guid.Empty, [RootKey]);
    }

    [Test]
    public void PublishedStructure_RebuildIndexYieldsAllDocuments()
    {
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentIndexingService.Rebuild(IndexAliases.PublishedContent, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], "The root title", 12);
            VerifyDocumentPropertyValues(documents[1], "The child title", 34);
            VerifyDocumentPropertyValues(documents[2], "The grandchild title", 56);
            VerifyDocumentPropertyValues(documents[3], "The great grandchild title", 78);
        });
    }

    [Test]
    public void PublishedStructure_RebuildOmitsTrashedContent()
    {
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentService.MoveToRecycleBin(Grandchild());
        ContentService.MoveToRecycleBin(Child());

        // at this point we have:
        // - Root in the content tree root (the only item not in the recycle bin)
        // - Child and Grandchild in the recycle bin root
        // - GreatGrandchild in the recycle bin below Grandchild

        ContentIndexingService.Rebuild(IndexAliases.PublishedContent, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootKey));
        VerifyDocumentStructureValues(documents[0], RootKey, Guid.Empty, [RootKey]);
    }
}
