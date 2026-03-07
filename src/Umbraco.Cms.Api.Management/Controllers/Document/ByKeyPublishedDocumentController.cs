using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides API endpoints for managing published documents identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyPublishedDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

/// <summary>
/// Initializes a new instance of the <see cref="ByKeyPublishedDocumentController"/> class, which handles published document operations by key.
/// </summary>
/// <param name="authorizationService">Service used to authorize access to document operations.</param>
/// <param name="contentEditingService">Service used for editing content.</param>
/// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    public ByKeyPublishedDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _documentPresentationFactory = documentPresentationFactory;
    }

    /// <summary>
    /// Retrieves a published document identified by the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the <see cref="PublishedDocumentResponseModel"/> if the published document is found;
    /// otherwise, returns <c>404 Not Found</c> if the document does not exist or is not published, or <c>403 Forbidden</c> if the user is not authorized.
    /// </returns>
    [HttpGet("{id:guid}/published")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PublishedDocumentResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document.")]
    [EndpointDescription("Gets a document identified by the provided Id.")]
    public async Task<IActionResult> ByKeyPublished(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IContent? content = await _contentEditingService.GetAsync(id);
        if (content == null || content.Published is false)
        {
            return DocumentNotFound();
        }

        PublishedDocumentResponseModel model = await _documentPresentationFactory.CreatePublishedResponseModelAsync(content);

        return Ok(model);
    }
}
