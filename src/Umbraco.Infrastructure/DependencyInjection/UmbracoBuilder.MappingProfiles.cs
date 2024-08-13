using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Mapping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Registers the core Umbraco mapper definitions
    /// </summary>
    public static IUmbracoBuilder AddCoreMappingProfiles(this IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IUmbracoMapper, UmbracoMapper>();

        builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
            .Add<RelationModelMapDefinition>()
            .Add<TagMapDefinition>()
            .Add<IdentityMapDefinition>();

        builder.Services.AddTransient<CommonMapper>();

        return builder;
    }
}
