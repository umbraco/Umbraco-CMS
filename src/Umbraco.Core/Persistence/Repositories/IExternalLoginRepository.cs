using System.Collections.Generic;
using Umbraco.Cms.Core.Models.Identity;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface IExternalLoginRepository : IReadWriteQueryRepository<int, IIdentityUserLogin>, IQueryRepository<IIdentityUserToken>
    {
        /// <summary>
        /// Replaces all external login providers for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="logins"></param>
        void Save(int userId, IEnumerable<IExternalLogin> logins);

        /// <summary>
        /// Replaces all external login provider tokens for the providers specified for the user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tokens"></param>
        void Save(int userId, IEnumerable<IExternalLoginToken> tokens);

        void DeleteUserLogins(int memberId);
    }
}
