using Lifti;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Search.Configuration;
using Umbraco.Search.Lifti.SpecialisedSearchers;
using Umbraco.Search.Services;
using Umbraco.Search.SpecialisedSearchers;
using Umbraco.Search.ValueSet;
using Umbraco.Search.ValueSet.ValueSetBuilders;
using IContentValueSetBuilder = Umbraco.Search.ValueSet.ValueSetBuilders.IContentValueSetBuilder;

namespace Umbraco.Search.Lifti.DepedencyInjection;

public static class UmbracoBuilderExtensions
{
    public static IServiceCollection RegisterIndexExternal(this IServiceCollection services)
    {
        services.AddInMemoryIndex<IContent, IContentValueSetBuilder>(Constants.UmbracoIndexes
            .ExternalIndexName);
        return services;
    }

    public static IServiceCollection RegisterIndexInternal(this IServiceCollection services)
    {
        services.AddInMemoryIndex<IContent, IContentValueSetBuilder>(Constants.UmbracoIndexes
            .InternalIndexName);
        return services;
    }

    /// <summary>
    /// Registers an Examine index
    /// </summary>
    public static IServiceCollection AddInMemoryIndex<TIndexedModelType, TValueSetBuilder>(
        this IServiceCollection serviceCollection,
        string name) where TIndexedModelType : IUmbracoEntity
        where TValueSetBuilder : notnull, IValueSetBuilder<TIndexedModelType>
    {
        serviceCollection.AddSingleton<ILiftiIndex>(serviceCollection => new UmbracoLiftiIndex(name,
            new FullTextIndexBuilder<string>()
                .WithObjectTokenization<UmbracoValueSet>(o => o
                    .WithKey(c => c.Id)
                    .WithDynamicFields("Fields", c => c.Values?.ToDictionary(x => x.Key, x => x.Value.Select(x=>x.ToString()))!, tokenizationOptions: tokenOptions => tokenOptions.WithStemming()))
                .Build()));
        serviceCollection.AddSingleton<IUmbracoIndex>(serviceCollection =>
            new UmbracoMemoryIndex<TIndexedModelType>(
                serviceCollection.GetRequiredService<ILiftiIndexManager>().GetIndex(name),
                serviceCollection.GetRequiredService<TValueSetBuilder>()));
        serviceCollection.AddSingleton<IUmbracoSearcher>(serviceCollection =>
            new UmbracoMemorySearcher<TIndexedModelType>(
                serviceCollection.GetRequiredService<ILiftiIndexManager>().GetIndex(name),
                name));
        return serviceCollection;
    }

    public static IServiceCollection RegisterIndexMember(this IServiceCollection services)
    {
        services.AddInMemoryIndex<IMember, IValueSetBuilder<IMember>>(Constants.UmbracoIndexes
            .MembersIndexName);
        return services;
    }

    public static IServiceCollection RegisterContentApiIndex(this IServiceCollection services)
    {
        services.AddSingleton<ILiftiIndex>(serviceCollection => new UmbracoLiftiIndex(Constants
                .UmbracoIndexes
                .DeliveryApiContentIndexName,
            new FullTextIndexBuilder<string>().WithObjectTokenization<UmbracoValueSet>(o => o
                    .WithKey(c => c.Id)
                    .WithDynamicFields("Fields", c => c.Values?.ToDictionary(x => x.Key, x => x.Value.Select(x=>x.ToString()))!, tokenizationOptions: tokenOptions => tokenOptions.WithStemming()))
                .Build()));
        // This is the long way to add IOptions but gives us access to the
        // services collection which we need to get the dir factory
        services.AddSingleton<IUmbracoIndex>(serviceCollection =>
            new DeliveryApiContentInMemoryIndex(serviceCollection.GetRequiredService<ILiftiIndexManager>().GetIndex(
                    Constants
                        .UmbracoIndexes
                        .DeliveryApiContentIndexName),
                serviceCollection.GetRequiredService<IContentValueSetBuilder>()));
        services.AddSingleton<IUmbracoSearcher>(serviceCollection =>
            new UmbracoMemorySearcher<IContent>(serviceCollection.GetRequiredService<ILiftiIndexManager>().GetIndex(
                Constants
                    .UmbracoIndexes
                    .DeliveryApiContentIndexName), Constants
                .UmbracoIndexes
                .DeliveryApiContentIndexName));
        return services;
    }

    public static IServiceCollection AddInMemoryServices(this IServiceCollection services)
    {
        services.AddSingleton<ISearchProvider, InMemorySearchProvider>();
        services.AddSingleton<IUmbracoIndexesConfiguration>(services =>
        {
            IIndexConfigurationFactory configuration
                = services.GetRequiredService<IIndexConfigurationFactory>();
            return configuration.GetConfiguration();
        });
        services.AddSingleton<ILiftiIndexManager, LiftiIndexManager>();
        services.AddTransient<ISearchMainDomHandler, InMemoryIndexingMainDomHandler>();

        services.AddSingleton<IBackOfficeSearcher, BackOfficeInMemorySearcher>();
        // Create the indexes
        services
            .RegisterIndexExternal()
            .RegisterIndexInternal()
            .RegisterIndexMember()
            .RegisterContentApiIndex();
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
        services.AddInMemoryServices();


        return umbracoBuilder;
    }
}
