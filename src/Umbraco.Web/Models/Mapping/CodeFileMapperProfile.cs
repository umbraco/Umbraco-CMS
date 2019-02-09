using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Stylesheet = Umbraco.Core.Models.Stylesheet;

namespace Umbraco.Web.Models.Mapping
{
    public class CodeFileMapperProfile : Profile
    {
        public CodeFileMapperProfile()
        {
            CreateMap<Stylesheet, EntityBasic>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(sheet => sheet.Id))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(sheet => sheet.Alias))
                .ForMember(dest => dest.Key, opt => opt.MapFrom(sheet => sheet.Key))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(sheet => sheet.Name))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(_ => -1))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(sheet => sheet.Path))
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.Ignore())
                .ForMember(dest => dest.Icon, opt => opt.Ignore());

            CreateMap<IPartialView, CodeFileDisplay>()
                .ForMember(dest => dest.FileType, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Snippet, opt => opt.Ignore());

            CreateMap<Script, CodeFileDisplay>()
                .ForMember(dest => dest.FileType, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Snippet, opt => opt.Ignore());

            CreateMap<Stylesheet, CodeFileDisplay>()
                .ForMember(dest => dest.FileType, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Snippet, opt => opt.Ignore());

            CreateMap<CodeFileDisplay, IPartialView>()
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalPath, opt => opt.Ignore())
                .ForMember(dest => dest.HasIdentity, opt => opt.Ignore());

            CreateMap<CodeFileDisplay, Script>()
                .IgnoreEntityCommonProperties()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Key, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalPath, opt => opt.Ignore())
                .ForMember(dest => dest.HasIdentity, opt => opt.Ignore());
        }
    }
}
