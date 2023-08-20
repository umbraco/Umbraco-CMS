using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Search.Configuration;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Examine.Configuration;
using Umbraco.Search.Indexing;
using Umbraco.Search.Indexing.Populators;
using Umbraco.Search.NotificationHandlers;
using Umbraco.Search.Services;
using Umbraco.Search.ValueSet.ValueSetBuilders;
using ContentIndexingNotificationHandler = Umbraco.Search.NotificationHandlers.ContentIndexingNotificationHandler;
using ContentTypeIndexingNotificationHandler =
    Umbraco.Search.NotificationHandlers.ContentTypeIndexingNotificationHandler;
using IndexCreatorSettings = Umbraco.Search.Examine.Configuration.IndexCreatorSettings;
using IUmbracoIndexingHandler = Umbraco.Search.NotificationHandlers.IUmbracoIndexingHandler;
using LanguageIndexingNotificationHandler = Umbraco.Search.NotificationHandlers.LanguageIndexingNotificationHandler;
using MediaIndexingNotificationHandler = Umbraco.Search.NotificationHandlers.MediaIndexingNotificationHandler;
using MemberIndexingNotificationHandler = Umbraco.Search.NotificationHandlers.MemberIndexingNotificationHandler;

namespace Umbraco.Search.Examine;

/// <summary>
///     Provides extension methods to the <see cref="IUmbracoBuilder" /> class.
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddExamine(this IUmbracoBuilder builder)
    {
        UmbracoOptionsAttribute? umbracoOptionsAttribute =
            typeof(IndexCreatorSettings).GetCustomAttribute<UmbracoOptionsAttribute>();
        if (umbracoOptionsAttribute is null)
        {
            throw new ArgumentException($"{typeof(IndexCreatorSettings)} do not have the UmbracoOptionsAttribute.");
        }

        OptionsBuilder<IndexCreatorSettings>? optionsBuilder = builder.Services.AddOptions<IndexCreatorSettings>()
            .Bind(
                builder.Config.GetSection(umbracoOptionsAttribute.ConfigurationKey),
                o => o.BindNonPublicProperties = umbracoOptionsAttribute.BindNonPublicProperties)
            .ValidateDataAnnotations();
        builder.AddNotificationHandler<ContentCacheRefresherNotification, ContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, ContentTypeIndexingNotificationHandler>();
        builder.AddNotificationHandler<MediaCacheRefresherNotification, MediaIndexingNotificationHandler>();
        builder.AddNotificationHandler<MemberCacheRefresherNotification, MemberIndexingNotificationHandler>();
        builder.AddNotificationHandler<LanguageCacheRefresherNotification, LanguageIndexingNotificationHandler>();

        builder.AddNotificationHandler<UmbracoRequestBeginNotification, RebuildOnStartupHandler>();
        builder.Services.AddTransient<ISearchMainDomHandler, ExamineIndexingMainDomHandler>();
        builder.Services.AddSingleton<IIndexConfigurationFactory, IndexConfigurationFactory>();
        builder.Services.AddSingleton<ISearchProvider, ExamineSearchProvider>();
        return builder;
    }
}
