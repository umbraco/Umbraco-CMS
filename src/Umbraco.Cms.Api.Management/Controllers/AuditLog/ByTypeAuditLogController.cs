using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.AuditLog;

[ApiVersion("1.0")]
public class ByTypeAuditLogController : AuditLogControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogPresentationFactory _auditLogPresentationFactory;

    public ByTypeAuditLogController(IAuditService auditService, IAuditLogPresentationFactory auditLogPresentationFactory)
    {
        _auditService = auditService;
        _auditLogPresentationFactory = auditLogPresentationFactory;
    }

    [HttpGet("type/{logType}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AuditLogResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ByType(AuditType logType, DateTime? sinceDate = null, int skip = 0, int take = 100)
    {
        IAuditItem[] result = _auditService.GetLogs(logType, sinceDate).ToArray();
        IEnumerable<AuditLogResponseModel> mapped = _auditLogPresentationFactory.CreateAuditLogViewModel(result.Skip(skip).Take(take));
        var viewModel = new PagedViewModel<AuditLogResponseModel>
        {
            Total = result.Length,
            Items = mapped,
        };

        return await Task.FromResult(Ok(viewModel));
    }
}
