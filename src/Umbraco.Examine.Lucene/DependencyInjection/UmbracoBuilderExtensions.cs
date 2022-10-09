using Examine;
using Examine.Lucene.Directories;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Examine.DependencyInjection;

public static class UmbracoBuilderExtensions
{
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
            .AddExamineLuceneIndex<UmbracoContentIndex, ConfigurationEnabledDirectoryFactory>(Constants.UmbracoIndexes
                .InternalIndexName)
            .AddExamineLuceneIndex<UmbracoContentIndex, ConfigurationEnabledDirectoryFactory>(Constants.UmbracoIndexes
                .ExternalIndexName)
            .AddExamineLuceneIndex<UmbracoMemberIndex, ConfigurationEnabledDirectoryFactory>(Constants.UmbracoIndexes
                .MembersIndexName)
            .ConfigureOptions<ConfigureIndexOptions>();

        services.AddSingleton<IApplicationRoot, UmbracoApplicationRoot>();
        services.AddSingleton<ILockFactory, UmbracoLockFactory>();
        services.AddSingleton<ConfigurationEnabledDirectoryFactory>();

        return umbracoBuilder;
    }
}
