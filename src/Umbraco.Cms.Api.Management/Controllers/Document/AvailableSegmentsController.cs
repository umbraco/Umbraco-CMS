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

[Obsolete("This controller is temporary and will be removed in a future release (planned for v20). A more permanent solution will follow.")]
[ApiVersion("1.0")]
public class AvailableSegmentsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ISegmentService _segmentService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AvailableSegmentsController(
        IAuthorizationService authorizationService,
        ISegmentService segmentService,
        IUmbracoMapper umbracoMapper)
    {
        _authorizationService = authorizationService;
        _segmentService = segmentService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet("{id:guid}/available-segment-options")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SegmentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
