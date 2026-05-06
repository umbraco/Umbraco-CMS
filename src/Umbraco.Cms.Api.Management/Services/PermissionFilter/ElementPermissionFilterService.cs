using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.PermissionFilter;

/// <summary>
/// Provides functionality to filter element entities based on the current user's permissions.
/// </summary>
internal sealed class ElementPermissionFilterService : PermissionFilterServiceBase, IElementPermissionFilterService
{
    private readonly IElementPermissionService _elementPermissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementPermissionFilterService"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to the current backoffice user's security context.</param>
    /// <param name="elementPermissionService">Service used to retrieve element permissions.</param>
    public ElementPermissionFilterService(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementPermissionService elementPermissionService)
        : base(backOfficeSecurityAccessor)
        => _elementPermissionService = elementPermissionService;

    /// <inheritdoc/>
    protected override string BrowseActionLetter(IEntitySlim entity)
        => entity.NodeObjectType == Constants.ObjectTypes.Element
            ? ActionElementBrowse.ActionLetter
            : ActionElementContainerBrowse.ActionLetter;

    /// <inheritdoc/>
    protected override Task<IEnumerable<NodePermissions>> GetPermissionsAsync(IUser user, IEnumerable<Guid> entityKeys)
        => _elementPermissionService.GetPermissionsAsync(user, entityKeys);
}
