using System.Collections.Generic;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IExternalLoginRepository : IReadWriteQueryRepository<int, IIdentityUserLogin>
    {
        void SaveUserLogins(int memberId, IEnumerable<IUserLoginInfo> logins);
        void DeleteUserLogins(int memberId);
    }
}
