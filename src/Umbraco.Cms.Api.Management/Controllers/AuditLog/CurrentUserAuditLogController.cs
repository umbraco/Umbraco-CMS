using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.AuditLogs;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.AuditLog;

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

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<AuditLogWithUsernameViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<AuditLogWithUsernameViewModel>>> CurrentUser(Direction orderDirection = Direction.Descending, DateTime? sinceDate = null, int skip = 0, int take = 100)
    {
        // FIXME: Pull out current backoffice user when its implemented.
        // var userId = _backOfficeSecurityAccessor.BackOfficeSecurity?.GetUserId().Result ?? -1;
        var userId = Constants.Security.SuperUserId;

        IUser? user = _userService.GetUserById(userId);

        if (user is null)
        {
            throw new PanicException("Could not find current user");
        }

        PagedModel<IAuditItem> result = await _auditService.GetPagedItemsByUser(
            user.Key,
            skip,
            take,
            orderDirection,
            null,
            sinceDate);

        IEnumerable<AuditLogWithUsernameViewModel> mapped = _auditLogViewModelFactory.CreateAuditLogWithUsernameViewModels(result.Items.Skip(skip).Take(take));
        var viewModel = new PagedViewModel<AuditLogWithUsernameViewModel>
        {
            Total = result.Total,
            Items = mapped,
        };

        return Ok(viewModel);
    }
}
