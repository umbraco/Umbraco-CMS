using Umbraco.Cms.Core.Security.OperationStatus;

namespace Umbraco.Cms.Core.Security;

public interface IBackOfficeUserClientCredentialsManager
{
    Task<BackOfficeIdentityUser?> FindUserAsync(string clientId);

    Task<IEnumerable<string>> GetClientIdsAsync(Guid userKey);

    Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> SaveAsync(Guid userKey, string clientId, string clientSecret);

    Task<Attempt<BackOfficeUserClientCredentialsOperationStatus>> DeleteAsync(Guid userKey, string clientId);
}
