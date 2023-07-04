using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Lucene.Net.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Examine.Configuration;
using Umbraco.Search.Examine.ValueSetBuilders;
using Umbraco.Search.SpecialisedSearchers;

namespace Umbraco.Search.Examine.Lucene.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    public static IServiceCollection RegisterIndexExternal(this IServiceCollection services)
    {
        services.AddExamineLuceneIndex<UmbracoExamineLuceneIndex, ConfigurationEnabledDirectoryFactory>(Constants
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
        services.AddExamineLuceneIndex<UmbracoExamineLuceneIndex, ConfigurationEnabledDirectoryFactory>(Constants
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
    public static IServiceCollection AddExamineLuceneIndex<TIndex, TDirectoryFactory>(
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
        services.AddExamineLuceneIndex<UmbracoExamineLuceneIndex, ConfigurationEnabledDirectoryFactory>(Constants
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
        services.AddExamineLuceneIndex<DeliveryApiContentIndex, ConfigurationEnabledDirectoryFactory>(Constants
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
    public static IUmbracoBuilder AddExamineLuceneIndexes(this IUmbracoBuilder umbracoBuilder)
    {
        umbracoBuilder.AddExamine();
        IServiceCollection services = umbracoBuilder.Services;

        services.AddSingleton<IBackOfficeExamineSearcher, BackOfficeExamineSearcher>();
        //todo: restore when indexes are working
        services.AddUnique<IIndexDiagnosticsFactory, ExamineLuceneIndexDiagnosticsFactory>();

        services.AddExamine();

        // Create the indexes
        services
            .RegisterIndexExternal()
            .RegisterIndexInternal()
            .RegisterIndexMember()
            .RegisterContentApiIndex()
            .ConfigureOptions<ConfigureIndexOptions>();

        services.AddSingleton<IApplicationRoot, UmbracoApplicationRoot>();
        services.AddSingleton<ILockFactory, UmbracoLockFactory>();
        services.AddSingleton<ConfigurationEnabledDirectoryFactory>();

        return umbracoBuilder;
    }
}
