using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

public interface IForgotPasswordUriProvider
{
    Task<Attempt<Uri, UserOperationStatus>> CreateForgotPasswordUriAsync(IUser user);
}
