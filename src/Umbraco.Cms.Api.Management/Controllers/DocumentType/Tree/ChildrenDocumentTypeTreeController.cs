using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentType.Tree;

/// <summary>
/// Controller responsible for handling API requests related to the tree structure of child document types in the management section.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenDocumentTypeTreeController : DocumentTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenDocumentTypeTreeController"/> class, which manages the retrieval of child document types in the tree structure.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="contentTypeService">Service used for managing content types in the Umbraco CMS.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ChildrenDocumentTypeTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenDocumentTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">The <see cref="IEntityService"/> used to manage entities.</param>
    /// <param name="flagProviders">The <see cref="FlagProviderCollection"/> containing flag providers for the tree.</param>
    /// <param name="contentTypeService">The <see cref="IContentTypeService"/> used to manage content types.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenDocumentTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IContentTypeService contentTypeService)
        : base(entityService, flagProviders, contentTypeService)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of child document type tree items for the specified parent.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier of the parent document type tree item.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="foldersOnly">If <c>true</c>, only folder items are included in the results.</param>
    /// <returns>A paged view model containing the child document type tree items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document type tree child items.")]
    [EndpointDescription("Gets a paginated collection of document type tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<DocumentTypeTreeItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
