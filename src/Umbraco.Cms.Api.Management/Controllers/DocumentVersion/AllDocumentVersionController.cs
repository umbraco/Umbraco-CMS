using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentVersion;

/// <summary>
/// API controller responsible for managing and providing access to all versions of documents within the system.
/// </summary>
[ApiVersion("1.0")]
public class AllDocumentVersionController : DocumentVersionControllerBase
{
    private readonly IContentVersionService _contentVersionService;
    private readonly IDocumentVersionPresentationFactory _documentVersionPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllDocumentVersionController"/> class.
    /// </summary>
    /// <param name="contentVersionService">The content version service.</param>
    /// <param name="documentVersionPresentationFactory">The document version presentation factory.</param>
    /// <param name="authorizationService">Service for handling authorization checks for the current user.</param>
    [ActivatorUtilitiesConstructor]
    public AllDocumentVersionController(
        IContentVersionService contentVersionService,
        IDocumentVersionPresentationFactory documentVersionPresentationFactory,
        IAuthorizationService authorizationService)
    {
        _contentVersionService = contentVersionService;
        _documentVersionPresentationFactory = documentVersionPresentationFactory;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AllDocumentVersionController"/> class.
    /// </summary>
    /// <param name="contentVersionService">The content version service.</param>
    /// <param name="documentVersionPresentationFactory">The document version presentation factory.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public AllDocumentVersionController(
        IContentVersionService contentVersionService,
        IDocumentVersionPresentationFactory documentVersionPresentationFactory)
        : this(
            contentVersionService,
            documentVersionPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Retrieves a paginated list of versions for a specified document, optionally filtered by culture.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="documentId">The unique identifier of the document whose versions are to be retrieved.</param>
    /// <param name="culture">An optional culture identifier to filter document versions by culture; if null, all cultures are included.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return in the result set (used for pagination).</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a <see cref="PagedViewModel{DocumentVersionItemResponseModel}"/> representing the paginated collection of document versions, or a problem response if the document is not found or the request is invalid.
    /// </returns>
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedViewModel<DocumentVersionItemResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets a paginated collection of versions for a specific document.")]
    [EndpointDescription("Gets a paginated collection of versions for a specific document and optional culture. Each result describes the version and includes details of the document type, editor, version date, and published status.")]
    public async Task<IActionResult> All(
        CancellationToken cancellationToken,
        [Required] Guid documentId,
        string? culture,
        int skip = 0,
        int take = 100)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, documentId),
            AuthorizationPolicies.ContentPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<PagedModel<ContentVersionMeta>?, ContentVersionOperationStatus> attempt =
            await _contentVersionService.GetPagedContentVersionsAsync(documentId, culture, skip, take);
        if (attempt.Success is false)
        {
            return MapFailure(attempt.Status);
        }

        var pagedViewModel = new PagedViewModel<DocumentVersionItemResponseModel>
        {
            Total = attempt.Result!.Total,
            Items = await _documentVersionPresentationFactory.CreateMultipleAsync(attempt.Result.Items),
        };

        return Ok(pagedViewModel);
    }
}
