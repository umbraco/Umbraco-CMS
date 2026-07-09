using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;

/// <summary>
/// Controller responsible for handling operations related to the ancestors tree of document types in the management API.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsDocumentTypeTreeController : DocumentTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDocumentTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for managing and retrieving entities within Umbraco.</param>
    /// <param name="contentTypeService">Service used for managing and retrieving content types in Umbraco.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public AncestorsDocumentTypeTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDocumentTypeTreeController"/> class, which manages operations related to ancestor document types in the tree structure.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of flag providers used to determine additional metadata or state for entities.</param>
    /// <param name="contentTypeService">Service used for managing content types in the CMS.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsDocumentTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IContentTypeService contentTypeService)
        : base(entityService, flagProviders, contentTypeService)
    {
    }

    /// <summary>
    /// Asynchronously retrieves the ancestor document type items for the specified descendant document type ID.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantId">The unique identifier of the descendant document type whose ancestors are to be retrieved.</param>
    /// <returns>An <see cref="ActionResult{T}"/> containing a collection of <see cref="DocumentTypeTreeItemResponseModel"/> representing the ancestor document types.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor document type items.")]
    [EndpointDescription("Gets a collection of document type items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<DocumentTypeTreeItemResponseModel>>> Ancestors(CancellationToken cancellationToken, Guid descendantId)
        => await GetAncestors(descendantId);
}
