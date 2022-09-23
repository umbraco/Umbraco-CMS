using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.ManagementApi.Factories;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class FactoryBuilderExtensions
{
    internal static IUmbracoBuilder AddFactories(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IIndexDiagnosticsFactory, IndexDiagnosticsFactory>();
        builder.Services.AddTransient<IIndexRebuilder, ExamineIndexRebuilder>();
        builder.Services.AddTransient<IExamineIndexViewModelFactory, ExamineIndexViewModelFactory>();

        return builder;
    }

}
