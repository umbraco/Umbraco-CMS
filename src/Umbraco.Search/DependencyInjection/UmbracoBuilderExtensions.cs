
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;
using Umbraco.Search.Configuration;
using Umbraco.Search.Indexing;
using Umbraco.Search.Indexing.Populators;
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

        builder.Services.AddSingleton<IUmbracoTreeSearcherFields, UmbracoTreeSearcherFields>();

        builder.Services.AddSingleton<IBackOfficeExamineSearcher, NoopBackOfficeExamineSearcher>();
        builder.Services.AddScoped<UmbracoTreeSearcher>();
        builder.Services.AddSingleton<IIndexPopulator, MemberIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, ContentIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, PublishedContentIndexPopulator>();
        builder.Services.AddSingleton<IIndexPopulator, MediaIndexPopulator>();
        builder.Services.AddSingleton<IIndexRebuilder, IndexRebuilder>();
        builder.Services.AddTransient<IIndexCountService, IndexCountService>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, SearchTelemetryProvider>();
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<SearchMapper>();
        return builder;
    }
}
