using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class TemplateModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            config.CreateMap<ITemplate, TemplateDisplay>()
                .ForMember(x => x.Notifications, exp => exp.Ignore());

            config.CreateMap<TemplateDisplay, Template>()
                .IgnoreDeletableEntityCommonProperties()
                .ForMember(x => x.Path, exp => exp.Ignore())                
                .ForMember(x => x.VirtualPath, exp => exp.Ignore())
                .ForMember(x => x.Path, exp => exp.Ignore())
                .ForMember(x => x.MasterTemplateId, exp => exp.Ignore()) // ok, assigned when creating the template
                .ForMember(x => x.IsMasterTemplate, exp => exp.Ignore())
                .ForMember(x => x.HasIdentity, exp => exp.Ignore());
        }
    }
}
