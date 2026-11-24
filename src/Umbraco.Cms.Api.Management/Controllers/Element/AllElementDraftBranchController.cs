using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Controllers.ElementDraftBranch;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Provides an API endpoint for retrieving all named draft branches of an element.
/// </summary>
[ApiVersion("1.0")]
public class AllElementDraftBranchController : ElementDraftBranchControllerBase
{
    private readonly IElementDraftBranchService _elementDraftBranchService;
    private readonly IElementDraftBranchPresentationFactory _elementDraftBranchPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllElementDraftBranchController"/> class.
    /// </summary>
    public AllElementDraftBranchController(
        IElementDraftBranchService elementDraftBranchService,
        IElementDraftBranchPresentationFactory elementDraftBranchPresentationFactory)
    {
        _elementDraftBranchService = elementDraftBranchService;
        _elementDraftBranchPresentationFactory = elementDraftBranchPresentationFactory;
    }

    /// <summary>
    /// Retrieves all named draft branches for an element, in creation order.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element whose draft branches are to be retrieved.</param>
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ElementDraftBranchResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets all draft branches for an element.")]
    [EndpointDescription("Gets all named draft branches for an element, in creation order.")]
    public Task<IActionResult> All(CancellationToken cancellationToken, Guid id)
    {
        IEnumerable<IContentDraftBranch> branches = _elementDraftBranchService.GetByContentKey(id);
        return Task.FromResult<IActionResult>(Ok(_elementDraftBranchPresentationFactory.CreateMultiple(branches)));
    }
}
