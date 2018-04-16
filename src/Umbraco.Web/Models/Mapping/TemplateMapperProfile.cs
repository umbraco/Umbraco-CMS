using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class TemplateMapperProfile : Profile
    {
        public TemplateMapperProfile()
        {
            CreateMap<ITemplate, TemplateDisplay>()
                .ForMember(dest => dest.Notifications, opt => opt.Ignore());

            CreateMap<TemplateDisplay, Template>()
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.VirtualPath, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.MasterTemplateId, opt => opt.Ignore()) // ok, assigned when creating the template
                .ForMember(dest => dest.IsMasterTemplate, opt => opt.Ignore())
                .ForMember(dest => dest.HasIdentity, opt => opt.Ignore());
        }
    }
}
