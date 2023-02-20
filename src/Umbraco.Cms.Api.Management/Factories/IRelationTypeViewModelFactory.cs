using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IRelationTypeViewModelFactory
{
    IRelationType CreateRelationType(RelationTypeSavingViewModel relationTypeSavingViewModel);

    void MapUpdateModelToRelationType(RelationTypeUpdatingViewModel relationTypeUpdatingViewModel, IRelationType target);
}
