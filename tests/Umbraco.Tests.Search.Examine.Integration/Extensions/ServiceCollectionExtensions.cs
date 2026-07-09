using Examine;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Search.Provider.Examine.Configuration;
using Umbraco.Cms.Search.Provider.Examine.DependencyInjection;
using Umbraco.Cms.Search.Provider.Examine.Services;

namespace Umbraco.Tests.Search.Examine.Integration.Extensions;

internal static class ServiceCollectionExtensions
{
    private static readonly string[] IndexAliases =
    [
        Cms.Core.Constants.IndexAliases.DraftContent,
        Cms.Core.Constants.IndexAliases.PublishedContent,
        Cms.Core.Constants.IndexAliases.DraftMedia,
        Cms.Core.Constants.IndexAliases.DraftMembers,
    ];

    public static IServiceCollection AddExamineSearchProviderServicesForTest<TIndex, TDirectoryFactory>(this IServiceCollection services)
        where TIndex : LuceneIndex
        where TDirectoryFactory : class, IDirectoryFactory
    {
        services.ConfigureOptions<TestIndexConfigureOptions>();
        services.Configure<SearcherOptions>(options => options.MaxFacetValues = 250);
        services.AddSingleton<TDirectoryFactory>();

        // Register dual indexes (_a and _b) per logical alias for zero-downtime reindexing
        foreach (var alias in IndexAliases)
        {
            services.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(alias + ActiveIndexManager.SuffixA, _ => { });
            services.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(alias + ActiveIndexManager.SuffixB, _ => { });
        }

        services.AddExamineSearchProviderServices();

        // Override to use ActiveIndexManager for zero-downtime reindexing in integration tests
        services.AddSingleton<IActiveIndexManager, ActiveIndexManager>();

        services.AddSingleton<IIndexCommitMonitor, IndexCommitMonitor>();

        return services;
    }

    public static IServiceCollection AddExamineSearchProviderServicesWithoutZeroDowntimeForTest<TIndex, TDirectoryFactory>(this IServiceCollection services)
        where TIndex : LuceneIndex
        where TDirectoryFactory : class, IDirectoryFactory
    {
        services.ConfigureOptions<TestIndexConfigureOptions>();
        services.Configure<SearcherOptions>(options => options.MaxFacetValues = 250);
        services.AddSingleton<TDirectoryFactory>();

        // Register single indexes per alias (no zero-downtime reindexing)
        foreach (var alias in IndexAliases)
        {
            services.AddExamineLuceneIndex<TIndex, TDirectoryFactory>(alias, _ => { });
        }

        services.AddExamineSearchProviderServices();

        services.AddSingleton<IActiveIndexManager, NoopActiveIndexManager>();

        return services;
    }
}
