using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;

namespace Umbraco.Cms.ManagementApi.Mapping.Relation;

public class RelationViewModelsMapDefinition : IMapDefinition
{
    private readonly IRelationService _relationService;

    public RelationViewModelsMapDefinition(IRelationService relationService)
    {
        _relationService = relationService;
    }
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

        Tuple<IUmbracoEntity, IUmbracoEntity>? entities = _relationService.GetEntitiesFromRelation(source);

        if (entities is not null)
        {
            target.ParentName = entities.Item1.Name;
            target.ChildName = entities.Item2.Name;
        }
    }
}
