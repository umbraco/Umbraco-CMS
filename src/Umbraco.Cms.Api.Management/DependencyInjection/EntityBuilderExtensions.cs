using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Item;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class EntityBuilderExtensions
{
    internal static IUmbracoBuilder AddEntities(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<ItemTypeMapDefinition>();
        builder.Services.AddUnique<IFileItemPresentationFactory, FileItemPresentationFactory>();

        return builder;
    }
}
