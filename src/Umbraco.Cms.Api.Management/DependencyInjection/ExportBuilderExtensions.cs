using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ExportBuilderExtensions
{
    internal static IUmbracoBuilder AddExport(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IUdtFileContentFactory, UdtFileContentFactory>();

        return builder;
    }
}
