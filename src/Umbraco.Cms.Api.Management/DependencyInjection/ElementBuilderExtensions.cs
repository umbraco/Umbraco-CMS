using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Element;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class ElementBuilderExtensions
{
    internal static IUmbracoBuilder AddElements(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IElementPresentationFactory, ElementPresentationFactory>();
        builder.Services.AddTransient<IElementEditingPresentationFactory, ElementEditingPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<ElementMapDefinition>();

        return builder;
    }
}
