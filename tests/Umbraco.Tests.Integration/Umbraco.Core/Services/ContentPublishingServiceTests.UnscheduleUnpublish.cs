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
    public async Task Can_UnSchedule_Unpublish_Invariant()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var setupData = await CreateInvariantContentAsync(doctype);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishInvariantAsync(setupData);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var unscheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(unscheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsNull(content!.PublishDate);
            Assert.IsFalse(schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Expire).Any());
            Assert.IsTrue(schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Release).Any());
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Unschedule_Unpublish_Single_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishForAllCulturesAsync(setupData, setupInfo);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.AreEqual(5, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Unschedule_Unpublish_Some_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishForAllCulturesAsync(setupData, setupInfo);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate }
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate }
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.AreEqual(4, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Unschedule_Unpublish_All_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishForAllCulturesAsync(setupData, setupInfo);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangBe.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangBe.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.AreEqual(3, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_NOT_Unschedule_Unpublish_Unknown_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishForAllCulturesAsync(setupData, setupInfo);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = UnknownCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsFalse(scheduleAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, scheduleAttempt.Status);
        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(6, schedules.FullSchedule.Count);
        });
    }
}
