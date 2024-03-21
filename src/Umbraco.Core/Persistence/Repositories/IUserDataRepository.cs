using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IUserDataRepository
{
    Task<IUserData?> GetAsync(Guid key);

    Task<PagedModel<IUserData>> GetAsync(int skip, int take, IUserDataFilter? filter = null);

    Task<IUserData> Save(IUserData userData);

    Task<IUserData> Update(IUserData userData);

    Task Delete(IUserData userData);
}
