using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
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
    public SiblingsDocumentTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IContentTypeService contentTypeService)
        : base(entityService, flagProviders, contentTypeService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubsetViewModel<DocumentTypeTreeItemResponseModel>>> Siblings(
        CancellationToken cancellationToken,
        Guid target,
        int before,
        int after,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetSiblings(target, before, after);
    }
}
