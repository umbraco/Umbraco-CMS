using Examine;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Extensions;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all core Umbraco services required to run which may be replaced later in the pipeline.
    /// </summary>
    public static IUmbracoBuilder AddSearchServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IExamineManager, ExamineManager>();
        return builder;
    }
}
