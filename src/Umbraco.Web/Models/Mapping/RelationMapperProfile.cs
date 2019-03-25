using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class RelationMapperProfile : IMapperProfile
    {
        public void SetMaps(Mapper mapper)
        {
            mapper.Define<IRelationType, RelationTypeDisplay>(source => new RelationTypeDisplay(), Map);
            mapper.Define<IRelation, RelationDisplay>(source => new RelationDisplay(), Map);
            mapper.Define<RelationTypeSave, IRelationType>(Map);
        }

        // Umbraco.Code.MapAll -Icon -Trashed -Alias -AdditionalData
        // Umbraco.Code.MapAll -Relations -ParentId -Notifications
        private static void Map(IRelationType source, RelationTypeDisplay target)
        {
            target.ChildObjectType = source.ChildObjectType;
            target.Id = source.Id;
            target.IsBidirectional = source.IsBidirectional;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentObjectType = source.ParentObjectType;
            target.Udi = Udi.Create(Constants.UdiEntityType.RelationType, source.Key);
            target.Path = "-1," + source.Id;

            // Set the "friendly" names for the parent and child object types
            target.ParentObjectTypeName = ObjectTypes.GetUmbracoObjectType(source.ParentObjectType).GetFriendlyName();
            target.ChildObjectTypeName = ObjectTypes.GetUmbracoObjectType(source.ChildObjectType).GetFriendlyName();
        }

        // Umbraco.Code.MapAll -ParentName -ChildName
        private static void Map(IRelation source, RelationDisplay target)
        {
            target.ChildId = source.ChildId;
            target.Comment = source.Comment;
            target.CreateDate = source.CreateDate;
            target.ParentId = source.ParentId;
        }

        // Umbraco.Code.MapAll -CreateDate -UpdateDate -DeleteDate
        private static void Map(RelationTypeSave source, IRelationType target)
        {
            target.Alias = source.Alias;
            target.ChildObjectType = source.ChildObjectType;
            target.Id = source.Id.TryConvertTo<int>().Result;
            target.IsBidirectional = source.IsBidirectional;
            target.Key = source.Key;
            target.Name = source.Name;
            target.ParentObjectType = source.ParentObjectType;
        }
    }
}
