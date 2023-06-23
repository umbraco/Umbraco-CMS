using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.Relation;

namespace Umbraco.Cms.Api.Management.Mapping.Relation;

public class RelationViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IRelation, RelationResponseModel>((source, context) => new RelationResponseModel(), Map);
    }

    // Umbraco.Code.MapAll -ParentName -ChildName
    private void Map(IRelation source, RelationResponseModel target, MapperContext context)
    {
        target.ChildId = source.ChildId;
        target.Comment = source.Comment;
        target.CreateDate = source.CreateDate;
        target.ParentId = source.ParentId;
    }
}
