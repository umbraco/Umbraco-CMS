using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

internal abstract class ContentEditingServiceSortChildrenNotificationTestsBase : UmbracoIntegrationTest
{
    protected static int SortedNotificationCount { get; set; }

    protected IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    protected IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    [SetUp]
    public void ResetNotificationCount() => SortedNotificationCount = 0;

    protected async Task<IContent> CreateRootWithChildrenAsync()
    {
        var childContentType = ContentTypeBuilder.CreateBasicContentType("childPage", "Child Page");
        await ContentTypeService.CreateAsync(childContentType, Constants.Security.SuperUserKey);

        var rootContentType = ContentTypeBuilder.CreateBasicContentType("rootPage", "Root Page");
        rootContentType.AllowedAsRoot = true;
        rootContentType.AllowedContentTypes = [new ContentTypeSort(childContentType.Key, 1, childContentType.Alias)];
        await ContentTypeService.CreateAsync(rootContentType, Constants.Security.SuperUserKey);

        var root = (await ContentEditingService.CreateAsync(
            new ContentCreateModel
            {
                ContentTypeKey = rootContentType.Key, ParentKey = Constants.System.RootKey, Variants = [new() { Name = "Root" }],
            },
            Constants.Security.SuperUserKey)).Result.Content!;

        foreach (var name in new[] { "banana", "apple", "cherry" })
        {
            await ContentEditingService.CreateAsync(
                new ContentCreateModel
                {
                    ContentTypeKey = childContentType.Key, ParentKey = root.Key, Variants = [new() { Name = name }],
                },
                Constants.Security.SuperUserKey);
        }

        return root;
    }

    protected sealed class SortedNotificationRecorder : INotificationHandler<ContentSortedNotification>
    {
        public void Handle(ContentSortedNotification notification) => SortedNotificationCount++;
    }
}

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
