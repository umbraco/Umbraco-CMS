using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

/// <summary>
/// Provides API endpoints for managing temporary files identified by a unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyTemporaryFileController : TemporaryFileControllerBase
{
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IUmbracoMapper _umbracoMapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyTemporaryFileController"/> class.
    /// </summary>
    /// <param name="temporaryFileService">The service used for managing temporary files.</param>
    /// <param name="umbracoMapper">The mapper used for mapping Umbraco objects.</param>
    public ByKeyTemporaryFileController(ITemporaryFileService temporaryFileService, IUmbracoMapper umbracoMapper)
    {
        _temporaryFileService = temporaryFileService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>Retrieves a temporary file by its unique identifier.</summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the temporary file.</param>
    /// <returns>An <see cref="IActionResult"/> containing the temporary file data if found; otherwise, an error result.</returns>
    [HttpGet($"{{{nameof(id)}}}")]


    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(TemporaryFileResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a temporary file.")]
    [EndpointDescription("Gets a temporary file identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        TemporaryFileModel? model = await _temporaryFileService.GetAsync(id);
        if (model == null)
        {
            return TemporaryFileNotFound();
        }

        return Ok(_umbracoMapper.Map<TemporaryFileModel, TemporaryFileResponseModel>(model));
    }
}
