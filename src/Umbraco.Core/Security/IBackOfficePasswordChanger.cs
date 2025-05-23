using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security;

public interface IBackOfficePasswordChanger
{
    Task<Attempt<PasswordChangedModel?>> ChangeBackOfficePassword(ChangeBackOfficeUserPasswordModel model, IUser? performingUser);
}
