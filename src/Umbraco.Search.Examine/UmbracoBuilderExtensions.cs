using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Examine.ValueSetBuilders;
using Umbraco.Search.Indexing;
using Umbraco.Search.Indexing.Populators;
using Umbraco.Search.NotificationHandlers;
using Umbraco.Search.Services;

namespace Umbraco.Search.Examine;

/// <summary>
///     Provides extension methods to the <see cref="IUmbracoBuilder" /> class.
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddExamine(this IUmbracoBuilder builder)
    {
        // populators are not a collection: one cannot remove ours, and can only add more
        // the container can inject IEnumerable<IIndexPopulator> and get them all
        builder.Services.AddSingleton<IIndexPopulator, MemberIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, ContentIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, PublishedContentIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, MediaIndexPopulator>();

        builder.Services.AddSingleton<IIndexRebuilder, IndexRebuilder>();
        builder.Services.AddSingleton<IUmbracoIndexingHandler, ExamineUmbracoIndexingHandler>();
        builder.Services.AddSingleton<ISearchMainDomHandler,ExamineIndexingMainDomHandler>();
        builder.Services.AddUnique<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
        builder.Services.AddUnique<IPublishedContentValueSetBuilder>(factory =>
            new ContentValueSetBuilder(
                factory.GetRequiredService<PropertyEditorCollection>(),
                factory.GetRequiredService<UrlSegmentProviderCollection>(),
                factory.GetRequiredService<IUserService>(),
                factory.GetRequiredService<IShortStringHelper>(),
                factory.GetRequiredService<IScopeProvider>(),
                true));
        builder.Services.AddUnique<IContentValueSetBuilder>(factory =>
            new ContentValueSetBuilder(
                factory.GetRequiredService<PropertyEditorCollection>(),
                factory.GetRequiredService<UrlSegmentProviderCollection>(),
                factory.GetRequiredService<IUserService>(),
                factory.GetRequiredService<IShortStringHelper>(),
                factory.GetRequiredService<IScopeProvider>(),
                false));
        builder.Services.AddUnique<IValueSetBuilder<IMedia>, MediaValueSetBuilder>();
        builder.Services.AddUnique<IValueSetBuilder<IMember>, MemberValueSetBuilder>();
        builder.Services.AddUnique<IDeliveryApiContentIndexValueSetBuilder, DeliveryApiContentIndexValueSetBuilder>();
        builder.Services.AddUnique<IDeliveryApiContentIndexFieldDefinitionBuilder, DeliveryApiContentIndexFieldDefinitionBuilder>();
        builder.Services.AddUnique<IDeliveryApiContentIndexHelper, DeliveryApiContentIndexHelper>();
        builder.Services.AddSingleton<IDeliveryApiIndexingHandler, DeliveryApiIndexingHandler>();

        builder.AddNotificationHandler<ContentCacheRefresherNotification, ContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, ContentTypeIndexingNotificationHandler>();
        builder.AddNotificationHandler<MediaCacheRefresherNotification, MediaIndexingNotificationHandler>();
        builder.AddNotificationHandler<MemberCacheRefresherNotification, MemberIndexingNotificationHandler>();
        builder.AddNotificationHandler<LanguageCacheRefresherNotification, LanguageIndexingNotificationHandler>();

        builder.AddNotificationHandler<UmbracoRequestBeginNotification, RebuildOnStartupHandler>();

        return builder;
    }
}
