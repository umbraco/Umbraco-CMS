using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Relation;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class RelationPresentationFactory : IRelationPresentationFactory
{
    private readonly IRelationService _relationService;
    private readonly IEntityService _entityService;

    public RelationPresentationFactory(IRelationService relationService, IEntityService entityService)
    {
        _relationService = relationService;
        _entityService = entityService;
    }

    public RelationResponseModel Create(IRelation relation)
    {
        IEntitySlim child = _entityService.Get(relation.ChildId)!;
        IEntitySlim parent = _entityService.Get(relation.ParentId)!;

        var relationResponseModel = new RelationResponseModel(
            new ReferenceByIdModel(relation.RelationType.Key),
            new RelationReferenceModel(parent.Key),
            new RelationReferenceModel(child.Key))
        {
            Id = relation.Key,
            Comment = relation.Comment,
            CreateDate = relation.CreateDate,
        };
        Tuple<IUmbracoEntity, IUmbracoEntity>? entities = _relationService.GetEntitiesFromRelation(relation);

        if (entities is not null)
        {
            relationResponseModel.Parent.Name = entities.Item1.Name;
            relationResponseModel.Child.Name = entities.Item2.Name;
        }

        return relationResponseModel;
    }

    public IEnumerable<RelationResponseModel> CreateMultiple(IEnumerable<IRelation> relations) => relations.Select(Create);
}
