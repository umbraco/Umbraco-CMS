using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Composing.CompositionExtensions
{
    public static class WebMappingProfiles
    {
        public static Composition ComposeWebMappingProfiles(this Composition composition)
        {
            // register the profiles
            composition.WithCollectionBuilder<MapperProfileCollectionBuilder>()
                .Append<AuditMapperProfile>()
                .Append<CodeFileMapperProfile>()
                .Append<DataTypeMapperProfile>()
                .Append<DictionaryMapperProfile>()
                .Append<MacroMapperProfile>()
                .Append<RedirectUrlMapperProfile>()
                .Append<RelationMapperProfile>()
                .Append<SectionMapperProfile>()
                .Append<TagMapperProfile>()
                .Append<TemplateMapperProfile>()
                .Append<UserMapperProfile>()
                .Append<LanguageMapperProfile>();

            //register the profiles
            //composition.Register<Profile, AuditMapperProfile>();
            //composition.Register<Profile, CodeFileMapperProfile>();
            composition.Register<Profile, ContentMapperProfile>();
            composition.Register<Profile, ContentPropertyMapperProfile>();
            composition.Register<Profile, ContentTypeMapperProfile>();
            //composition.Register<Profile, DataTypeMapperProfile>();
            composition.Register<Profile, EntityMapperProfile>();
            //composition.Register<Profile, DictionaryMapperProfile>();
            //composition.Register<Profile, MacroMapperProfile>();
            composition.Register<Profile, MediaMapperProfile>();
            composition.Register<Profile, MemberMapperProfile>();
            //composition.Register<Profile, RedirectUrlMapperProfile>();
            //composition.Register<Profile, RelationMapperProfile>();
            //composition.Register<Profile, SectionMapperProfile>();
            //composition.Register<Profile, TagMapperProfile>();
            //composition.Register<Profile, TemplateMapperProfile>();
            //composition.Register<Profile, UserMapperProfile>();
            //composition.Register<Profile, LanguageMapperProfile>();

            //register any resolvers, etc.. that the profiles use
            composition.Register<ContentUrlResolver>();
            composition.Register<ContentTreeNodeUrlResolver<IContent, ContentTreeController>>();
            composition.Register<TabsAndPropertiesResolver<IContent, ContentVariantDisplay>>();
            composition.Register<TabsAndPropertiesResolver<IMedia, MediaItemDisplay>>();
            composition.Register<ContentTreeNodeUrlResolver<IMedia, MediaTreeController>>();
            composition.Register<MemberTabsAndPropertiesResolver>();
            composition.Register<MemberTreeNodeUrlResolver>();
            composition.Register<MemberBasicPropertiesResolver>();
            composition.Register<MediaAppResolver>();
            composition.Register<ContentAppResolver>();

            return composition;
        }
    }
}
