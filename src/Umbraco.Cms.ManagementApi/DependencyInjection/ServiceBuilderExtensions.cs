using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.ManagementApi.Services;
using Umbraco.New.Cms.Infrastructure.Services;

namespace Umbraco.Cms.ManagementApi.DependencyInjection;

public static class ServiceBuilderExtensions
{
    internal static IUmbracoBuilder AddServices(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IExamineManagerService, ExamineManagerService>();
        builder.Services.AddTransient<ITemporaryIndexingService, TemporaryIndexingService>();
        return builder;
    }

}
