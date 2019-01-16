using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class RelationMapperProfile : Profile
    {
        public RelationMapperProfile()
        {
            // FROM IRelationType to RelationTypeDisplay
            CreateMap<IRelationType, RelationTypeDisplay>()
                .ForMember(dest => dest.Icon, opt => opt.Ignore())
                .ForMember(dest => dest.Trashed, opt => opt.Ignore())
                .ForMember(dest => dest.Alias, opt => opt.Ignore())
                .ForMember(dest => dest.Path, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalData, opt => opt.Ignore())
                .ForMember(dest => dest.ChildObjectTypeName, opt => opt.Ignore())
                .ForMember(dest => dest.ParentObjectTypeName, opt => opt.Ignore())
                .ForMember(dest => dest.Relations, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Notifications, opt => opt.Ignore())
                .ForMember(dest => dest.Udi, opt => opt.MapFrom(content => Udi.Create(Constants.UdiEntityType.RelationType, content.Key)))
                .AfterMap((src, dest) =>
                {
                    // Build up the path
                    dest.Path = "-1," + src.Id;

                    // Set the "friendly" names for the parent and child object types
                    dest.ParentObjectTypeName = ObjectTypes.GetUmbracoObjectType(src.ParentObjectType).GetFriendlyName();
                    dest.ChildObjectTypeName = ObjectTypes.GetUmbracoObjectType(src.ChildObjectType).GetFriendlyName();
                });

            // FROM IRelation to RelationDisplay
            CreateMap<IRelation, RelationDisplay>()
                .ForMember(dest => dest.ParentName, opt => opt.Ignore())
                .ForMember(dest => dest.ChildName, opt => opt.Ignore());

            // FROM RelationTypeSave to IRelationType
            CreateMap<RelationTypeSave, IRelationType>()
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.DeleteDate, opt => opt.Ignore());
        }
    }
}
