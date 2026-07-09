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
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.MediaType;

/// <summary>
/// API controller responsible for handling requests to create new media types in the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessMediaTypes)]
public class CreateMediaTypeController : MediaTypeControllerBase
{
    private readonly IMediaTypeEditingPresentationFactory _mediaTypeEditingPresentationFactory;
    private readonly IMediaTypeEditingService _mediaTypeEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMediaTypeController"/> class.
    /// </summary>
    /// <param name="mediaTypeEditingPresentationFactory">Factory for creating media type editing presentation models.</param>
    /// <param name="mediaTypeEditingService">Service for editing media types.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public CreateMediaTypeController(
        IMediaTypeEditingPresentationFactory mediaTypeEditingPresentationFactory,
        IMediaTypeEditingService mediaTypeEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _mediaTypeEditingPresentationFactory = mediaTypeEditingPresentationFactory;
        _mediaTypeEditingService = mediaTypeEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Creates a new media type based on the provided configuration.
    /// </summary>
    /// <param name="cancellationToken">Token to observe while waiting for the task to complete.</param>
    /// <param name="requestModel">The model containing the configuration for the new media type.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the creation operation.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a new media type.")]
    [EndpointDescription("Creates a new media type with the configuration specified in the request model.")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateMediaTypeRequestModel requestModel)
    {
        MediaTypeCreateModel model = _mediaTypeEditingPresentationFactory.MapCreateModel(requestModel);
        Attempt<IMediaType?, ContentTypeOperationStatus> result = await _mediaTypeEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? CreatedAtId<ByKeyMediaTypeController>(controller => nameof(controller.ByKey), result.Result!.Key)
            : OperationStatusResult(result.Status);
    }
}
