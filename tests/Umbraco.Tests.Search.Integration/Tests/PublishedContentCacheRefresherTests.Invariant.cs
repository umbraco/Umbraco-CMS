using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Integration.Tests;

public partial class PublishedContentCacheRefresherTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Invariant_PublishRoot(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        if (publishDescendants)
        {
            ContentService.Save(Get(setup.RootKey));
            ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        }
        else
        {
            ContentService.Save(Get(setup.RootKey));
            ContentService.Publish(Get(setup.RootKey), ["*"]);
        }

        // the result must be same no matter if descendants are included or not, because the root was unpublished to begin with
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshBranch));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(setup.RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Invariant_RepublishChild(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        ContentService.Save(Get(setup.RootKey));
        ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        if (publishDescendants)
        {
            // we need to change something, otherwise the branch publish will detect "no changes" and no notifications will be invoked
            IContent content = Get(setup.ChildKey);
            content.Name = "Updated";
            ContentService.Save(content);

            ContentService.Save(Get(setup.ChildKey));
            ContentService.PublishBranch(Get(setup.ChildKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        }
        else
        {
            ContentService.Save(Get(setup.ChildKey));
            ContentService.Publish(Get(setup.ChildKey), ["*"]);
        }

        // the result must be same no matter if descendants are included or not, because the child was already published
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshNode));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(setup.ChildKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Invariant_UnpublishRoot(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        ContentService.Save(Get(setup.RootKey));
        ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.Unpublish(Get(setup.RootKey));

        // the result must be same no matter if descendants are included or not, because unpublish explicitly affects the whole branch
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(setup.RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_UnpublishChild()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        ContentService.Save(Get(setup.RootKey));
        ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.Unpublish(Get(setup.ChildKey));

        // the result must be same no matter if descendants are included or not, because unpublish explicitly affects the whole branch
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(setup.ChildKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_MoveRootToRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        ContentService.Save(Get(setup.RootKey));
        ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.MoveToRecycleBin(Get(setup.RootKey));

        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(setup.RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_MoveChildToRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        ContentService.Save(Get(setup.RootKey));
        ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.MoveToRecycleBin(Get(setup.ChildKey));

        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(setup.ChildKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_DeletePublishedRoot()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        ContentService.Save(Get(setup.RootKey));
        ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.Delete(Get(setup.RootKey));

        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(setup.RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_DeletePublishedChild()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        ContentService.Save(Get(setup.RootKey));
        ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.Delete(Get(setup.ChildKey));

        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(setup.ChildKey));
            Assert.That(payloads[0].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Invariant_DeleteRootFromRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) setup = await SetupInvariantContentTest();
        ContentService.Save(Get(setup.RootKey));
        ContentService.PublishBranch(Get(setup.RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentService.MoveToRecycleBin(Get(setup.RootKey));
        ResetNotificationPayloads();

        ContentService.Delete(Get(setup.RootKey));

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
        ContentService.Save(root);

        Content child = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Child")
            .WithParent(root)
            .Build();
        ContentService.Save(child);

        Content grandchild = new ContentBuilder()
            .WithContentType(contentType)
            .WithName("Grandchild")
            .WithParent(child)
            .Build();
        ContentService.Save(grandchild);

        ResetNotificationPayloads();

        return (root.Key, child.Key, grandchild.Key);
    }
}
