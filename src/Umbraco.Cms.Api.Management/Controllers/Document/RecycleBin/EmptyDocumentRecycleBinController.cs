using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

/// <summary>
/// Controller responsible for emptying the document recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class EmptyDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentService _contentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyDocumentRecycleBinController"/> class, responsible for handling requests to empty the document recycle bin.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities within the system.</param>
    /// <param name="authorizationService">Service used to authorize user actions and permissions.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office user security context.</param>
    /// <param name="contentService">Service for managing content items, including deletion and retrieval.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models for API responses.</param>
    public EmptyDocumentRecycleBinController(
        IEntityService entityService,
        IAuthorizationService authorizationService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentService contentService,
        IDocumentPresentationFactory documentPresentationFactory)
        : base(entityService, documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentService = contentService;
    }

    /// <summary>
    /// Permanently deletes all documents from the document recycle bin.
    /// This action cannot be undone.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Empties the document recycle bin.")]
    [EndpointDescription("Permanently deletes all documents in the recycle bin. This operation cannot be undone.")]
    public async Task<IActionResult> EmptyRecycleBin(CancellationToken cancellationToken)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.RecycleBin(ActionDelete.ActionLetter),
            AuthorizationPolicies.ContentPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        OperationResult result = await _contentService.EmptyRecycleBinAsync(CurrentUserKey(_backOfficeSecurityAccessor));
        return result.Success
            ? Ok()
            : OperationStatusResult(result);
    }
}
