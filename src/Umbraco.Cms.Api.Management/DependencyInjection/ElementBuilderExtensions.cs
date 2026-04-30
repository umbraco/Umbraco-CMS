using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Element;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

/// <summary>
/// Provides extension methods for registering element-related services and mappings with the Umbraco builder.
/// </summary>
internal static class ElementBuilderExtensions
{
    /// <summary>
    /// Registers element factories, permission filter services, and map definitions with the Umbraco builder.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to add element services to.</param>
    /// <returns>The <see cref="IUmbracoBuilder"/> for chaining.</returns>
    internal static IUmbracoBuilder AddElements(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IElementPresentationFactory, ElementPresentationFactory>();
        builder.Services.AddTransient<IElementEditingPresentationFactory, ElementEditingPresentationFactory>();
        builder.Services.AddTransient<IElementVersionPresentationFactory, ElementVersionPresentationFactory>();
        builder.Services.AddScoped<IElementPermissionFilterService, ElementPermissionFilterService>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<ElementMapDefinition>()
            .Add<ElementVersionMapDefinition>();

        return builder;
    }
}
