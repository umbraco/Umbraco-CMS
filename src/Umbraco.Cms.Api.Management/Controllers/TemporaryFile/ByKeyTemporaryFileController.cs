using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

[ApiVersion("1.0")]
public class ByKeyTemporaryFileController : TemporaryFileControllerBase
{
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IUmbracoMapper _umbracoMapper;

    public ByKeyTemporaryFileController(ITemporaryFileService temporaryFileService, IUmbracoMapper umbracoMapper)
    {
        _temporaryFileService = temporaryFileService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet($"{{{nameof(id)}}}")]


    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(TemporaryFileResponseModel), StatusCodes.Status200OK)]
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
