﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType.Folder;

[ApiVersion("1.0")]
public class CreateMediaTypeFolderController : MediaTypeFolderControllerBase
{
    public CreateMediaTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeContainerService mediaTypeContainerService)
        : base(backOfficeSecurityAccessor, mediaTypeContainerService)
    {
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateFolderRequestModel createFolderRequestModel)
        => await CreateFolderAsync<ByKeyMediaTypeFolderController>(
            createFolderRequestModel,
            controller => nameof(controller.ByKey)).ConfigureAwait(false);
}
