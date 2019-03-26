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
                .Append<AuditMapperProfile>()
                .Append<CodeFileMapperProfile>()
                .Append<ContentMapperProfile>()
                .Append<ContentPropertyMapperProfile>()
                .Append<ContentTypeMapperProfile>()
                .Append<DataTypeMapperProfile>()
                .Append<EntityMapperProfile>()
                .Append<DictionaryMapperProfile>()
                .Append<MacroMapperProfile>()
                .Append<MediaMapperProfile>()
                .Append<MemberMapperProfile>()
                .Append<RedirectUrlMapperProfile>()
                .Append<RelationMapperProfile>()
                .Append<SectionMapperProfile>()
                .Append<TagMapperProfile>()
                .Append<TemplateMapperProfile>()
                .Append<UserMapperProfile>()
                .Append<LanguageMapperProfile>();

            composition.Register<CommonMapper>();

            return composition;
        }
    }
}
