using System.Security.Claims;

namespace Umbraco.Cms.Core.Security;

public interface ICoreBackOfficeSignInManager
{
    Task<ClaimsPrincipal?> CreateUserPrincipalAsync(Guid userKey);
}
