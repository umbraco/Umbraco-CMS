using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.MemberType;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class MemberTypeBuilderExtensions
{
    internal static IUmbracoBuilder AddMemberTypes(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IMemberTypePresentationFactory, MemberTypePresentationFactory>();
        builder.Services.AddTransient<IMemberTypeEditingPresentationFactory, MemberTypeEditingPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MemberTypeMapDefinition>();
        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MemberTypeCompositionMapDefinition>();

        return builder;
    }
}
