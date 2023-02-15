using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogController;

public class ByKeyAuditLogController : AuditLogControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogViewModelFactory _auditLogViewModelFactory;

    public ByKeyAuditLogController(IAuditService auditService, IAuditLogViewModelFactory auditLogViewModelFactory)
    {
        _auditService = auditService;
        _auditLogViewModelFactory = auditLogViewModelFactory;
    }

    [HttpGet("ByKey")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AuditLogByTypeViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<AuditLogByTypeViewModel>>> ByKey(Guid key, Direction orderDirection = Direction.Descending, DateTime? sinceDate = null, int skip = 0, int take = 100)
    {
        IEnumerable<IAuditItem> result = _auditService.GetItemsByKey(key, skip, take, out var totalRecords, orderDirection, sinceDate);
        IEnumerable<AuditLogByTypeViewModel> mapped = _auditLogViewModelFactory.CreateAuditLogViewModel(result.Skip(skip).Take(take));
        var viewModel = new PagedViewModel<AuditLogByTypeViewModel>
        {
            Total = totalRecords,
            Items = mapped,
        };

        return viewModel;
    }
}
