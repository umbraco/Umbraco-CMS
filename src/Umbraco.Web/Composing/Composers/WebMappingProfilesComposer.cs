using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Trees;

namespace Umbraco.Web.Composing.Composers
{
    public static class WebMappingProfilesComposer
    {
        public static IContainer ComposeWebMappingProfiles(this IContainer container)
        {
            //register the profiles
            container.Register<AuditMapperProfile>();
            container.Register<CodeFileMapperProfile>();
            container.Register<ContentMapperProfile>();
            container.Register<ContentPropertyMapperProfile>();
            container.Register<ContentTypeMapperProfile>();
            container.Register<DataTypeMapperProfile>();
            container.Register<EntityMapperProfile>();
            container.Register<DictionaryMapperProfile>();
            container.Register<MacroMapperProfile>();
            container.Register<MediaMapperProfile>();
            container.Register<MemberMapperProfile>();
            container.Register<RedirectUrlMapperProfile>();
            container.Register<RelationMapperProfile>();
            container.Register<SectionMapperProfile>();
            container.Register<TagMapperProfile>();
            container.Register<TemplateMapperProfile>();
            container.Register<UserMapperProfile>();
            container.Register<LanguageMapperProfile>();

            //register any resolvers, etc.. that the profiles use
            container.Register<ContentUrlResolver>();
            container.Register<ContentTreeNodeUrlResolver<IContent, ContentTreeController>>();
            container.Register<TabsAndPropertiesResolver<IContent, ContentItemDisplay>>();
            container.Register<TabsAndPropertiesResolver<IMedia, MediaItemDisplay>>();
            container.Register<ContentTreeNodeUrlResolver<IMedia, MediaTreeController>>();
            container.Register<MemberTabsAndPropertiesResolver>();
            container.Register<MemberTreeNodeUrlResolver>();
            container.Register<MemberBasicPropertiesResolver>();

            return container;
        }
    }
}
