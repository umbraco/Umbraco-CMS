using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

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
        builder.Services.AddSingleton<IIndexPopulator, DeliveryApiContentIndexPopulator>();

        builder.Services.AddSingleton<IIndexRebuilder, ExamineIndexRebuilder>();
        builder.Services.AddSingleton<IUmbracoIndexingHandler, ExamineUmbracoIndexingHandler>();
        builder.Services.AddSingleton<ExamineIndexingMainDomHandler>();
        builder.Services.AddUnique<IUmbracoIndexConfig, UmbracoIndexConfig>();
        builder.Services.AddUnique<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
        builder.Services.AddUnique<IPublishedContentValueSetBuilder>(factory =>
            new ContentValueSetBuilder(
                factory.GetRequiredService<PropertyEditorCollection>(),
                factory.GetRequiredService<UrlSegmentProviderCollection>(),
                factory.GetRequiredService<IUserService>(),
                factory.GetRequiredService<IShortStringHelper>(),
                factory.GetRequiredService<IScopeProvider>(),
                true,
                factory.GetRequiredService<ILocalizationService>()));
        builder.Services.AddUnique<IContentValueSetBuilder>(factory =>
            new ContentValueSetBuilder(
                factory.GetRequiredService<PropertyEditorCollection>(),
                factory.GetRequiredService<UrlSegmentProviderCollection>(),
                factory.GetRequiredService<IUserService>(),
                factory.GetRequiredService<IShortStringHelper>(),
                factory.GetRequiredService<IScopeProvider>(),
                false,
                factory.GetRequiredService<ILocalizationService>()));
        builder.Services.AddUnique<IValueSetBuilder<IMedia>, MediaValueSetBuilder>();
        builder.Services.AddUnique<IValueSetBuilder<IMember>, MemberValueSetBuilder>();
        builder.Services.AddUnique<IDeliveryApiContentIndexValueSetBuilder, DeliveryApiContentIndexValueSetBuilder>();
        builder.Services.AddUnique<IDeliveryApiContentIndexFieldDefinitionBuilder, DeliveryApiContentIndexFieldDefinitionBuilder>();
        builder.Services.AddUnique<IDeliveryApiContentIndexHelper, DeliveryApiContentIndexHelper>();
        builder.Services.AddSingleton<IDeliveryApiIndexingHandler, DeliveryApiIndexingHandler>();
        builder.Services.AddSingleton<ExamineIndexRebuilder>();
        builder.Services.AddUnique<IDeliveryApiCompositeIdHandler, DeliveryApiCompositeIdHandler>();

        builder.AddNotificationHandler<ContentCacheRefresherNotification, ContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<PublicAccessCacheRefresherNotification, ContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, ContentTypeIndexingNotificationHandler>();
        builder.AddNotificationHandler<ContentCacheRefresherNotification, DeliveryApiContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, DeliveryApiContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<PublicAccessCacheRefresherNotification, DeliveryApiContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<MediaCacheRefresherNotification, MediaIndexingNotificationHandler>();
        builder.AddNotificationHandler<MemberCacheRefresherNotification, MemberIndexingNotificationHandler>();
        builder.AddNotificationHandler<LanguageCacheRefresherNotification, LanguageIndexingNotificationHandler>();

        builder.AddNotificationHandler<UmbracoRequestBeginNotification, RebuildOnStartupHandler>();

        return builder;
    }
}
