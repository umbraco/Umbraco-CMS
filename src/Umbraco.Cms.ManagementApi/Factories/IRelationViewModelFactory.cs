using Umbraco.Cms.Core.Models;
using Umbraco.Cms.ManagementApi.ViewModels.Relation;

namespace Umbraco.Cms.ManagementApi.Factories;

public interface IRelationViewModelFactory
{
    RelationViewModel Create(IRelation relation);

    IEnumerable<RelationViewModel> CreateMultiple(IEnumerable<IRelation> relations);
}
