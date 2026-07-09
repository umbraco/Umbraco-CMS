using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal sealed partial class ContentTypeEditingServiceTests : ContentTypeEditingServiceTestsBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeChangedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, ContentTypeCacheRefreshedNotificationHandler>();
        builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
        base.CustomTestSetup(builder);
    }

    [SetUp]
    public void SetupTest() => ContentTypeCacheRefreshedNotificationHandler.ContentTypeCacheRefreshed = null;

    private class ContentTypeCacheRefreshedNotificationHandler : INotificationHandler<ContentTypeCacheRefresherNotification>
    {
        public static Action<ContentTypeCacheRefresher.JsonPayload[]>? ContentTypeCacheRefreshed { get; set; }

        public void Handle(ContentTypeCacheRefresherNotification notification)
        {
            if (notification.MessageType != MessageType.RefreshByPayload || notification.MessageObject is not ContentTypeCacheRefresher.JsonPayload[] payloads)
            {
                throw new NotSupportedException();
            }

            ContentTypeCacheRefreshed?.Invoke(payloads);
        }
    }

}
