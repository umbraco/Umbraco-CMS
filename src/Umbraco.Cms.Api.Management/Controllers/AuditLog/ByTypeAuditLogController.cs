using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogController;

public class ByTypeAuditLogController : AuditLogControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogViewModelFactory _auditLogViewModelFactory;

    public ByTypeAuditLogController(IAuditService auditService, IAuditLogViewModelFactory auditLogViewModelFactory)
    {
        _auditService = auditService;
        _auditLogViewModelFactory = auditLogViewModelFactory;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AuditLogByTypeViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<AuditLogByTypeViewModel>>> ByType(AuditType logType, DateTime? sinceDate = null, int skip = 0, int take = 100)
    {
        IEnumerable<IAuditItem> result = _auditService.GetLogs(logType, sinceDate);
        IEnumerable<AuditLogByTypeViewModel> mapped = _auditLogViewModelFactory.CreateAuditLogByTypeViewModel(result.Skip(skip).Take(take));
        var viewModel = new PagedViewModel<AuditLogByTypeViewModel>
        {
            Total = result.Count(),
            Items = mapped,
        };

        return viewModel;
    }
}
