using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.Relation;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IRelationViewModelFactory
{
    RelationViewModel Create(IRelation relation);

    IEnumerable<RelationViewModel> CreateMultiple(IEnumerable<IRelation> relations);
}
