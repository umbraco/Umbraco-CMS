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
/// Controller for managing the tree structure of child document blueprints.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenDocumentBlueprintTreeController : DocumentBlueprintTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenDocumentBlueprintTreeController"/> class, which manages the retrieval of child document blueprint nodes in the tree structure.
    /// </summary>
    /// <param name="entityService">The service used to interact with and retrieve entity data.</param>
    /// <param name="documentPresentationFactory">The factory responsible for creating document presentation models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ChildrenDocumentBlueprintTreeController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenDocumentBlueprintTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used to manage and retrieve entities within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document blueprints.</param>
    /// <param name="documentPresentationFactory">Factory responsible for creating document presentation models.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenDocumentBlueprintTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, flagProviders, documentPresentationFactory)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentBlueprintTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document blueprint tree child items.")]
    [EndpointDescription("Gets a paginated collection of document blueprint tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<DocumentBlueprintTreeItemResponseModel>>> Children(CancellationToken cancellationToken, Guid parentId, int skip = 0, int take = 100, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetChildren(parentId, skip, take);
    }
}
