using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public partial class PublishedContentCacheRefresherTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Invariant_PublishRoot(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        if (publishDescendants)
        {
            await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
            ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        }
        else
        {
            await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
            ContentService.Publish(Get(RootKey), ["*"]);
        }

        // the result must be same no matter if descendants are included or not, because the root was unpublished to begin with
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshBranch));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Invariant_RepublishChild(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        if (publishDescendants)
        {
            // we need to change something, otherwise the branch publish will detect "no changes" and no notifications will be invoked
            IContent content = Get(ChildKey);
            content.Name = "Updated";
            await ContentService.SaveAsync(content, null, null, CancellationToken.None);

            await ContentService.SaveAsync(Get(ChildKey), null, null, CancellationToken.None);
            ContentService.PublishBranch(Get(ChildKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        }
        else
        {
            await ContentService.SaveAsync(Get(ChildKey), null, null, CancellationToken.None);
            ContentService.Publish(Get(ChildKey), ["*"]);
        }

        // the result must be same no matter if descendants are included or not, because the child was already published
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshNode));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(ChildKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Invariant_UnpublishRoot(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.Unpublish(Get(RootKey));

        // the result must be same no matter if descendants are included or not, because unpublish explicitly affects the whole branch
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_UnpublishChild()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.Unpublish(Get(ChildKey));

        // the result must be same no matter if descendants are included or not, because unpublish explicitly affects the whole branch
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(ChildKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_MoveRootToRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.MoveToRecycleBin(Get(RootKey));

        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_MoveChildToRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.MoveToRecycleBin(Get(ChildKey));

        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(ChildKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_DeletePublishedRoot()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        await ContentService.DeleteAsync(Get(RootKey), null, CancellationToken.None);

        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_DeletePublishedChild()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        await ContentService.DeleteAsync(Get(ChildKey), null, CancellationToken.None);

        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(ChildKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_DeleteRootFromRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupInvariantContentTest();
        await ContentService.SaveAsync(Get(RootKey), null, null, CancellationToken.None);
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentService.MoveToRecycleBin(Get(RootKey));
        ResetNotificationPayloads();

        await ContentService.DeleteAsync(Get(RootKey), null, CancellationToken.None);

        // no payload expected; it should've already been handled when moving the content to the recycle bin
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(0));
    }

    private async Task<(Guid RootKey, Guid ChildKey, Guid GrandchildKey)> SetupInvariantContentTest()
    {
        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.Nothing)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Root")
            .Build();
        await ContentService.SaveAsync(root, null, null, CancellationToken.None);

        Content child = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Child")
            .WithParent(root)
            .Build();
        await ContentService.SaveAsync(child, null, null, CancellationToken.None);

        Content grandchild = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Grandchild")
            .WithParent(child)
            .Build();
        await ContentService.SaveAsync(grandchild, null, null, CancellationToken.None);

        ResetNotificationPayloads();

        return (root.Key, child.Key, grandchild.Key);
    }
}
