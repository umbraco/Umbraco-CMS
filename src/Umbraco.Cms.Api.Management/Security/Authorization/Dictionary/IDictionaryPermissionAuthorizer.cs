using System.Security.Principal;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Dictionary;

public interface IDictionaryPermissionAuthorizer
{
    Task<bool> IsAuthorizedForCultures(IPrincipal currentUser, ISet<string> culturesToCheck);
}
