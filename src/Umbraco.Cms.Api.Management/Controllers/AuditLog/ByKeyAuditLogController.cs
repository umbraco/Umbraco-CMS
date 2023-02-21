using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.AuditLog;

public class ByKeyAuditLogController : AuditLogControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogViewModelFactory _auditLogViewModelFactory;

    public ByKeyAuditLogController(IAuditService auditService, IAuditLogViewModelFactory auditLogViewModelFactory)
    {
        _auditService = auditService;
        _auditLogViewModelFactory = auditLogViewModelFactory;
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AuditlogViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByKey(Guid key, Direction orderDirection = Direction.Descending, DateTime? sinceDate = null, int skip = 0, int take = 100)
    {
        PagedModel<IAuditItem> result = await _auditService.GetItemsByKeyAsync(key, skip, take, orderDirection, sinceDate);
        IEnumerable<AuditlogViewModel> mapped = _auditLogViewModelFactory.CreateAuditLogViewModel(result.Items);
        var viewModel = new PagedViewModel<AuditlogViewModel>
        {
            Total = result.Total,
            Items = mapped,
        };

        return Ok(viewModel);
    }
}
