using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Persistence;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.Provider.Examine.Configuration;
using Umbraco.Cms.Search.Provider.Examine.Services;
using ServicesCollectionExtensions = Examine.ServicesCollectionExtensions;

namespace Umbraco.Cms.Search.Provider.Examine.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static void AddExamineSearchProviderServices(this IServiceCollection services)
    {
        ServicesCollectionExtensions.AddExamine(services);
        services.ConfigureOptions<ConfigureIndexOptions>();

        // register the in-memory searcher and indexer so they can be used explicitly for index registrations
        services.AddTransient<IExamineIndexer, Indexer>();
        services.AddTransient<IExamineSearcher, Searcher>();

        services.AddTransient<IIndexer, Indexer>();
        services.AddTransient<ISearcher, Searcher>();

        services.Configure<IndexOptions>(options =>
        {
            options.RegisterContentIndex<IExamineIndexer, IExamineSearcher, IDraftContentChangeStrategy>(Core.Constants.IndexAliases.DraftContent, UmbracoObjectTypes.Document);
            options.RegisterContentIndex<IExamineIndexer, IExamineSearcher, IPublishedContentChangeStrategy>(Core.Constants.IndexAliases.PublishedContent, UmbracoObjectTypes.Document);
            options.RegisterContentIndex<IExamineIndexer, IExamineSearcher, IDraftContentChangeStrategy>(Core.Constants.IndexAliases.DraftMedia, UmbracoObjectTypes.Media);
            options.RegisterContentIndex<IExamineIndexer, IExamineSearcher, IDraftContentChangeStrategy>(Core.Constants.IndexAliases.DraftMembers, UmbracoObjectTypes.Member);
        });
    }
}
