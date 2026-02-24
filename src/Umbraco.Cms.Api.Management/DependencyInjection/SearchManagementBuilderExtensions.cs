using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

public static class SearchManagementBuilderExtensions
{
    internal static IUmbracoBuilder AddSearchManagement(this IUmbracoBuilder builder)
    {
        // Add examine service
        builder.Services.AddTransient<IExamineManagerService, ExamineManagerService>();

        // TODO (V19): Revert to simple AddTransient<IIndexingRebuilderService, IndexingRebuilderService>()
        // when the obsolete constructors in IndexingRebuilderService are removed.
        // The explicit factory is needed to avoid ambiguous constructor resolution.
        builder.Services.AddTransient<IIndexingRebuilderService>(sp =>
            new IndexingRebuilderService(
                sp.GetRequiredService<IIndexRebuilder>(),
                sp.GetRequiredService<ILogger<IndexingRebuilderService>>(),
                sp.GetRequiredService<ILongRunningOperationService>(),
                sp.GetRequiredService<IServerRoleAccessor>()));

        // Add factories
        builder.Services.AddTransient<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
        builder.Services.AddTransient<IIndexPresentationFactory, IndexPresentationFactory>();

        return builder;
    }
}
