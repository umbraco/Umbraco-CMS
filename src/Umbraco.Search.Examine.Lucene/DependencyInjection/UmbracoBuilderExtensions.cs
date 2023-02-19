using Examine;
using Examine.Lucene.Directories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Search;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Examine;
using Umbraco.Search.Examine.ValueSetBuilders;
using Umbraco.Search.SpecialisedSearchers;

namespace Umbraco.Cms.Infrastructure.Examine.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    public static IServiceCollection RegisterIndex<T>(this IServiceCollection services, string indexName)
    {
        services.AddExamineLuceneIndex<UmbracoExamineIndex, ConfigurationEnabledDirectoryFactory>(indexName)
            .AddSingleton<IUmbracoIndex<T>>(services => new UmbracoExamineIndex<T>(services
                .GetRequiredService<IExamineManager>().GetIndex(indexName), services.GetRequiredService<IValueSetBuilder<T>>()))
            .AddSingleton<IUmbracoSearcher<T>>(services => new UmbracoExamineSearcher<T>(services
            .GetRequiredService<IExamineManager>().GetIndex(indexName).Searcher));

        return services;
    }
    /// <summary>
    ///     Adds the Examine indexes for Umbraco
    /// </summary>
    /// <param name="umbracoBuilder"></param>
    /// <returns></returns>
    public static IUmbracoBuilder AddExamineIndexes(this IUmbracoBuilder umbracoBuilder)
    {
        IServiceCollection services = umbracoBuilder.Services;

        services.AddSingleton<IBackOfficeExamineSearcher, BackOfficeExamineSearcher>();
        services.AddSingleton<IIndexDiagnosticsFactory, LuceneIndexDiagnosticsFactory>();

        services.AddExamine();

        // Create the indexes
        services
            .RegisterIndex<IContent>(Constants.UmbracoIndexes
                .InternalIndexName)
            .RegisterIndex<IContent>(Constants.UmbracoIndexes
                .ExternalIndexName)
            .RegisterIndex<IContent>(Constants.UmbracoIndexes
                .MembersIndexName)
            .ConfigureOptions<ConfigureIndexOptions>();

        services.AddSingleton<IApplicationRoot, UmbracoApplicationRoot>();
        services.AddSingleton<ILockFactory, UmbracoLockFactory>();
        services.AddSingleton<ConfigurationEnabledDirectoryFactory>();

        return umbracoBuilder;
    }
}
