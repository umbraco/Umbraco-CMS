using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ContentPublishingServiceTests : UmbracoIntegrationTestWithContent
{
    private IUserService UserService => GetRequiredService<IUserService>();

    private IAuditService AuditService => GetRequiredService<IAuditService>();

    private DateTime ScheduledActionsDue => _scheduleUnPublishDate.AddMinutes(1);

    [Test]
    public async Task Scheduled_Publish_Is_Performed_On_Behalf_Of_The_User_Who_Scheduled_It()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);
        var scheduler = CreateUser("Scheduler");
        var writer = CreateUser("Writer");

        await ScheduleAsync(content.Key, Constants.System.InvariantCulture, scheduler, publishDate: _schedulePublishDate);
        SaveAs(content.Key, writer);

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        Assert.AreEqual(scheduler.Id, (await GetAuditItemAsync(content.Id, AuditType.Publish)).UserId);
    }

    [Test]
    public async Task Scheduled_Unpublish_Is_Performed_On_Behalf_Of_The_User_Who_Scheduled_It()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);
        ContentService.Publish(content, ["*"]);
        var scheduler = CreateUser("Scheduler");
        var writer = CreateUser("Writer");

        await ScheduleAsync(content.Key, Constants.System.InvariantCulture, scheduler, unpublishDate: _scheduleUnPublishDate);
        SaveAs(content.Key, writer);

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        Assert.AreEqual(scheduler.Id, (await GetAuditItemAsync(content.Id, AuditType.Unpublish)).UserId);
    }

    [Test]
    public async Task Scheduled_Publish_Of_Cultures_Is_Performed_On_Behalf_Of_The_User_Who_Scheduled_Them()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);
        var scheduler = CreateUser("Scheduler");
        var writer = CreateUser("Writer");

        await ScheduleAsync(content.Key, langEn.IsoCode, scheduler, publishDate: _schedulePublishDate);
        await ScheduleAsync(content.Key, langDa.IsoCode, scheduler, publishDate: _schedulePublishDate);
        SaveAs(content.Key, writer);

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        content = ContentService.GetById(content.Key)!;
        IAuditItem publish = await GetAuditItemAsync(content.Id, AuditType.PublishVariant);
        Assert.Multiple(() =>
        {
            CollectionAssert.AreEquivalent(new[] { langEn.IsoCode, langDa.IsoCode }, content.PublishedCultures);
            Assert.AreEqual(scheduler.Id, publish.UserId);
        });
    }

    [Test]
    public async Task Scheduled_Unpublish_Of_Cultures_Is_Performed_On_Behalf_Of_The_User_Who_Scheduled_Them()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);
        ContentService.Publish(content, ["*"]);
        var scheduler = CreateUser("Scheduler");
        var writer = CreateUser("Writer");

        await ScheduleAsync(content.Key, langDa.IsoCode, scheduler, unpublishDate: _scheduleUnPublishDate);
        await ScheduleAsync(content.Key, langBe.IsoCode, scheduler, unpublishDate: _scheduleUnPublishDate);
        SaveAs(content.Key, writer);

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        content = ContentService.GetById(content.Key)!;
        IAuditItem unpublish = await GetAuditItemAsync(content.Id, AuditType.UnpublishVariant);
        Assert.Multiple(() =>
        {
            CollectionAssert.AreEquivalent(new[] { langEn.IsoCode }, content.PublishedCultures);
            Assert.AreEqual(scheduler.Id, unpublish.UserId);
        });
    }

    [Test]
    public async Task Scheduled_Publish_Of_Cultures_Scheduled_By_Different_Users_Is_Performed_On_Behalf_Of_The_Last_Writer()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);
        var englishScheduler = CreateUser("EnglishScheduler");
        var danishScheduler = CreateUser("DanishScheduler");
        var writer = CreateUser("Writer");

        await ScheduleAsync(content.Key, langEn.IsoCode, englishScheduler, publishDate: _schedulePublishDate);
        await ScheduleAsync(content.Key, langDa.IsoCode, danishScheduler, publishDate: _schedulePublishDate);
        SaveAs(content.Key, writer);

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        content = ContentService.GetById(content.Key)!;
        IAuditItem publish = await GetAuditItemAsync(content.Id, AuditType.PublishVariant);
        Assert.Multiple(() =>
        {
            CollectionAssert.AreEquivalent(new[] { langEn.IsoCode, langDa.IsoCode }, content.PublishedCultures);
            Assert.AreEqual(writer.Id, publish.UserId);
        });
    }

    [Test]
    public async Task Rescheduling_For_The_Same_Date_Keeps_The_User_Who_Scheduled_It()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);
        var scheduler = CreateUser("Scheduler");
        var otherUser = CreateUser("OtherUser");

        await ScheduleAsync(content.Key, Constants.System.InvariantCulture, scheduler, publishDate: _schedulePublishDate);
        await ScheduleAsync(content.Key, Constants.System.InvariantCulture, otherUser, publishDate: _schedulePublishDate);

        ContentSchedule schedule = ContentService.GetContentScheduleByContentId(content.Id).GetSchedule(ContentScheduleAction.Release).Single();
        Assert.AreEqual(scheduler.Id, schedule.UserId);
    }

    [Test]
    public async Task Rescheduling_For_Another_Date_Is_Attributed_To_The_User_Rescheduling_It()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);
        var scheduler = CreateUser("Scheduler");
        var otherUser = CreateUser("OtherUser");

        await ScheduleAsync(content.Key, Constants.System.InvariantCulture, scheduler, publishDate: _schedulePublishDate);
        await ScheduleAsync(content.Key, Constants.System.InvariantCulture, otherUser, publishDate: _schedulePublishDate.AddHours(1));

        ContentSchedule schedule = ContentService.GetContentScheduleByContentId(content.Id).GetSchedule(ContentScheduleAction.Release).Single();
        Assert.AreEqual(otherUser.Id, schedule.UserId);
    }

    [Test]
    public async Task Schedule_Saved_With_Content_Is_Performed_On_Behalf_Of_The_User_Saving_It()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);
        var scheduler = CreateUser("Scheduler");
        var writer = CreateUser("Writer");

        ContentService.Save(ContentService.GetById(content.Key)!, scheduler.Id, ContentScheduleCollection.CreateWithEntry(_schedulePublishDate, null));
        SaveAs(content.Key, writer);

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        Assert.AreEqual(scheduler.Id, (await GetAuditItemAsync(content.Id, AuditType.Publish)).UserId);
    }

    [Test]
    public async Task Schedule_Not_Attributed_To_A_User_Is_Performed_On_Behalf_Of_The_Last_Writer()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);
        var writer = CreateUser("Writer");

        ContentService.PersistContentSchedule(content, ContentScheduleCollection.CreateWithEntry(_schedulePublishDate, null));
        SaveAs(content.Key, writer);

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        Assert.AreEqual(writer.Id, (await GetAuditItemAsync(content.Id, AuditType.Publish)).UserId);
    }

    [Test]
    public async Task Deleting_The_User_Who_Scheduled_An_Action_Keeps_The_Schedule()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var content = await CreateInvariantContentAsync(doctype);
        var scheduler = CreateUser("Scheduler");
        var writer = CreateUser("Writer");

        await ScheduleAsync(content.Key, Constants.System.InvariantCulture, scheduler, publishDate: _schedulePublishDate);
        SaveAs(content.Key, writer);
        UserService.Delete(scheduler, true);

        ContentSchedule schedule = ContentService.GetContentScheduleByContentId(content.Id).GetSchedule(ContentScheduleAction.Release).Single();
        Assert.IsNull(schedule.UserId);

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        Assert.AreEqual(writer.Id, (await GetAuditItemAsync(content.Id, AuditType.Publish)).UserId);
    }

    [Test]
    public async Task Scheduled_Publish_Of_Cultures_By_Different_Users_Publishes_Them_With_The_Mandatory_Culture()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        langEn.IsMandatory = true;
        Assert.IsTrue((await LanguageService.UpdateAsync(langEn, Constants.Security.SuperUserKey)).Success);
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);
        var danishScheduler = CreateUser("DanishScheduler");
        var englishScheduler = CreateUser("EnglishScheduler");

        await ScheduleAsync(content.Key, langDa.IsoCode, danishScheduler, publishDate: _schedulePublishDate);
        await ScheduleAsync(content.Key, langEn.IsoCode, englishScheduler, publishDate: _schedulePublishDate.AddHours(1));

        ContentService.PerformScheduledPublish(ScheduledActionsDue);

        content = ContentService.GetById(content.Key)!;
        CollectionAssert.AreEquivalent(new[] { langEn.IsoCode, langDa.IsoCode }, content.PublishedCultures);
    }

    [Test]
    public async Task Scheduled_Unpublish_Of_Cultures_By_Different_Users_Including_The_Mandatory_Culture_Unpublishes_The_Document()
    {
        var (langEn, langDa, langBe, contentType) = await SetupVariantDoctypeAsync();
        langEn.IsMandatory = true;
        Assert.IsTrue((await LanguageService.UpdateAsync(langEn, Constants.Security.SuperUserKey)).Success);
        var content = await CreateVariantContentAsync(langEn, langDa, langBe, contentType);
        ContentService.Publish(content, ["*"]);
        var englishScheduler = CreateUser("EnglishScheduler");
        var danishScheduler = CreateUser("DanishScheduler");

        await ScheduleAsync(content.Key, langEn.IsoCode, englishScheduler, unpublishDate: _scheduleUnPublishDate.AddHours(-1));
        await ScheduleAsync(content.Key, langDa.IsoCode, danishScheduler, unpublishDate: _scheduleUnPublishDate);

        PublishResultType[] results = ContentService.PerformScheduledPublish(ScheduledActionsDue)
            .Where(x => x.Content.Key == content.Key)
            .Select(x => x.Result)
            .ToArray();

        content = ContentService.GetById(content.Key)!;
        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new[] { PublishResultType.SuccessUnpublishMandatoryCulture }, results);
            Assert.IsFalse(content.Published);
        });
    }

    private IUser CreateUser(string suffix)
    {
        IUser user = new UserBuilder().WithSuffix(suffix).Build();
        UserService.Save(user);
        return user;
    }

    private async Task ScheduleAsync(Guid contentKey, string culture, IUser user, DateTime? publishDate = null, DateTime? unpublishDate = null)
    {
        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            contentKey,
            [
                new()
                {
                    Culture = culture,
                    Schedule = new ContentScheduleModel { PublishDate = publishDate, UnpublishDate = unpublishDate },
                },
            ],
            user.Key);
        Assert.IsTrue(scheduleAttempt.Success);
    }

    private void SaveAs(Guid contentKey, IUser user)
        => Assert.IsTrue(ContentService.Save(ContentService.GetById(contentKey)!, user.Id).Success);

    private async Task<IAuditItem[]> GetAuditItemsAsync(int contentId, AuditType auditType)
        => (await AuditService.GetItemsByEntityAsync(contentId, 0, int.MaxValue)).Items
            .Where(x => x.AuditType == auditType)
            .ToArray();

    private async Task<IAuditItem> GetAuditItemAsync(int contentId, AuditType auditType)
        => (await GetAuditItemsAsync(contentId, auditType)).Single();

}
