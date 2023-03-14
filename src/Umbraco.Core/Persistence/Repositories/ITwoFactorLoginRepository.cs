using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface ITwoFactorLoginRepository : IReadRepository<int, ITwoFactorLogin>, IWriteRepository<ITwoFactorLogin>
{
    Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey);

    Task<bool> DeleteUserLoginsAsync(Guid userOrMemberKey, string providerName);

    Task<IEnumerable<ITwoFactorLogin>> GetByUserOrMemberKeyAsync(Guid userOrMemberKey);
}
