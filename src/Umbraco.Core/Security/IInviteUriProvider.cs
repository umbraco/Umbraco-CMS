using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

public interface IInviteUriProvider
{
    Task<Attempt<Uri, UserOperationStatus>> CreateInviteUriAsync(IUser invitee);
}
