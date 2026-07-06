using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

public class MediaContentTests : MediaTestBase
{
    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    [Test]
    public void FullStructure_YieldsAllDocuments()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], null, null);
            VerifyDocumentPropertyValues(documents[1], null, null);
            VerifyDocumentPropertyValues(documents[2], "The root alt text", 1234);
            VerifyDocumentPropertyValues(documents[3], "The child alt text", 5678);
            VerifyDocumentPropertyValues(documents[4], "The grandchild alt text", 9012);
        });
    }

    [Test]
    public void FullStructure_YieldsStructuralFields()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            VerifyDocumentStructureValues(documents[0], RootFolderKey, Guid.Empty, [RootFolderKey]);
            VerifyDocumentStructureValues(documents[1], ChildFolderKey, RootFolderKey, [RootFolderKey, ChildFolderKey]);
            VerifyDocumentStructureValues(documents[2], RootMediaKey, Guid.Empty, [RootMediaKey]);
            VerifyDocumentStructureValues(documents[3], ChildMediaKey, RootFolderKey, [RootFolderKey, ChildMediaKey]);
            VerifyDocumentStructureValues(documents[4], GrandchildMediaKey, ChildFolderKey, [RootFolderKey, ChildFolderKey, GrandchildMediaKey]);
        });
    }

    [Test]
    public void FullStructure_YieldsSystemFields()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            VerifyDocumentSystemValues(documents[0], RootFolder(), []);
            VerifyDocumentSystemValues(documents[1], ChildFolder(), []);
            VerifyDocumentSystemValues(documents[2], RootMedia(), ["tag1", "tag2"]);
            VerifyDocumentSystemValues(documents[3], ChildMedia(), ["tag3", "tag4"]);
            VerifyDocumentSystemValues(documents[4], GrandchildMedia(), ["tag5", "tag6"]);
        });
    }

    [Test]
    public void FullStructure_UpdatesStructuralFieldsWhenChildrenAreMoved()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        var secondRootFolderKey = Guid.NewGuid();
        Media secondRootFolder = new MediaBuilder()
            .WithKey(secondRootFolderKey)
            .WithMediaType(MediaTypeService.Get(RootFolder().ContentType.Key)!)
            .WithName("Second Root folder")
            .Build();
        MediaService.Save(secondRootFolder);

        Attempt<OperationResult?> moveResult = MediaService.Move(ChildFolder(), secondRootFolder.Id);
        Assert.That(moveResult.Result?.Result, Is.EqualTo(OperationResultType.Success));
        moveResult = MediaService.Move(ChildMedia(), secondRootFolder.Id);
        Assert.That(moveResult.Result?.Result, Is.EqualTo(OperationResultType.Success));

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(6));

        Assert.Multiple(() =>
        {
            // first root folder and root media did not move (still at tree root level)
            VerifyDocumentStructureValues(documents[0], RootFolderKey, Guid.Empty, [RootFolderKey]);
            VerifyDocumentStructureValues(documents[2], RootMediaKey, Guid.Empty, [RootMediaKey]);
            // child folder, child media and grandchild media moved under second root folder, and all paths should be updated accordingly
            VerifyDocumentStructureValues(documents[1], ChildFolderKey, secondRootFolderKey, [secondRootFolderKey, ChildFolderKey]);
            VerifyDocumentStructureValues(documents[3], ChildMediaKey, secondRootFolderKey, [secondRootFolderKey, ChildMediaKey]);
            VerifyDocumentStructureValues(documents[4], GrandchildMediaKey, ChildFolderKey, [secondRootFolderKey, ChildFolderKey, GrandchildMediaKey]);
            // second root is also at tree root level
            VerifyDocumentStructureValues(documents[5], secondRootFolderKey, Guid.Empty, [secondRootFolderKey]);
        });
    }

    [Test]
    public void FullStructure_UpdatesStructuralFieldsWhenRootsAreMoved()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        var secondRootFolderKey = Guid.NewGuid();
        Media secondRootFolder = new MediaBuilder()
            .WithKey(secondRootFolderKey)
            .WithMediaType(MediaTypeService.Get(RootFolder().ContentType.Key)!)
            .WithName("Second Root folder")
            .Build();
        MediaService.Save(secondRootFolder);

        Attempt<OperationResult?> moveResult = MediaService.Move(RootFolder(), secondRootFolder.Id);
        Assert.That(moveResult.Result?.Result, Is.EqualTo(OperationResultType.Success));
        moveResult = MediaService.Move(RootMedia(), secondRootFolder.Id);
        Assert.That(moveResult.Result?.Result, Is.EqualTo(OperationResultType.Success));

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(6));

        Assert.Multiple(() =>
        {
            // first root folder and root media are now located below second root, and all paths should be updated accordingly
            VerifyDocumentStructureValues(documents[0], RootFolderKey, secondRootFolderKey, [secondRootFolderKey, RootFolderKey]);
            VerifyDocumentStructureValues(documents[1], ChildFolderKey, RootFolderKey, [secondRootFolderKey, RootFolderKey, ChildFolderKey]);
            VerifyDocumentStructureValues(documents[2], RootMediaKey, secondRootFolderKey, [secondRootFolderKey, RootMediaKey]);
            VerifyDocumentStructureValues(documents[3], ChildMediaKey, RootFolderKey, [secondRootFolderKey, RootFolderKey, ChildMediaKey]);
            VerifyDocumentStructureValues(documents[4], GrandchildMediaKey, ChildFolderKey, [secondRootFolderKey, RootFolderKey, ChildFolderKey, GrandchildMediaKey]);
            // second root is the only one at tree root level
            VerifyDocumentStructureValues(documents[5], secondRootFolderKey, Guid.Empty, [secondRootFolderKey]);
        });
    }

    [Test]
    public void FullStructure_UpdatesSystemFieldsWhenRootsAreTrashed()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);
        MediaService.MoveToRecycleBin(RootFolder());
        MediaService.MoveToRecycleBin(RootMedia());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            VerifyDocumentSystemValues(documents[0], RootFolder(), []);
            VerifyDocumentSystemValues(documents[1], ChildFolder(), []);
            VerifyDocumentSystemValues(documents[2], RootMedia(), ["tag1", "tag2"]);
            VerifyDocumentSystemValues(documents[3], ChildMedia(), ["tag3", "tag4"]);
            VerifyDocumentSystemValues(documents[4], GrandchildMedia(), ["tag5", "tag6"]);
        });
    }

    [Test]
    public void FullStructure_UpdatesStructuralFieldsWhenRootsAreTrashed()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);
        MediaService.MoveToRecycleBin(RootFolder());
        MediaService.MoveToRecycleBin(RootMedia());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            // root folder and root media are trashed (and all descendants), media recycle bin is the new parent of all roots
            VerifyDocumentStructureValues(documents[0], RootFolderKey, Constants.System.RecycleBinMediaKey, [Constants.System.RecycleBinMediaKey, RootFolderKey]);
            VerifyDocumentStructureValues(documents[1], ChildFolderKey, RootFolderKey, [Constants.System.RecycleBinMediaKey, RootFolderKey, ChildFolderKey]);
            VerifyDocumentStructureValues(documents[2], RootMediaKey, Constants.System.RecycleBinMediaKey, [Constants.System.RecycleBinMediaKey, RootMediaKey]);
            VerifyDocumentStructureValues(documents[3], ChildMediaKey, RootFolderKey, [Constants.System.RecycleBinMediaKey, RootFolderKey, ChildMediaKey]);
            VerifyDocumentStructureValues(documents[4], GrandchildMediaKey, ChildFolderKey, [Constants.System.RecycleBinMediaKey, RootFolderKey, ChildFolderKey, GrandchildMediaKey]);
        });
    }

    [Test]
    public void FullStructure_UpdatesStructuralFieldsWhenChildrenAreTrashed()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);
        MediaService.MoveToRecycleBin(ChildFolder());
        MediaService.MoveToRecycleBin(ChildMedia());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            // root folder and root media remain in place (media tree root)
            VerifyDocumentStructureValues(documents[0], RootFolderKey, Guid.Empty, [RootFolderKey]);
            VerifyDocumentStructureValues(documents[2], RootMediaKey, Guid.Empty, [RootMediaKey]);

            // child folder and child media (and by extension, grandchild media) are trashed, media recycle bin is the new parent of all children
            VerifyDocumentStructureValues(documents[1], ChildFolderKey, Constants.System.RecycleBinMediaKey, [Constants.System.RecycleBinMediaKey, ChildFolderKey]);
            VerifyDocumentStructureValues(documents[3], ChildMediaKey, Constants.System.RecycleBinMediaKey, [Constants.System.RecycleBinMediaKey, ChildMediaKey]);
            VerifyDocumentStructureValues(documents[4], GrandchildMediaKey, ChildFolderKey, [Constants.System.RecycleBinMediaKey, ChildFolderKey, GrandchildMediaKey]);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void FullStructure_RemovesAllDocumentsWhenRootIsDeleted(bool moveToRecycleBinBeforeDeleting)
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        if (moveToRecycleBinBeforeDeleting)
        {
            MediaService.MoveToRecycleBin(RootFolder());
        }

        MediaService.Delete(RootFolder());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(1));

        Assert.Multiple(() =>
        {
            // root media remain unaffected by the deletion
            VerifyDocumentStructureValues(documents[0], RootMediaKey, Guid.Empty, [RootMediaKey]);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void FullStructure_RemovesAllDescendantDocumentsWhenChildIsDeleted(bool moveToRecycleBinBeforeDeleting)
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        if (moveToRecycleBinBeforeDeleting)
        {
            MediaService.MoveToRecycleBin(ChildFolder());
        }

        MediaService.Delete(ChildFolder());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            // root folder, root media and child media remain unaffected by the deletion
            VerifyDocumentStructureValues(documents[0], RootFolderKey, Guid.Empty, [RootFolderKey]);
            VerifyDocumentStructureValues(documents[1], RootMediaKey, Guid.Empty, [RootMediaKey]);
            VerifyDocumentStructureValues(documents[2], ChildMediaKey, RootFolderKey, [RootFolderKey, ChildMediaKey]);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public void DraftStructure_RebuildIndexYieldsAllDocuments(bool populateIndexBeforeRebuild)
    {
        if (populateIndexBeforeRebuild)
        {
            MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);
        }

        ContentIndexingService.Rebuild(IndexAliases.Media, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildFolderKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildMediaKey));
            Assert.That(documents[3].Id, Is.EqualTo(ChildMediaKey));
            Assert.That(documents[4].Id, Is.EqualTo(RootMediaKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentPropertyValues(documents[0], null, null);
            VerifyDocumentPropertyValues(documents[1], null, null);
            VerifyDocumentPropertyValues(documents[2], "The grandchild alt text", 9012);
            VerifyDocumentPropertyValues(documents[3], "The child alt text", 5678);
            VerifyDocumentPropertyValues(documents[4], "The root alt text", 1234);
        });
    }

    [Test]
    public void DraftStructure_RebuildIncludesTrashedContent()
    {
        MediaService.MoveToRecycleBin(ChildFolder());
        MediaService.MoveToRecycleBin(RootMedia());

        // at this point we have:
        // - RootFolder in the media tree root
        // - ChildMedia below RootFolder
        // - RootMedia and ChildFolder in the recycle bin root
        // - GrandchildMedia in the recycle bin below ChildFolder

        ContentIndexingService.Rebuild(IndexAliases.Media, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildMediaKey));
            Assert.That(documents[2].Id, Is.EqualTo(ChildFolderKey));
            Assert.That(documents[3].Id, Is.EqualTo(GrandchildMediaKey));
            Assert.That(documents[4].Id, Is.EqualTo(RootMediaKey));
        });

        Assert.Multiple(() =>
        {
            VerifyDocumentStructureValues(documents[0], RootFolderKey, Guid.Empty, [RootFolderKey]);
            VerifyDocumentStructureValues(documents[1], ChildMediaKey, RootFolderKey, [RootFolderKey, ChildMediaKey]);
            VerifyDocumentStructureValues(documents[2], ChildFolderKey, Constants.System.RecycleBinMediaKey, [Constants.System.RecycleBinMediaKey, ChildFolderKey]);
            VerifyDocumentStructureValues(documents[3], GrandchildMediaKey, ChildFolderKey, [Constants.System.RecycleBinMediaKey, ChildFolderKey, GrandchildMediaKey]);
            VerifyDocumentStructureValues(documents[4], RootMediaKey, Constants.System.RecycleBinMediaKey, [Constants.System.RecycleBinMediaKey, RootMediaKey]);
        });
    }

    private void VerifyDocumentPropertyValues(TestIndexDocument document, string? altText, int? bytes)
        => Assert.Multiple(() =>
        {
            var altTextValue = document.Fields.FirstOrDefault(f => f.FieldName == "altText")?.Value.Texts?.SingleOrDefault();
            Assert.That(altTextValue, Is.EqualTo(altText));

            var bytesValue = document.Fields.FirstOrDefault(f => f.FieldName == "bytes")?.Value.Integers?.SingleOrDefault();
            Assert.That(bytesValue, Is.EqualTo(bytes));
        });
}
