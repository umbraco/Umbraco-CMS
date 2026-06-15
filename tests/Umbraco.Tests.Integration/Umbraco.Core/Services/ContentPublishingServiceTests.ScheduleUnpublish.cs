using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ContentPublishingServiceTests : UmbracoIntegrationTestWithContent
{
    [Test]
    public async Task Can_Schedule_Unpublish_Invariant()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(content!.PublishDate, Is.Null);
            Assert.That(
                schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Expire).Single().Date, Is.EqualTo(_scheduleUnPublishDate));
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Can_Schedule_Unpublish_Single_Culture()
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
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(
                schedules.GetSchedule(langEn.IsoCode, ContentScheduleAction.Expire).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Can_Schedule_Unpublish_Some_Cultures()
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
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = langDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(
                schedules.GetSchedule(langEn.IsoCode, ContentScheduleAction.Expire).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(
                schedules.GetSchedule(langDa.IsoCode, ContentScheduleAction.Expire).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task Can_Schedule_Unpublish_All_Cultures()
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
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = langDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = langBe.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(
                schedules.GetSchedule(langEn.IsoCode, ContentScheduleAction.Expire).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(
                schedules.GetSchedule(langDa.IsoCode, ContentScheduleAction.Expire).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(
                schedules.GetSchedule(langBe.IsoCode, ContentScheduleAction.Expire).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(3));
        });
    }

    [Test]
    public async Task Cannot_Schedule_Unpublish_Unknown_Culture()
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
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = langDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = UnknownCulture,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate }
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.False);
        Assert.That(scheduleAttempt.Status, Is.EqualTo(ContentPublishingOperationStatus.InvalidCulture));

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(schedules.FullSchedule, Is.Empty);
        });
    }
}
