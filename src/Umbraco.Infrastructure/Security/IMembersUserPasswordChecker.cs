using System.Threading.Tasks;
using Umbraco.Core.Security;

namespace Umbraco.Infrastructure.Security
{
    /// <summary>
    /// Used by the MembersUserManager to check the username/password which allows for developers to more easily
    /// set the logic for this procedure.
    /// </summary>
    public interface IMembersUserPasswordChecker
    {
        /// <summary>
        /// Checks a password for a member
        /// </summary>
        /// <remarks>
        /// TODO: what should our implementation be for members?
        /// </remarks>
        Task<MembersUserPasswordCheckerResult> CheckPasswordAsync(MembersIdentityUser user, string password);
    }
}
