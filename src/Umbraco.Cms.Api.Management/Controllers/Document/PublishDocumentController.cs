using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.ContentPublishing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Provides API endpoints for publishing documents within the Umbraco CMS.
/// </summary>
[ApiVersion("1.0")]
public class PublishDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentPublishingService _contentPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublishDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize user actions.</param>
    /// <param name="contentPublishingService">Service responsible for publishing content.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="documentPresentationFactory">Factory for creating document presentation models.</param>
    public PublishDocumentController(
        IAuthorizationService authorizationService,
        IContentPublishingService contentPublishingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _contentPublishingService = contentPublishingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _documentPresentationFactory = documentPresentationFactory;
    }

    /// <summary>
    /// Publishes the specified document by its unique identifier, using the provided publish schedule and related data.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the document to be published.</param>
    /// <param name="requestModel">The request model containing publish schedules, cultures, and other publishing options.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> representing the result of the publish operation:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> if the document was published successfully.</description></item>
    /// <item><description><c>400 Bad Request</c> if the request is invalid or publishing fails due to validation errors.</description></item>
    /// <item><description><c>404 Not Found</c> if the document does not exist.</description></item>
    /// </list>
    /// </returns>
    [HttpPut("{id:guid}/publish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Publishes a document.")]
    [EndpointDescription("Publishes a document identified by the provided Id.")]
    public async Task<IActionResult> Publish(CancellationToken cancellationToken, Guid id, PublishDocumentRequestModel requestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionPublish.ActionLetter, id, requestModel.PublishSchedules.Where(x => x.Culture is not null).Select(x=>x.Culture!)),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<List<CulturePublishScheduleModel>, ContentPublishingOperationStatus> modelResult = _documentPresentationFactory.CreateCulturePublishScheduleModels(requestModel);

        if (modelResult.Success is false)
        {
            return DocumentPublishingOperationStatusResult(modelResult.Status);
        }

        Attempt<ContentPublishingResult, ContentPublishingOperationStatus> attempt = await _contentPublishingService.PublishAsync(
            id,
            modelResult.Result,
            CurrentUserKey(_backOfficeSecurityAccessor));
        return attempt.Success
            ? Ok()
            : DocumentPublishingOperationStatusResult(attempt.Status, invalidPropertyAliases: attempt.Result.InvalidPropertyAliases);
    }
}
