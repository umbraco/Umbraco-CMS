using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint.Tree;

/// <summary>
/// Controller responsible for retrieving and managing the ancestor nodes in the document blueprint tree within the Umbraco CMS management API.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsDocumentBlueprintTreeController : DocumentBlueprintTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDocumentBlueprintTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the controller.</param>
    /// <param name="documentPresentationFactory">Factory responsible for creating document presentation models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public AncestorsDocumentBlueprintTreeController(IEntityService entityService, IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsDocumentBlueprintTreeController"/> class, which manages the retrieval of ancestor document blueprint tree nodes.
    /// </summary>
    /// <param name="entityService">Service used for entity operations within the document blueprint tree.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for tree nodes.</param>
    /// <param name="documentPresentationFactory">Factory responsible for creating document presentation models.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsDocumentBlueprintTreeController(IEntityService entityService, FlagProviderCollection flagProviders, IDocumentPresentationFactory documentPresentationFactory)
    : base(entityService, flagProviders, documentPresentationFactory)
    {
    }

    /// <summary>
    /// Retrieves all ancestor document blueprint items for the specified descendant blueprint.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantId">The unique identifier of the descendant document blueprint whose ancestors are to be retrieved.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a collection of ancestor <see cref="DocumentBlueprintTreeItemResponseModel"/> items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DocumentBlueprintTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor document blueprint items.")]
    [EndpointDescription("Gets a collection of document blueprint items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<DocumentBlueprintTreeItemResponseModel>>> Ancestors(CancellationToken cancellationToken, Guid descendantId)
        => await GetAncestors(descendantId);
}
