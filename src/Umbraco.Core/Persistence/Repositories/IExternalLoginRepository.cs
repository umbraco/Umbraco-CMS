using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IExternalLoginRepository : IReadWriteQueryRepository<int, IIdentityUserLogin>
    {
        void Save(int userId, IEnumerable<IExternalLogin> logins);
        void DeleteUserLogins(int memberId);
    }
}
