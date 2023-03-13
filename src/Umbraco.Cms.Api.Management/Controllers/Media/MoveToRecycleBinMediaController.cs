﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

public class MoveToRecycleBinMediaController : MediaControllerBase
{
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public MoveToRecycleBinMediaController(IMediaEditingService mediaEditingService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaEditingService = mediaEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpDelete("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveToRecycleBin(Guid key)
    {
        Attempt<IMedia?, ContentEditingOperationStatus> result = await _mediaEditingService.MoveToRecycleBinAsync(key, CurrentUserId(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
