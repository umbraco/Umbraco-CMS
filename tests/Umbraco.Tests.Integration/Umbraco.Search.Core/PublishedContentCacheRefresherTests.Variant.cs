using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

public partial class PublishedContentCacheRefresherTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Variant_PublishRoot_MultipleCultures(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        if (publishDescendants)
        {
            ContentService.Save(Get(RootKey));
            ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        }
        else
        {
            ContentService.Save(Get(RootKey));
            ContentService.Publish(Get(RootKey), ["*"]);
        }

        // the result must be same no matter if descendants are included or not, because the root was unpublished to begin with
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshBranch));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.EquivalentTo(new[] { "en-US", "da-DK" }));
        });
    }

    [TestCase("en-US", true)]
    [TestCase("en-US", false)]
    [TestCase("da-DK", true)]
    [TestCase("da-DK", false)]
    public async Task Variant_PublishRoot_SingleCulture(string cultureToPublish, bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, [cultureToPublish]);

        // the result must be same no matter if descendants are included or not, because the root was unpublished to begin with
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshBranch));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.EquivalentTo(new[] { cultureToPublish }));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Variant_PublishRoot_CultureByCulture(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        if (publishDescendants)
        {
            ContentService.Save(Get(RootKey));
            ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["en-US"]);
            ContentService.Save(Get(RootKey));
            ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["da-DK"]);
        }
        else
        {
            ContentService.Save(Get(RootKey));
            ContentService.Publish(Get(RootKey), ["en-US"]);
            ContentService.Save(Get(RootKey));
            ContentService.Publish(Get(RootKey), ["da-DK"]);
        }

        // the result must be same no matter if descendants are included or not, because the root was unpublished to begin with
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshBranch));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.EquivalentTo(new[] { "en-US" }));

            Assert.That(payloads[1].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshBranch));
            Assert.That(payloads[1].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[1].AffectedCultures, Is.EquivalentTo(new[] { "da-DK" }));
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Variant_RepublishChild_MultipleCultures(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        if (publishDescendants)
        {
            // we need to change something, otherwise the branch publish will detect "no changes" and no notifications will be invoked
            IContent content = Get(ChildKey);
            content.SetCultureName("Updated EN", "en-US");
            content.SetCultureName("Updated DA", "da-DK");
            ContentService.Save(content);

            ContentService.Save(Get(ChildKey));
            ContentService.PublishBranch(Get(ChildKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        }
        else
        {
            ContentService.Save(Get(ChildKey));
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

    [TestCase("en-US", true)]
    [TestCase("en-US", false)]
    [TestCase("da-DK", true)]
    [TestCase("da-DK", false)]
    public async Task Variant_RepublishChild_SingleCultures(string cultureToPublish, bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        if (publishDescendants)
        {
            // we need to change something, otherwise the branch publish will detect "no changes" and no notifications will be invoked
            IContent content = Get(ChildKey);
            content.SetCultureName("Updated EN", "en-US");
            content.SetCultureName("Updated DA", "da-DK");
            ContentService.Save(content);

            ContentService.Save(Get(ChildKey));
            ContentService.PublishBranch(Get(ChildKey), PublishBranchFilter.IncludeUnpublished, [cultureToPublish]);
        }
        else
        {
            ContentService.Save(Get(ChildKey));
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
    public async Task Variant_UnpublishRoot_AllCultures(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
        ContentService.PublishBranch(Get(RootKey), publishDescendants ? PublishBranchFilter.IncludeUnpublished : PublishBranchFilter.Default, ["*"]);
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
    public async Task Variant_UnpublishChild_AllCultures()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
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

    [TestCase(true)]
    [TestCase(false)]
    public async Task Variant_UnpublishRoot_CultureByCulture(bool publishDescendants)
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
        ContentService.PublishBranch(Get(RootKey), publishDescendants ? PublishBranchFilter.IncludeUnpublished : PublishBranchFilter.Default, ["*"]);
        ResetNotificationPayloads();

        ContentService.Unpublish(Get(RootKey), "da-DK");
        ContentService.Unpublish(Get(RootKey), "en-US");

        // the result must be same no matter if descendants are included or not, because unpublish explicitly affects the whole branch
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            // the first payload is "refresh branch" because the content is still published in one culture
            Assert.That(payloads[0].ChangeTypes, Is.EqualTo(TreeChangeTypes.RefreshBranch));
            Assert.That(payloads[0].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[0].AffectedCultures, Is.EquivalentTo(new[] { "da-DK" }));

            // the second payload is "remove" because the content is completely unpublished
            Assert.That(payloads[1].ChangeTypes, Is.EqualTo(TreeChangeTypes.Remove));
            Assert.That(payloads[1].ContentKey, Is.EqualTo(RootKey));
            Assert.That(payloads[1].AffectedCultures, Is.Empty);
        });
    }

    [Test]
    public async Task Variant_MoveRootToRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
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
    public async Task Variant_MoveChildToRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
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
    public async Task Variant_DeletePublishedRoot()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.Delete(Get(RootKey));

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
    public async Task Variant_DeletePublishedChild()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ResetNotificationPayloads();

        ContentService.Delete(Get(ChildKey));

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
    public async Task Variant_DeleteRootFromRecycleBin()
    {
        (Guid RootKey, Guid ChildKey, Guid GrandchildKey) = await SetupVariantContentTest();
        ContentService.Save(Get(RootKey));
        ContentService.PublishBranch(Get(RootKey), PublishBranchFilter.IncludeUnpublished, ["*"]);
        ContentService.MoveToRecycleBin(Get(RootKey));
        ResetNotificationPayloads();

        ContentService.Delete(Get(RootKey));

        // no payload expected; it should've already been handled when moving the content to the recycle bin
        List<PublishedContentCacheRefresher.JsonPayload> payloads = GetNotificationPayloads();
        Assert.That(payloads, Has.Count.EqualTo(0));
    }

    private async Task<(Guid RootKey, Guid ChildKey, Guid GrandchildKey)> SetupVariantContentTest()
    {
        await GetRequiredService<ILanguageService>().CreateAsync(
            new LanguageBuilder().WithCultureInfo("da-DK").Build(),
            Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("variant")
            .WithContentVariation(ContentVariation.CultureAndSegment)
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        contentType.AllowedContentTypes = [new ContentTypeSort(contentType.Key, 0, contentType.Alias)];
        await ContentTypeService.UpdateAsync(contentType, Constants.Security.SuperUserKey);

        Content root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Root EN")
            .WithCultureName("da-DK", "Root DA")
            .Build();
        ContentService.Save(root);

        Content child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Child EN")
            .WithCultureName("da-DK", "Child DA")
            .WithParent(root)
            .Build();
        ContentService.Save(child);

        Content grandchild = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Grandchild EN")
            .WithCultureName("da-DK", "Grandchild DA")
            .WithParent(child)
            .Build();
        ContentService.Save(grandchild);

        ResetNotificationPayloads();

        return (root.Key, child.Key, grandchild.Key);
    }
}
