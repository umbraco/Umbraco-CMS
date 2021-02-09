using System.Collections.Generic;
using Umbraco.Cms.Core.Models.Identity;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IExternalLoginRepository : IReadWriteQueryRepository<int, IIdentityUserLogin>
    {
        void Save(int userId, IEnumerable<IExternalLogin> logins);
        void DeleteUserLogins(int memberId);
    }
}
