﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

public class DeleteTemporaryFileController : TemporaryFileControllerBase
{
    private readonly ITemporaryFileService _temporaryFileService;

    public DeleteTemporaryFileController(ITemporaryFileService temporaryFileService) => _temporaryFileService = temporaryFileService;

    [HttpDelete($"{{{nameof(key)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid key)
    {
        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result = await _temporaryFileService.DeleteAsync(key);

        return result.Success
            ? Ok()
            : TemporaryFileStatusResult(result.Status);
    }
}
