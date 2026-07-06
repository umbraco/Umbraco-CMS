using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Search.Integration.Services;

namespace Umbraco.Tests.Search.Integration.Tests;

/// <summary>
/// Tests that media changes are handled by IPublishedContentChangeStrategy.
/// See https://github.com/umbraco/Umbraco.Cms.Search/issues/108
/// </summary>
public class PublishedMediaTests : MediaTestBase
{
    private const string PublishedMediaIndexAlias = "Umb_PublishedMedia_Test";

    private IContentIndexingService ContentIndexingService => GetRequiredService<IContentIndexingService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.Services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<IIndexer, ISearcher, IPublishedContentChangeStrategy>(
                PublishedMediaIndexAlias, UmbracoObjectTypes.Media);
        });
    }

    [Test]
    public void FullStructure_YieldsAllDocuments()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildFolderKey));
            Assert.That(documents[2].Id, Is.EqualTo(RootMediaKey));
            Assert.That(documents[3].Id, Is.EqualTo(ChildMediaKey));
            Assert.That(documents[4].Id, Is.EqualTo(GrandchildMediaKey));

            Assert.That(documents.All(d => d.ObjectType is UmbracoObjectTypes.Media), Is.True);
        });
    }

    [Test]
    public void FullStructure_YieldsPropertyValues()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
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

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
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

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
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

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(6));

        Assert.Multiple(() =>
        {
            VerifyDocumentStructureValues(documents[0], RootFolderKey, Guid.Empty, [RootFolderKey]);
            VerifyDocumentStructureValues(documents[2], RootMediaKey, Guid.Empty, [RootMediaKey]);
            VerifyDocumentStructureValues(documents[1], ChildFolderKey, secondRootFolderKey, [secondRootFolderKey, ChildFolderKey]);
            VerifyDocumentStructureValues(documents[3], ChildMediaKey, secondRootFolderKey, [secondRootFolderKey, ChildMediaKey]);
            VerifyDocumentStructureValues(documents[4], GrandchildMediaKey, ChildFolderKey, [secondRootFolderKey, ChildFolderKey, GrandchildMediaKey]);
            VerifyDocumentStructureValues(documents[5], secondRootFolderKey, Guid.Empty, [secondRootFolderKey]);
        });
    }

    [Test]
    public void FullStructure_RemovesDocumentsWhenRootsAreTrashed()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);
        MediaService.MoveToRecycleBin(RootFolder());
        MediaService.MoveToRecycleBin(RootMedia());

        // unlike the draft media index, the published index should not contain trashed media
        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Is.Empty);
    }

    [Test]
    public void FullStructure_RemovesDocumentsWhenChildrenAreTrashed()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);
        MediaService.MoveToRecycleBin(ChildFolder());
        MediaService.MoveToRecycleBin(ChildMedia());

        // root items should remain, children and their descendants should be removed
        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(RootMediaKey));
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

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(1));

        Assert.That(documents[0].Id, Is.EqualTo(RootMediaKey));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void PublishedStructure_RebuildIndexYieldsAllDocuments(bool populateIndexBeforeRebuild)
    {
        if (populateIndexBeforeRebuild)
        {
            MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);
        }

        ContentIndexingService.Rebuild(PublishedMediaIndexAlias, DefaultOrigin);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildFolderKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildMediaKey));
            Assert.That(documents[3].Id, Is.EqualTo(ChildMediaKey));
            Assert.That(documents[4].Id, Is.EqualTo(RootMediaKey));
        });
    }

    [Test]
    public void PublishedStructure_RebuildExcludesTrashedContent()
    {
        MediaService.MoveToRecycleBin(ChildFolder());
        MediaService.MoveToRecycleBin(RootMedia());

        ContentIndexingService.Rebuild(PublishedMediaIndexAlias, DefaultOrigin);

        // unlike the draft media index, the published index should not contain trashed media
        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildMediaKey));
        });
    }

    [Test]
    public void FullStructure_DoesNotYieldDocumentsInDraftMediaIndex()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        // the published media index should have documents
        IReadOnlyList<TestIndexDocument> publishedDocuments = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(publishedDocuments, Has.Count.EqualTo(5));

        // the draft media index should also have documents (existing behavior unchanged)
        IReadOnlyList<TestIndexDocument> draftDocuments = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(draftDocuments, Has.Count.EqualTo(5));
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
