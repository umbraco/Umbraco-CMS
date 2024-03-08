using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security.Authorization;

public interface IDictionaryPermissionAuthorizer
{
    Task<bool> IsAuthorizedForCultures(IUser currentUser, ISet<string> culturesToCheck);
}
