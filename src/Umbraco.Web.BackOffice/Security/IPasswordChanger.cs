using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security;

public interface IPasswordChanger<TUser> where TUser : UmbracoIdentityUser
{
    [Obsolete("Please use method that also takes a nullable IUser, scheduled for removal in v13")]
    public Task<Attempt<PasswordChangedModel?>> ChangePasswordWithIdentityAsync(
        ChangingPasswordModel passwordModel,
        IUmbracoUserManager<TUser> userMgr);

    public Task<Attempt<PasswordChangedModel?>> ChangePasswordWithIdentityAsync(
        ChangingPasswordModel passwordModel,
        IUmbracoUserManager<TUser> userMgr,
        IUser? currentUser) => ChangePasswordWithIdentityAsync(passwordModel, userMgr);
}
