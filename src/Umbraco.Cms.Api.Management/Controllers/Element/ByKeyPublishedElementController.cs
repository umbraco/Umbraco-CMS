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
/// Provides API endpoints for managing published elements identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyPublishedElementController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementService _elementService;
    private readonly IElementPresentationFactory _elementPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByKeyPublishedElementController"/> class, which handles published element operations by key.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to element operations.</param>
    /// <param name="elementService">Service for retrieving element data within the CMS.</param>
    /// <param name="elementPresentationFactory">Factory for creating element presentation models.</param>
    public ByKeyPublishedElementController(
        IAuthorizationService authorizationService,
        IElementService elementService,
        IElementPresentationFactory elementPresentationFactory)
    {
        _authorizationService = authorizationService;
        _elementService = elementService;
        _elementPresentationFactory = elementPresentationFactory;
    }

    /// <summary>
    /// Retrieves a published element identified by the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the element to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the <see cref="PublishedElementResponseModel"/> if the published element is found;
    /// otherwise, returns <c>404 Not Found</c> if the element does not exist or is not published, or <c>403 Forbidden</c> if the user is not authorized.
    /// </returns>
    [HttpGet("{id:guid}/published")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PublishedElementResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a published element.")]
    [EndpointDescription("Gets a published element identified by the provided Id.")]
    public async Task<IActionResult> ByKeyPublished(CancellationToken cancellationToken, Guid id)
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
        if (element is null || element.Published is false)
        {
            return ContentEditingOperationStatusResult(ContentEditingOperationStatus.NotFound);
        }

        PublishedElementResponseModel model = await _elementPresentationFactory.CreatePublishedResponseModelAsync(element);

        return Ok(model);
    }
}
