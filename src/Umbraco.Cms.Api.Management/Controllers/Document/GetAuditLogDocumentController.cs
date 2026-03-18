using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

/// <summary>
/// Controller responsible for retrieving the audit log of a document.
/// </summary>
[ApiVersion("1.0")]
public class GetAuditLogDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuditService _auditService;
    private readonly IAuditLogPresentationFactory _auditLogPresentationFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Controllers.Document.GetAuditLogDocumentController"/> class.
    /// </summary>
    /// <param name="authorizationService">Service for handling authorization and access control.</param>
    /// <param name="auditService">Service for retrieving audit log entries.</param>
    /// <param name="auditLogPresentationFactory">Factory for creating audit log presentation models.</param>
    public GetAuditLogDocumentController(
        IAuthorizationService authorizationService,
        IAuditService auditService,
        IAuditLogPresentationFactory auditLogPresentationFactory)
    {
        _authorizationService = authorizationService;
        _auditService = auditService;
        _auditLogPresentationFactory = auditLogPresentationFactory;
    }

    /// <summary>
    /// Retrieves a paginated list of audit log entries for the specified document.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the document whose audit log is requested.</param>
    /// <param name="orderDirection">The sort direction for the audit log entries (ascending or descending).</param>
    /// <param name="sinceDate">An optional date; only audit log entries created on or after this date are included.</param>
    /// <param name="skip">The number of entries to skip (for pagination).</param>
    /// <param name="take">The maximum number of entries to return (for pagination).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paged view model of audit log entries.</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/audit-log")]
    [ProducesResponseType(typeof(PagedViewModel<AuditLogResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the audit log for a document.")]
    [EndpointDescription("Gets a paginated collection of audit log entries for the document identified by the provided Id.")]
    public async Task<IActionResult> GetAuditLog(CancellationToken cancellationToken, Guid id, Direction orderDirection = Direction.Descending, DateTimeOffset? sinceDate = null, int skip = 0, int take = 100)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        PagedModel<IAuditItem> result = await _auditService.GetItemsByKeyAsync(id, UmbracoObjectTypes.Document, skip, take, orderDirection, sinceDate);
        IEnumerable<AuditLogResponseModel> mapped = _auditLogPresentationFactory.CreateAuditLogViewModel(result.Items);
        var viewModel = new PagedViewModel<AuditLogResponseModel>
        {
            Total = result.Total,
            Items = mapped,
        };

        return Ok(viewModel);
    }
}
