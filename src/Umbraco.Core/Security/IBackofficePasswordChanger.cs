using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security;

public interface IBackofficePasswordChanger
{
    Task<Attempt<PasswordChangedModel?>> ChangeBackofficePassword(ChangeBackofficeUserPasswordModel model);
}
