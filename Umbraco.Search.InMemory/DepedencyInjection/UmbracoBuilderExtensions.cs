using Examine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
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
        services.AddInMemoryIndex<UmbracoExamineLuceneIndex, ConfigurationEnabledDirectoryFactory>(Constants
                .UmbracoIndexes
                .ExternalIndexName)
            .AddSingleton<IUmbracoIndex>(services =>
                new UmbracoExamineIndex<IContent>(
                    services
                        .GetRequiredService<IExamineManager>().GetIndex(Constants.UmbracoIndexes
                            .ExternalIndexName),
                    services.GetRequiredService<IPublishedContentValueSetBuilder>()))
            .AddSingleton<IUmbracoSearcher>(
                services => new UmbracoExamineSearcher<IContent>(
                    services
                        .GetRequiredService<IExamineManager>().GetIndex(Constants.UmbracoIndexes
                            .ExternalIndexName).Searcher,
                    services
                        .GetRequiredService<IUmbracoContextFactory>()));

        return services;
    }

    public static IServiceCollection RegisterIndexInternal(this IServiceCollection services)
    {
        services.AddInMemoryIndex<UmbracoExamineLuceneIndex, ConfigurationEnabledDirectoryFactory>(Constants
                .UmbracoIndexes
                .InternalIndexName)
            .AddSingleton<IUmbracoIndex>(services => new UmbracoExamineIndex<IContent>(
                services
                    .GetRequiredService<IExamineManager>().GetIndex(Constants.UmbracoIndexes
                        .InternalIndexName),
                services.GetRequiredService<IContentValueSetBuilder>()))
            .AddSingleton<IUmbracoSearcher>(services => new UmbracoExamineSearcher<IContent>(
                services
                    .GetRequiredService<IExamineManager>().GetIndex(Constants.UmbracoIndexes
                        .InternalIndexName).Searcher,
                services
                    .GetRequiredService<IUmbracoContextFactory>()));

        return services;
    }

    /// <summary>
    /// Registers an Examine index
    /// </summary>
    public static IServiceCollection AddInMemoryIndex<TIndex, TDirectoryFactory>(
        this IServiceCollection serviceCollection,
        string name,
        FieldDefinitionCollection? fieldDefinitions = null,
        Analyzer? analyzer = null,
        IValueSetValidator? validator = null,
        IReadOnlyDictionary<string, IFieldValueTypeFactory>? indexValueTypesFactory = null,
        bool publishedOnly = false)
        where TIndex : UmbracoExamineLuceneIndex
        where TDirectoryFactory : class, IDirectoryFactory
    {
        // This is the long way to add IOptions but gives us access to the
        // services collection which we need to get the dir factory
        serviceCollection.AddSingleton<IConfigureOptions<LuceneDirectoryIndexOptions>>(
            services => new ConfigureNamedOptions<LuceneDirectoryIndexOptions>(
                name,
                (options) =>
                {
                    options.Analyzer = analyzer;
                    options.Validator = validator;
                    options.IndexValueTypesFactory = indexValueTypesFactory;
                    options.FieldDefinitions = fieldDefinitions ?? options.FieldDefinitions;
                    options.DirectoryFactory = services.GetRequiredService<TDirectoryFactory>();
                }));

        return serviceCollection.AddSingleton<IIndex>(services =>
        {
            IOptionsMonitor<LuceneDirectoryIndexOptions> options
                = services.GetRequiredService<IOptionsMonitor<LuceneDirectoryIndexOptions>>();
            ILoggerFactory loggerFactory
                = services.GetRequiredService<ILoggerFactory>();
            IHostingEnvironment hostingEnvironment
                = services.GetRequiredService<IHostingEnvironment>();
            IRuntimeState runtimeState
                = services.GetRequiredService<IRuntimeState>();
            IUmbracoIndexesConfiguration configuration
                = services.GetRequiredService<IUmbracoIndexesConfiguration>();
            TIndex index = ActivatorUtilities.CreateInstance<TIndex>(
                services,
                new object[] { loggerFactory, name, options, configuration, hostingEnvironment, runtimeState });

            return index;
        });
    }

    public static IServiceCollection RegisterIndexMember(this IServiceCollection services)
    {
        services.AddInMemoryIndex<UmbracoExamineLuceneIndex, ConfigurationEnabledDirectoryFactory>(Constants
                .UmbracoIndexes
                .MembersIndexName)
            .AddSingleton<IUmbracoIndex>(services => new UmbracoExamineIndex<IMember>(
                services
                    .GetRequiredService<IExamineManager>().GetIndex(Constants.UmbracoIndexes
                        .MembersIndexName),
                services.GetRequiredService<IValueSetBuilder<IMember>>()))
            .AddSingleton<IUmbracoSearcher>(
                services => new UmbracoExamineSearcher<IMember>(
                    services.GetRequiredService<IExamineManager>()
                        .GetIndex(Constants.UmbracoIndexes
                            .MembersIndexName).Searcher,
                    services.GetRequiredService<IUmbracoContextFactory>()));

        return services;
    }

    public static IServiceCollection RegisterContentApiIndex(this IServiceCollection services)
    {
        services.AddInMemoryIndex<DeliveryApiContentIndex, ConfigurationEnabledDirectoryFactory>(Constants
                .UmbracoIndexes
                .DeliveryApiContentIndexName)
            .AddSingleton<IUmbracoIndex>(services => new UmbracoExamineIndex<IContent>(
                services
                    .GetRequiredService<IExamineManager>().GetIndex(Constants.UmbracoIndexes
                        .DeliveryApiContentIndexName),
                services.GetRequiredService<IDeliveryApiContentIndexValueSetBuilder>()))
            .AddSingleton<IUmbracoSearcher>(services => new UmbracoExamineSearcher<IMember>(
                services
                    .GetRequiredService<IExamineManager>().GetIndex(Constants.UmbracoIndexes
                        .DeliveryApiContentIndexName).Searcher,
                services
                    .GetRequiredService<IUmbracoContextFactory>()));

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
