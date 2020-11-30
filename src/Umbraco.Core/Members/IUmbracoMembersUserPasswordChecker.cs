using System.Threading.Tasks;

namespace Umbraco.Core.Members
{
    /// <summary>
    /// Used by the UmbracoMembersUserManager to check the username/password which allows for developers to more easily
    /// set the logic for this procedure.
    /// </summary>
    public interface IUmbracoMembersUserPasswordChecker
    {
        /// <summary>
        /// Checks a password for a member
        /// </summary>
        /// <param name="member"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<UmbracoMembersUserPasswordCheckerResult> CheckPasswordAsync(UmbracoMembersIdentityUser member, string password);
    }
}
