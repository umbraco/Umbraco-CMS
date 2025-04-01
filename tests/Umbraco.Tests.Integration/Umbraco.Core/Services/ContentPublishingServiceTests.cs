using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    PublishedRepositoryEvents = true,
    WithApplication = true)]
public class ContentPublishingServiceTests : UmbracoIntegrationTestWithContent
{
    private const string UnknownCulture = "ke-Ke";

    private readonly DateTime _schedulePublishDate = DateTime.UtcNow.AddDays(1).TruncateTo(DateTimeExtensions.DateTruncate.Second);
    private readonly DateTime _scheduleUnPublishDate = DateTime.UtcNow.AddDays(2).TruncateTo(DateTimeExtensions.DateTruncate.Second);

    [SetUp]
    public new void Setup() => ContentRepositoryBase.ThrowOnWarning = true;

    [TearDown]
    public void Teardown() => ContentRepositoryBase.ThrowOnWarning = false;

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    #region Publish

    [Test]
    public async Task Can_Publish_Single_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel> { new() { Culture = setupInfo.LangEn.IsoCode } },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        var content = ContentService.GetById(setupData.Key);
        Assert.AreEqual(1, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Publish_Some_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new() { Culture = setupInfo.LangEn.IsoCode }, new() { Culture = setupInfo.LangDa.IsoCode },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        var content = ContentService.GetById(setupData.Key);
        Assert.AreEqual(2, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Publish_All_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new() { Culture = setupInfo.LangEn.IsoCode },
                new() { Culture = setupInfo.LangDa.IsoCode },
                new() { Culture = setupInfo.LangBe.IsoCode },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);
        var content = ContentService.GetById(setupData.Key);
        Assert.AreEqual(3, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_NOT_Publish_Invariant_In_Variant_Setup()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel> { new() { Culture = Constants.System.InvariantCulture } },
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CannotPublishInvariantWhenVariant, publishAttempt.Status);

        var content = ContentService.GetById(setupData.Key);
        Assert.AreEqual(0, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_Publish_Invariant_In_Invariant_Setup()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var setupData = await CreateInvariantContentAsync(doctype);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel> { new() { Culture = Constants.System.InvariantCulture } },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(publishAttempt.Success);

        var content = ContentService.GetById(setupData.Key);
        Assert.NotNull(content!.PublishDate);
    }
    //todo more tests for invariant
    //todo update schedule date

    [Test]
    public async Task Can_NOT_Publish_Unknown_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new() { Culture = setupInfo.LangEn.IsoCode },
                new() { Culture = setupInfo.LangDa.IsoCode },
                new() { Culture = UnknownCulture },
            },
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, publishAttempt.Status);

        var content = ContentService.GetById(setupData.Key);
        Assert.AreEqual(0, content!.PublishedCultures.Count());
    }

    [Test]
    public async Task Can_NOT_Publish_Scheduled_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                }
            },
            Constants.Security.SuperUserKey);

        if (scheduleAttempt.Success is false)
        {
            throw new Exception("Setup failed");
        }

        var publishAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel> { new() { Culture = setupInfo.LangEn.IsoCode } },
            Constants.Security.SuperUserKey);

        Assert.IsFalse(publishAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.CultureAwaitingRelease, publishAttempt.Status);

        var content = ContentService.GetById(setupData.Key);
        Assert.AreEqual(0, content!.PublishedCultures.Count());
    }

    #endregion

    #region Schedule Publish

    [Test]
    public async Task Can_Schedule_Publish_Invariant()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var setupData = await CreateInvariantContentAsync(doctype);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

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
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Schedule_Publish_Some_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
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
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(2, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Schedule_Publish_All_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
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
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangBe.IsoCode, ContentScheduleAction.Release).Single().Date);
            Assert.AreEqual(3, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_NOT_Schedule_Publish_Unknown_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
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
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate }
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsFalse(scheduleAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, scheduleAttempt.Status);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(0, schedules.FullSchedule.Count);
        });
    }

    #endregion

    #region Schedule Unpublish

    [Test]
    public async Task Can_Schedule_Unpublish_Invariant()
    {
        var doctype = await SetupInvariantDoctypeAsync();
        var setupData = await CreateInvariantContentAsync(doctype);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsNull(content!.PublishDate);
            Assert.AreEqual(
                _scheduleUnPublishDate,
                schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Expire).Single().Date);
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Schedule_Unpublish_Single_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Single().Date);
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Schedule_Unpublish_Some_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Expire).Single().Date);
            Assert.AreEqual(2, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Schedule_Unpublish_All_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangBe.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Expire).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Expire).Single().Date);
            Assert.AreEqual(
                _schedulePublishDate,
                schedules.GetSchedule(setupInfo.LangBe.IsoCode, ContentScheduleAction.Expire).Single().Date);
            Assert.AreEqual(3, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_NOT_Schedule_Unpublish_Unknown_Culture()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate },
                },
                new()
                {
                    Culture = UnknownCulture,
                    Schedule = new ContentScheduleModel { UnpublishDate = _schedulePublishDate }
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsFalse(scheduleAttempt.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.InvalidCulture, scheduleAttempt.Status);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.AreEqual(0, schedules.FullSchedule.Count);
        });
    }

    #endregion

    #region Unschedule Publish

    [Test]
    public async Task Can_UnSchedule_Publish_Invariant()
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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(unscheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsNull(content!.PublishDate);
            Assert.IsFalse(schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Release).Any());
            Assert.IsTrue(schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Expire).Any());
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Unschedule_Publish_Single_Culture()
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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Any());
            Assert.AreEqual(5, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Unschedule_Publish_Some_Cultures()
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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate }
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate }
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Release).Any());
            Assert.AreEqual(4, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_Unschedule_Publish_All_Cultures()
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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangBe.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(0, content!.PublishedCultures.Count());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangEn.IsoCode, ContentScheduleAction.Release).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangDa.IsoCode, ContentScheduleAction.Release).Any());
            Assert.IsFalse(schedules.GetSchedule(setupInfo.LangBe.IsoCode, ContentScheduleAction.Release).Any());
            Assert.AreEqual(3, schedules.FullSchedule.Count);
        });
    }

    [Test]
    public async Task Can_NOT_Unschedule_Publish_Unknown_Culture()
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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
                new()
                {
                    Culture = UnknownCulture,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
            },
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

    #endregion

    #region Unschedule Unpublish

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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            },
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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            },
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
            new List<CulturePublishScheduleModel>
            {
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
            },
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
            new List<CulturePublishScheduleModel>
            {
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
            },
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
            new List<CulturePublishScheduleModel>
            {
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
            },
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

    #endregion

    #region Clean Schedule

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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel(),
                },
            },
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
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule = new ContentScheduleModel(),
                },
            },
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
            new List<CulturePublishScheduleModel>
            {
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
            },
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
            new List<CulturePublishScheduleModel>
            {
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
            },
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

    #endregion

    #region Combinations

    [Test]
    public async Task Can_Publish_And_Schedule_Different_Cultures()
    {
        var setupInfo = await SetupVariantDoctypeAsync();
        var setupData = await CreateVariantContentAsync(
            setupInfo.LangEn,
            setupInfo.LangDa,
            setupInfo.LangBe,
            setupInfo.contentType);

        var scheduleAttempt = await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            },
            Constants.Security.SuperUserKey);

        Assert.IsTrue(scheduleAttempt.Success);

        var schedules = ContentService.GetContentScheduleByContentId(setupData.Id);
        var content = ContentService.GetById(setupData.Key);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, content!.PublishedCultures.Count());
            Assert.AreEqual(1, schedules.FullSchedule.Count);
        });
    }
    #endregion

    #region Helper methods

    private async Task<(ILanguage LangEn, ILanguage LangDa, ILanguage LangBe, IContentType contentType)>
        SetupVariantDoctypeAsync()
    {
        var langEn = (await LanguageService.GetAsync("en-US"))!;
        var langDa = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDa, Constants.Security.SuperUserKey);
        var langBe = new LanguageBuilder()
            .WithCultureInfo("nl-BE")
            .Build();
        await LanguageService.CreateAsync(langBe, Constants.Security.SuperUserKey);

        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("variantContent")
            .WithName("Variant Content")
            .WithContentVariation(ContentVariation.Culture)
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .WithSupportsPublishing(true)
            .Done()
            .Build();

        contentType.AllowedAsRoot = true;
        var createAttempt = await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data structure");
        }

        return (langEn, langDa, langBe, contentType);
    }

    private async Task<IContent> CreateVariantContentAsync(ILanguage langEn, ILanguage langDa, ILanguage langBe,
        IContentType contentType)
    {
        var documentKey = Guid.NewGuid();

        var createModel = new ContentCreateModel
        {
            Key = documentKey,
            ContentTypeKey = contentType.Key,
            Variants = new[]
            {
                new VariantModel
                {
                    Name = langEn.CultureName,
                    Culture = langEn.IsoCode,
                    Properties = Enumerable.Empty<PropertyValueModel>(),
                },
                new VariantModel
                {
                    Name = langDa.CultureName,
                    Culture = langDa.IsoCode,
                    Properties = Enumerable.Empty<PropertyValueModel>(),
                },
                new VariantModel
                {
                    Name = langBe.CultureName,
                    Culture = langBe.IsoCode,
                    Properties = Enumerable.Empty<PropertyValueModel>(),
                }
            }
        };

        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data");
        }

        return createAttempt.Result.Content!;
    }

    private async Task<IContentType> SetupInvariantDoctypeAsync()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        var contentType = new ContentTypeBuilder()
            .WithAlias("invariantContent")
            .WithName("Invariant Content")
            .AddPropertyGroup()
            .WithAlias("content")
            .WithName("Content")
            .WithSupportsPublishing(true)
            .Done()
            .Build();

        contentType.AllowedAsRoot = true;
        var createAttempt = await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data structure");
        }

        return contentType;
    }

    private async Task<IContent> CreateInvariantContentAsync(IContentType contentType)
    {
        var documentKey = Guid.NewGuid();

        var createModel = new ContentCreateModel
        {
            Key = documentKey, ContentTypeKey = contentType.Key, InvariantName = "Test",
        };

        var createAttempt = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
        if (createAttempt.Success is false)
        {
            throw new Exception("Something unexpected went wrong setting up the test data");
        }

        return createAttempt.Result.Content!;
    }

    private async Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>>
        SchedulePublishAndUnPublishForAllCulturesAsync(
            IContent setupData,
            (ILanguage LangEn, ILanguage LangDa, ILanguage LangBe, IContentType contentType) setupInfo)
        => await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = setupInfo.LangEn.IsoCode,
                    Schedule =
                        new ContentScheduleModel
                        {
                            PublishDate = _schedulePublishDate, UnpublishDate = _scheduleUnPublishDate,
                        },
                },
                new()
                {
                    Culture = setupInfo.LangDa.IsoCode,
                    Schedule =
                        new ContentScheduleModel
                        {
                            PublishDate = _schedulePublishDate, UnpublishDate = _scheduleUnPublishDate,
                        },
                },
                new()
                {
                    Culture = setupInfo.LangBe.IsoCode,
                    Schedule = new ContentScheduleModel
                    {
                        PublishDate = _schedulePublishDate, UnpublishDate = _scheduleUnPublishDate,
                    },
                },
            },
            Constants.Security.SuperUserKey);

    private async Task<Attempt<ContentPublishingResult, ContentPublishingOperationStatus>>
        SchedulePublishAndUnPublishInvariantAsync(
            IContent setupData)
        => await ContentPublishingService.PublishAsync(
            setupData.Key,
            new List<CulturePublishScheduleModel>
            {
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule =
                        new ContentScheduleModel
                        {
                            PublishDate = _schedulePublishDate, UnpublishDate = _scheduleUnPublishDate,
                        },
                },
            },
            Constants.Security.SuperUserKey);

    #endregion
}
