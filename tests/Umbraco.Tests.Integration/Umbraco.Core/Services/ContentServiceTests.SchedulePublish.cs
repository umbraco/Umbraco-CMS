using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class ContentServiceTests
{
    private IElementService ElementServiceForScheduling => GetRequiredService<IElementService>();

    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private readonly DateTime _schedulePublishDate = DateTime.UtcNow.AddDays(1).TruncateTo(DateTimeExtensions.DateTruncate.Second);
    private readonly DateTime _scheduleUnPublishDate = DateTime.UtcNow.AddDays(2).TruncateTo(DateTimeExtensions.DateTruncate.Second);

    [Test]
    public async Task PerformScheduledPublish_Only_Clears_Document_Schedules()
    {
        // Create an element with a scheduled publish
        var elementType = ContentTypeBuilder.CreateBasicElementType();
        elementType.AllowedAsRoot = true;
        var elementTypeResult = await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        Assert.IsTrue(elementTypeResult.Success, "Failed to create element content type");

        var element = ElementBuilder.CreateBasicElement(elementType);
        ElementServiceForScheduling.Save(element);

        var elementScheduleAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(elementScheduleAttempt.Success);

        // Create a document with a scheduled publish at the same time
        var docType = ContentTypeBuilder.CreateBasicContentType();
        docType.AllowedAsRoot = true;
        var docTypeResult = await ContentTypeService.CreateAsync(docType, Constants.Security.SuperUserKey);
        Assert.IsTrue(docTypeResult.Success, "Failed to create document content type");

        var document = ContentBuilder.CreateBasicContent(docType);
        ContentService.Save(document);

        var docScheduleAttempt = await ContentPublishingService.PublishAsync(
            document.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { PublishDate = _schedulePublishDate },
                },
            ],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(docScheduleAttempt.Success);

        // Run scheduled publishing for documents - should only process document schedules
        // Note: The base class (UmbracoIntegrationTestWithContent) creates a Subpage with a past
        // release schedule, so more than one result may be returned.
        var publishDate = _schedulePublishDate.AddMinutes(1);
        var docResults = ContentService.PerformScheduledPublish(publishDate).ToList();
        Assert.IsNotEmpty(docResults, "Document scheduled publishing should process at least one document");

        // Verify the element schedule is still intact after document scheduled publishing ran
        var elementSchedulesAfterDocPublish = ElementServiceForScheduling.GetContentScheduleByContentId(element.Key);
        Assert.AreEqual(
            1,
            elementSchedulesAfterDocPublish.FullSchedule.Count,
            "Element schedule must not be cleared by document scheduled publishing");
    }

    [Test]
    public async Task PerformScheduledUnpublish_Only_Clears_Document_Schedules()
    {
        // Create and publish an element, then schedule unpublish
        var elementType = ContentTypeBuilder.CreateBasicElementType();
        elementType.AllowedAsRoot = true;
        var elementTypeResult = await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        Assert.IsTrue(elementTypeResult.Success, "Failed to create element content type");

        var element = ElementBuilder.CreateBasicElement(elementType);
        ElementServiceForScheduling.Save(element);

        var elementPublishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(elementPublishAttempt.Success, "Element publish should succeed");

        var elementScheduleAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
            ],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(elementScheduleAttempt.Success);

        // Create and publish a document, then schedule unpublish at the same time
        var docType = ContentTypeBuilder.CreateBasicContentType();
        docType.AllowedAsRoot = true;
        var docTypeResult = await ContentTypeService.CreateAsync(docType, Constants.Security.SuperUserKey);
        Assert.IsTrue(docTypeResult.Success, "Failed to create document content type");

        var document = ContentBuilder.CreateBasicContent(docType);
        ContentService.Save(document);

        var docPublishAttempt = await ContentPublishingService.PublishAsync(
            document.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(docPublishAttempt.Success, "Document publish should succeed");

        var docScheduleAttempt = await ContentPublishingService.PublishAsync(
            document.Key,
            [
                new()
                {
                    Culture = Constants.System.InvariantCulture,
                    Schedule = new ContentScheduleModel { UnpublishDate = _scheduleUnPublishDate },
                },
            ],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(docScheduleAttempt.Success);

        // Run scheduled publishing for documents - should only process document schedules
        var unpublishDate = _scheduleUnPublishDate.AddMinutes(1);
        var docResults = ContentService.PerformScheduledPublish(unpublishDate).ToList();
        Assert.IsNotEmpty(docResults, "Document scheduled publishing should process at least one document");

        // Verify the element expiration schedule is still intact
        var elementSchedulesAfterDocUnpublish = ElementServiceForScheduling.GetContentScheduleByContentId(element.Key);
        Assert.AreEqual(
            1,
            elementSchedulesAfterDocUnpublish.FullSchedule.Count,
            "Element expiration schedule must not be cleared by document scheduled publishing");
    }
}
