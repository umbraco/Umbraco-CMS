using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

public partial class ElementServiceTests
{
    private IElementPublishingService ElementPublishingService => GetRequiredService<IElementPublishingService>();

    private IContentService ContentServiceForScheduling => GetRequiredService<IContentService>();

    private IContentPublishingService ContentPublishingService => GetRequiredService<IContentPublishingService>();

    private readonly DateTime _schedulePublishDate = DateTime.UtcNow.AddDays(1).TruncateTo(DateTimeExtensions.DateTruncate.Second);
    private readonly DateTime _scheduleUnPublishDate = DateTime.UtcNow.AddDays(2).TruncateTo(DateTimeExtensions.DateTruncate.Second);

    [Test]
    public async Task PerformScheduledPublish_Only_Clears_Element_Schedules()
    {
        // Create an element with a scheduled publish
        var elementType = ContentTypeBuilder.CreateBasicElementType();
        elementType.AllowedAsRoot = true;
        var elementTypeResult = await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        Assert.IsTrue(elementTypeResult.Success, "Failed to create element content type");

        var element = ElementBuilder.CreateBasicElement(elementType);
        ElementService.Save(element);

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
        ContentServiceForScheduling.Save(document);

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

        // Run scheduled publishing for elements - should only process element schedules
        var publishDate = _schedulePublishDate.AddMinutes(1);
        var elementResults = ElementService.PerformScheduledPublish(publishDate).ToList();
        Assert.AreEqual(1, elementResults.Count, "Element scheduled publishing should process one element");
        Assert.IsTrue(elementResults[0].Success, $"Element scheduled publish should succeed, got: {elementResults[0].Result}");

        // Verify the document schedule is still intact after element scheduled publishing ran
        var docSchedulesAfterElementPublish = ContentServiceForScheduling.GetContentScheduleByContentId(document.Key);
        Assert.AreEqual(
            1,
            docSchedulesAfterElementPublish.FullSchedule.Count,
            "Document schedule must not be cleared by element scheduled publishing");
    }

    [Test]
    public async Task PerformScheduledUnpublish_Only_Clears_Element_Schedules()
    {
        // Create and publish an element, then schedule unpublish
        var elementType = ContentTypeBuilder.CreateBasicElementType();
        elementType.AllowedAsRoot = true;
        var elementTypeResult = await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);
        Assert.IsTrue(elementTypeResult.Success, "Failed to create element content type");

        var element = ElementBuilder.CreateBasicElement(elementType);
        ElementService.Save(element);

        var publishAttempt = await ElementPublishingService.PublishAsync(
            element.Key,
            [new() { Culture = Constants.System.InvariantCulture }],
            Constants.Security.SuperUserKey);
        Assert.IsTrue(publishAttempt.Success, "Element publish should succeed");

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
        ContentServiceForScheduling.Save(document);

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

        // Run scheduled publishing for elements - should only process element schedules
        var unpublishDate = _scheduleUnPublishDate.AddMinutes(1);
        var elementResults = ElementService.PerformScheduledPublish(unpublishDate).ToList();
        Assert.AreEqual(1, elementResults.Count, "Element scheduled unpublishing should process one element");
        Assert.IsTrue(elementResults[0].Success, $"Element scheduled unpublish should succeed, got: {elementResults[0].Result}");

        // Verify the document expiration schedule is still intact
        var docSchedulesAfterElementUnpublish = ContentServiceForScheduling.GetContentScheduleByContentId(document.Key);
        Assert.AreEqual(
            1,
            docSchedulesAfterElementUnpublish.FullSchedule.Count,
            "Document expiration schedule must not be cleared by element scheduled publishing");
    }
}
