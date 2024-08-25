using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security;

public interface IPasswordChanger<TUser> where TUser : UmbracoIdentityUser
{
    public Task<Attempt<PasswordChangedModel?>> ChangePasswordWithIdentityAsync(
        ChangingPasswordModel passwordModel,
        IUmbracoUserManager<TUser> userMgr,
        IUser? currentUser);
}
