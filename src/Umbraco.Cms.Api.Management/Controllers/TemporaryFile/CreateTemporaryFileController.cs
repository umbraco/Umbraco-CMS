using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

[ApiVersion("1.0")]
public class CreateTemporaryFileController : TemporaryFileControllerBase
{
    private readonly ITemporaryFileService _temporaryFileService;
    private readonly IUmbracoMapper _umbracoMapper;

    public CreateTemporaryFileController(ITemporaryFileService temporaryFileService, IUmbracoMapper umbracoMapper)
    {
        _temporaryFileService = temporaryFileService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpPost("")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        [FromForm] CreateTemporaryFileRequestModel model)
    {
        CreateTemporaryFileModel createModel = _umbracoMapper.Map<CreateTemporaryFileRequestModel, CreateTemporaryFileModel>(model)!;

        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result = await _temporaryFileService.CreateAsync(createModel);

        return result.Success
            ? CreatedAtId<ByKeyTemporaryFileController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : TemporaryFileStatusResult(result.Status);
    }
}

