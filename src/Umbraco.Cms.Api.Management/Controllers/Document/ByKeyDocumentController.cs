using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services.Querying;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Controller for managing documents in Umbraco that are identified by their unique key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IContentQueryService _contentQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.ByKeyDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize access to document resources.</param>
    /// <param name="documentPresentationFactory">Factory responsible for creating document presentation models.</param>
    /// <param name="contentQueryService">Service for querying content data within the CMS.</param>
    public ByKeyDocumentController(
        IAuthorizationService authorizationService,
        IDocumentPresentationFactory documentPresentationFactory,
        IContentQueryService contentQueryService)
    {
        _authorizationService = authorizationService;
        _documentPresentationFactory = documentPresentationFactory;
        _contentQueryService = contentQueryService;
    }

    /// <summary>
    /// Retrieves a document by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique <see cref="Guid"/> of the document to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="DocumentResponseModel"/> if the document is found;
    /// otherwise, a 404 Not Found or 403 Forbidden error response.
    /// </returns>
    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a document.")]
    [EndpointDescription("Gets a document identified by the provided Id.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        var contentWithScheduleAttempt = await _contentQueryService.GetWithSchedulesAsync(id);

        if (contentWithScheduleAttempt.Success == false)
        {
            return ContentQueryOperationStatusResult(contentWithScheduleAttempt.Status);
        }

        DocumentResponseModel model = await _documentPresentationFactory.CreateResponseModelAsync(
            contentWithScheduleAttempt.Result!.Content,
            contentWithScheduleAttempt.Result.Schedules);
        return Ok(model);
    }
}
