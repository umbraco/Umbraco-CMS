using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentPublishingServiceTests
{
    [Test]
    public async Task Can_Publish_Root()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, _allCultures, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Publish_Single_Item_Does_Not_Publish_Children()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, _allCultures, Constants.Security.SuperUserKey);

        VerifyIsPublished(Textpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Publish_Child_Of_Root()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, new[] { "*" }, Constants.Security.SuperUserKey);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, _allCultures, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsPublished(Subpage.Key);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Publish_Branch_Does_Not_Publish_Unpublished_Children_Unless_Explicitly_Instructed_To(bool force)
    {
        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, force, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);

        VerifyIsPublished(Textpage.Key);

        if (force)
        {
            Assert.AreEqual(4, result.Result.Count);
            Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Textpage.Key]);
            Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Subpage.Key]);
            Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Subpage2.Key]);
            Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Subpage3.Key]);
            VerifyIsPublished(Subpage.Key);
            VerifyIsPublished(Subpage2.Key);
            VerifyIsPublished(Subpage3.Key);
        }
        else
        {
            Assert.AreEqual(1, result.Result.Count);
            Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Textpage.Key]);
            VerifyIsNotPublished(Subpage.Key);
            VerifyIsNotPublished(Subpage2.Key);
            VerifyIsNotPublished(Subpage3.Key);
        }
    }

    [Test]
    public async Task Can_Publish_Branch_Beneath_Root()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, _allCultures, Constants.Security.SuperUserKey);
        var subpage2Subpage = ContentBuilder.CreateSimpleContent(ContentType, "Text Page 2-2", Subpage2.Id);
        ContentService.Save(subpage2Subpage, -1);

        VerifyIsNotPublished(Subpage2.Key);
        var result = await ContentPublishingService.PublishBranchAsync(Subpage2.Key, _allCultures, true, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Result.Count);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Subpage2.Key]);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[subpage2Subpage.Key]);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(subpage2Subpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Cancel_Publishing_With_Notification()
    {
        ContentNotificationHandler.PublishingContent = notification => notification.Cancel = true;

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, _allCultures, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CancelledByEvent, result.Result);
    }

    [Test]
    public async Task Can_Publish_Variant_Content()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, new[] { langEn.IsoCode, langDa.IsoCode }, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langDa.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Branch_Of_Variant_Content()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        root.SetValue("title", "EN root title", culture: langEn.IsoCode);
        root.SetValue("title", "DA root title", culture: langDa.IsoCode);
        ContentService.Save(root);

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN child")
            .WithCultureName(langDa.IsoCode, "DA child")
            .WithParent(root)
            .Build();
        child.SetValue("title", "EN child title", culture: langEn.IsoCode);
        child.SetValue("title", "DA child title", culture: langDa.IsoCode);
        ContentService.Save(child);

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode, langDa.IsoCode }, true, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Result!.Count);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[root.Key]);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[child.Key]);

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(2, root.PublishedCultures.Count());
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langDa.IsoCode));

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(2, child.PublishedCultures.Count());
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langDa.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Culture_With_Other_Culture_Invalid()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, new[] { langEn.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.First().InvariantEquals(langEn.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Culture_Branch_With_Other_Culture_Invalid()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        root.SetValue("title", "EN title", culture: langEn.IsoCode);
        root.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(root);

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN child")
            .WithCultureName(langDa.IsoCode, "DA child")
            .WithParent(root)
            .Build();
        child.SetValue("title", "EN child title", culture: langEn.IsoCode);
        child.SetValue("title", "DA child title", culture: langDa.IsoCode);
        ContentService.Save(child);

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode }, true, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.Result!.Count);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[root.Key]);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[child.Key]);

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(1, root.PublishedCultures.Count());
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langEn.IsoCode));

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(1, child.PublishedCultures.Count());
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langEn.IsoCode));
    }

    [Test]
    public async Task Cannot_Publish_Variant_Content_With_Mandatory_Culture()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest(true);

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, new[] { langEn.IsoCode, langDa.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Non_Existing_Content()
    {
        var result = await ContentPublishingService.PublishAsync(Guid.NewGuid(), _allCultures, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentNotFound, result.Result);
    }

    [Test]
    public async Task Cannot_Publish_Branch_Of_Non_Existing_Content()
    {
        var key = Guid.NewGuid();
        var result = await ContentPublishingService.PublishBranchAsync(key, _allCultures, true, Constants.Security.SuperUserKey);
        Assert.IsFalse(result);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentNotFound, result.Result[key]);
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Content()
    {
        var content = await CreateInvalidContent();

        var result = await ContentPublishingService.PublishAsync(content.Key, _allCultures, Constants.Security.SuperUserKey);

        Assert.IsFalse(result);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Result);
        VerifyIsNotPublished(content.Key);
    }

    [Test]
    public async Task Cannot_Publish_Branch_With_Invalid_Parent()
    {
        var content = await CreateInvalidContent(Textpage);
        var child = ContentBuilder.CreateSimpleContent(ContentType, "Child page", content.Id);
        ContentService.Save(child, -1);
        Assert.AreEqual(content.Id, ContentService.GetById(child.Key)!.ParentId);

        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, true, Constants.Security.SuperUserKey);

        Assert.IsFalse(result);
        Assert.AreEqual(5, result.Result.Count);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Textpage.Key]);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Subpage.Key]);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Subpage2.Key]);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result[Subpage3.Key]);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Result[content.Key]);
        VerifyIsPublished(Textpage.Key);
        VerifyIsPublished(Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(Subpage3.Key);
        VerifyIsNotPublished(content.Key);
        VerifyIsNotPublished(child.Key);
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Variant_Content()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, new[] { langEn.IsoCode, langDa.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Result);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Variant_Content_Without_Mandatory_Culture()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest(true);

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, new[] { langDa.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.MandatoryCultureMissing, result.Result);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Culture_Branch_With_Invalid_Culture()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        root.SetValue("title", "EN title", culture: langEn.IsoCode);
        root.SetValue("title", null, culture: langDa.IsoCode);
        ContentService.Save(root);

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN child")
            .WithCultureName(langDa.IsoCode, "DA child")
            .WithParent(root)
            .Build();
        child.SetValue("title", "EN child title", culture: langEn.IsoCode);
        child.SetValue("title", "DA child title", culture: langDa.IsoCode);
        ContentService.Save(child);

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode, langDa.IsoCode }, true, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(1, result.Result!.Count);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Result[root.Key]);

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(0, root.PublishedCultures.Count());

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(0, child.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Child_Of_Unpublished_Parent()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, _allCultures, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.PathNotPublished, result.Result);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Cannot_Publish_From_Trash()
    {
        ContentService.MoveToRecycleBin(Subpage);
        Assert.IsTrue(ContentService.GetById(Subpage.Key)!.Trashed);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, _allCultures, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InTrash, result.Result);
        VerifyIsNotPublished(Subpage.Key);
    }
}
