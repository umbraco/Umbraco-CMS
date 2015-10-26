using System.Threading.Tasks;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Used by the BackOfficeUserManager to check the username/password which allows for developers to more easily 
    /// set the logic for this procedure.
    /// </summary>
    public interface IBackOfficeUserPasswordChecker
    {
        Task<BackOfficeUserPasswordCheckerResult> CheckPasswordAsync(BackOfficeIdentityUser user, string password);
    }
}