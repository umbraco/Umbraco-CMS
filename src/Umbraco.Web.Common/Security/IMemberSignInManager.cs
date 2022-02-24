using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core.Security;


namespace Umbraco.Cms.Web.Common.Security
{
    public interface IMemberSignInManager
    {
        // TODO: We could have a base interface for these to share with IBackOfficeSignInManager
        Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);
        Task SignInAsync(MemberIdentityUser user, bool isPersistent, string authenticationMethod = null);
        Task SignOutAsync();
    }
}
