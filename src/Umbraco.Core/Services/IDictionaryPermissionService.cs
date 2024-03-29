using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IDictionaryPermissionService
{
    /// <inheritdoc/>
    Task<DictionaryAuthorizationStatus> AuthorizeCultureAccessAsync(IUser user, ISet<string> culturesToCheck);
}
