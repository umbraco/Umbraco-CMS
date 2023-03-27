using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;

namespace Umbraco.Cms.ManagementApi.Factories;

public class RelationViewModelFactory : IRelationViewModelFactory
{
    private readonly IRelationService _relationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public RelationViewModelFactory(IRelationService relationService, IUmbracoMapper umbracoMapper)
    {
        _relationService = relationService;
        _umbracoMapper = umbracoMapper;
    }

    public RelationViewModel Create(IRelation relation)
    {
        RelationViewModel relationViewModel = _umbracoMapper.Map<RelationViewModel>(relation)!;
        Tuple<IUmbracoEntity, IUmbracoEntity>? entities = _relationService.GetEntitiesFromRelation(relation);

        if (entities is not null)
        {
            relationViewModel.ParentName = entities.Item1.Name;
            relationViewModel.ChildName = entities.Item2.Name;
        }

        return relationViewModel;
    }

    public IEnumerable<RelationViewModel> CreateMultiple(IEnumerable<IRelation> relations) => relations.Select(Create);
}
