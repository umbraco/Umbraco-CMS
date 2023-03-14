using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.Mapping;
using Umbraco.Cms.Core.Security;
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
            .Add<AuditMapDefinition>()
            .Add<CodeFileMapDefinition>()
            .Add<ContentPropertyMapDefinition>()
            .Add<ContentTypeMapDefinition>()
            .Add<DataTypeMapDefinition>()
            .Add<EntityMapDefinition>()
            .Add<DictionaryMapDefinition>()
            .Add<MacroMapDefinition>()
            .Add<RedirectUrlMapDefinition>()
            .Add<RelationMapDefinition>()
            .Add<SectionMapDefinition>()
            .Add<TagMapDefinition>()
            .Add<TemplateMapDefinition>()
            .Add<UserMapDefinition>()
            .Add<MemberMapDefinition>()
            .Add<LanguageMapDefinition>()
            .Add<IdentityMapDefinition>();

        builder.Services.AddTransient<CommonMapper>();
        builder.Services.AddTransient<MemberTabsAndPropertiesMapper>();

        return builder;
    }
}
