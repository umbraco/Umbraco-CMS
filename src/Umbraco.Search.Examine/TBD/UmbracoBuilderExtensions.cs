using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;
using Umbraco.Search.Examine.Configuration;
using Umbraco.Search.NotificationHandlers;

namespace Umbraco.Search.Examine.TBD;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all core Umbraco services required to run which may be replaced later in the pipeline.
    /// </summary>
    public static IUmbracoBuilder AddExamineSearchServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IExamineManager, ExamineManager>();
        builder.Services.AddSingleton<ISearchProvider, ExamineSearchProvider>();
        builder.Services.AddUnique(typeof(IExamineIndexConfiguration), typeof(ExamineIndexConfiguration));
        builder.Services.AddSingleton<IExamineIndexConfigurationFactory, ExamineIndexConfigurationFactory>();
        builder.Services.AddUnique(services =>
        {
            var factory = services.GetRequiredService<IExamineIndexConfigurationFactory>();
            return factory.GetConfiguration();
        });

        return builder;
    }
}
