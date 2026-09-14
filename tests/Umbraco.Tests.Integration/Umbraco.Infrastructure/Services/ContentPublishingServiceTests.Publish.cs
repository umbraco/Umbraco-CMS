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

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.Success));
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Publish_Single_Item_Does_Not_Publish_Children()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);

        VerifyIsPublished(Textpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Publish_Child_Of_Root()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.Success));
        VerifyIsPublished(Subpage.Key);
    }

    [TestCase(PublishBranchFilter.Default)]
    [TestCase(PublishBranchFilter.IncludeUnpublished)]
    [TestCase(PublishBranchFilter.ForceRepublish)]
    [TestCase(PublishBranchFilter.All)]
    public async Task Publish_Branch_Does_Not_Publish_Unpublished_Children_Unless_Instructed_To(PublishBranchFilter publishBranchFilter)
    {
        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, publishBranchFilter, Constants.Security.SuperUserKey, false);

        Assert.That(result.Success, Is.True);

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
        await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);
        var subpage2Subpage = ContentBuilder.CreateSimpleContent(ContentType, "Text Page 2-2", Subpage2.Id);
        ContentService.Save(subpage2Subpage, -1);

        VerifyIsNotPublished(Subpage2.Key);
        var result = await ContentPublishingService.PublishBranchAsync(Subpage2.Key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);

        Assert.That(result.Success, Is.True);
        AssertBranchResultSuccess(result.Result, Subpage2.Key, subpage2Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(subpage2Subpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Cancel_Publishing_With_Notification()
    {
        ContentNotificationHandler.PublishingContent = notification => notification.Cancel = true;

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.CancelledByEvent));
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

        var result = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new CulturePublishScheduleModel { Culture = langEn.IsoCode },
                new CulturePublishScheduleModel { Culture = langDa.IsoCode }
            ],
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.Success));

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.PublishedCultures.Count(), Is.EqualTo(2));
        Assert.That(content.PublishedCultures.InvariantContains(langEn.IsoCode), Is.True);
        Assert.That(content.PublishedCultures.InvariantContains(langDa.IsoCode), Is.True);
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

        var publishResult = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new CulturePublishScheduleModel { Culture = langEn.IsoCode },
                new CulturePublishScheduleModel { Culture = langDa.IsoCode }
            ],
            Constants.Security.SuperUserKey);

        Assert.That(publishResult.Success, Is.True);
        Assert.That(publishResult.Status, Is.EqualTo(ContentPublishingOperationStatus.Success));

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.PublishedCultures.Count(), Is.EqualTo(2));
        Assert.That(content.PublishedCultures.InvariantContains(langEn.IsoCode), Is.True);
        Assert.That(content.PublishedCultures.InvariantContains(langDa.IsoCode), Is.True);

        var unpublishResult = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string>() { "*" }, Constants.Security.SuperUserKey);

        Assert.That(unpublishResult.Success, Is.True);
        Assert.That(unpublishResult.Result, Is.EqualTo(ContentPublishingOperationStatus.Success));

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.PublishedCultures.Count(), Is.EqualTo(0));

        publishResult = await ContentPublishingService.PublishAsync(
            content.Key,
            [new CulturePublishScheduleModel { Culture = langDa.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.That(publishResult.Success, Is.True);
        Assert.That(publishResult.Status, Is.EqualTo(ContentPublishingOperationStatus.Success));

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.PublishedCultures.Count(), Is.EqualTo(1));
        Assert.That(content.PublishedCultures.InvariantContains(langDa.IsoCode), Is.True);
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

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode, langDa.IsoCode }, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);
        Assert.That(result.Success, Is.True);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

        root = ContentService.GetById(root.Key)!;
        Assert.That(root.PublishedCultures.Count(), Is.EqualTo(2));
        Assert.That(root.PublishedCultures.InvariantContains(langEn.IsoCode), Is.True);
        Assert.That(root.PublishedCultures.InvariantContains(langDa.IsoCode), Is.True);

        child = ContentService.GetById(child.Key)!;
        Assert.That(child.PublishedCultures.Count(), Is.EqualTo(2));
        Assert.That(child.PublishedCultures.InvariantContains(langEn.IsoCode), Is.True);
        Assert.That(child.PublishedCultures.InvariantContains(langDa.IsoCode), Is.True);
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

        var result = await ContentPublishingService.PublishAsync(content.Key, [new CulturePublishScheduleModel { Culture = langEn.IsoCode }], Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.Success));

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.PublishedCultures.Count(), Is.EqualTo(1));
        Assert.That(content.PublishedCultures.First().InvariantEquals(langEn.IsoCode), Is.True);
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

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode }, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);
        Assert.That(result.Success, Is.True);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

        root = ContentService.GetById(root.Key)!;
        Assert.That(root.PublishedCultures.Count(), Is.EqualTo(1));
        Assert.That(root.PublishedCultures.InvariantContains(langEn.IsoCode), Is.True);

        child = ContentService.GetById(child.Key)!;
        Assert.That(child.PublishedCultures.Count(), Is.EqualTo(1));
        Assert.That(child.PublishedCultures.InvariantContains(langEn.IsoCode), Is.True);
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

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode }, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);
        Assert.That(result.Success, Is.True);
        AssertBranchResultSuccess(result.Result, root.Key, child.Key);

        root = ContentService.GetById(root.Key)!;
        Assert.That(root.PublishedCultures.Count(), Is.EqualTo(1));
        Assert.That(root.PublishedCultures.InvariantContains(langEn.IsoCode), Is.True);

        child = ContentService.GetById(child.Key)!;
        Assert.That(child.PublishedCultures.Count(), Is.EqualTo(1));
        Assert.That(child.PublishedCultures.InvariantContains(langEn.IsoCode), Is.True);
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

        var result = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new CulturePublishScheduleModel { Culture = langEn.IsoCode },
                new CulturePublishScheduleModel { Culture = langDa.IsoCode }
            ],
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.Success));

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.PublishedCultures.Count(), Is.EqualTo(2));
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

        var result = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new CulturePublishScheduleModel { Culture = "en-US" },
                new CulturePublishScheduleModel { Culture = "da-DK" }
            ],
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);

        content = ContentService.GetById(content.Key)!;
        content.SetValue("variantValue", "EN value updated", culture: "en-US");
        content.SetValue("variantValue", "DA value updated", culture: "da-DK");
        content.SetValue("invariantValue", null);
        ContentService.Save(content);

        result = await ContentPublishingService.PublishAsync(
            content.Key,
            culturesToRepublish.Select(culture => new CulturePublishScheduleModel { Culture = culture }).ToArray(),
            Constants.Security.SuperUserKey);

        content = ContentService.GetById(content.Key)!;

        Assert.Multiple(() =>
        {
            Assert.That(content.GetValue("invariantValue", published: false), Is.EqualTo(null));
            Assert.That(content.GetValue("variantValue", culture: "en-US", published: false), Is.EqualTo("EN value updated"));
            Assert.That(content.GetValue("variantValue", culture: "da-DK", published: false), Is.EqualTo("DA value updated"));

            Assert.That(content.GetValue("invariantValue", published: true), Is.EqualTo("Invariant value"));
        });

        if (expectedSuccess)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True);

                var expectedPublishedEnglishValue = culturesToRepublish.Contains("en-US")
                    ? "EN value updated"
                    : "EN value";
                var expectedPublishedDanishValue = culturesToRepublish.Contains("da-DK")
                    ? "DA value updated"
                    : "DA value";
                Assert.That(content.GetValue("variantValue", culture: "en-US", published: true), Is.EqualTo(expectedPublishedEnglishValue));
                Assert.That(content.GetValue("variantValue", culture: "da-DK", published: true), Is.EqualTo(expectedPublishedDanishValue));
            });
        }
        else
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(content.GetValue("variantValue", culture: "en-US", published: true), Is.EqualTo("EN value"));
                Assert.That(content.GetValue("variantValue", culture: "da-DK", published: true), Is.EqualTo("DA value"));
            });
        }
    }

    [Test]
    public async Task Cannot_Publish_Non_Existing_Content()
    {
        var result = await ContentPublishingService.PublishAsync(Guid.NewGuid(), _allCultures.Select(culture => new CulturePublishScheduleModel { Culture = culture }).ToArray(), Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.ContentNotFound));
    }

    [Test]
    public async Task Cannot_Publish_Branch_Of_Non_Existing_Content()
    {
        var key = Guid.NewGuid();
        var result = await ContentPublishingService.PublishBranchAsync(key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);
        Assert.That((bool)result, Is.False);
        AssertBranchResultFailed(result.Result, (key, ContentPublishingOperationStatus.ContentNotFound));
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Content()
    {
        var content = await CreateInvalidContent();

        var result = await ContentPublishingService.PublishAsync(content.Key, _allCultures.Select(culture => new CulturePublishScheduleModel { Culture = culture }).ToArray(), Constants.Security.SuperUserKey);

        Assert.That((bool)result, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.ContentInvalid));

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.That(invalidPropertyAliases, Has.Length.EqualTo(3));
        Assert.That(invalidPropertyAliases, Does.Contain("title"));
        Assert.That(invalidPropertyAliases, Does.Contain("bodyText"));
        Assert.That(invalidPropertyAliases, Does.Contain("author"));

        VerifyIsNotPublished(content.Key);
    }

    [Test]
    public async Task Cannot_Publish_Branch_With_Invalid_Parent()
    {
        var content = await CreateInvalidContent(Textpage);
        var child = ContentBuilder.CreateSimpleContent(ContentType, "Child page", content.Id);
        ContentService.Save(child, -1);
        Assert.That(ContentService.GetById(child.Key)!.ParentId, Is.EqualTo(content.Id));

        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);

        Assert.That(result.Success, Is.False);
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

        var result = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new CulturePublishScheduleModel { Culture = langEn.IsoCode },
                new CulturePublishScheduleModel { Culture = langDa.IsoCode }
            ],
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.ContentInvalid));

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.PublishedCultures.Count(), Is.EqualTo(0));
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

        var result = await ContentPublishingService.PublishAsync(
            content.Key,
            [new CulturePublishScheduleModel { Culture = langDa.IsoCode }],
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.MandatoryCultureMissing));

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.PublishedCultures.Count(), Is.EqualTo(0));
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

        var result = await ContentPublishingService.PublishBranchAsync(root.Key, new[] { langEn.IsoCode, langDa.IsoCode }, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);
        Assert.That(result.Success, Is.False);
        AssertBranchResultFailed(result.Result, (root.Key, ContentPublishingOperationStatus.ContentInvalid));

        root = ContentService.GetById(root.Key)!;
        Assert.That(root.PublishedCultures.Count(), Is.EqualTo(0));

        child = ContentService.GetById(child.Key)!;
        Assert.That(child.PublishedCultures.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task Cannot_Publish_Child_Of_Unpublished_Parent()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.PathNotPublished));
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Cannot_Publish_From_Trash()
    {
        ContentService.MoveToRecycleBin(Subpage);
        Assert.That(ContentService.GetById(Subpage.Key)!.Trashed, Is.True);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.InTrash));
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Content_After_Adding_Validation_To_Existing_Property()
    {
        Textpage.SetValue("title", string.Empty);
        Textpage.SetValue("author", "This is not a number");
        ContentService.Save(Textpage);

        var result = await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        VerifyIsPublished(Textpage.Key);

        ContentType.PropertyTypes.First(pt => pt.Alias == "title").Mandatory = true;
        ContentType.PropertyTypes.First(pt => pt.Alias == "author").ValidationRegExp = "^\\d*$";
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        result = await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.ContentInvalid));

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.That(invalidPropertyAliases, Has.Length.EqualTo(2));
        Assert.That(invalidPropertyAliases, Does.Contain("title"));
        Assert.That(invalidPropertyAliases, Does.Contain("author"));

        // despite the failure to publish, the page should remain published
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Content_After_Adding_Mandatory_Property()
    {
        var result = await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
        VerifyIsPublished(Textpage.Key);

        ContentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar)
            {
                Alias = "mandatoryProperty", Name = "Mandatory Property", Mandatory = true
            });
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        result = await ContentPublishingService.PublishAsync(Textpage.Key, [new CulturePublishScheduleModel()], Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.ContentInvalid));

        var invalidPropertyAliases = result.Result.InvalidPropertyAliases.ToArray();
        Assert.That(invalidPropertyAliases, Has.Length.EqualTo(1));
        Assert.That(invalidPropertyAliases, Does.Contain("mandatoryProperty"));

        // despite the failure to publish, the page should remain published
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Cannot_Republish_Branch_After_Adding_Mandatory_Property()
    {
        var result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);
        Assert.That(result.Success, Is.True);
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
        await ContentTypeService.UpdateAsync(ContentType, Constants.Security.SuperUserKey);

        // force an update on the root page so it is valid (and also subject to branch republishing).
        // if we didn't do this, the children would never be considered for branch publishing, as the publish logic
        // stops at the first invalid parent.
        // as an added bonus, this lets us test a partially successful branch publish :)
        var textPage = ContentService.GetById(Textpage.Key)!;
        textPage.SetValue("mandatoryProperty", "This is a valid value");
        ContentService.Save(textPage);

        result = await ContentPublishingService.PublishBranchAsync(Textpage.Key, _allCultures, PublishBranchFilter.IncludeUnpublished, Constants.Security.SuperUserKey, false);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.FailedBranch));
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

        var result = await ContentPublishingService.PublishAsync(
            content.Key,
            [new CulturePublishScheduleModel { Culture = cultureCode }],
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.InvalidCulture));
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

        var result = await ContentPublishingService.PublishAsync(
            content.Key,
            [new CulturePublishScheduleModel { Culture = cultureCode }],
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.InvalidCulture));
    }

    [Test]
    public async Task Can_Publish_Invariant_Content_With_Cultures_Provided_If_The_Default_Culture_Is_Exclusively_Provided()
    {
        var result = await ContentPublishingService.PublishAsync(
            Textpage.Key,
            [new CulturePublishScheduleModel { Culture = "en-US" }],
            Constants.Security.SuperUserKey);
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task Can_Publish_Invariant_Content_With_Cultures_Provided_If_The_Default_Culture_Is_Provided_With_Other_Cultures()
    {
        var result = await ContentPublishingService.PublishAsync(
            Textpage.Key,
            [
                new CulturePublishScheduleModel { Culture = "en-US" },
                new CulturePublishScheduleModel { Culture = "da-DK" }
            ],
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.True);
    }

    [Test]
    public async Task Cannot_Publish_Invariant_Content_With_Cultures_Provided_That_Do_Not_Include_The_Default_Culture()
    {
        var result = await ContentPublishingService.PublishAsync(
            Textpage.Key,
            [new CulturePublishScheduleModel { Culture = "da-DK" }],
            Constants.Security.SuperUserKey);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(ContentPublishingOperationStatus.InvalidCulture));
    }

    private void AssertBranchResultSuccess(ContentPublishingBranchResult result, params Guid[] expectedKeys)
    {
        var items = result.SucceededItems.ToArray();
        Assert.That(items, Has.Length.EqualTo(expectedKeys.Length));
        foreach (var key in expectedKeys)
        {
            var item = items.FirstOrDefault(i => i.Key == key);
            Assert.That(item, Is.Not.Null);
            Assert.That(item.OperationStatus, Is.EqualTo(ContentPublishingOperationStatus.Success));
        }
    }

    private void AssertBranchResultFailed(ContentPublishingBranchResult result, params (Guid, ContentPublishingOperationStatus)[] expectedFailures)
    {
        var items = result.FailedItems.ToArray();
        Assert.That(items, Has.Length.EqualTo(expectedFailures.Length));
        foreach (var (key, status) in expectedFailures)
        {
            var item = items.FirstOrDefault(i => i.Key == key);
            Assert.That(item, Is.Not.Null);
            Assert.That(item.OperationStatus, Is.EqualTo(status));
        }
    }
}
