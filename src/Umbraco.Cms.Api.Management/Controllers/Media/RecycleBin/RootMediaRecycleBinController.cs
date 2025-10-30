﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media.RecycleBin;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.RecycleBin;

[ApiVersion("1.0")]
public class RootMediaRecycleBinController : MediaRecycleBinControllerBase
{
    public RootMediaRecycleBinController(IEntityService entityService, IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, mediaPresentationFactory)
    {
    }

    [HttpGet("root")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaRecycleBinItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets media at the root of the recycle bin.")]
    [EndpointDescription("Gets a paginated collection of media items at the root level of the recycle bin.")]
    public async Task<ActionResult<PagedViewModel<MediaRecycleBinItemResponseModel>>> Root(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
        => await GetRoot(skip, take);
}
