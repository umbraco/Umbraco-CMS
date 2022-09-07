using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;

namespace Umbraco.Cms.ManagementApi.Mapping.Relation;

public class RelationViewModelsMapDefinition : IMapDefinition
{
    public void DefineMaps(IUmbracoMapper mapper)
    {
        mapper.Define<IRelation, RelationViewModel>((source, context) => new RelationViewModel(), Map);
    }

    // Umbraco.Code.MapAll -ParentName -ChildName
    private void Map(IRelation source, RelationViewModel target, MapperContext context)
    {
        target.ChildId = source.ChildId;
        target.Comment = source.Comment;
        target.CreateDate = source.CreateDate;
        target.ParentId = source.ParentId;
    }
}
