using LightInject;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Composing.Composers
{
    public static class WebMappingProfilesComposer
    {
        public static IServiceRegistry ComposeWebMappingProfiles(this IServiceRegistry registry)
        {
            //register the profiles
            registry.Register<AuditMapperProfile>();
            registry.Register<CodeFileMapperProfile>();
            registry.Register<ContentMapperProfile>();
            registry.Register<ContentPropertyMapperProfile>();
            registry.Register<ContentTypeMapperProfile>();
            registry.Register<DataTypeMapperProfile>();
            registry.Register<EntityMapperProfile>();
            registry.Register<DictionaryMapperProfile>();
            registry.Register<MacroMapperProfile>();
            registry.Register<MediaMapperProfile>();
            registry.Register<MemberMapperProfile>();
            registry.Register<RedirectUrlMapperProfile>();
            registry.Register<RelationMapperProfile>();
            registry.Register<SectionMapperProfile>();
            registry.Register<TagMapperProfile>();
            registry.Register<TemplateMapperProfile>();
            registry.Register<UserMapperProfile>();
            registry.Register<LanguageMapperProfile>();

            //register any resolvers, etc.. that the profiles use
            registry.Register<ContentUrlResolver>();
            registry.Register<ContentTreeNodeUrlResolver<IContent, ContentTreeController>>();
            registry.Register<TabsAndPropertiesResolver<IContent, ContentItemDisplay>>();
            registry.Register<TabsAndPropertiesResolver<IMedia, MediaItemDisplay>>();
            registry.Register<ContentTreeNodeUrlResolver<IMedia, MediaTreeController>>();
            registry.Register<MemberTabsAndPropertiesResolver>();
            registry.Register<MemberTreeNodeUrlResolver>();
            registry.Register<MemberBasicPropertiesResolver>();

            return registry;
        }
    }
}
