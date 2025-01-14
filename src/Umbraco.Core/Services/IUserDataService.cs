using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Core.Services;

public interface IUserDataService
{
    public Task<IUserData?> GetAsync(Guid key);

    public Task<PagedModel<IUserData>> GetAsync(
        int skip,
        int take,
        IUserDataFilter? filter = null);

    public Task<Attempt<IUserData, UserDataOperationStatus>> CreateAsync(IUserData userData);

    public Task<Attempt<IUserData, UserDataOperationStatus>> UpdateAsync(IUserData userData);

    public Task<Attempt<UserDataOperationStatus>> DeleteAsync(Guid key);
}
