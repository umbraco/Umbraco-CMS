using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class ObjectTypePresentationFactory : IObjectTypePresentationFactory
{
    private readonly IRelationService _relationService;

    public ObjectTypePresentationFactory(IRelationService relationService)
    {
        _relationService = relationService;
    }

    public IEnumerable<ObjectTypeResponseModel> Create()
        => _relationService
            .GetAllowedObjectTypes()
            .Select(umbracoObjectType => new ObjectTypeResponseModel
            {
                Id = umbracoObjectType.GetGuid(),
                Name = umbracoObjectType.GetFriendlyName(),
            });
}
