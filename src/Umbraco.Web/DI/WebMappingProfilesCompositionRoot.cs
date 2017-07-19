using LightInject;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.DI
{
    public sealed class WebMappingProfilesCompositionRoot : ICompositionRoot
    {
        public void Compose(IServiceRegistry container)
        {
            container.Register<ContentProfile>();
            container.Register<ContentPropertyProfile>();
            container.Register<ContentTypeProfile>();
            container.Register<DataTypeProfile>();
            container.Register<EntityProfile>();
            container.Register<MacroProfile>();
            container.Register<MediaProfile>();
            container.Register<MemberProfile>();
            container.Register<RelationProfile>();
            container.Register<SectionProfile>();
            container.Register<TabProfile>();
            container.Register<UserProfile>();
            container.Register<DashboardProfile>();
            container.Register<TemplateProfile>();
        }
    }
}