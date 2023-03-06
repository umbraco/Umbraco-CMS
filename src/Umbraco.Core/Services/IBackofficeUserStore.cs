using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Manages persistence of users.
/// </summary>
public interface IBackofficeUserStore
{
    Task<UserOperationStatus> SaveAsync(IUser user);

    Task<IUser?> GetAsync(int id);

    Task<IUser?> GetAsync(Guid key);
}
