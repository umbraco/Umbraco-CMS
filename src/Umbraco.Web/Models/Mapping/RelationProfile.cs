using AutoMapper;
using Umbraco.Core.Models;
using Relation = Umbraco.Web.Models.ContentEditing.Relation;
using RelationType = Umbraco.Web.Models.ContentEditing.RelationType;

namespace Umbraco.Web.Models.Mapping
{
    internal class RelationProfile : Profile
    {
        public RelationProfile()
        {
            //FROM IRelationType TO RelationType
            CreateMap<IRelationType, RelationType>();

            //FROM IRelation TO Relation
            CreateMap<IRelation, Relation>();
        }
    }
}
