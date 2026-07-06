using Examine;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Search.Core.Notifications;
using Umbraco.Cms.Search.Provider.Examine.Configuration;
using Umbraco.Cms.Search.Provider.Examine.NotificationHandlers;
using Umbraco.Cms.Search.Provider.Examine.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Provider.Examine.DependencyInjection;

public static class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds the Umbraco Search provider for Examine.
    /// </summary>
    public static IUmbracoBuilder AddExamineSearchProvider(this IUmbracoBuilder builder)
    {
        IConfigurationSection section = builder.Config.GetSection(ExamineSearchProviderSettings.SectionName);
        builder.Services.Configure<ExamineSearchProviderSettings>(section);

        ExamineSearchProviderSettings settings = section.Get<ExamineSearchProviderSettings>() ?? new ExamineSearchProviderSettings();

        if (settings.ZeroDowntimeIndexing)
        {
            builder.AddActiveAndShadowIndex(Core.Constants.IndexAliases.DraftContent);
            builder.AddActiveAndShadowIndex(Core.Constants.IndexAliases.PublishedContent);
            builder.AddActiveAndShadowIndex(Core.Constants.IndexAliases.DraftMedia);
            builder.AddActiveAndShadowIndex(Core.Constants.IndexAliases.DraftMembers);

            builder.Services.AddSingleton<IActiveIndexManager, ActiveIndexManager>();

            // NOTE: this notification handler should ONLY be active when zero downtime reindexing is in effect
            builder.AddNotificationHandler<IndexRebuildStartingNotification, ZeroDowntimeRebuildNotificationHandler>();
            builder.AddNotificationAsyncHandler<IndexRebuildCompletedNotification, ZeroDowntimeRebuildNotificationHandler>();
        }
        else
        {
            builder.Services.AddExamineLuceneIndex<LuceneIndex, ConfigurationEnabledDirectoryFactory>(Core.Constants.IndexAliases.DraftContent, _ => { });
            builder.Services.AddExamineLuceneIndex<LuceneIndex, ConfigurationEnabledDirectoryFactory>(Core.Constants.IndexAliases.PublishedContent, _ => { });
            builder.Services.AddExamineLuceneIndex<LuceneIndex, ConfigurationEnabledDirectoryFactory>(Core.Constants.IndexAliases.DraftMedia, _ => { });
            builder.Services.AddExamineLuceneIndex<LuceneIndex, ConfigurationEnabledDirectoryFactory>(Core.Constants.IndexAliases.DraftMembers, _ => { });

            builder.Services.AddSingleton<IActiveIndexManager, NoopActiveIndexManager>();
        }

        builder.Services.AddSingleton<IIndexCommitMonitor, IndexCommitMonitor>();

        // This is needed, due to locking of indexes on Azure, to read more on this issue go here: https://github.com/umbraco/Umbraco-CMS/pull/15571
        builder.Services.AddSingleton<UmbracoTempEnvFileSystemDirectoryFactory>();
        builder.Services.RemoveAll<SyncedFileSystemDirectoryFactory>();
        builder.Services.AddSingleton<SyncedFileSystemDirectoryFactory>(
            s =>
            {
                var tempDir = UmbracoTempEnvFileSystemDirectoryFactory.GetTempPath(
                    s.GetRequiredService<IApplicationIdentifier>(), s.GetRequiredService<IHostingEnvironment>());

                return ActivatorUtilities.CreateInstance<SyncedFileSystemDirectoryFactory>(
                    s, new DirectoryInfo(tempDir), s.GetRequiredService<IApplicationRoot>().ApplicationRoot);
            });

        builder.AddNotificationHandler<UmbracoApplicationStartedNotification, RebuildNotificationHandler>();

        builder.Services.AddExamineSearchProviderServices();

        builder.AddBackOfficeOpenApiDocument(
            Constants.Api.Name,
            document => document
                .WithTitle("Umbraco Search Examine API")
                .WithBackOfficeAuthentication()
                .ConfigureOpenApiOptions(options =>
                {
                    options.AddDocumentTransformer((openApiDocument, _, _) =>
                    {
                        openApiDocument.Info.Version = "1.0";
                        return Task.CompletedTask;
                    });

                    // Emit short operation IDs (the controller action name) so the generated TypeScript
                    // client has concise method names instead of the verbose path-based defaults.
                    options.AddOperationTransformer<ActionNameOperationIdTransformer>();
                }));

        return builder;
    }

    /// <summary>
    /// Disables the handling of the default Examine indexes from Umbraco Core.
    /// </summary>
    /// <remarks>
    /// This prevents Umbraco from maintaining the default Examine indexes, thus freeing up server resources.
    ///
    /// Only use this if the default Examine indexes are no longer used; that is, if Umbraco Search powers both
    /// frontend search, backoffice search and the Delivery API (when applicable).
    /// </remarks>
    public static IUmbracoBuilder DisableDefaultExamineIndexes(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ExamineManager>();
        builder.Services.AddUnique<IExamineManager, MaskedCoreIndexesExamineManager>();

        return builder;
    }

    // Sets each operation's ID to the controller action name, matching the operation IDs the generated
    // TypeScript client was built against.
    private sealed class ActionNameOperationIdTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
        {
            if (context.Description.ActionDescriptor.RouteValues.TryGetValue("action", out var action)
                && string.IsNullOrWhiteSpace(action) is false)
            {
                operation.OperationId = action;
            }

            return Task.CompletedTask;
        }
    }

    private static void AddActiveAndShadowIndex(this IUmbracoBuilder builder, string alias)
    {
        builder.Services.AddExamineLuceneIndex<LuceneIndex, ConfigurationEnabledDirectoryFactory>(alias + ActiveIndexManager.SuffixA, _ => { });
        builder.Services.AddExamineLuceneIndex<LuceneIndex, ConfigurationEnabledDirectoryFactory>(alias + ActiveIndexManager.SuffixB, _ => { });
}
}
