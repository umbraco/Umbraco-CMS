using Examine;
using Examine.Lucene.Directories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;

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

        services.AddSingleton<UmbracoTempEnvFileSystemDirectoryFactory>();
        services.RemoveAll<SyncedFileSystemDirectoryFactory>();
        services.AddSingleton<SyncedFileSystemDirectoryFactory>(
            s =>
            {
                var baseDir = s.GetRequiredService<IApplicationRoot>().ApplicationRoot;

                var tempDir = UmbracoTempEnvFileSystemDirectoryFactory.GetTempPath(
                    s.GetRequiredService<IApplicationIdentifier>(), s.GetRequiredService<IHostingEnvironment>());

                return ActivatorUtilities.CreateInstance<SyncedFileSystemDirectoryFactory>(
                    s,
                    new object[]
                    {
                        new DirectoryInfo(tempDir), s.GetRequiredService<IApplicationRoot>().ApplicationRoot
                    });
            });

    // Create the indexes
    services
            .AddExamineLuceneIndex<UmbracoContentIndex, ConfigurationEnabledDirectoryFactory>(Constants.UmbracoIndexes
                .InternalIndexName)
            .AddExamineLuceneIndex<UmbracoContentIndex, ConfigurationEnabledDirectoryFactory>(Constants.UmbracoIndexes
                .ExternalIndexName)
            .AddExamineLuceneIndex<UmbracoMemberIndex, ConfigurationEnabledDirectoryFactory>(Constants.UmbracoIndexes
                .MembersIndexName)
            .AddExamineLuceneIndex<DeliveryApiContentIndex, ConfigurationEnabledDirectoryFactory>(Constants.UmbracoIndexes
                .DeliveryApiContentIndexName)
            .ConfigureOptions<ConfigureIndexOptions>();

        services.AddSingleton<IApplicationRoot, UmbracoApplicationRoot>();
        services.AddSingleton<ILockFactory, UmbracoLockFactory>();
        services.AddSingleton<ConfigurationEnabledDirectoryFactory>();

        return umbracoBuilder;
    }
}
