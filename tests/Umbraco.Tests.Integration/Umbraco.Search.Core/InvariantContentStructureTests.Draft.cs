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
    public void DraftStructure_YieldsAllDocuments()
    {
        ContentService.SaveAsync([Root(), Child(), Grandchild(), GreatGrandchild()], Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None).GetAwaiter().GetResult();

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
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
    public void DraftStructure_YieldsNoPublishedDocuments()
    {
        ContentService.SaveAsync([Root(), Child(), Grandchild(), GreatGrandchild()], Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None).GetAwaiter().GetResult();

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.PublishedContent);
        Assert.That(documents, Has.Count.EqualTo(0));
    }

    [Test]
    public async Task DraftRoot_YieldsOnlyDraftRoot()
    {
        await ContentService.SaveAsync(Root(), Cms.Core.Constants.Security.SuperUserKey, null, CancellationToken.None);

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(1));
        Assert.That(documents[0].Id, Is.EqualTo(RootKey));
    }

    [Test]
    public async Task DraftStructure_WithGrandchildInRecycleBin_YieldsAllDocuments()
    {
        ContentService.SaveAsync([Root(), Child(), Grandchild(), GreatGrandchild()], Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None).GetAwaiter().GetResult();

        Attempt<ContentMoveToRecycleBinOperationStatus> result = await ContentService.MoveToRecycleBinAsync(Grandchild(), Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(Grandchild().Trashed, Is.True);
            Assert.That(GreatGrandchild().Trashed, Is.True);
        });

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
            Assert.That(documents[2].Id, Is.EqualTo(GrandchildKey));
            Assert.That(documents[3].Id, Is.EqualTo(GreatGrandchildKey));
        });
    }

    [Test]
    public void DraftStructure_WithGrandchildDeleted_YieldsNothingBelowChild()
    {
        ContentService.SaveAsync([Root(), Child(), Grandchild(), GreatGrandchild()], Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None).GetAwaiter().GetResult();

        Attempt<ContentDeleteOperationStatus> result = ContentService.DeleteAsync(Grandchild(), Cms.Core.Constants.Security.SuperUserKey, CancellationToken.None).GetAwaiter().GetResult();
        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(ContentService.GetByIdAsync(GreatGrandchildKey, CancellationToken.None).GetAwaiter().GetResult(), Is.Null);
        });

        IReadOnlyList<TestIndexDocument> documents = IndexerAndSearcher.Dump(IndexAliases.DraftContent);
        Assert.That(documents, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(documents[0].Id, Is.EqualTo(RootKey));
            Assert.That(documents[1].Id, Is.EqualTo(ChildKey));
        });
    }
}
