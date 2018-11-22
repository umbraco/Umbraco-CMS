using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Mapping;
using Relation = Umbraco.Web.Models.ContentEditing.Relation;
using RelationType = Umbraco.Web.Models.ContentEditing.RelationType;

namespace Umbraco.Web.Models.Mapping
{
    internal class RelationModelMapper : MapperConfiguration
    {
        public override void ConfigureMappings(IConfiguration config, ApplicationContext applicationContext)
        {
            //FROM IRelationType TO RelationType
            config.CreateMap<IRelationType, RelationType>();

            //FROM IRelation TO Relation
            config.CreateMap<IRelation, Relation>();
        }
    }
}