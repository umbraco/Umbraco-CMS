using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
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

        Assert.That(publishAttempt.Success, Is.True);
        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(1));
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

        Assert.That(publishAttempt.Success, Is.True);
        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(2));
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

        Assert.That(publishAttempt.Success, Is.True);
        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(3));
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

        Assert.That(publishAttempt.Success, Is.False);
        Assert.That(publishAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.CannotPublishInvariantWhenVariant));

        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
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

        Assert.That(publishAttempt.Success, Is.True);

        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishDate, Is.Not.Null);
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

        Assert.That(publishAttempt.Success, Is.False);
        Assert.That(publishAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.ContentInvalid));

        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishDate, Is.Null);
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

        Assert.That(publishAttempt.Success, Is.False);
        Assert.That(publishAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.ContentInvalid));
        Assert.That(string.Join(",", publishAttempt.Result.InvalidPropertyAliases), Is.EqualTo("title"));

        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0)); // Even though the Danish culture was valid, we still don't publish if if any are invalid.
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

        Assert.That(publishAttempt.Success, Is.True);
        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(1));
        Assert.That(content!.PublishedCultures.First(), Is.EqualTo(langDa.IsoCode));
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

        Assert.That(publishAttempt.Success, Is.False);
        Assert.That(publishAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.InvalidCulture));

        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
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

        Assert.That(publishAttempt.Success, Is.False);
        Assert.That(publishAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.CultureAwaitingRelease));

        content = ContentService.GetById(content.Key);
        Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
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

        Assert.That(publishAttempt.Success, Is.False);
        Assert.That(publishAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.PathNotPublished));

        // Now publish the parent and re-try publishing the child.
        publishAttempt = await ContentPublishingService.PublishAsync(
            parentContent.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.That(publishAttempt.Success, Is.True);

        publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.That(publishAttempt.Success, Is.True);
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
        Assert.That(publishAttempt.Success, Is.False);
        Assert.That(publishAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.PathNotPublished));

        // Now publish the parent and re-try publishing the child.
        publishAttempt = await ContentPublishingService.PublishAsync(
            parentContent.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.That(publishAttempt.Success, Is.True);

        publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.That(publishAttempt.Success, Is.True);
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
        Assert.That(publishAttempt.Success, Is.True);

        // Publish child in English, should succeed.
        publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.That(publishAttempt.Success, Is.True);

        // Publish child in Danish, should also succeed.
        publishAttempt = await ContentPublishingService.PublishAsync(
            childContent.Key,
            [new() { Culture = langDa.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.That(publishAttempt.Success, Is.True);
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
        Assert.That(publishAttempt.Success, Is.True);

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
        Assert.That(publishAttempt.Success, Is.True);

        content = ContentService.GetById(content.Key)!;
        Assert.That(content.GetPublishDate(langDa.IsoCode), Is.EqualTo(firstPublishDateDa));
        Assert.That(content.GetPublishDate(langBe.IsoCode), Is.EqualTo(firstPublishDateBe));

        var lastPublishDateEn = content.GetPublishDate(langEn.IsoCode)
                                ?? throw new InvalidOperationException("Expected a publish date for EN");
        Assert.That(lastPublishDateEn, Is.GreaterThan(firstPublishDateEn));
    }

    [Test]
    public async Task Publishing_Single_Culture_Persists_Expected_Property_Data()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);

        using (var scope = ScopeProvider.CreateScope())
        {
            var dtos = scope.Database.Fetch<PropertyDataDto>();
            Assert.That(dtos, Has.Count.EqualTo(18));  // 3 properties * 3 cultures * 2 (published + edited).
            scope.Complete();
        }

        var publishAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [new() { Culture = langEn.IsoCode }],
            Constants.Security.SuperUserKey);
        Assert.That(publishAttempt.Success, Is.True);

        using (var scope = ScopeProvider.CreateScope())
        {
            var dtos = scope.Database.Fetch<PropertyDataDto>();
            Assert.That(dtos, Has.Count.EqualTo(19)); // + 3 for published populated title property, - 2 for existing published properties of other cultures.
            scope.Complete();
        }
    }
}
