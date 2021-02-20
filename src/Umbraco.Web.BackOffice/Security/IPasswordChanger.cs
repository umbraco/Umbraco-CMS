using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Identity;
using Umbraco.Infrastructure.Security;
using Umbraco.Web.Models;

namespace Umbraco.Web.BackOffice.Security
{
    public interface IPasswordChanger<TUser> where TUser : UmbracoIdentityUser
    {
        public Task<Attempt<PasswordChangedModel>> ChangePasswordWithIdentityAsync(
            ChangingPasswordModel passwordModel,
            IUmbracoUserManager<TUser> userMgr);
    }
}
