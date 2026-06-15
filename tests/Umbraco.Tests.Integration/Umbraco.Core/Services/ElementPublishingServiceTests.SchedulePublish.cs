using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ElementPublishingServiceTests
{
    private IElementService ElementService => GetRequiredService<IElementService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private readonly DateTime _schedulePublishDate = DateTime.UtcNow.AddDays(1).TruncateTo(DateTimeExtensions.DateTruncate.Second);
    private readonly DateTime _scheduleUnPublishDate = DateTime.UtcNow.AddDays(2).TruncateTo(DateTimeExtensions.DateTruncate.Second);

    [Test]
    public async Task Can_Schedule_Publish_Invariant()
    {
        var elementType = await SetupInvariantElementTypeAsync();
        var element = await CreateInvariantContentAsync(elementType);

        var scheduleAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ElementService.GetContentScheduleByContentId(element.Key);

        Assert.Multiple(() =>
        {
            Assert.That(element.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(
                schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Release).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Can_Schedule_Publish_Single_Culture()
    {
        var (langEn, langDa, langBe, elementType) = await SetupVariantElementTypeAsync();
        var element = await CreateVariantElementAsync(langEn, langDa, langBe, elementType);

        var scheduleAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new()
                {
                    Culture = langEn.IsoCode,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ElementService.GetContentScheduleByContentId(element.Key);

        Assert.Multiple(() =>
        {
            Assert.That(element.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(
                schedules.GetSchedule(langEn.IsoCode, ContentScheduleAction.Release).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Can_Schedule_Publish_And_Unpublish()
    {
        var elementType = await SetupInvariantElementTypeAsync();
        var element = await CreateInvariantContentAsync(elementType);

        var scheduleAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel
                    {
                        PublishDate = _schedulePublishDate,
                        UnpublishDate = _scheduleUnPublishDate,
                    },
                },
            ],
            Constants.Security.SuperUserKey);

        Assert.That(scheduleAttempt.Success, Is.True);

        var schedules = ElementService.GetContentScheduleByContentId(element.Key);

        Assert.Multiple(() =>
        {
            Assert.That(element.PublishedCultures.Count(), Is.EqualTo(0));
            Assert.That(
                schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Release).Single().Date, Is.EqualTo(_schedulePublishDate));
            Assert.That(
                schedules.GetSchedule(Constants.System.InvariantCulture, ContentScheduleAction.Expire).Single().Date, Is.EqualTo(_scheduleUnPublishDate));
            Assert.That(schedules.FullSchedule, Has.Count.EqualTo(2));
        });
    }
}
