using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Signs;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;

public class SiblingsDocumentTypeTreeController : DocumentTypeTreeControllerBase
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SiblingsDocumentTypeTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    [ActivatorUtilitiesConstructor]
    public SiblingsDocumentTypeTreeController(IEntityService entityService, SignProviderCollection signProviders, IContentTypeService contentTypeService)
        : base(entityService, signProviders, contentTypeService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public Task<ActionResult<IEnumerable<DocumentTypeTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after) =>
        GetSiblings(target, before, after);
}
