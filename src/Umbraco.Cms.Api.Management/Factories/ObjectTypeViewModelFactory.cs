using Umbraco.Cms.Api.Management.ViewModels.RelationType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class ObjectTypeViewModelFactory : IObjectTypeViewModelFactory
{
    private readonly IAllowedRelationObjectTypesService _allowedRelationObjectTypesService;

    public ObjectTypeViewModelFactory(IAllowedRelationObjectTypesService allowedRelationObjectTypesService) => _allowedRelationObjectTypesService = allowedRelationObjectTypesService;

    public IEnumerable<ObjectTypeResponseModel> Create()
        => _allowedRelationObjectTypesService
            .Get()
            .Select(umbracoObjectType => new ObjectTypeResponseModel
            {
                Id = umbracoObjectType.GetGuid(),
                Name = umbracoObjectType.GetFriendlyName(),
            });
}
