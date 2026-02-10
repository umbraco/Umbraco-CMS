using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.ElementVersion;

[ApiVersion("1.0")]
public class ByKeyElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyElementVersionController(
        IElementVersionService elementVersionService,
        IUmbracoMapper umbracoMapper)
    {
        _elementVersionService = elementVersionService;
        _umbracoMapper = umbracoMapper;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ElementVersionResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets a specific element version.")]
    [EndpointDescription("Gets a specific element version by its Id. If found, the result describes the version and includes details of the element type, editor, version date, and published status.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IElement?, ContentVersionOperationStatus> attempt =
            await _elementVersionService.GetAsync(id);

        return attempt.Success
            ? Ok(_umbracoMapper.Map<ElementVersionResponseModel>(attempt.Result))
            : MapFailure(attempt.Status);
    }
}
