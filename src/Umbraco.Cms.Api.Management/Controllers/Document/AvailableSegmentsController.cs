using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Segment;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides API endpoints for retrieving and managing available segments associated with documents.
/// </summary>
[Obsolete("This controller is temporary. A more permanent solution will follow. Scheduled for removal in Umbraco 20.")]
[ApiVersion("1.0")]
public class AvailableSegmentsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISegmentService _segmentService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvailableSegmentsController"/> class, which manages API endpoints for retrieving available document segments in Umbraco.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to controller actions.</param>
    /// <param name="segmentService">Service for retrieving and managing document segments.</param>
    /// <param name="umbracoMapper">The mapper used to convert between Umbraco domain models and API models.</param>
    public AvailableSegmentsController(
        IAuthorizationService authorizationService,
        ISegmentService segmentService,
        IUmbracoMapper umbracoMapper)
    {
        _authorizationService = authorizationService;
        _segmentService = segmentService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a paged collection of available content segments for a specified document.
    /// </summary>
    /// <param name="id">The unique identifier of the document for which to retrieve available segments.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="skip">The number of segments to skip before starting to collect the result set (used for paging).</param>
    /// <param name="take">The maximum number of segments to return (used for paging).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{SegmentResponseModel}"/> representing the paged collection of available segments.</returns>
    [HttpGet("{id:guid}/available-segment-options")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SegmentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets available segments.")]
    [EndpointDescription("Gets a collection of available content segments for the system.")]
    public async Task<IActionResult> GetAvailableSegmentOptions(
        Guid id,
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<PagedModel<Core.Models.Segment>?, SegmentOperationStatus> pagedAttempt =
            await _segmentService.GetPagedSegmentsForDocumentAsync(id, skip, take);

        if (pagedAttempt.Success is false)
        {
            return MapFailure(pagedAttempt.Status);
        }

        var viewModel = new PagedViewModel<SegmentResponseModel>
        {
            Items = _umbracoMapper.MapEnumerable<Core.Models.Segment, SegmentResponseModel>(pagedAttempt.Result!.Items),
            Total = pagedAttempt.Result!.Total,
        };

        return Ok(viewModel);
    }

    private IActionResult MapFailure(SegmentOperationStatus status)
        => OperationStatusResult(
            status,
            problemDetailsBuilder => status switch
            {
                _ => StatusCode(
                    StatusCodes.Status500InternalServerError,
                    problemDetailsBuilder
                        .WithTitle("Unknown segment operation status.")
                        .Build()),
            });
}
