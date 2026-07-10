using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// API controller responsible for validating element update requests without persisting changes.
/// </summary>
[ApiVersion("1.0")]
public class ValidateUpdateElementController : UpdateElementControllerBase
{
    private readonly IElementEditingPresentationFactory _elementEditingPresentationFactory;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateUpdateElementController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to element update operations.</param>
    /// <param name="elementEditingPresentationFactory">Factory for creating element editing presentation models.</param>
    /// <param name="elementEditingService">Service responsible for element editing functionality.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    public ValidateUpdateElementController(
        IAuthorizationService authorizationService,
        IElementEditingPresentationFactory elementEditingPresentationFactory,
        IElementEditingService elementEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _elementEditingPresentationFactory = elementEditingPresentationFactory;
        _elementEditingService = elementEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Validates the request model for updating an element without actually updating it.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element to validate the update for.</param>
    /// <param name="requestModel">The element update request model to validate.</param>
    /// <returns>An <see cref="IActionResult"/> representing the validation result.</returns>
    [HttpPut("{id:guid}/validate")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Validates updating an element.")]
    [EndpointDescription("Validates the request model for updating an element without actually updating it.")]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken, Guid id, ValidateUpdateElementRequestModel requestModel)
        => await HandleRequest(id, requestModel, async () =>
        {
            ValidateElementUpdateModel model = _elementEditingPresentationFactory.MapValidateUpdateModel(requestModel);
            Attempt<ContentValidationResult, ContentEditingOperationStatus> result =
                await _elementEditingService.ValidateUpdateAsync(id, model, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? Ok()
                : ElementEditingOperationStatusResult(result.Status, requestModel, result.Result);
        });
}
