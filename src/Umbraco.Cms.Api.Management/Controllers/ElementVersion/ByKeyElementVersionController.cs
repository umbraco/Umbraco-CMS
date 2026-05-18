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

/// <summary>
/// Controller for retrieving a specific element version by its unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyElementVersionController : ElementVersionControllerBase
{
    private readonly IElementVersionService _elementVersionService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyElementVersionController"/> class.
    /// </summary>
    /// <param name="elementVersionService">Service for managing element versions.</param>
    /// <param name="umbracoMapper">Mapper for converting domain models to view models.</param>
    public ByKeyElementVersionController(
        IElementVersionService elementVersionService,
        IUmbracoMapper umbracoMapper)
    {
        _elementVersionService = elementVersionService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    /// Retrieves a specific element version by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element version to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the element version if found.</returns>
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
