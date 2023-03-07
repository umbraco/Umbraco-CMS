using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security;

public interface ICoreBackofficeUserManager
{
    public Task<IdentityCreationResult> CreateAsync(UserCreateModel createModel);
}
