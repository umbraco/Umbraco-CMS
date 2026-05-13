using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

/// <summary>
/// Controller for managing elements in Umbraco that are identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyElementController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementService _elementService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyElementController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to element resources.</param>
    /// <param name="elementService">Service for retrieving element data within the CMS.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    public ByKeyElementController(
        IAuthorizationService authorizationService,
        IElementService elementService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _authorizationService = authorizationService;
        _elementService = elementService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    /// <summary>
    /// Retrieves an element by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique <see cref="Guid"/> of the element to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing an <see cref="ElementResponseModel"/> if the element is found;
    /// otherwise, a 404 Not Found or 403 Forbidden error response.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ElementResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets an element.")]
    [EndpointDescription("Gets an element identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementBrowse.ActionLetter, id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IElement? element = _elementService.GetById(id);
        if (element is null)
        {
            return ContentEditingOperationStatusResult(ContentEditingOperationStatus.NotFound);
        }

        ContentScheduleCollection contentScheduleCollection = _elementService.GetContentScheduleByContentId(id);

        ElementResponseModel model = _elementPresentationFactory.CreateResponseModel(element, contentScheduleCollection);
        return Ok(model);
    }
}
