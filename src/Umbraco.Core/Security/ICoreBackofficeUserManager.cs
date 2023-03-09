using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

public interface ICoreBackofficeUserManager
{
    public Task<IdentityCreationResult> CreateAsync(UserCreateModel createModel);

    public Task<Attempt<string, UserOperationStatus>> GenerateEmailConfirmationTokenAsync(IUser user);
}
