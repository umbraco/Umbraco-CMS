using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IExternalLoginRepository : IReadWriteQueryRepository<int, IIdentityUserLogin>
    {
        [Obsolete("Use the overload specifying IIdentityUserLoginExtended instead")]
        void SaveUserLogins(int memberId, IEnumerable<UserLoginInfo> logins);
        void Save(int userId, IEnumerable<IExternalLogin> logins);
        void DeleteUserLogins(int memberId);
    }
}
