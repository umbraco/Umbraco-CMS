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
/// Serves as the root API controller for managing and retrieving the document type tree structure in Umbraco CMS.
/// Provides endpoints related to the root of the document type tree within the management API.
/// </summary>
[ApiVersion("1.0")]
public class RootDocumentTypeTreeController : DocumentTypeTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootDocumentTypeTreeController"/> class, which manages the root nodes of the document type tree in the Umbraco management API.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities.</param>
    /// <param name="contentTypeService">The service used to manage and retrieve content types.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public RootDocumentTypeTreeController(IEntityService entityService, IContentTypeService contentTypeService)
        : base(entityService, contentTypeService)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootDocumentTypeTreeController"/> class.
    /// </summary>
    /// <param name="entityService">An <see cref="IEntityService"/> instance used for managing entities within the system.</param>
    /// <param name="flagProviders">A <see cref="FlagProviderCollection"/> containing providers for entity flags.</param>
    /// <param name="contentTypeService">An <see cref="IContentTypeService"/> instance used for managing content types.</param>
    [ActivatorUtilitiesConstructor]
    public RootDocumentTypeTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IContentTypeService contentTypeService)
        : base(entityService, flagProviders, contentTypeService)
    {
    }

    /// <summary>
    /// Retrieves a paginated collection of document type items from the root of the tree.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="foldersOnly">If <c>true</c>, only folder items are returned; otherwise, all document type items are included.</param>
    /// <returns>A paginated collection of <see cref="DocumentTypeTreeItemResponseModel"/> items from the root of the tree.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document type items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of document type items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<DocumentTypeTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetRoot(skip, take);
    }
}
