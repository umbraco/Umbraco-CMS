using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.MediaType;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// Controller responsible for handling update operations on media types.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class UpdateMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeEditingPresentationFactory _mediaTypeEditingPresentationFactory;
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaTypeService _mediaTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMediaTypeController"/> class, responsible for handling media type update operations.
    /// </summary>
    /// <param name="mediaTypeEditingPresentationFactory">Factory for creating media type editing presentation models.</param>
    /// <param name="mediaTypeEditingService">Service for editing media types.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="mediaTypeService">Service for managing media types.</param>
    public UpdateMediaTypeController(
        IMediaTypeEditingPresentationFactory mediaTypeEditingPresentationFactory,
        IMediaTypeEditingService mediaTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaTypeService mediaTypeService)
    {
        _mediaTypeEditingPresentationFactory = mediaTypeEditingPresentationFactory;
        _mediaTypeEditingService = mediaTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaTypeService = mediaTypeService;
    }

    /// <summary>
    /// Updates the specified media type with new details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the media type to update.</param>
    /// <param name="requestModel">The model containing the updated media type details.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the outcome of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates a media type.")]
    [EndpointDescription("Updates a media type identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateMediaTypeRequestModel requestModel)
    {
        IMediaType? mediaType = await _mediaTypeService.GetAsync(id);
        if (mediaType is null)
        {
            return OperationStatusResult(ContentTypeOperationStatus.NotFound);
        }

        MediaTypeUpdateModel model = _mediaTypeEditingPresentationFactory.MapUpdateModel(requestModel);
        Attempt<IMediaType?, ContentTypeOperationStatus> result = await _mediaTypeEditingService.UpdateAsync(mediaType, model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }
}
