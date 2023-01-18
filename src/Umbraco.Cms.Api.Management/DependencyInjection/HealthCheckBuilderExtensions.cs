using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.HealthCheck;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class HealthCheckBuilderExtensions
{
    internal static IUmbracoBuilder AddHealthCheck(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IHealthCheckGroupWithResultViewModelFactory, HealthCheckGroupWithResultViewModelFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<HealthCheckViewModelsMapDefinition>();

        return builder;
    }
}
