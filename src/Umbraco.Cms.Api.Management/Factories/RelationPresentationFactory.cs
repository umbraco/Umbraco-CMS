using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Relation;

namespace Umbraco.Cms.Api.Management.Factories;

public class RelationPresentationFactory : IRelationPresentationFactory
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public RelationPresentationFactory(IRelationService relationService, IUmbracoMapper umbracoMapper)
    {
        _relationService = relationService;
        _umbracoMapper = umbracoMapper;
    }

    public RelationResponseModel Create(IRelation relation)
    {
        RelationResponseModel relationResponseModel = _umbracoMapper.Map<RelationResponseModel>(relation)!;
        Tuple<IUmbracoEntity, IUmbracoEntity>? entities = _relationService.GetEntitiesFromRelation(relation);

        if (entities is not null)
        {
            relationResponseModel.ParentName = entities.Item1.Name;
            relationResponseModel.ChildName = entities.Item2.Name;
        }

        return relationResponseModel;
    }

    public IEnumerable<RelationResponseModel> CreateMultiple(IEnumerable<IRelation> relations) => relations.Select(Create);
}
