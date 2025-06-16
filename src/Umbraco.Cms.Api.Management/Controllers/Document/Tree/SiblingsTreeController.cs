using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Tree;

[ApiVersion("1.0")]
public class SiblingsTreeController : DocumentTreeControllerBase
{
    public SiblingsTreeController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        IPublicAccessService publicAccessService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory)
        : base(
            entityService,
            userStartNodeEntitiesService,
            dataTypeService,
            publicAccessService,
            appCaches,
            backofficeSecurityAccessor,
            documentPresentationFactory)
    {
    }

    [HttpGet("siblings")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<IEnumerable<DocumentTreeItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid siblingId, int before, int after)
    {
        IEntitySlim[] siblings = EntityService.GetSiblings(siblingId, UmbracoObjectTypes.Document, before, after).ToArray();
        Guid? parentKey = siblings.FirstOrDefault()?.Key;
        if (parentKey is null)
        {
            return Task.FromResult<ActionResult<IEnumerable<DocumentTreeItemResponseModel>>>(NotFound());
        }

        DocumentTreeItemResponseModel[] treeItemsViewModels = MapTreeItemViewModels(parentKey, siblings);
        return Task.FromResult<ActionResult<IEnumerable<DocumentTreeItemResponseModel>>>(treeItemsViewModels);
    }
}
