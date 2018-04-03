using LightInject;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.Composing.CompositionRoots
{
    public sealed class WebMappingProfilesCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry container)
        {
            container.Register<AuditMapperProfile>();
            container.Register<CodeFileMapperProfile>();
            container.Register<ContentMapperProfile>();
            container.Register<ContentPropertyMapperProfile>();
            container.Register<ContentTypeMapperProfile>();
            container.Register<DataTypeMapperProfile>();
            container.Register<EntityMapperProfile>();
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
        }
    }
}
