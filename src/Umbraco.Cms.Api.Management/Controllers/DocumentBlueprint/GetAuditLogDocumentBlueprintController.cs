using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentBlueprints)]
public class GetAuditLogDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogPresentationFactory _auditLogPresentationFactory;
    private readonly IAuthorizationService _authorizationService;

    public GetAuditLogDocumentBlueprintController(
        IAuditService auditService,
        IAuditLogPresentationFactory auditLogPresentationFactory,
        IAuthorizationService authorizationService)
    {
        _auditService = auditService;
        _auditLogPresentationFactory = auditLogPresentationFactory;
        _authorizationService = authorizationService;
    }

    [Obsolete("Use the constructor with all parameters. Scheduled for removal in Umbraco 21.")]
    public GetAuditLogDocumentBlueprintController(
        IAuditService auditService,
        IAuditLogPresentationFactory auditLogPresentationFactory)
        : this(
            auditService,
            auditLogPresentationFactory,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/audit-log")]
    [ProducesResponseType(typeof(PagedViewModel<AuditLogResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the audit log for a document blueprint.")]
    [EndpointDescription("Gets a paginated collection of audit log entries for the document blueprint identified by the provided Id.")]
    public async Task<IActionResult> GetAuditLog(CancellationToken cancellationToken, Guid id, Direction orderDirection = Direction.Descending, DateTimeOffset? sinceDate = null, int skip = 0, int take = 100)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            DocumentBlueprintPermissionResource.WithKeys(id),
            AuthorizationPolicies.DocumentBlueprintPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        PagedModel<IAuditItem> result = await _auditService.GetItemsByKeyAsync(id, UmbracoObjectTypes.DocumentBlueprint, skip, take, orderDirection, sinceDate);
        IEnumerable<AuditLogResponseModel> mapped = _auditLogPresentationFactory.CreateAuditLogViewModel(result.Items);
        var viewModel = new PagedViewModel<AuditLogResponseModel>
        {
            Total = result.Total,
            Items = mapped,
        };

        return Ok(viewModel);
    }
}
