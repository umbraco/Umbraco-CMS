using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services;

public interface IBackOfficeExternalLoginService
{
    Task<Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>> ExternalLoginStatusForUser(Guid userid);
}
