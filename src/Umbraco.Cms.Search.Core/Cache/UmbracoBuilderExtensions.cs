using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Search.Core.Cache.ContentType;
using Umbraco.Cms.Search.Core.Cache.Language;
using Umbraco.Cms.Search.Core.Cache.Media;
using Umbraco.Cms.Search.Core.Cache.MediaType;
using Umbraco.Cms.Search.Core.Cache.Member;
using Umbraco.Cms.Search.Core.Cache.MemberType;
using Umbraco.Cms.Search.Core.Cache.PublicAccess;

namespace Umbraco.Cms.Search.Core.Cache;

/*
 * This wires up notification handlers for custom distributed cache refreshers for content and public access changes.
 *
 * Eventually these cache refreshers should probably be added to core, or the core cache refreshers should be
 * retrofitted with a higher level of granularity.
 *
 * ## Published content cache refresher ##
 *
 * The core distributed caching for content changes cannot tell the difference between "something was published" and
 * "something was saved". We need that to perform only the indexing operations strictly necessary when maintaining
 * indexes for published and draft content, respectively.
 *
 * This custom cache refresher adds that level of granularity.
 *
 * It also adds the ability to distinguish between "publish" and "republish" at culture level, because we only want to
 * trigger a full reindex of all descendants in a given culture (or invariant) when (un)publishing a new culture - not
 * when republishing an already published culture.
 *
 * ## Detailed public access cache refresher ##
 *
 * The core distributed caching for public access changes is lacking detail; it ever only broadcasts that
 * "something has changed - please refresh everything". While that works for the core to invalidate any caching of
 * public access configuration entries, it does not work for search: it's too costly to "just refresh everything".
 *
 * This custom cache refresher for public access changes has a better granularity, which means we can
 * make an informed decision of how much to re-index when public access changes occur.
 *
 */
public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddCustomCacheRefresherNotificationHandlers(this IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentPublishedNotification, PublishedContentNotificationHandler>();
        builder.AddNotificationHandler<ContentUnpublishedNotification, PublishedContentNotificationHandler>();
        builder.AddNotificationHandler<ContentMovedNotification, PublishedContentNotificationHandler>();
        builder.AddNotificationHandler<ContentMovedToRecycleBinNotification, PublishedContentNotificationHandler>();

        builder.AddNotificationHandler<PublicAccessEntrySavedNotification, PublicAccessNotificationHandler>();
        builder.AddNotificationHandler<PublicAccessEntryDeletedNotification, PublicAccessNotificationHandler>();

        builder.AddNotificationHandler<ContentSavedNotification, DraftContentNotificationHandler>();
        builder.AddNotificationHandler<ContentMovedNotification, DraftContentNotificationHandler>();
        builder.AddNotificationHandler<ContentMovedToRecycleBinNotification, DraftContentNotificationHandler>();
        builder.AddNotificationHandler<ContentDeletedNotification, DraftContentNotificationHandler>();

        builder.AddNotificationHandler<MediaSavedNotification, DraftMediaNotificationHandler>();
        builder.AddNotificationHandler<MediaMovedNotification, DraftMediaNotificationHandler>();
        builder.AddNotificationHandler<MediaMovedToRecycleBinNotification, DraftMediaNotificationHandler>();
        builder.AddNotificationHandler<MediaDeletedNotification, DraftMediaNotificationHandler>();

        builder.AddNotificationHandler<MemberSavedNotification, DraftMemberNotificationHandler>();
        builder.AddNotificationHandler<MemberDeletedNotification, DraftMemberNotificationHandler>();

        builder.AddNotificationHandler<ContentTypeChangedNotification, ContentTypeNotificationHandler>();
        builder.AddNotificationHandler<MediaTypeChangedNotification, MediaTypeNotificationHandler>();
        builder.AddNotificationHandler<MemberTypeChangedNotification, MemberTypeNotificationHandler>();
        builder.AddNotificationHandler<LanguageDeletedNotification, LanguageNotificationHandler>();

        return builder;
    }
}
