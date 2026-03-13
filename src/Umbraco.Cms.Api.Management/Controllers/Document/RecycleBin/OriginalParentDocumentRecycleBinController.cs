using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document.RecycleBin;

/// <summary>
/// Controller responsible for managing operations related to the recycle bin for original parent documents in the CMS.
/// </summary>
[ApiVersion("1.0")]
public class OriginalParentDocumentRecycleBinController : DocumentRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IDocumentRecycleBinQueryService _documentRecycleBinQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OriginalParentDocumentRecycleBinController"/> class.
    /// Handles operations related to retrieving the original parent of documents in the recycle bin.
    /// </summary>
    /// <param name="entityService">The service used for entity operations.</param>
    /// <param name="authorizationService">The service used to authorize user actions.</param>
    /// <param name="documentPresentationFactory">The factory for creating document presentation models.</param>
    /// <param name="documentRecycleBinQueryService">The service for querying the document recycle bin.</param>
    public OriginalParentDocumentRecycleBinController(
        IEntityService entityService,
        IAuthorizationService authorizationService,
        IDocumentPresentationFactory documentPresentationFactory,
        IDocumentRecycleBinQueryService documentRecycleBinQueryService)
        : base(entityService, documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _documentPresentationFactory = documentPresentationFactory;
        _documentRecycleBinQueryService = documentRecycleBinQueryService;
    }

    /// <summary>
    /// Retrieves the original parent of a document before it was moved to the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing a <see cref="ReferenceByIdModel"/> for the original parent, or <c>null</c> if the parent is the root.
    /// </returns>
    [HttpGet("{id:guid}/original-parent")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ReferenceByIdModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets the original parent of a document in the recycle bin.")]
    [EndpointDescription("Gets the original parent location of a document before it was moved to the recycle bin.")]
    public async Task<IActionResult> OriginalParent(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.RecycleBin(ActionBrowse.ActionLetter),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType> getParentAttempt = await _documentRecycleBinQueryService.GetOriginalParentAsync(id);
        return getParentAttempt.Success switch
        {
            true when getParentAttempt.Status == RecycleBinQueryResultType.Success
                => Ok(new ReferenceByIdModel(getParentAttempt.Result!.Key)),
            true when getParentAttempt.Status == RecycleBinQueryResultType.ParentIsRoot
                => Ok(null),
            _ => MapAttemptFailure(getParentAttempt.Status),
        };
    }

    private IActionResult MapAttemptFailure(RecycleBinQueryResultType status)
        => MapRecycleBinQueryAttemptFailure(status, "document");
}
