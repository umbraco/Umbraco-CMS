using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Composing.Composers
{
    public static class WebMappingProfilesComposer
    {
        public static Composition ComposeWebMappingProfiles(this Composition composition)
        {
            var container = composition.Container;

            //register the profiles
            container.Register<Profile, AuditMapperProfile>();
            container.Register<Profile, CodeFileMapperProfile>();
            container.Register<Profile, ContentMapperProfile>();
            container.Register<Profile, ContentPropertyMapperProfile>();
            container.Register<Profile, ContentTypeMapperProfile>();
            container.Register<Profile, DataTypeMapperProfile>();
            container.Register<Profile, EntityMapperProfile>();
            container.Register<Profile, DictionaryMapperProfile>();
            container.Register<Profile, MacroMapperProfile>();
            container.Register<Profile, MediaMapperProfile>();
            container.Register<Profile, MemberMapperProfile>();
            container.Register<Profile, RedirectUrlMapperProfile>();
            container.Register<Profile, RelationMapperProfile>();
            container.Register<Profile, SectionMapperProfile>();
            container.Register<Profile, TagMapperProfile>();
            container.Register<Profile, TemplateMapperProfile>();
            container.Register<Profile, UserMapperProfile>();
            container.Register<Profile, LanguageMapperProfile>();

            //register any resolvers, etc.. that the profiles use
            container.Register<ContentUrlResolver>();
            container.Register<ContentTreeNodeUrlResolver<IContent, ContentTreeController>>();
            container.Register<TabsAndPropertiesResolver<IContent, ContentVariantDisplay>>();
            container.Register<TabsAndPropertiesResolver<IMedia, MediaItemDisplay>>();
            container.Register<ContentTreeNodeUrlResolver<IMedia, MediaTreeController>>();
            container.Register<MemberTabsAndPropertiesResolver>();
            container.Register<MemberTreeNodeUrlResolver>();
            container.Register<MemberBasicPropertiesResolver>();
            container.Register<MediaAppResolver>();
            container.Register<ContentAppResolver>();

            return composition;
        }
    }
}
