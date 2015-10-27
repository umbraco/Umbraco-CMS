using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IExternalLoginRepository : IRepositoryQueryable<int, IIdentityUserLogin>
    {
        void SaveUserLogins(int memberId, IEnumerable<UserLoginInfo> logins);
        void DeleteUserLogins(int memberId);
    }
}