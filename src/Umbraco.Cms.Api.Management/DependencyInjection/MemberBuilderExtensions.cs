using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Mapping.Member;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Cms.Api.Management.DependencyInjection;

internal static class MemberBuilderExtensions
{
    internal static IUmbracoBuilder AddMember(this IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IMemberPresentationFactory, MemberPresentationFactory>();
        builder.Services.AddTransient<IMemberEditingPresentationFactory, MemberEditingPresentationFactory>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>().Add<MemberMapDefinition>();

        return builder;
    }
}
