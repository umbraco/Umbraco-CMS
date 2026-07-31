using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Controller responsible for handling update-and-publish operations on elements in the management API.
/// </summary>
[ApiVersion("1.0")]
public class UpdateAndPublishElementController : UpdateElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementEditingPresentationFactory _elementEditingPresentationFactory;
    private readonly IElementEditingService _elementEditingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAndPublishElementController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service for verifying user permissions to update and publish.</param>
    /// <param name="elementEditingPresentationFactory">Factory for creating element editing presentation models.</param>
    /// <param name="elementEditingService">Service for managing element updates.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for the back office user security context.</param>
    public UpdateAndPublishElementController(
        IAuthorizationService authorizationService,
        IElementEditingPresentationFactory elementEditingPresentationFactory,
        IElementEditingService elementEditingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : base(authorizationService)
    {
        _authorizationService = authorizationService;
        _elementEditingPresentationFactory = elementEditingPresentationFactory;
        _elementEditingService = elementEditingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Updates the specified element with new details provided in the request model, and subsequently publishes the element in the cultures provided.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element to update.</param>
    /// <param name="requestModel">The model containing the updated element details.</param>
    /// <returns>An <see cref="IActionResult"/> representing the outcome of the update operation.</returns>
    [HttpPut("{id:guid}/update-and-publish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates and publishes an element.")]
    [EndpointDescription("Updates and publishes an element identified by the provided Id with the details from the request model.")]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken,
        Guid id,
        UpdateAndPublishElementRequestModel requestModel)
        => await HandleRequest(id, requestModel, async () =>
        {
            // The base HandleRequest verifies the user can update the element.
            // Updating-and-publishing additionally requires publish permission, so we check that here.
            AuthorizationResult publishAuthorizationResult = await _authorizationService.AuthorizeResourceAsync(
                User,
                ElementPermissionResource.WithKeys(ActionElementPublish.ActionLetter, id, requestModel.CulturesToPublish),
                AuthorizationPolicies.ElementPermissionByResource);

            if (publishAuthorizationResult.Succeeded is false)
            {
                return Forbidden();
            }

            ElementUpdateModel model = _elementEditingPresentationFactory.MapUpdateModel(requestModel);
            Guid currentUserKey = CurrentUserKey(_backOfficeSecurityAccessor);
            Attempt<ElementUpdateResult, ContentEditingOperationStatus> result =
                await _elementEditingService.UpdateAndPublishAsync(id, model, requestModel.CulturesToPublish, currentUserKey);

            return result.Success
                ? Ok()
                : ContentEditingOperationStatusResult(result.Status);
        });
}
