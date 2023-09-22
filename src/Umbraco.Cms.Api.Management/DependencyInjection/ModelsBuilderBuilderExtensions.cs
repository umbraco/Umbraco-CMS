using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.ModelsBuilder;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ModelsBuilderBuilderExtensions
{
    internal static IUmbracoBuilder AddModelsBuilder(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IModelsBuilderPresentationFactory, ModelsBuilderPresentationFactory>();

        return builder;
    }
}
