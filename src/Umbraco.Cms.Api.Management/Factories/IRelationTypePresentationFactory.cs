using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IRelationTypePresentationFactory
{
    IRelationType CreateRelationType(CreateRelationTypeRequestModel createRelationTypeRequestModel);

    void MapUpdateModelToRelationType(UpdateRelationTypeRequestModel updateRelationTypeRequestModel, IRelationType target);
}
