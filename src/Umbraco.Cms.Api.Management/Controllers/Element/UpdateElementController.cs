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
/// Controller responsible for handling update operations on elements in the management API.
/// </summary>
[ApiVersion("1.0")]
public class UpdateElementController : UpdateElementControllerBase
{
    private readonly IElementEditingPresentationFactory _elementEditingPresentationFactory;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateElementController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service for verifying user permissions.</param>
    /// <param name="elementEditingPresentationFactory">Factory for creating element editing presentation models.</param>
    /// <param name="elementEditingService">Service for managing element updates.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office user security context.</param>
    public UpdateElementController(
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

    /// <summary>Updates the specified element with new details provided in the request model.</summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element to update.</param>
    /// <param name="requestModel">The model containing the updated element details.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the update operation.</returns>
    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates an element.")]
    [EndpointDescription("Updates an element identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(CancellationToken cancellationToken, Guid id, UpdateElementRequestModel requestModel)
        => await HandleRequest(id, requestModel, async () =>
        {
            ElementUpdateModel model = _elementEditingPresentationFactory.MapUpdateModel(requestModel);
            Attempt<ElementUpdateResult, ContentEditingOperationStatus> result =
                await _elementEditingService.UpdateAsync(id, model, CurrentUserKey(_backOfficeSecurityAccessor));

            return result.Success
                ? Ok()
                : ContentEditingOperationStatusResult(result.Status);
        });
}
