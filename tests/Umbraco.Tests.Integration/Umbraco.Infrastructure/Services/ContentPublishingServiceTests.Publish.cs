using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Integration.Attributes;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentPublishingServiceTests
{
    [Test]
    public async Task Can_Publish_Root()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Publish_Single_Item_Does_Not_Publish_Children()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        VerifyIsPublished(Textpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Publish_Child_Of_Root()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);
        VerifyIsPublished(Subpage.Key);
    }

    [TestCase(PublishBranchFilter.Default)]
    [TestCase(PublishBranchFilter.IncludeUnpublished)]
    [TestCase(PublishBranchFilter.ForceRepublish)]
    [TestCase(PublishBranchFilter.All)]
    public async Task Publish_Branch_Does_Not_Publish_Unpublished_Children_Unless_Instructed_To(PublishBranchFilter publishBranchFilter)
    {
        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, publishBranchFilter, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);

        VerifyIsPublished(Textpage.Key);

        if (publishBranchFilter.HasFlag(PublishBranchFilter.IncludeUnpublished))
        {
            AssertBranchResultSuccess(result.Result, Textpage.Key, Subpage.Key, Subpage2.Key, Subpage3.Key);
            VerifyIsPublished(Subpage.Key);
            VerifyIsPublished(Subpage2.Key);
            VerifyIsPublished(Subpage3.Key);
        }
        else
        {
            AssertBranchResultSuccess(result.Result, Textpage.Key);
            VerifyIsNotPublished(Subpage.Key);
            VerifyIsNotPublished(Subpage2.Key);
            VerifyIsNotPublished(Subpage3.Key);
        }
    }

    [Test]
    public async Task Can_Publish_Branch_Beneath_Root()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        var subpage2Subpage = ContentBuilder.CreateSimpleContent(ContentType, "Text Page 2-2", Subpage2.Id);
        ContentService.Save(subpage2Subpage, -1);

        VerifyIsNotPublished(Subpage2.Key);
        var result = await ContentPublishingService.PublishBranchAsync(Subpage2.Key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        AssertBranchResultSuccess(result.Result, Subpage2.Key, subpage2Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(subpage2Subpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Cancel_Publishing_With_Notification()
    {
        ContentNotificationHandler.PublishingContent = notification => notification.Cancel = true;

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CancelledByEvent, result.Status);
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

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langDa.IsoCode));
    }

    [Test]
    public async Task Can_Publish_All_Variants_And_Unpublish_All_Variants_And_Publish_A_Single_Variant()
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

        var publishResult = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);

        Assert.IsTrue(publishResult.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, publishResult.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langEn.IsoCode));
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langDa.IsoCode));

        var unpublishResult = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string>() { "*" }, Constants.Security.SuperUserKey);

        Assert.IsTrue(unpublishResult.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, unpublishResult.Result);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());

        publishResult = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langDa.IsoCode }), Constants.Security.SuperUserKey);

        Assert.IsTrue(publishResult.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, publishResult.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
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

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode, langDa.IsoCode }, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

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

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode }), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);

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

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode }, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(1, root.PublishedCultures.Count());
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langEn.IsoCode));

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(1, child.PublishedCultures.Count());
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langEn.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Culture_Branch_Without_Other_Culture()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        IContent root = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        root.SetValue("title", "EN title", culture: langEn.IsoCode);
        root.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(root);

        root = ContentService.GetById(root.Key)!;

        IContent child = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN child")
            .WithCultureName(langDa.IsoCode, "DA child")
            .WithParent(root)
            .Build();
        child.SetValue("title", "EN child title", culture: langEn.IsoCode);
        child.SetValue("title", "DA child title", culture: langDa.IsoCode);
        ContentService.Save(child);

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode }, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(1, root.PublishedCultures.Count());
        Assert.IsTrue(root.PublishedCultures.InvariantContains(langEn.IsoCode));

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(1, child.PublishedCultures.Count());
        Assert.IsTrue(child.PublishedCultures.InvariantContains(langEn.IsoCode));
    }

    [Test]
    public async Task Can_Publish_Variant_Content_With_Mandatory_Culture()
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

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
    }

    [TestCase(true, "da-DK")]
    [TestCase(false, "en-US")]
    [TestCase(false, "en-US", "da-DK")]
    public async Task Publish_Invalid_Invariant_Property_WithoutAllowEditInvariantFromNonDefault(bool expectedSuccess, params string[] culturesToRepublish)
        => await Publish_Invalid_Invariant_Property(expectedSuccess, culturesToRepublish);

    [TestCase(false, "da-DK")]
    [TestCase(false, "en-US")]
    [TestCase(false, "en-US", "da-DK")]
    [ConfigureBuilder(ActionName = nameof(ConfigureAllowEditInvariantFromNonDefaultTrue))]
    public async Task Publish_Invalid_Invariant_Property_WithAllowEditInvariantFromNonDefault(bool expectedSuccess, params string[] culturesToRepublish)
        => await Publish_Invalid_Invariant_Property(expectedSuccess, culturesToRepublish);

    private async Task Publish_Invalid_Invariant_Property(bool expectedSuccess, params string[] culturesToRepublish)
    {
        var contentType = await SetupVariantInvariantTest();

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "EN")
            .WithCultureName("da-DK", "DA")
            .Build();
        content.SetValue("variantValue", "EN value", culture: "en-US");
        content.SetValue("variantValue", "DA value", culture: "da-DK");
        content.SetValue("invariantValue", "Invariant value");
        ContentService.Save(content);

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string> { "en-US", "da-DK" }), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);

        content = ContentService.GetById(content.Key)!;
        content.SetValue("variantValue", "EN value updated", culture: "en-US");
        content.SetValue("variantValue", "DA value updated", culture: "da-DK");
        content.SetValue("invariantValue", null);
        ContentService.Save(content);

        result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>(culturesToRepublish)), Constants.Security.SuperUserKey);

        content = ContentService.GetById(content.Key)!;

        Assert.Multiple(() =>
        {
            Assert.AreEqual(null, content.GetValue("invariantValue", published: false));
            Assert.AreEqual("EN value updated", content.GetValue("variantValue", culture: "en-US", published: false));
            Assert.AreEqual("DA value updated", content.GetValue("variantValue", culture: "da-DK", published: false));

            Assert.AreEqual("Invariant value", content.GetValue("invariantValue", published: true));
        });

        if (expectedSuccess)
        {
            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.Success);

                var expectedPublishedEnglishValue = culturesToRepublish.Contains("en-US")
                    ? "EN value updated"
                    : "EN value";
                var expectedPublishedDanishValue = culturesToRepublish.Contains("da-DK")
                    ? "DA value updated"
                    : "DA value";
                Assert.AreEqual(expectedPublishedEnglishValue, content.GetValue("variantValue", culture: "en-US", published: true));
                Assert.AreEqual(expectedPublishedDanishValue, content.GetValue("variantValue", culture: "da-DK", published: true));
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.IsFalse(result.Success);
                Assert.AreEqual("EN value", content.GetValue("variantValue", culture: "en-US", published: true));
                Assert.AreEqual("DA value", content.GetValue("variantValue", culture: "da-DK", published: true));
            });
        }
    }

    [Test]
    public async Task Cannot_Publish_Non_Existing_Content()
    {
        var result = await ContentPublishingService.PublishAsync(Guid.NewGuid(), MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentNotFound, result.Status);
    }

    [Test]
    public async Task Cannot_Publish_Branch_Of_Non_Existing_Content()
    {
        var key = Guid.NewGuid();
        var result = await ContentPublishingService.PublishBranchAsync(key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);
        Assert.IsFalse(result);
        AssertBranchResultFailed(result.Result, (key, ContentPublishingOperationStatus.ContentNotFound));
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Content()
    {
        var content = await CreateInvalidContent();

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsFalse(result);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status);

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.AreEqual(3, invalidPropertyAliases.Length);
        Assert.Contains("title", invalidPropertyAliases);
        Assert.Contains("bodyText", invalidPropertyAliases);
        Assert.Contains("author", invalidPropertyAliases);

        VerifyIsNotPublished(content.Key);
    }

    [Test]
    public async Task Cannot_Publish_Branch_With_Invalid_Parent()
    {
        var content = await CreateInvalidContent(Textpage);
        var child = ContentBuilder.CreateSimpleContent(ContentType, "Child page", content.Id);
        ContentService.Save(child, -1);
        Assert.AreEqual(content.Id, ContentService.GetById(child.Key)!.ParentId);

        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        AssertBranchResultSuccess(result.Result, Textpage.Key, Subpage.Key, Subpage2.Key, Subpage3.Key);
        AssertBranchResultFailed(result.Result, (content.Key, ContentPublishingOperationStatus.ContentInvalid));
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

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status);

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

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langDa.IsoCode }), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.MandatoryCultureMissing, result.Status);

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

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode, langDa.IsoCode }, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        AssertBranchResultFailed(result.Result, (root.Key, ContentPublishingOperationStatus.ContentInvalid));

        root = ContentService.GetById(root.Key)!;
        Assert.AreEqual(0, root.PublishedCultures.Count());

        child = ContentService.GetById(child.Key)!;
        Assert.AreEqual(0, child.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Child_Of_Unpublished_Parent()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.PathNotPublished, result.Status);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Cannot_Publish_From_Trash()
    {
        ContentService.MoveToRecycleBin(Subpage);
        Assert.IsTrue(ContentService.GetById(Subpage.Key)!.Trashed);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InTrash, result.Status);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Content_After_Adding_Validation_To_Existing_Property()
    {
        Textpage.SetValue("title", string.Empty);
        Textpage.SetValue("author", "This is not a number");
        ContentService.Save(Textpage);

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyIsPublished(Textpage.Key);

        ContentType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        ContentType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExp = "^\\d*$";
        await ContentTypeService.SaveAsync(ContentType, Constants.Security.SuperUserKey);

        result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status);

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.AreEqual(2, invalidPropertyAliases.Length);
        Assert.Contains("title", invalidPropertyAliases);
        Assert.Contains("author", invalidPropertyAliases);

        // despite the failure to publish, the page should remain published
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Content_After_Adding_Mandatory_Property()
    {
        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyIsPublished(Textpage.Key);

        ContentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar)
            {
                Alias = "mandatoryProperty", Name = "Mandatory Property", Mandatory = true
            });
        await ContentTypeService.SaveAsync(ContentType, Constants.Security.SuperUserKey);

        result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, result.Status);

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.AreEqual(1, invalidPropertyAliases.Length);
        Assert.Contains("mandatoryProperty", invalidPropertyAliases);

        // despite the failure to publish, the page should remain published
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Branch_After_Adding_Mandatory_Property()
    {
        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        VerifyIsPublished(Textpage.Key);
        VerifyIsPublished(Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(Subpage3.Key);

        // force an update on the child pages so they will be subject to branch republishing
        foreach (var key in new [] { Subpage.Key, Subpage2.Key, Subpage3.Key })
        {
            var content = ContentService.GetById(key)!;
            content.SetValue("title", "Updated");
            ContentService.Save(content);
        }

        ContentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar)
            {
                Alias = "mandatoryProperty", Name = "Mandatory Property", Mandatory = true
            });
        await ContentTypeService.SaveAsync(ContentType, Constants.Security.SuperUserKey);

        // force an update on the root page so it is valid (and also subject to branch republishing).
        // if we didn't do this, the children would never be considered for branch publishing, as the publish logic
        // stops at the first invalid parent.
        // as an added bonus, this lets us test a partially successful branch publish :)
        var textPage = ContentService.GetById(Textpage.Key)!;
        textPage.SetValue("mandatoryProperty", "This is a valid value");
        ContentService.Save(textPage);

        result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.FailedBranch, result.Status);
        AssertBranchResultSuccess(result.Result, Textpage.Key);
        AssertBranchResultFailed(
            result.Result,
            (Subpage.Key, ContentPublishingOperationStatus.ContentInvalid),
            (Subpage2.Key, ContentPublishingOperationStatus.ContentInvalid),
            (Subpage3.Key, ContentPublishingOperationStatus.ContentInvalid));

        // despite the failure to publish, the entier branch should remain published
        VerifyIsPublished(Textpage.Key);
        VerifyIsPublished(Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(Subpage3.Key);
    }

    [TestCase("en-us")]
    [TestCase("da-dk")]
    public async Task Cannot_Publish_Incorrect_Culture_Code(string cultureCode)
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

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { cultureCode }), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, result.Status);
    }

    [TestCase("de-DE")]
    [TestCase("es-ES")]
    public async Task Cannot_Publish_Non_Existing_Culture(string cultureCode)
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

        var result = await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { cultureCode }), Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, result.Status);
    }

    [Test]
    public async Task Can_Publish_Invariant_Content_With_Cultures_Provided_If_The_Default_Culture_Is_Exclusively_Provided()
    {
        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(new HashSet<string>() { "en-US" }), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task Can_Publish_Invariant_Content_With_Cultures_Provided_If_The_Default_Culture_Is_Provided_With_Other_Cultures()
    {
        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(new HashSet<string>() { "en-US", "da-DK" }), Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task Cannot_Publish_Invariant_Content_With_Cultures_Provided_That_Do_Not_Include_The_Default_Culture()
    {
        var result = await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(new HashSet<string>() { "da-DK" }), Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, result.Status);
    }

    private void AssertBranchResultSuccess(ContentPublishingBranchResult result, params Guid[] expectedKeys)
    {
        var items = result.SucceededItems.ToArray();
        Assert.AreEqual(expectedKeys.Length, items.Length);
        foreach (var key in expectedKeys)
        {
            var item = items.FirstOrDefault(i => i.Key == key);
            Assert.IsNotNull(item);
            Assert.AreEqual(ContentPublishingOperationStatus.Success, item.OperationStatus);
        }
    }

    private void AssertBranchResultFailed(ContentPublishingBranchResult result, params (Guid, ContentPublishingOperationStatus)[] expectedFailures)
    {
        var items = result.FailedItems.ToArray();
        Assert.AreEqual(expectedFailures.Length, items.Length);
        foreach (var (key, status) in expectedFailures)
        {
            var item = items.FirstOrDefault(i => i.Key == key);
            Assert.IsNotNull(item);
            Assert.AreEqual(status, item.OperationStatus);
        }
    }
}
