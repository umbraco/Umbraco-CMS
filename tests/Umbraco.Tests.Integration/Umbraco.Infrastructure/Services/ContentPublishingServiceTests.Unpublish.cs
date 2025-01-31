using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentPublishingServiceTests
{
    [Test]
    public async Task Can_Unpublish_Root()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        VerifyIsPublished(Textpage.Key);

        var result = await ContentPublishingService.UnpublishAsync(Textpage.Key, null, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsNotPublished(Textpage.Key);
    }

    [Test]
    public async Task Can_Unpublish_Child()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        VerifyIsPublished(Textpage.Key);
        VerifyIsPublished(Subpage.Key);

        var result = await ContentPublishingService.UnpublishAsync(Subpage.Key, null, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsPublished(Textpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Unpublish_Structure()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        await ContentPublishingService.PublishAsync(Subpage2.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        await ContentPublishingService.PublishAsync(Subpage3.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        VerifyIsPublished(Textpage.Key);
        VerifyIsPublished(Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(Subpage3.Key);

        var result = await ContentPublishingService.UnpublishAsync(Textpage.Key, null, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsNotPublished(Textpage.Key);
        // the sub pages are still published...
        VerifyIsPublished(Subpage.Key);
        VerifyIsPublished(Subpage2.Key);
        VerifyIsPublished(Subpage3.Key);
        // ... but should no longer be routable because the parent is unpublished
        Assert.IsFalse(ContentService.IsPathPublished(Subpage));
        Assert.IsFalse(ContentService.IsPathPublished(Subpage2));
        Assert.IsFalse(ContentService.IsPathPublished(Subpage3));
    }

    [Test]
    public async Task Can_Unpublish_Unpublished_Content()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.UnpublishAsync(Textpage.Key, null, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsNotPublished(Textpage.Key);
    }

    [Test]
    public async Task Can_Cancel_Unpublishing_With_Notification()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);
        VerifyIsPublished(Textpage.Key);

        ContentNotificationHandler.UnpublishingContent = notification => notification.Cancel = true;

        var result = await ContentPublishingService.UnpublishAsync(Textpage.Key, null, Constants.Security.SuperUserKey);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CancelledByEvent, result.Result);
        VerifyIsPublished(Textpage.Key);
    }

    [Test]
    public async Task Can_Unpublish_Single_Culture()
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
        await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        VerifyIsPublished(content.Key);

        var result = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string> { langEn.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
        Assert.IsTrue(content.PublishedCultures.InvariantContains(langDa.IsoCode));
    }

    [Test]
    public async Task Can_Unpublish_All_Cultures()
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
        await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        VerifyIsPublished(content.Key);

        var result = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string>(){"*"}, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsNotPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Unpublish_All_Cultures_With_Multiple_Cultures_Method()
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
        await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        VerifyIsPublished(content.Key);

        var result = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsNotPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Unpublish_Multiple_Cultures()
    {
        var (langEn, langDa, contentType) = await SetupVariantTest();

        var langSe = new LanguageBuilder()
            .WithCultureInfo("sv-SE")
            .Build();
        await LanguageService.CreateAsync(langSe, Constants.Security.SuperUserKey);

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .WithCultureName(langSe.IsoCode, "SE root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        content.SetValue("title", "SE title", culture: langSe.IsoCode);
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode, langSe.IsoCode }), Constants.Security.SuperUserKey);
        VerifyIsPublished(content.Key);
        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(3, content.PublishedCultures.Count());

        var result = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string> { langDa.IsoCode, langSe.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsPublished(content.Key);
        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Unpublish_All_Cultures_By_Unpublishing_Mandatory_Culture()
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
        await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        VerifyIsPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());

        var result = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string> { langEn.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsNotPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(0, content.PublishedCultures.Count());
    }



    [Test]
    public async Task Can_Unpublish_Non_Mandatory_Cultures()
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
        await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        VerifyIsPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());

        var result = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string>() { langDa.IsoCode }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(1, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Unpublish_From_Trash()
    {
        ContentService.MoveToRecycleBin(Subpage);
        Assert.IsTrue(ContentService.GetById(Subpage.Key)!.Trashed);

        var result = await ContentPublishingService.UnpublishAsync(Subpage.Key, null, Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Result);
        VerifyIsNotPublished(Subpage.Key);
    }

    [TestCase("en-us")]
    [TestCase("da-dk")]
    public async Task Cannot_Unpublish_Incorrect_Culture_Code(string cultureCode)
    {
        var (langEn, langDa, contentType) = await SetupVariantTest(false);

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        VerifyIsPublished(content.Key);

        var result = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string>() { cultureCode }, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, result.Result);
        VerifyIsPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
    }

    [TestCase("de-DE")]
    [TestCase("es-ES")]
    public async Task Cannot_Unpublish_Non_Existing_Culture(string cultureCode)
    {
        var (langEn, langDa, contentType) = await SetupVariantTest(false);

        IContent content = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName(langEn.IsoCode, "EN root")
            .WithCultureName(langDa.IsoCode, "DA root")
            .Build();
        content.SetValue("title", "EN title", culture: langEn.IsoCode);
        content.SetValue("title", "DA title", culture: langDa.IsoCode);
        ContentService.Save(content);
        await ContentPublishingService.PublishAsync(content.Key, MakeModel(new HashSet<string>() { langEn.IsoCode, langDa.IsoCode }), Constants.Security.SuperUserKey);
        VerifyIsPublished(content.Key);

        var result = await ContentPublishingService.UnpublishAsync(content.Key, new HashSet<string>() { cultureCode }, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, result.Result);
        VerifyIsPublished(content.Key);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(2, content.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Unpublish_Invariant_Content_With_Cultures_Provided_If_The_Default_Culture_Is_Exclusively_Provided()
    {
        var result = await ContentPublishingService.UnpublishAsync(Textpage.Key, new HashSet<string>() { "en-US" }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task Can_Unpublish_Invariant_Content_With_Cultures_Provided_If_The_Default_Culture_Is_Provided_With_Other_Cultures()
    {
        var result = await ContentPublishingService.UnpublishAsync(Textpage.Key, new HashSet<string>() { "en-US", "da-DK" }, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public async Task Cannot_Unpublish_Invariant_Content_With_Cultures_Provided_That_Do_Not_Include_The_Default_Culture()
    {
        var result = await ContentPublishingService.UnpublishAsync(Textpage.Key, new HashSet<string>() { "da-DK" }, Constants.Security.SuperUserKey);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CannotPublishVariantWhenNotVariant, result.Result);
    }
}
