using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ContentPublishingServiceTests : UmbracoIntegrationTestWithContent
{
    [Test]
    public async Task Can_Clear_Schedule_Invariant()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var setupData = await CreateInvariantContentAsync(doctype);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishInvariantAsync(setupData);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var clearScheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel(),
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(clearScheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsNull(content!.PublishDate);
            Assert.AreEqual(0, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Clear_Schedule_Single_Culture()
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
                    Schedule = new ContentScheduleModel(),
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.AreEqual(4, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Clear_Schedule_Some_Cultures()
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
                    Schedule = new ContentScheduleModel(),
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel(),
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Release).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Expire).Any());
            Assert.AreEqual(2, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Clear_Schedule_All_Cultures()
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
                    Schedule = new ContentScheduleModel(),
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel(),
                },
                new()
                {
                    Culture = setupInfo.LangBe.IsoCode,
                    Schedule = new ContentScheduleModel(),
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(0, schedules.FullSchedule.Count);
        });
    }
}
