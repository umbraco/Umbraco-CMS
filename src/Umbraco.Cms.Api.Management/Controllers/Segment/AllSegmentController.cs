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

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class AllSegmentController : SegmentControllerBase
{
    private readonly ISegmentService _segmentService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllSegmentController(ISegmentService segmentService, IUmbracoMapper umbracoMapper)
    {
        _segmentService = segmentService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SegmentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
