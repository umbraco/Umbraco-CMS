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
        var setupData = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        var content = ContentService.GetById(setupData.Key);
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
                new() { Culture = langEn.IsoCode }, new() { Culture = langDa.IsoCode },
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
}
