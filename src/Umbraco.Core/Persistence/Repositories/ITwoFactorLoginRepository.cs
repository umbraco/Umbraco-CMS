using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories
{
    public interface ITwoFactorLoginRepository: IAsyncReadRepository<int, ITwoFactorLogin>, IAsyncWriteRepository<ITwoFactorLogin>
    {
        Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey);
        Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey, string providerName);

        Task<IEnumerable<ITwoFactorLogin>> GetByUserOrMemberKeyAsync(Guid userOrMemberKey);
    }

}
