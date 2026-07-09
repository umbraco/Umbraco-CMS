using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods for creating presentation models that represent relation types in the API.
/// </summary>
public interface IRelationTypePresentationFactory
{
    /// <summary>
    /// Creates reference response models asynchronously from the given relation item models.
    /// </summary>
    /// <param name="relationItemModels">The collection of relation item models to create reference response models from.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable of reference response models.</returns>
    Task<IEnumerable<IReferenceResponseModel>> CreateReferenceResponseModelsAsync(IEnumerable<RelationItemModel> relationItemModels);
}
