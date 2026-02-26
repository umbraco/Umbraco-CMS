using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services.PermissionFilter;

/// <summary>
/// Provides functionality to filter element entities based on the current user's permissions.
/// </summary>
internal sealed class ElementPermissionFilterService : PermissionFilterServiceBase, IElementPermissionFilterService
{
    private readonly IUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPermissionFilterService"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to the current backoffice user's security context.</param>
    /// <param name="userService">Service used to retrieve user and element permissions.</param>
    public ElementPermissionFilterService(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserService userService)
        : base(backOfficeSecurityAccessor)
        => _userService = userService;

    /// <inheritdoc/>
    protected override string BrowseActionLetter => ActionElementBrowse.ActionLetter;

    /// <inheritdoc/>
    protected override Task<Attempt<IEnumerable<NodePermissions>, UserOperationStatus>> GetPermissionsAsync(
        Guid userKey,
        HashSet<Guid> entityKeys)
        => _userService.GetElementPermissionsAsync(userKey, entityKeys);
}
