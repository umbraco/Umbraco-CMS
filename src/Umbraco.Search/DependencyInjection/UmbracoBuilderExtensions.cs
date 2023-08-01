
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
using Umbraco.Search.Telemetry;
using Umbraco.Search.ValueSet.ValueSetBuilders;
using ContentIndexPopulator = Umbraco.Search.Indexing.Populators.ContentIndexPopulator;
using IBackOfficeExamineSearcher = Umbraco.Search.SpecialisedSearchers.IBackOfficeExamineSearcher;
using IIndexDiagnosticsFactory = Umbraco.Search.Diagnostics.IIndexDiagnosticsFactory;
using IIndexRebuilder = Umbraco.Search.Indexing.IIndexRebuilder;
using IndexDiagnosticsFactory = Umbraco.Search.Diagnostics.IndexDiagnosticsFactory;
using IUmbracoTreeSearcherFields = Umbraco.Search.Configuration.IUmbracoTreeSearcherFields;
using MediaIndexPopulator = Umbraco.Search.Indexing.Populators.MediaIndexPopulator;
using MemberIndexPopulator = Umbraco.Search.Indexing.Populators.MemberIndexPopulator;
using NoopBackOfficeExamineSearcher = Umbraco.Search.SpecialisedSearchers.NoopBackOfficeExamineSearcher;
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

        builder.Services.AddSingleton<IUmbracoTreeSearcherFields, Configuration.UmbracoTreeSearcherFields>();

        builder.Services.AddUnique<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();
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
