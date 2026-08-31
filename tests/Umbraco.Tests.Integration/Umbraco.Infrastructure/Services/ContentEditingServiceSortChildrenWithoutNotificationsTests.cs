using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentEditingServiceSortChildrenWithoutNotificationsTests : ContentEditingServiceSortChildrenNotificationTestsBase
{
    // Leaves ContentSettings.SortChildrenByFieldFiresNotifications at its default (false).
    protected override void CustomTestSetup(IUmbracoBuilder builder)
        => builder.AddNotificationHandler<ContentSortedNotification, SortedNotificationRecorder>();

    [Test]
    public async Task Sort_By_Field_Does_Not_Fire_Sorted_Notification_By_Default()
    {
        var root = await CreateRootWithChildrenAsync();

        var result = await ContentEditingService.SortByFieldAsync(root.Key, ContentSortField.Name, Direction.Descending, culture: null, Constants.Security.SuperUserKey);

        Assert.AreEqual(ContentEditingOperationStatus.Success, result);
        Assert.AreEqual(0, SortedNotificationCount, "Expected no ContentSortedNotification on the default bulk path.");
    }
}
