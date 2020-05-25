using Umbraco.Core.Mapping;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Core.Composing.CompositionExtensions

{
    public static class CoreMappingProfiles
    {
        public static Composition ComposeCoreMappingProfiles(this Composition composition)
        {
            composition.RegisterUnique<UmbracoMapper>();

            composition.WithCollectionBuilder<MapDefinitionCollectionBuilder>()
                .Add<AuditMapDefinition>()
                .Add<CodeFileMapDefinition>()
                //.Add<ContentMapDefinition>() //TODO Add these
                .Add<ContentPropertyMapDefinition>()
                .Add<ContentTypeMapDefinition>()
                .Add<DataTypeMapDefinition>()
                .Add<EntityMapDefinition>()
                .Add<DictionaryMapDefinition>()
                .Add<MacroMapDefinition>()
               // .Add<MediaMapDefinition>() //TODO Add these
              //  .Add<MemberMapDefinition>() //TODO Add these
                .Add<RedirectUrlMapDefinition>()
                .Add<RelationMapDefinition>()
                .Add<SectionMapDefinition>()
                .Add<TagMapDefinition>()
                .Add<TemplateMapDefinition>()
                .Add<UserMapDefinition>()
                .Add<LanguageMapDefinition>()
               // .Add<IdentityMapDefinition>() //TODO Add these
               ;

            return composition;
        }
    }
}
