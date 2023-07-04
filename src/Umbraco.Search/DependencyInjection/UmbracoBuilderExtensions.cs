
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Search;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;
using Umbraco.Extensions;
using Umbraco.Search.Configuration;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Indexing;
using Umbraco.Search.Indexing.Populators;
using Umbraco.Search.NotificationHandlers;
using Umbraco.Search.Services;
using Umbraco.Search.SpecialisedSearchers;
using Umbraco.Search.SpecialisedSearchers.Tree;
using Umbraco.Search.Telemetry;

namespace Umbraco.Search.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all core Umbraco services required to run which may be replaced later in the pipeline.
    /// </summary>
    public static IUmbracoBuilder AddSearchServices(this IUmbracoBuilder builder)
    {
        // populators are not a collection: one cannot remove ours, and can only add more
        // the container can inject IEnumerable<IIndexPopulator> and get them all

        builder.Services.AddSingleton<IUmbracoTreeSearcherFields, Configuration.UmbracoTreeSearcherFields>();

        builder.Services.AddSingleton<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();
        builder.Services.AddScoped<SpecialisedSearchers.Tree.UmbracoTreeSearcher>();
        builder.Services.AddSingleton<IIndexPopulator, MemberIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, ContentIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, PublishedContentIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, MediaIndexPopulator>();
        builder.Services.AddSingleton<IIndexRebuilder, IndexRebuilder>();
        builder.Services.AddSingleton<IIndexPopulator, DeliveryApiContentIndexPopulator>();
        builder.Services.AddUnique<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
        builder.AddNotificationHandler<ContentCacheRefresherNotification, DeliveryApiContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeCacheRefresherNotification, DeliveryApiContentIndexingNotificationHandler>();
        builder.AddNotificationHandler<PublicAccessCacheRefresherNotification, DeliveryApiContentIndexingNotificationHandler>();

        builder.Services.AddTransient<IIndexCountService, IndexCountService>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, SearchTelemetryProvider>();
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<SearchMapper>();
        return builder;
    }
    /// <summary>
    ///     Sets the UmbracoTreeSearcherFields to change fields that can be searched in the backoffice.
    /// </summary>
    /// <typeparam name="T">The type of the Umbraco tree searcher fields.</typeparam>
    /// <param name="builder">The builder.</param>
    public static IUmbracoBuilder SetTreeSearcherFields<T>(this IUmbracoBuilder builder)
        where T : class, IUmbracoTreeSearcherFields
    {
        builder.Services.AddUnique<IUmbracoTreeSearcherFields, T>();
        return builder;
    }

    /// <summary>
    ///     Sets the UmbracoTreeSearcherFields to change fields that can be searched in the backoffice.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="factory">A function creating a TreeSearcherFields</param>
    public static IUmbracoBuilder SetTreeSearcherFields(
        this IUmbracoBuilder builder,
        Func<IServiceProvider, IUmbracoTreeSearcherFields> factory)
    {
        builder.Services.AddUnique(factory);
        return builder;
    }

    /// <summary>
    ///     Sets the UmbracoTreeSearcherFields to change fields that can be searched in the backoffice.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="treeSearcherFields">An UmbracoTreeSearcherFields.</param>
    public static IUmbracoBuilder SetTreeSearcherFields(this IUmbracoBuilder builder, IUmbracoTreeSearcherFields treeSearcherFields)
    {
        builder.Services.AddUnique(treeSearcherFields);
        return builder;
    }
}
