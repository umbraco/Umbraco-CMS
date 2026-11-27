using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class MediaStructureTests : MediaTestBase
{
    [Test]
    public async Task FullStructure_YieldsAllDocuments()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
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
    public async Task FullStructure_YieldsNoDraftContentDocuments()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task RootOnly_YieldsOnlyRoot()
    {
        await MediaService.SaveAsync(RootFolder(), Constants.Security.SuperUserKey, CancellationToken.None);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
    }

    [Test]
    public async Task FullStructure_WithChildFolderInRecycleBin_YieldsAllDocuments()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

        Attempt<ContentMoveToRecycleBinOperationStatus> result = await MediaService.MoveToRecycleBinAsync(ChildFolder(), Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(ChildFolder().Trashed, Is.True);
            Assert.That(GrandchildMedia().Trashed, Is.True);
        });

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildFolderKey));
            Assert.That(documents[2].Id, Is.EqualTo(RootMediaKey));
            Assert.That(documents[3].Id, Is.EqualTo(ChildMediaKey));
            Assert.That(documents[4].Id, Is.EqualTo(GrandchildMediaKey));
        });
    }

    [Test]
    public async Task FullStructure_WithChildFolderDeleted_YieldsNothingBelowRootFolder()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

        Attempt<ContentDeleteOperationStatus> result = await MediaService.DeleteAsync(ChildFolder(), Constants.Security.SuperUserKey, CancellationToken.None);
        IMedia? grandchildMedia = await MediaService.GetByIdAsync(GrandchildMediaKey, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(grandchildMedia, Is.Null);
        });

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(3));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(RootMediaKey));
            Assert.That(documents[2].Id, Is.EqualTo(ChildMediaKey));
        });
    }

    [Test]
    public async Task DeleteMediaType_RemovesDocuments()
    {
        await MediaService.SaveAsync([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()], Constants.Security.SuperUserKey, CancellationToken.None);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        await MediaTypeService.DeleteAsync(RootMedia().ContentType.Key, Cms.Core.Constants.Security.SuperUserKey);

        documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildFolderKey));

            Assert.That(documents.All(d => d.ObjectType is UmbracoObjectTypes.Media), Is.True);
        });
    }
}
