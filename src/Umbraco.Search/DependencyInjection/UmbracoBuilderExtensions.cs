using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;
using Umbraco.Extensions;
using Umbraco.Search.Indexing.Populators;
using Umbraco.Search.NotificationHandlers;
using Umbraco.Search.Services;
using Umbraco.Search.SpecialisedSearchers;
using Umbraco.Search.Telemetry;
using Umbraco.Search.ValueSet.ValueSetBuilders;
using ContentIndexPopulator = Umbraco.Search.Indexing.Populators.ContentIndexPopulator;
using IIndexDiagnosticsFactory = Umbraco.Search.Diagnostics.IIndexDiagnosticsFactory;
using IIndexRebuilder = Umbraco.Search.Indexing.IIndexRebuilder;
using IndexDiagnosticsFactory = Umbraco.Search.Diagnostics.IndexDiagnosticsFactory;
using IUmbracoTreeSearcherFields = Umbraco.Search.Configuration.IUmbracoTreeSearcherFields;
using MediaIndexPopulator = Umbraco.Search.Indexing.Populators.MediaIndexPopulator;
using MemberIndexPopulator = Umbraco.Search.Indexing.Populators.MemberIndexPopulator;
using PublishedContentIndexPopulator = Umbraco.Search.Indexing.Populators.PublishedContentIndexPopulator;

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
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<SearchMapper>();

        builder
            .AddNotificationHandler<ContentCacheRefresherNotification, DeliveryApiContentIndexingNotificationHandler>();
        builder
            .AddNotificationHandler<ContentTypeCacheRefresherNotification,
                DeliveryApiContentIndexingNotificationHandler>();
        builder
            .AddNotificationHandler<PublicAccessCacheRefresherNotification,
                DeliveryApiContentIndexingNotificationHandler>();
        builder.Services.AddSearchServices();
        return builder;
    }

    public static IServiceCollection AddSearchServices(this IServiceCollection services)
    {
        services.AddSingleton<IUmbracoTreeSearcherFields, Configuration.UmbracoTreeSearcherFields>();

        services.AddUnique<IBackOfficeSearcher, NoopBackOfficeSearcher>();
        services.AddScoped<SpecialisedSearchers.Tree.UmbracoTreeSearcher>();
        services.AddSingleton<IIndexPopulator, MemberIndexPopulator>();
        services.AddSingleton<IIndexPopulator, ContentIndexPopulator>();
        services.AddSingleton<IIndexPopulator, PublishedContentIndexPopulator>();
        services.AddSingleton<IIndexPopulator, MediaIndexPopulator>();
        services.AddSingleton<IIndexRebuilder, IndexRebuilder>();
        services.AddSingleton<IIndexPopulator, DeliveryApiContentIndexPopulator>();
        services.AddUnique<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
        services.AddUnique<IPublishedContentValueSetBuilder>(factory =>
            new ContentValueSetBuilder(
                factory.GetRequiredService<PropertyEditorCollection>(),
                factory.GetRequiredService<UrlSegmentProviderCollection>(),
                factory.GetRequiredService<IUserService>(),
                factory.GetRequiredService<IShortStringHelper>(),
                factory.GetRequiredService<IScopeProvider>(),
                true));
        services.AddUnique<IContentValueSetBuilder>(factory =>
            new ContentValueSetBuilder(
                factory.GetRequiredService<PropertyEditorCollection>(),
                factory.GetRequiredService<UrlSegmentProviderCollection>(),
                factory.GetRequiredService<IUserService>(),
                factory.GetRequiredService<IShortStringHelper>(),
                factory.GetRequiredService<IScopeProvider>(),
                false));

        services.AddUnique<IValueSetBuilder<IMedia>, MediaValueSetBuilder>();
        services.AddUnique<IValueSetBuilder<IMember>, MemberValueSetBuilder>();
        services.AddUnique<IDeliveryApiContentIndexValueSetBuilder, DeliveryApiContentIndexValueSetBuilder>();
        services
            .AddUnique<IDeliveryApiContentIndexFieldDefinitionBuilder, DeliveryApiContentIndexFieldDefinitionBuilder>();
        services.AddUnique<IDeliveryApiContentIndexHelper, DeliveryApiContentIndexHelper>();
        services.AddSingleton<IDeliveryApiIndexingHandler, DeliveryApiIndexingHandler>();
        services.AddTransient<IIndexCountService, IndexCountService>();
        services.AddTransient<IDetailedTelemetryProvider, SearchTelemetryProvider>();
        return services;
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
    public static IUmbracoBuilder SetTreeSearcherFields(this IUmbracoBuilder builder,
        IUmbracoTreeSearcherFields treeSearcherFields)
    {
        builder.Services.AddUnique(treeSearcherFields);
        return builder;
    }
}
