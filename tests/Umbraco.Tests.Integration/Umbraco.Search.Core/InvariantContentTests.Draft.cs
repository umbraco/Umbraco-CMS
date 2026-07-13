using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public partial class InvariantContentTests
{
    [Test]
    public void DraftStructure_YieldsAllDocuments()
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
            VerifyDocumentPropertyValues(documents[0], "The root title (draft)", 13);
            VerifyDocumentPropertyValues(documents[1], "The child title (draft)", 35);
            VerifyDocumentPropertyValues(documents[2], "The grandchild title (draft)", 57);
            VerifyDocumentPropertyValues(documents[3], "The great grandchild title (draft)", 79);
        });
    }

    [Test]
    public void DraftStructure_YieldsStructuralFields()
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
    public void DraftStructure_YieldsSystemFields()
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
            // NOTE: unpublished content does not have any tags - tags are collected from published content only
            VerifyDocumentSystemValues(documents[0], Root(), []);
            VerifyDocumentSystemValues(documents[1], Child(), []);
            VerifyDocumentSystemValues(documents[2], Grandchild(), []);
            VerifyDocumentSystemValues(documents[3], GreatGrandchild(), []);
        });
    }

    [Test]
    public void PublishedDraftStructure_YieldsSystemFieldsWithTags()
    {
        ContentService.Save(Root());
        ContentService.PublishBranch(Root(), PublishBranchFilter.IncludeUnpublished, ["*"]);

        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
            // NOTE: tags are collected from published content only, so the "draft" tag added
            //       by SetupDraftContent() will not be listed here
            VerifyDocumentSystemValues(documents[0], Root(), ["tag1", "tag2"]);
            VerifyDocumentSystemValues(documents[1], Child(), ["tag3", "tag4"]);
            VerifyDocumentSystemValues(documents[2], Grandchild(), ["tag5", "tag6"]);
            VerifyDocumentSystemValues(documents[3], GreatGrandchild(), ["tag7", "tag8"]);
        });
    }

    [Test]
    public void DraftStructure_UpdatesSystemFieldsWhenRootIsTrashed()
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);
        ContentService.MoveToRecycleBin(Root());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
            // NOTE: unpublished content does not have any tags - tags are collected from published content only
            VerifyDocumentSystemValues(documents[0], Root(), []);
            VerifyDocumentSystemValues(documents[1], Child(), []);
            VerifyDocumentSystemValues(documents[2], Grandchild(), []);
            VerifyDocumentSystemValues(documents[3], GreatGrandchild(), []);
        });
    }

    [Test]
    public void DraftStructure_UpdatesStructuralFieldsWhenRootIsTrashed()
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);
        ContentService.MoveToRecycleBin(Root());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
            // all content is trashed, content recycle bin is the new parent of root
            VerifyDocumentStructureValues(documents[0], RootKey, Constants.System.RecycleBinContentKey, [Constants.System.RecycleBinContentKey, RootKey]);
            VerifyDocumentStructureValues(documents[1], ChildKey, RootKey, [Constants.System.RecycleBinContentKey, RootKey, ChildKey]);
            VerifyDocumentStructureValues(documents[2], GrandchildKey, ChildKey, [Constants.System.RecycleBinContentKey, RootKey, ChildKey, GrandchildKey]);
            VerifyDocumentStructureValues(documents[3], GreatGrandchildKey, GrandchildKey, [Constants.System.RecycleBinContentKey, RootKey, ChildKey, GrandchildKey, GreatGrandchildKey]);
        });
    }

    [Test]
    public void DraftStructure_UpdatesStructuralFieldsWhenChildIsTrashed()
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);
        ContentService.MoveToRecycleBin(Child());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
            // root has not moved - it's still at the content tree root
            VerifyDocumentStructureValues(documents[0], RootKey, Guid.Empty, [RootKey]);

            // child is trashed, content recycle bin is the new parent of child, and root is no longer part of the path for any children
            VerifyDocumentStructureValues(documents[1], ChildKey, Constants.System.RecycleBinContentKey, [Constants.System.RecycleBinContentKey, ChildKey]);
            VerifyDocumentStructureValues(documents[2], GrandchildKey, ChildKey, [Constants.System.RecycleBinContentKey, ChildKey, GrandchildKey]);
            VerifyDocumentStructureValues(documents[3], GreatGrandchildKey, GrandchildKey, [Constants.System.RecycleBinContentKey, ChildKey, GrandchildKey, GreatGrandchildKey]);
        });
    }

    [Test]
    public void DraftStructure_UpdatesStructuralFieldsWhenRootIsMoved()
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        var secondRootKey = Guid.NewGuid();
        Content secondRoot = new ContentBuilder()
            .WithKey(secondRootKey)
            .WithContentType(ContentTypeService.Get(Root().ContentType.Key)!)
            .WithName("Second Root")
            .Build();
        ContentService.Save(secondRoot);

        OperationResult moveResult = ContentService.Move(Root(), secondRoot.Id);
        Assert.That(moveResult.Result, Is.EqualTo(OperationResultType.Success));

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
    public void DraftStructure_UpdatesStructuralFieldsWhenChildIsMoved()
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        var secondRootKey = Guid.NewGuid();
        Content secondRoot = new ContentBuilder()
            .WithKey(secondRootKey)
            .WithContentType(ContentTypeService.Get(Root().ContentType.Key)!)
            .WithName("Second Root")
            .Build();
        ContentService.Save(secondRoot);

        OperationResult moveResult = ContentService.Move(Child(), secondRoot.Id);
        Assert.That(moveResult.Result, Is.EqualTo(OperationResultType.Success));

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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

    [TestCase(true)]
    [TestCase(false)]
    public void DraftStructure_RemovesAllDocumentsWhenRootIsDeleted(bool moveToRecycleBinBeforeDeleting)
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        if (moveToRecycleBinBeforeDeleting)
        {
            ContentService.MoveToRecycleBin(Root());
        }

        ContentService.Delete(Root());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(0));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DraftStructure_RemovesAllDescendantDocumentsWhenChildIsDeleted(bool moveToRecycleBinBeforeDeleting)
    {
        SetupDraftContent();
        ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);

        if (moveToRecycleBinBeforeDeleting)
        {
            ContentService.MoveToRecycleBin(Child());
        }

        ContentService.Delete(Child());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootKey));
        VerifyDocumentStructureValues(documents[0], RootKey, Guid.Empty, [RootKey]);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DraftStructure_RebuildIndexYieldsAllDocuments(bool populateIndexBeforeRebuild)
    {
        SetupDraftContent();
        if (populateIndexBeforeRebuild)
        {
            ContentService.Save([Root(), Child(), Grandchild(), GreatGrandchild()]);
        }

        ContentIndexingService.Rebuild(IndexAliases.DraftContent, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
            VerifyDocumentPropertyValues(documents[0], "The root title (draft)", 13);
            VerifyDocumentPropertyValues(documents[1], "The child title (draft)", 35);
            VerifyDocumentPropertyValues(documents[2], "The grandchild title (draft)", 57);
            VerifyDocumentPropertyValues(documents[3], "The great grandchild title (draft)", 79);
        });
    }

    [Test]
    public void DraftStructure_RebuildIncludesTrashedContent()
    {
        SetupDraftContent();
        ContentService.MoveToRecycleBin(Grandchild());
        ContentService.MoveToRecycleBin(Child());

        // at this point we have:
        // - Root in the content tree root (the only item not in the recycle bin)
        // - Child and Grandchild in the recycle bin root
        // - GreatGrandchild in the recycle bin below Grandchild

        ContentIndexingService.Rebuild(IndexAliases.DraftContent, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GreatGrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(ChildKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentStructureValues(documents[0], RootKey, Guid.Empty, [RootKey]);
            VerifyDocumentStructureValues(documents[1], GrandchildKey, Constants.System.RecycleBinContentKey, [Constants.System.RecycleBinContentKey, GrandchildKey]);
            VerifyDocumentStructureValues(documents[2], GreatGrandchildKey, GrandchildKey, [Constants.System.RecycleBinContentKey, GrandchildKey, GreatGrandchildKey]);
            VerifyDocumentStructureValues(documents[3], ChildKey, Constants.System.RecycleBinContentKey, [Constants.System.RecycleBinContentKey, ChildKey]);
        });
    }
}
