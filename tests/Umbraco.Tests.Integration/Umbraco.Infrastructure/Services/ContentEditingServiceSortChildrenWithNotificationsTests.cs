using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class ContentEditingServiceSortChildrenWithNotificationsTests : ContentEditingServiceSortChildrenNotificationTestsBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.Configure<ContentSettings>(config => config.SortChildrenByFieldFiresNotifications = true);
        builder.AddNotificationHandler<ContentSortedNotification, SortedNotificationRecorder>();
    }

    [Test]
    public async Task Sort_By_Field_Fires_Sorted_Notification_When_Opted_In()
    {
        var root = await CreateRootWithChildrenAsync();

        var result = await ContentEditingService.SortByFieldAsync(root.Key, ContentSortField.Name, Direction.Descending, culture: null, Constants.Security.SuperUserKey);

        Assert.AreEqual(ContentEditingOperationStatus.Success, result);
        Assert.Greater(SortedNotificationCount, 0, "Expected a ContentSortedNotification when the opt-in setting is enabled.");
    }
}
