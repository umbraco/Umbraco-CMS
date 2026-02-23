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

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class GetAuditLogElementController : ElementControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuditService _auditService;
    private readonly IAuditLogPresentationFactory _auditLogPresentationFactory;

    public GetAuditLogElementController(
        IAuthorizationService authorizationService,
        IAuditService auditService,
        IAuditLogPresentationFactory auditLogPresentationFactory)
    {
        _authorizationService = authorizationService;
        _auditService = auditService;
        _auditLogPresentationFactory = auditLogPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/audit-log")]
    [ProducesResponseType(typeof(PagedViewModel<AuditLogResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the audit log for an element.")]
    [EndpointDescription("Gets a paginated collection of audit log entries for the element identified by the provided Id.")]
    public async Task<IActionResult> GetAuditLog(CancellationToken cancellationToken, Guid id, Direction orderDirection = Direction.Descending, DateTimeOffset? sinceDate = null, int skip = 0, int take = 100)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementBrowse.ActionLetter, id),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        PagedModel<IAuditItem> result = await _auditService.GetItemsByKeyAsync(id, UmbracoObjectTypes.Element, skip, take, orderDirection, sinceDate);
        IEnumerable<AuditLogResponseModel> mapped = _auditLogPresentationFactory.CreateAuditLogViewModel(result.Items);
        var viewModel = new PagedViewModel<AuditLogResponseModel>
        {
            Total = result.Total,
            Items = mapped,
        };

        return Ok(viewModel);
    }
}
