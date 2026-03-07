using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Media;

    /// <summary>
    /// Controller responsible for handling API operations related to the creation of media items.
    /// </summary>
[ApiVersion("1.0")]
public class CreateMediaController : CreateMediaControllerBase
{
    private readonly IMediaEditingPresentationFactory _mediaEditingPresentationFactory;
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMediaController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="mediaEditingPresentationFactory">Factory for creating media editing presentation models.</param>
    /// <param name="mediaEditingService">Service for handling media editing operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public CreateMediaController(
        IAuthorizationService authorizationService,
        IMediaEditingPresentationFactory mediaEditingPresentationFactory,
        IMediaEditingService mediaEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _mediaEditingPresentationFactory = mediaEditingPresentationFactory;
        _mediaEditingService = mediaEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a new media item using the details provided in the request model.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="requestModel">The model containing the details for the media to be created.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the result of the create operation.
    /// Returns <c>201 Created</c> with the created media on success, <c>400 Bad Request</c> if the request is invalid, or <c>404 Not Found</c> if the parent media is not found.
    /// </returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new media.")]
    [EndpointDescription("Creates a new media with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateMediaRequestModel requestModel)
        => await HandleRequest(requestModel.Parent?.Id, async () =>
        {
            MediaCreateModel model = _mediaEditingPresentationFactory.MapCreateModel(requestModel);
            Attempt<MediaCreateResult, ContentEditingOperationStatus> result = await _mediaEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? CreatedAtId<ByKeyMediaController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
                : MediaEditingOperationStatusResult(result.Status, requestModel, result.Result.ValidationResult);
        });
}
