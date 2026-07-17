using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.TemporaryFile;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class TemporaryFileBuilderExtensions
{
    internal static IUmbracoBuilder AddTemporaryFiles(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<TemporaryFileViewModelsMapDefinition>();
        builder.Services.AddTransient<ITemporaryFileConfigurationPresentationFactory>(serviceProvider => new TemporaryFileConfigurationPresentationFactory(
            serviceProvider.GetRequiredService<IOptionsSnapshot<ContentSettings>>(),
            serviceProvider.GetRequiredService<IOptionsSnapshot<RuntimeSettings>>()));

        return builder;
    }
}
