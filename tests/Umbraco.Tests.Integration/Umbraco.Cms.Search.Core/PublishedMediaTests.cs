using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Configuration;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

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
    public async Task FullStructure_YieldsAllDocuments()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

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
    public async Task FullStructure_YieldsPropertyValues()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

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
    public async Task FullStructure_YieldsStructuralFields()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

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
    public async Task FullStructure_YieldsSystemFields()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

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
    public async Task FullStructure_UpdatesStructuralFieldsWhenChildrenAreMoved()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

        var secondRootFolderKey = Guid.NewGuid();
        Media secondRootFolder = new MediaBuilder()
            .WithKey(secondRootFolderKey)
            .WithMediaType(MediaTypeService.GetAsync(RootFolder().ContentType.Key).GetAwaiter().GetResult()!)
            .WithName("Second Root folder")
            .Build();
        await MediaService.SaveAsync(secondRootFolder, Constants.Security.SuperUserKey, CancellationToken.None);

        Attempt<ContentMoveOperationStatus> moveResult = await MediaService.MoveAsync(ChildFolder(), secondRootFolder.Key, true, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.That(moveResult.Success, Is.True);
        moveResult = await MediaService.MoveAsync(ChildMedia(), secondRootFolder.Key, true, Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.That(moveResult.Success, Is.True);

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
    public async Task FullStructure_RemovesDocumentsWhenRootsAreTrashed()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);
        await MediaService.MoveToRecycleBinAsync(RootFolder(), Constants.Security.SuperUserKey, CancellationToken.None);
        await MediaService.MoveToRecycleBinAsync(RootMedia(), Constants.Security.SuperUserKey, CancellationToken.None);

        // unlike the draft media index, the published index should not contain trashed media
        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Is.Empty);
    }

    [Test]
    public async Task FullStructure_RemovesDocumentsWhenChildrenAreTrashed()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);
        await MediaService.MoveToRecycleBinAsync(ChildFolder(), Constants.Security.SuperUserKey, CancellationToken.None);
        await MediaService.MoveToRecycleBinAsync(ChildMedia(), Constants.Security.SuperUserKey, CancellationToken.None);

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
    public async Task FullStructure_RemovesAllDocumentsWhenRootIsDeleted(bool moveToRecycleBinBeforeDeleting)
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

        if (moveToRecycleBinBeforeDeleting)
        {
            await MediaService.MoveToRecycleBinAsync(RootFolder(), Constants.Security.SuperUserKey, CancellationToken.None);
        }

        await MediaService.DeleteAsync(RootFolder(), Constants.Security.SuperUserKey, CancellationToken.None);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(PublishedMediaIndexAlias);
        Assert.That(documents, Has.Count.EqualTo(1));

        Assert.That(documents[0].Id, Is.EqualTo(RootMediaKey));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task PublishedStructure_RebuildIndexYieldsAllDocuments(bool populateIndexBeforeRebuild)
    {
        if (populateIndexBeforeRebuild)
        {
            await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);
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
    public async Task PublishedStructure_RebuildExcludesTrashedContent()
    {
        await MediaService.MoveToRecycleBinAsync(ChildFolder(), Constants.Security.SuperUserKey, CancellationToken.None);
        await MediaService.MoveToRecycleBinAsync(RootMedia(), Constants.Security.SuperUserKey, CancellationToken.None);

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
    public async Task FullStructure_DoesNotYieldDocumentsInDraftMediaIndex()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

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
