using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.LogController;

public class CurrentUserAuditLogController : AuditLogControllerBase
{
    private readonly IAuditService _auditService;
    private readonly IAuditLogViewModelFactory _auditLogViewModelFactory;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserService _userService;

    public CurrentUserAuditLogController(
        IAuditService auditService,
        IAuditLogViewModelFactory auditLogViewModelFactory,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService)
    {
        _auditService = auditService;
        _auditLogViewModelFactory = auditLogViewModelFactory;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userService = userService;
    }

    [HttpGet("ByKey")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AuditlogViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<AuditlogViewModel>>> ByKey(Direction orderDirection = Direction.Descending, DateTime? sinceDate = null, int skip = 0, int take = 100)
    {
        var userId = _backOfficeSecurityAccessor.BackOfficeSecurity?.GetUserId().Result ?? -1;
        IUser? user = _userService.GetUserById(userId);
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);
        IEnumerable<IAuditItem> result = _auditService.GetPagedItemsByUser(
            user!.Key,
            skip,
            take,
            out var totalRecords,
            orderDirection,
            null,
            sinceDate);

        IEnumerable<AuditlogViewModel> mapped = _auditLogViewModelFactory.CreateAuditLogViewModel(result.Skip(skip).Take(take));
        var viewModel = new PagedViewModel<AuditlogViewModel>
        {
            Total = totalRecords,
            Items = mapped,
        };

        return viewModel;
    }
}
