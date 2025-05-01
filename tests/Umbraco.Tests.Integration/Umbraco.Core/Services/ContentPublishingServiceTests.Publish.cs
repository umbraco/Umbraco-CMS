using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ContentPublishingServiceTests : UmbracoIntegrationTestWithContent
{
    [Test]
    public async Task Can_Publish_Single_Culture()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        content = ContentService.GetById(content.Key);
        Assert.AreEqual(1, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Publish_Some_Cultures()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        content = ContentService.GetById(content.Key);
        Assert.AreEqual(2, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Publish_All_Cultures()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
                new() { Culture = langBe.IsoCode },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        content = ContentService.GetById(content.Key);
        Assert.AreEqual(3, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Invariant_In_Variant_Setup()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CannotPublishInvariantWhenVariant, publishAttempt.Status);

        content = ContentService.GetById(content.Key);
        Assert.AreEqual(0, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Publish_Invariant_In_Invariant_Setup()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);

        content = ContentService.GetById(content.Key);
        Assert.NotNull(content!.PublishDate);
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Content_In_Invariant_Setup()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype, titleValue: null);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, publishAttempt.Status);

        content = ContentService.GetById(content.Key);
        Assert.Null(content!.PublishDate);
    }

    [Test]
    public async Task Cannot_Publish_Invalid_Content_In_Variant_Setup()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType,
            englishTitleValue: null);   // English is invalid, Danish is valid.

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.ContentInvalid, publishAttempt.Status);
        Assert.AreEqual("title", string.Join(",", publishAttempt.Result.InvalidPropertyAliases));

        content = ContentService.GetById(content.Key);
        Assert.AreEqual(0, content!.PublishedCultures.Count()); // Even though the Danish culture was valid, we still don't publish if if any are invalid.
    }

    [Test]
    public async Task Can_Publish_Valid_Content_In_One_Culture_When_Another_Is_Invalid_In_Variant_Setup()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType,
            englishTitleValue: null);   // English is invalid, Danish is valid.

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new() { Culture = langDa.IsoCode },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        content = ContentService.GetById(content.Key);
        Assert.AreEqual(1, content!.PublishedCultures.Count());
        Assert.AreEqual(langDa.IsoCode, content!.PublishedCultures.First());
    }

    [Test]
    public async Task Cannot_Publish_Unknown_Culture()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
                new() { Culture = UnknownCulture },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, publishAttempt.Status);

        content = ContentService.GetById(content.Key);
        Assert.AreEqual(0, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Cannot_Publish_Scheduled_Culture()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new()
                {
                    Culture = langEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                }
            ],
            Constants.Security.SuperUserKey);

        if (scheduleAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CultureAwaitingRelease, publishAttempt.Status);

        content = ContentService.GetById(content.Key);
        Assert.AreEqual(0, content!.PublishedCultures.Count());
    }

    // TODO: The following three tests verify existing functionality that could be reconsidered.
    // The existing behaviour, verified on Umbraco 13 and 15 is as follows:
    //  - For invariant content, if a parent is unpublished and I try to publish the child, I get a ContentPublishingOperationStatus.PathNotPublished error.
    //  - For variant content, if I publish the parent in English but not Danish, I can publish the child in Danish.
    // This is inconsistent so we should consider if this is the desired behaviour.
    // For now though, the following tests verify the existing behaviour.

    [Test]
    public async Task Cannot_Publish_With_Unpublished_Parent()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var parentContent = await CreateInvariantContentAsync(doctype);
        var childContent = await CreateInvariantContentAsync(doctype, parentContent.Key);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.PathNotPublished, publishAttempt.Status);

        // Now publish the parent and re-try publishing the child.
        publishAttempt = await ContentPublishingService.PublishAsync(
            parentContent.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);
    }

    [Test]
    public async Task Cannot_Publish_Culture_With_Unpublished_Parent()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var parentContent = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType);
        var childContent = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType,
            parentContent.Key);

        // Publish child in English, should not succeed.
        var publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.PathNotPublished, publishAttempt.Status);

        // Now publish the parent and re-try publishing the child.
        publishAttempt = await ContentPublishingService.PublishAsync(
            parentContent.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);
    }

    [Test]
    public async Task Can_Publish_Culture_With_Unpublished_Parent_Culture()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var parentContent = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType);
        var childContent = await CreateVariantContentAsync(
            langEn,
            langDa,
            langBe,
            contentType,
            parentContent.Key);

        // Publish parent in English.
        var publishAttempt = await ContentPublishingService.PublishAsync(
            parentContent.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        // Publish child in English, should succeed.
        publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        // Publish child in Danish, should also succeed.
        publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = langDa.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);
    }

    [Test]
    public async Task Republishing_Single_Culture_Does_Not_Change_Publish_Or_Update_Date_For_Other_Cultures()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new() { Culture = langEn.IsoCode },
                new() { Culture = langDa.IsoCode },
                new() { Culture = langBe.IsoCode },
            ],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        var content = ContentService.GetById(setupData.Key)!;
        var firstPublishDateEn = content.GetPublishDate(langEn.IsoCode)
                                 ?? throw new InvalidOperationException("Expected a publish date for EN");
        var firstPublishDateDa = content.GetPublishDate(langDa.IsoCode)
                                 ?? throw new InvalidOperationException("Expected a publish date for DA");
        var firstPublishDateBe = content.GetPublishDate(langBe.IsoCode)
                                 ?? throw new InvalidOperationException("Expected a publish date for BE");

        Thread.Sleep(100);

        publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success);

        content = ContentService.GetById(content.Key)!;
        Assert.AreEqual(firstPublishDateDa, content.GetPublishDate(langDa.IsoCode));
        Assert.AreEqual(firstPublishDateBe, content.GetPublishDate(langBe.IsoCode));

        var lastPublishDateEn = content.GetPublishDate(langEn.IsoCode)
                                ?? throw new InvalidOperationException("Expected a publish date for EN");
        Assert.Greater(lastPublishDateEn, firstPublishDateEn);
    }
}
