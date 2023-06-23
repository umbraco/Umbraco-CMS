using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.RelationType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class RelationTypesBuilderExtensions
{
    internal static IUmbracoBuilder AddRelationTypes(this IUmbracoBuilder builder)
    {
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<RelationTypeViewModelsMapDefinition>();

        builder.Services.AddTransient<IObjectTypePresentationFactory, ObjectTypePresentationFactory>();
        builder.Services.AddTransient<IRelationTypePresentationFactory, RelationTypePresentationFactory>();
        return builder;
    }
}
