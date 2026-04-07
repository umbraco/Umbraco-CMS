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
/// Controller responsible for managing the document blueprint tree for sibling nodes.
/// </summary>
public class SiblingsDocumentBlueprintTreeController : DocumentBlueprintTreeControllerBase
{
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SiblingsDocumentBlueprintTreeController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsDocumentBlueprintTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for document blueprints.</param>
    /// <param name="documentPresentationFactory">Factory responsible for creating document presentation models.</param>
    [ActivatorUtilitiesConstructor]
    public SiblingsDocumentBlueprintTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, flagProviders, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Retrieves sibling items of a specified document blueprint in the tree.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the document blueprint whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the target item.</param>
    /// <param name="after">The number of sibling items to include after the target item.</param>
    /// <param name="foldersOnly">If true, only folder items are included in the results.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{T}"/> of <see cref="DocumentBlueprintTreeItemResponseModel"/> representing the sibling items.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<DocumentBlueprintTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of document blueprint tree sibling items.")]
    [EndpointDescription("Gets a collection of document blueprint tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<DocumentBlueprintTreeItemResponseModel>>> Siblings(
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
