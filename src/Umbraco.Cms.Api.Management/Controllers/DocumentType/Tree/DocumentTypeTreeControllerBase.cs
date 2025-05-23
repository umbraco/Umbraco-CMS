using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.Tree;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;

[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.DocumentType}")]
[ApiExplorerSettings(GroupName = "Document Type")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class DocumentTypeTreeControllerBase : FolderTreeControllerBase<DocumentTypeTreeItemResponseModel>
{
    private readonly IContentTypeService _contentTypeService;

    public DocumentTypeTreeControllerBase(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService) =>
        _contentTypeService = contentTypeService;

    protected override UmbracoObjectTypes ItemObjectType => UmbracoObjectTypes.DocumentType;

    protected override UmbracoObjectTypes FolderObjectType => UmbracoObjectTypes.DocumentTypeContainer;

    protected override DocumentTypeTreeItemResponseModel[] MapTreeItemViewModels(Guid? parentKey, IEntitySlim[] entities)
    {
        var contentTypes = _contentTypeService
            .GetMany(entities.Select(entity => entity.Id).ToArray())
            .ToDictionary(contentType => contentType.Id);

        return entities.Select(entity =>
        {
            DocumentTypeTreeItemResponseModel responseModel = MapTreeItemViewModel(parentKey, entity);
            if (contentTypes.TryGetValue(entity.Id, out IContentType? contentType))
            {
                responseModel.Icon = contentType.Icon ?? responseModel.Icon;
                responseModel.IsElement = contentType.IsElement;
            }

            return responseModel;
        }).ToArray();
    }
}
