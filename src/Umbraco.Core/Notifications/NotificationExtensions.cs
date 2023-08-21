using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Notifications;

public static class NotificationExtensions
{
    public static T WithState<T>(this T notification, IDictionary<string, object?>? state)
        where T : IStatefulNotification
    {
        notification.State = state!;
        return notification;
    }

    public static T WithStateFrom<T, TSource>(this T notification, TSource source)
        where T : IStatefulNotification
        where TSource : IStatefulNotification
        => notification.WithState(source.State);

    public static ContentPublishedNotification WithRelatedPublishedEntities(
        this ContentPublishedNotification notification, IList<IContent>? relatedEntities)
    {
        if (relatedEntities?.Any() != true)
        {
            return notification;
        }

        notification.RelatedPublishedEntities = relatedEntities.Where(relatedEntity =>
            notification.PublishedEntities.Any(publishedEntity =>
                publishedEntity.Id != relatedEntity.Id));

        return notification;
    }
}
