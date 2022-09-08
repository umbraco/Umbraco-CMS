using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Web.Common.Security;

public interface IPasswordChanger<TUser> where TUser : UmbracoIdentityUser
{
    public Task<Attempt<PasswordChangedModel?>> ChangePasswordWithIdentityAsync(
        ChangingPasswordModel passwordModel,
        IUmbracoUserManager<TUser> userMgr);
}
