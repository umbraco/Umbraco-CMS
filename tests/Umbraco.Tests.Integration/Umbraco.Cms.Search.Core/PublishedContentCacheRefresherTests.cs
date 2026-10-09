using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Cache;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Tests.Common.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public partial class PublishedContentCacheRefresherTests : TestBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);
        builder.AddNotificationHandler<PublishedContentCacheRefresherNotification, NotificationHandler>();
    }

    private static List<PublishedContentCacheRefresher.JsonPayload> GetNotificationPayloads()
        => NotificationHandler
            .Notifications
            .TrueForAll(n => n.MessageObject is ContentCacheRefresherNotificationPayload<PublishedContentCacheRefresher.JsonPayload>[])
            ? NotificationHandler.Notifications.SelectMany(n => ((ContentCacheRefresherNotificationPayload<PublishedContentCacheRefresher.JsonPayload>[])n.MessageObject).Single().Payloads).ToList()
            : throw new InvalidOperationException("One or more notification handler message objects were not of the expected JSON payload type");

    private static void ResetNotificationPayloads()
        => NotificationHandler.Notifications.Clear();

    private IContent Get(Guid key)
        => ContentService.GetById(key) ?? throw new ArgumentException("No content found with that key");

    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private class NotificationHandler : INotificationHandler<PublishedContentCacheRefresherNotification>
    {
        public static List<PublishedContentCacheRefresherNotification> Notifications { get; } = new();

        public void Handle(PublishedContentCacheRefresherNotification notification)
            => Notifications.Add(notification);
    }
}
