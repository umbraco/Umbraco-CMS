using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Core.Notifications;

/// <summary>
///     Base class for cache refresher notifications
/// </summary>
public abstract class CacheRefresherNotification : INotification
{
    public CacheRefresherNotification(object messageObject, MessageType messageType)
    {
        MessageObject = messageObject ?? throw new ArgumentNullException(nameof(messageObject));
        MessageType = messageType;
    }

    public object MessageObject { get; }

    public MessageType MessageType { get; }
}
