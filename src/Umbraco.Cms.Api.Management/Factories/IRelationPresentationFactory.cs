using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Api.Management.ViewModels.Relation;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IRelationPresentationFactory
{
    RelationResponseModel Create(IRelation relation);

    IEnumerable<RelationResponseModel> CreateMultiple(IEnumerable<IRelation> relations);
}
