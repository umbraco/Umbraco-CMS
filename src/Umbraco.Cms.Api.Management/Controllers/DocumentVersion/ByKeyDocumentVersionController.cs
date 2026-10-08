using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

/// <summary>
/// Controller for managing document version operations by document key.
/// </summary>
[ApiVersion("1.0")]
public class ByKeyDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.DocumentVersion.ByKeyDocumentVersionController"/> class.
    /// </summary>
    /// <param name="contentVersionService">An instance of <see cref="IContentVersionService"/> used to manage content versions.</param>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping Umbraco objects.</param>
    /// <param name="authorizationService">Service for handling authorization checks for the current user.</param>
    [ActivatorUtilitiesConstructor]
    public ByKeyDocumentVersionController(
        IContentVersionService contentVersionService,
        IUmbracoMapper umbracoMapper,
        IAuthorizationService authorizationService)
    {
        _contentVersionService = contentVersionService;
        _umbracoMapper = umbracoMapper;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.DocumentVersion.ByKeyDocumentVersionController"/> class.
    /// </summary>
    /// <param name="contentVersionService">An instance of <see cref="IContentVersionService"/> used to manage content versions.</param>
    /// <param name="umbracoMapper">An instance of <see cref="IUmbracoMapper"/> used for mapping Umbraco objects.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ByKeyDocumentVersionController(
        IContentVersionService contentVersionService,
        IUmbracoMapper umbracoMapper)
        : this(
            contentVersionService,
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Retrieves a specific document version by its unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document version to retrieve.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing the document version details—including document type, editor, version date, and published status—if found;
    /// otherwise, a <see cref="ProblemDetails"/> response indicating why the version could not be retrieved (e.g., not found or invalid request).
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentVersionResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets a specific document version.")]
    [EndpointDescription("Gets a specific document version by its Id. If found, the result describes the version and includes details of the document type, editor, version date, and published status.")]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
    {
        Attempt<IContent?, ContentVersionOperationStatus> attempt =
            await _contentVersionService.GetAsync(id, cancellationToken);
        if (attempt.Success is false || attempt.Result is null)
        {
            return MapFailure(attempt.Status);
        }

        IContent content = attempt.Result;
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, content.Key),
            AuthorizationPolicies.ContentPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return Ok(_umbracoMapper.Map<DocumentVersionResponseModel>(content));
    }
}
