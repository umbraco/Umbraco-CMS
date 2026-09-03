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
        var content = await CreateInvariantContentAsync(doctype);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishInvariantAsync(content);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var clearScheduleAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel(),
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(clearScheduleAttempt.Success, Is.True);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(content!.PublishDate, Is.Null);
            Assert.That(schedules.FullSchedule, Is.Empty);
        });
    }

    [Test]
    public async Task Can_Clear_Schedule_Single_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishForAllCulturesAsync(content, setupInfo);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
            [
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel(),
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Any(), Is.False);
            Assert.That(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Any(), Is.False);
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(4));
        });
    }

    [Test]
    public async Task Can_Clear_Schedule_Some_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishForAllCulturesAsync(content, setupInfo);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
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

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Any(), Is.False);
            Assert.That(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Any(), Is.False);
            Assert.That(schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Release).Any(), Is.False);
            Assert.That(schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Expire).Any(), Is.False);
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task Can_Clear_Schedule_All_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleSetupAttempt =
            await SchedulePublishAndUnPublishForAllCulturesAsync(content, setupInfo);

        if (scheduleSetupAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            content.Key,
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

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ContentService.GetContentScheduleByContentId(content.Id);
        content = ContentService.GetById(content.Key);

        Assert.Multiple(() =>
        {
            Assert.That(content!.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(schedules.FullSchedule, Is.Empty);
        });
    }
}
