using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security.Authorization;

public interface IContextualPermissionAuthorizer
{
    bool IsDenied(IUser currentUser, ContextualPermissionResource resource);
}
