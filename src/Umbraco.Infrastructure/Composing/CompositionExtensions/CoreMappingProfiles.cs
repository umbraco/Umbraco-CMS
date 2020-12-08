using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Mapping;
using Umbraco.Core.Security;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Core.Composing.CompositionExtensions

{
    public static class CoreMappingProfiles
    {
        /// <summary>
        /// Registers the core Umbraco mapper definitions
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IUmbracoBuilder ComposeCoreMappingProfiles(this IUmbracoBuilder builder)
        {
            builder.Services.AddUnique<UmbracoMapper>();

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
                .Add<IdentityMapDefinition>()
               ;

            builder.Services.AddTransient<CommonMapper>();
            builder.Services.AddTransient<MemberTabsAndPropertiesMapper>();

            return builder;
        }
    }
}
