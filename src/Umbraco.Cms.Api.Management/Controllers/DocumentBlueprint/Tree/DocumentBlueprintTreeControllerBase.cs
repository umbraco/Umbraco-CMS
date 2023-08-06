using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DocumentBlueprint}")]
[ApiExplorerSettings(GroupName = "Document Blueprint")]
[Authorize(Policy = "New" + AuthorizationPolicies.SectionAccessContent)]
public class DocumentBlueprintTreeControllerBase : EntityTreeControllerBase<DocumentBlueprintTreeItemResponseModel>
{
    private readonly IContentTypeService _contentTypeService;

    public DocumentBlueprintTreeControllerBase(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService) =>
        _contentTypeService = contentTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DocumentBlueprint;

    protected override DocumentBlueprintTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentId, IEntitySlim[] entities)
    {
        var contentTypeAliases = entities
            .OfType<IDocumentEntitySlim>()
            .Select(entity => entity.ContentTypeAlias)
            .ToArray();

        var contentTypeIds = _contentTypeService.GetAllContentTypeIds(contentTypeAliases).ToArray();
        var contentTypeByAlias = _contentTypeService
            .GetAll(contentTypeIds)
            .ToDictionary(contentType => contentType.Alias);

        return entities.Select(entity =>
        {
            DocumentBlueprintTreeItemResponseModel responseModel = base.MapTreeItemViewModel(parentId, entity);
            responseModel.HasChildren = false;

            if (entity is IDocumentEntitySlim documentEntitySlim
                && contentTypeByAlias.TryGetValue(documentEntitySlim.ContentTypeAlias, out IContentType? contentType))
            {
                responseModel.DocumentTypeId = contentType.Key;
                responseModel.DocumentTypeAlias = contentType.Alias;
                responseModel.DocumentTypeName = contentType.Name;
            }

            return responseModel;
        }).ToArray();
    }
}
