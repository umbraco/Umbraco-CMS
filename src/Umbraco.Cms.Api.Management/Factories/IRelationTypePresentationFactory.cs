using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IRelationTypePresentationFactory
{
    Task<IEnumerable<IReferenceResponseModel>> CreateReferenceResponseModelsAsync(IEnumerable<RelationItemModel> relationItemModels);
}
