using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

public partial class ContentPublishingServiceTests
{
    [Test]
    public async Task Can_Publish_Root_In_The_Future()
    {
        VerifyIsNotPublished(Textpage.Key);

        var result = await ContentPublishingService.PublishAsync(
            Textpage.Key,
            MakeModel(ContentScheduleCollection.CreateWithEntry("*", DateTime.Now.AddDays(1), null)),
            Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);
        VerifyIsNotPublished(Textpage.Key);
    }

    [Test]
    public async Task Publish_Single_Item_Does_Not_Publish_Children_In_The_Future()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(ContentScheduleCollection.CreateWithEntry("*", DateTime.Now.AddDays(1), null)), Constants.Security.SuperUserKey);

        VerifyIsNotPublished(Textpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

    [Test]
    public async Task Can_Publish_Child_Of_Root_In_The_Future()
    {
        await ContentPublishingService.PublishAsync(Textpage.Key, MakeModel(_allCultures), Constants.Security.SuperUserKey);

        var result = await ContentPublishingService.PublishAsync(Subpage.Key, MakeModel(ContentScheduleCollection.CreateWithEntry("*", DateTime.Now.AddDays(1), null)), Constants.Security.SuperUserKey);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(ContentPublishingOperationStatus.Success, result.Status);
        VerifyIsPublished(Textpage.Key);
        VerifyIsNotPublished(Subpage.Key);
    }

}
