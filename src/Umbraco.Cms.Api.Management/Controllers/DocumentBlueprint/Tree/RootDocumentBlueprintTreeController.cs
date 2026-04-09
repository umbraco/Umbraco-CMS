using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

/// <summary>
/// Provides an API controller responsible for managing the root node of the document blueprint tree in Umbraco CMS.
/// This controller enables operations related to retrieving and organizing document blueprints at the root level within the management API.
/// </summary>
[ApiVersion("1.0")]
public class RootDocumentBlueprintTreeController : DocumentBlueprintTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RootDocumentBlueprintTreeController"/> class, which manages the tree structure for root document blueprints.
    /// </summary>
    /// <param name="entityService">The service used to manage and retrieve entities within the system.</param>
    /// <param name="documentPresentationFactory">The factory responsible for creating document presentation models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public RootDocumentBlueprintTreeController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RootDocumentBlueprintTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the document blueprint tree.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document blueprints.</param>
    /// <param name="documentPresentationFactory">Factory responsible for creating document presentation models.</param>
    [ActivatorUtilitiesConstructor]
    public RootDocumentBlueprintTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, flagProviders, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of document blueprint items from the root of the tree, with optional filtering for folders only.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set.</param>
    /// <param name="take">The maximum number of items to return.</param>
    /// <param name="foldersOnly">If set to <c>true</c>, only folder items are returned.</param>
    /// <returns>A paginated view model containing document blueprint tree item response models.</returns>
    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentBlueprintTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document blueprint items from the root of the tree.")]
    [EndpointDescription("Gets a paginated collection of document blueprint items from the root of the tree with optional filtering.")]
    public async Task<ActionResult<PagedViewModel<DocumentBlueprintTreeItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100,
        bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetRoot(skip, take);
    }
}
