using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.User.Current;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.User.Current;

[ApiVersion("1.0")]
public class GetElementPermissionsCurrentUserController : CurrentUserControllerBase
{
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUmbracoMapper _mapper;
    private readonly IElementPermissionService _elementPermissionService;

    // TODO (V20): Remove the IUserService parameter from the constructor as it is not used in the current implementation.
    [ActivatorUtilitiesConstructor]
    public GetElementPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper,
        IElementPermissionService elementPermissionService)
    {
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mapper = mapper;
        _elementPermissionService = elementPermissionService;
    }

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 20.")]
    public GetElementPermissionsCurrentUserController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService,
        IUmbracoMapper mapper)
        : this(
            backOfficeSecurityAccessor,
            userService,
            mapper,
            StaticServiceProvider.Instance.GetRequiredService<IElementPermissionService>())
    {
    }

    [MapToApiVersion("1.0")]
    [HttpGet("permissions/element")]
    [ProducesResponseType(typeof(UserPermissionsResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissions(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        IUser currentUser = CurrentUser(_backOfficeSecurityAccessor);

        // Resolve permissions through IElementPermissionService so custom implementations are respected.
        NodePermissions[] permissions = (await _elementPermissionService.GetPermissionsAsync(currentUser, ids)).ToArray();

        // Preserve 404 behavior: if any requested ID was not found, return ElementNodeNotFound.
        if (ids.Count > 0 && permissions.Length < ids.Count)
        {
            return UserOperationStatusResult(UserOperationStatus.ElementNodeNotFound);
        }

        List<UserPermissionViewModel> viewModels = _mapper.MapEnumerable<NodePermissions, UserPermissionViewModel>(permissions);

        return Ok(new UserPermissionsResponseModel { Permissions = viewModels });
    }
}
