using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
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
