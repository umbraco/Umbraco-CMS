using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Factories;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.New.Cms.Infrastructure.Services;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class ExamineManagementBuilderExtensions
{
    internal static IUmbracoBuilder AddExamineManagement(this IUmbracoBuilder builder)
    {
        // Add examine service
        builder.Services.AddTransient<IExamineManagerService, ExamineManagerService>();
        builder.Services.AddTransient<IIndexingRebuilderService, IndexingRebuilderService>();

        // Add factories
        builder.Services.AddTransient<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
        builder.Services.AddTransient<IIndexRebuilder, ExamineIndexRebuilder>();
        builder.Services.AddTransient<IExamineIndexViewModelFactory, ExamineIndexViewModelFactory>();
        return builder;
    }
}
