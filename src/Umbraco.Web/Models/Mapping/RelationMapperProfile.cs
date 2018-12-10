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
                .ForMember(x => x.Icon, expression => expression.Ignore())
                .ForMember(x => x.Trashed, expression => expression.Ignore())
                .ForMember(x => x.Alias, expression => expression.Ignore())
                .ForMember(x => x.Path, expression => expression.Ignore())
                .ForMember(x => x.AdditionalData, expression => expression.Ignore())
                .ForMember(x => x.ChildObjectTypeName, expression => expression.Ignore())
                .ForMember(x => x.ParentObjectTypeName, expression => expression.Ignore())
                .ForMember(x => x.Relations, expression => expression.Ignore())
                .ForMember(
                    x => x.Udi,
                    expression => expression.MapFrom(
                        content => Udi.Create(Constants.UdiEntityType.RelationType, content.Key)))
                .AfterMap((src, dest) =>
                {
                    // Build up the path
                    dest.Path = "-1," + src.Id;

                    // Set the "friendly" names for the parent and child object types
                    dest.ParentObjectTypeName = ObjectTypes.GetUmbracoObjectType(src.ParentObjectType).GetFriendlyName();
                    dest.ChildObjectTypeName = ObjectTypes.GetUmbracoObjectType(src.ChildObjectType).GetFriendlyName();
                });

            // FROM IRelation to RelationDisplay
            CreateMap<IRelation, RelationDisplay>();

            // FROM RelationTypeSave to IRelationType
            CreateMap<RelationTypeSave, IRelationType>();
        }
    }
}
