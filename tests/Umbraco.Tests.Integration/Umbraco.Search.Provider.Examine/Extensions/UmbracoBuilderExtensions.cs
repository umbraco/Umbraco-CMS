using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Cms.Search.Provider.Examine.NotificationHandlers;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Extensions;

internal static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddExamineSearchProviderForTest<TIndex, TDirectoryFactory>(this IUmbracoBuilder builder)
        where TIndex : LuceneIndex
        where TDirectoryFactory : class, IDirectoryFactory
    {
        builder.Services.AddExamineSearchProviderServicesForTest<TIndex, TDirectoryFactory>();

        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MediaTreeChangeNotification, MediaTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MemberSavedNotification, MemberSavedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<MemberDeletedNotification, MemberDeletedDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<IndexRebuildStartingNotification, ZeroDowntimeRebuildNotificationHandler>();
        builder.AddNotificationAsyncHandler<IndexRebuildCompletedNotification, ZeroDowntimeRebuildNotificationHandler>();

        return builder;
    }
}
