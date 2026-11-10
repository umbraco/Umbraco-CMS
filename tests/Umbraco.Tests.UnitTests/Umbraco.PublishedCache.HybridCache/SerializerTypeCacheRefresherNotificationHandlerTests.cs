// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

[TestFixture]
public class SerializerTypeCacheRefresherNotificationHandlerTests
{
    /// <summary>
    ///     Verifies that a refresh of type definitions, as seen by a server that did not save them, clears the
    ///     serializer factory's type cache.
    /// </summary>
    [TestCase(MessageType.RefreshByPayload)]
    [TestCase(MessageType.RefreshAll)]
    public void Refreshing_Types_Clears_The_Serializer_Type_Cache(MessageType messageType)
    {
        var serializerFactory = new Mock<IContentCacheDataSerializerFactory>();
        var handler = new SerializerTypeCacheRefresherNotificationHandler(serializerFactory.Object);

        handler.Handle(new ContentTypeCacheRefresherNotification(new object(), messageType));

        serializerFactory.Verify(x => x.ClearTypeCache(), Times.Once);
    }
}
