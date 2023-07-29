using Examine;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;
using Umbraco.Search.Configuration;
using Umbraco.Search.Indexing;
using Umbraco.Search.Indexing.Populators;
using Umbraco.Search.InMemory.SpecialisedSearchers;
using IBackOfficeExamineSearcher = Umbraco.Search.SpecialisedSearchers.IBackOfficeExamineSearcher;
using IIndexDiagnosticsFactory = Umbraco.Search.Diagnostics.IIndexDiagnosticsFactory;

namespace Umbraco.Search.InMemory.DepedencyInjection;

public static class UmbracoBuilderExtensions
{
    public static IServiceCollection RegisterIndexExternal(this IServiceCollection services)
    {
        services.AddInMemoryIndex<IContent>(Constants.UmbracoIndexes
            .ExternalIndexName);

        return services;
    }

    public static IServiceCollection RegisterIndexInternal(this IServiceCollection services)
    {
        services.AddInMemoryIndex<IContent>(Constants.UmbracoIndexes
            .InternalIndexName);

        return services;
    }

    /// <summary>
    /// Registers an Examine index
    /// </summary>
    public static IServiceCollection AddInMemoryIndex<T>(
        this IServiceCollection serviceCollection,
        string name) where T : IUmbracoEntity
    {
        // This is the long way to add IOptions but gives us access to the
        // services collection which we need to get the dir factory
        serviceCollection.AddSingleton<IUmbracoIndex>(serviceCollection =>
            new UmbracoMemoryIndex<T>(serviceCollection.GetRequiredService<IMemoryCache>(), name))
        return serviceCollection;
    }

    public static IServiceCollection RegisterIndexMember(this IServiceCollection services)
    {
        services.AddInMemoryIndex<IMember>(Constants.UmbracoIndexes
            .MembersIndexName);

        return services;
    }

    public static IServiceCollection RegisterContentApiIndex(this IServiceCollection services)
    {
        // This is the long way to add IOptions but gives us access to the
        // services collection which we need to get the dir factory
        services.AddSingleton<IUmbracoIndex>(serviceCollection =>
            new DeliveryApiContentInMemoryIndex(serviceCollection.GetRequiredService<IMemoryCache>(), Constants
                .UmbracoIndexes
                .DeliveryApiContentIndexName));
        return services;
    }

    /// <summary>
    ///     Adds the Examine indexes for Umbraco
    /// </summary>
    /// <param name="umbracoBuilder"></param>
    /// <returns></returns>
    public static IUmbracoBuilder AddInMemoryIndexes(this IUmbracoBuilder umbracoBuilder)
    {
        IServiceCollection services = umbracoBuilder.Services;
        services.AddSingleton<ISearchProvider, InMemorySearchProvider>();
        services.AddSingleton<IUmbracoIndexesConfiguration>(services =>
        {
            IIndexConfigurationFactory configuration
                = services.GetRequiredService<IIndexConfigurationFactory>();
            return configuration.GetConfiguration();
        });
        services.AddSingleton<IBackOfficeExamineSearcher, BackOfficeInMemorySearcher>();
        // Create the indexes
        services
            .RegisterIndexExternal()
            .RegisterIndexInternal()
            .RegisterIndexMember()
            .RegisterContentApiIndex();


        return umbracoBuilder;
    }
}
