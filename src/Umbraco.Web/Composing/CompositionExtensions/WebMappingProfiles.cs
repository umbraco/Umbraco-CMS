using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Mapping;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.Composing.CompositionExtensions
{
    public static class WebMappingProfiles
    {
        public static Composition ComposeWebMappingProfiles(this Composition composition)
        {
            composition.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<AuditMapDefinition>()
                .Add<CodeFileMapDefinition>()
                .Add<ContentMapDefinition>()
                .Add<ContentPropertyMapDefinition>()
                .Add<ContentTypeMapDefinition>()
                .Add<DataTypeMapDefinition>()
                .Add<EntityMapDefinition>()
                .Add<DictionaryMapDefinition>()
                .Add<MacroMapDefinition>()
                .Add<MediaMapDefinition>()
                .Add<MemberMapDefinition>()
                .Add<RedirectUrlMapDefinition>()
                .Add<RelationMapDefinition>()
                .Add<SectionMapDefinition>()
                .Add<TagMapDefinition>()
                .Add<TemplateMapDefinition>()
                .Add<UserMapDefinition>()
                .Add<LanguageMapDefinition>();

            composition.Register<CommonMapper>();
            composition.Register<MemberTabsAndPropertiesMapper>();

            return composition;
        }
    }
}
