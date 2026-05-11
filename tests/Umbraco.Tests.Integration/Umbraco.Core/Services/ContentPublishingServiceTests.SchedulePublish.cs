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
    public async Task Can_Schedule_Publish_Invariant()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsNull(content!.PublishDate);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Schedule_Publish_Single_Culture()
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
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(langEn.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Schedule_Publish_Some_Cultures()
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
                },
                new()
                {
                    Culture = langDa.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(langEn.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(langDa.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(2, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Schedule_Publish_All_Cultures()
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
                },
                new()
                {
                    Culture = langDa.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = langBe.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(langEn.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(langDa.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(langBe.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(3, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Cannot_Schedule_Publish_Unknown_Culture()
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
                },
                new()
                {
                    Culture = langDa.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = UnknownCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate }
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(scheduleAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, scheduleAttempt.Status);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(0, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Publish_And_Schedule_Different_Cultures()
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
                },
                new()
                {
                    Culture = langDa.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, content!.PublishedCultures.Count());
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }
}
