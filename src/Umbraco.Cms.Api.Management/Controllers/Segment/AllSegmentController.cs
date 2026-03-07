using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Segment;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Segment;

    /// <summary>
    /// API controller responsible for managing and retrieving all content segments within the system.
    /// </summary>
[ApiVersion("1.0")]
public class AllSegmentController : SegmentControllerBase
{
    private readonly ISegmentService _segmentService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllSegmentController"/> class with the specified segment service and Umbraco mapper.
    /// </summary>
    /// <param name="segmentService">An instance of <see cref="ISegmentService"/> used to manage segments.</param>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping objects within Umbraco.</param>
    public AllSegmentController(ISegmentService segmentService, IUmbracoMapper umbracoMapper)
    {
        _segmentService = segmentService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SegmentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets a paginated collection of segments.")]
    [EndpointDescription("Gets a paginated collection of segments with support for filtering and pagination.")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken, int skip = 0, int take = 100)
    {
        Attempt<PagedModel<Core.Models.Segment>?, SegmentOperationStatus> pagedAttempt = await _segmentService.GetPagedSegmentsAsync(skip, take);

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
}
