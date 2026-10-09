using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DocumentBlueprint;

[ApiVersion("1.0")]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class GetAuditLogDocumentBlueprintController : DocumentBlueprintControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogPresentationFactory _auditLogPresentationFactory;

    public GetAuditLogDocumentBlueprintController(
        IAuditService auditService,
        IAuditLogPresentationFactory auditLogPresentationFactory)
    {
        _auditService = auditService;
        _auditLogPresentationFactory = auditLogPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/audit-log")]
    [ProducesResponseType(typeof(PagedViewModel<AuditLogResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the audit log for a document blueprint.")]
    [EndpointDescription("Gets a paginated collection of audit log entries for the document blueprint identified by the provided Id.")]
    public async Task<IActionResult> GetAuditLog(CancellationToken cancellationToken, Guid id, Direction orderDirection = Direction.Descending, DateTimeOffset? sinceDate = null, int skip = 0, int take = 100)
    {
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
