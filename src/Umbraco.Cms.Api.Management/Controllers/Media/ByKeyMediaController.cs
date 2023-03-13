﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

public class ByKeyMediaController : MediaControllerBase
{
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaPresentationModelFactory _mediaPresentationModelFactory;

    public ByKeyMediaController(IMediaEditingService mediaEditingService, IMediaPresentationModelFactory mediaPresentationModelFactory)
    {
        _mediaEditingService = mediaEditingService;
        _mediaPresentationModelFactory = mediaPresentationModelFactory;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid key)
    {
        IMedia? media = await _mediaEditingService.GetAsync(key);
        if (media == null)
        {
            return MediaNotFound();
        }

        MediaResponseModel model = await _mediaPresentationModelFactory.CreateResponseModelAsync(media);
        return Ok(model);
    }
}
