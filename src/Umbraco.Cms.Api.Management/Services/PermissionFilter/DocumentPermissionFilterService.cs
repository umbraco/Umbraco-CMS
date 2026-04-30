using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Services.PermissionFilter;

/// <summary>
/// Provides functionality to filter document entities based on the current user's permissions.
/// </summary>
internal sealed class DocumentPermissionFilterService : PermissionFilterServiceBase, IDocumentPermissionFilterService
{
    private readonly IContentPermissionService _contentPermissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPermissionFilterService"/> class.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to the current backoffice user's security context.</param>
    /// <param name="contentPermissionService">Service used to retrieve content permissions.</param>
    public DocumentPermissionFilterService(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentPermissionService contentPermissionService)
        : base(backOfficeSecurityAccessor)
        => _contentPermissionService = contentPermissionService;

    /// <inheritdoc/>
    protected override string BrowseActionLetter => ActionBrowse.ActionLetter;

    /// <inheritdoc/>
    protected override Task<IEnumerable<NodePermissions>> GetPermissionsAsync(IUser user, IEnumerable<Guid> entityKeys)
        => _contentPermissionService.GetPermissionsAsync(user, entityKeys);
}
