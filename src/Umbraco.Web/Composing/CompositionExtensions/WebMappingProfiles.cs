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
            composition.WithCollectionBuilder<MapperProfileCollectionBuilder>()
                .Add<AuditMapperProfile>()
                .Add<CodeFileMapperProfile>()
                .Add<ContentMapperProfile>()
                .Add<ContentPropertyMapperProfile>()
                .Add<ContentTypeMapperProfile>()
                .Add<DataTypeMapperProfile>()
                .Add<EntityMapperProfile>()
                .Add<DictionaryMapperProfile>()
                .Add<MacroMapperProfile>()
                .Add<MediaMapperProfile>()
                .Add<MemberMapperProfile>()
                .Add<RedirectUrlMapperProfile>()
                .Add<RelationMapperProfile>()
                .Add<SectionMapperProfile>()
                .Add<TagMapperProfile>()
                .Add<TemplateMapperProfile>()
                .Add<UserMapperProfile>()
                .Add<LanguageMapperProfile>();

            composition.Register<CommonMapper>();

            return composition;
        }
    }
}
