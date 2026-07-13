using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public class MediaStructureTests : MediaTestBase
{
    [Test]
    public void FullStructure_YieldsAllDocuments()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

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
    public void FullStructure_YieldsNoDraftContentDocuments()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(0));
    }

    [Test]
    public void RootOnly_YieldsOnlyRoot()
    {
        MediaService.Save(RootFolder());

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootFolderKey));
    }

    [Test]
    public void FullStructure_WithChildFolderInRecycleBin_YieldsAllDocuments()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        Attempt<OperationResult?> result = MediaService.MoveToRecycleBin(ChildFolder());
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
    public void FullStructure_WithChildFolderDeleted_YieldsNothingBelowRootFolder()
    {
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        Attempt<OperationResult?> result = MediaService.Delete(ChildFolder());
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(MediaService.GetById(GrandchildMediaKey), Is.Null);
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
        MediaService.Save([RootFolder(), ChildFolder(), RootMedia(), ChildMedia(), GrandchildMedia()]);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.Media);
        Assert.That(documents, Has.Count.EqualTo(5));

        await MediaTypeService.DeleteAsync(RootMedia().ContentType.Key, Constants.Security.SuperUserKey);

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
