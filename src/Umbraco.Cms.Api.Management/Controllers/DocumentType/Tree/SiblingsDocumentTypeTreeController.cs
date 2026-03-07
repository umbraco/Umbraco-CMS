using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;

    /// <summary>
    /// Provides API endpoints for retrieving and managing sibling document types in the document type tree.
    /// </summary>
public class SiblingsDocumentTypeTreeController : DocumentTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsDocumentTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="contentTypeService">Service used for managing content types in the Umbraco CMS.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SiblingsDocumentTypeTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsDocumentTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the controller.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document type tree nodes.</param>
    /// <param name="contentTypeService">Service used for managing content types.</param>
    [ActivatorUtilitiesConstructor]
    public SiblingsDocumentTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IContentTypeService contentTypeService)
        : base(entityService, flagProviders, contentTypeService)
    {
    }

    /// <summary>Gets a collection of document type tree sibling items.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="target">The target document type ID to find siblings for.</param>
    /// <param name="before">The number of sibling items to retrieve before the target.</param>
    /// <param name="after">The number of sibling items to retrieve after the target.</param>
    /// <param name="foldersOnly">Whether to include only folder items.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ActionResult with a subset view model of document type tree item responses.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document type tree sibling items.")]
    [EndpointDescription("Gets a collection of document type tree items that are siblings of the provided Id.")]
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
