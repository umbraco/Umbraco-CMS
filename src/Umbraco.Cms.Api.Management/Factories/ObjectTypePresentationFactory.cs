using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Provides methods to create presentation models for object types within the management API.
/// </summary>
public class ObjectTypePresentationFactory : IObjectTypePresentationFactory
{
    private readonly IRelationService _relationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectTypePresentationFactory"/> class with the specified relation service.
    /// </summary>
    /// <param name="relationService">An instance of <see cref="IRelationService"/> to be used by the factory.</param>
    public ObjectTypePresentationFactory(IRelationService relationService)
    {
        _relationService = relationService;
    }

    /// <summary>
    /// Creates a collection of <see cref="Umbraco.Cms.Api.Management.Models.ObjectTypeResponseModel"/> instances representing the allowed object types retrieved from the relation service.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{ObjectTypeResponseModel}"/> containing response models for each allowed object type.
    /// </returns>
    public IEnumerable<ObjectTypeResponseModel> Create()
        => _relationService
            .GetAllowedObjectTypes()
            .Select(umbracoObjectType => new ObjectTypeResponseModel
            {
                Id = umbracoObjectType.GetGuid(),
                Name = umbracoObjectType.GetFriendlyName(),
            });
}
