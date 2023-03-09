using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

public class UpdateMediaController : MediaControllerBase
{
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IMediaEditingFactory _mediaEditingFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateMediaController(
        IMediaEditingService mediaEditingService,
        IMediaEditingFactory mediaEditingFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaEditingService = mediaEditingService;
        _mediaEditingFactory = mediaEditingFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid key, MediaUpdateRequestModel updateRequestModel)
    {
        IMedia? media = await _mediaEditingService.GetAsync(key);
        if (media == null)
        {
            return NotFound();
        }

        MediaUpdateModel model = _mediaEditingFactory.MapUpdateModel(updateRequestModel);
        Attempt<IMedia, ContentEditingOperationStatus> result = await _mediaEditingService.UpdateAsync(media, model, CurrentUserId(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : ContentEditingOperationStatusResult(result.Status);
    }
}
