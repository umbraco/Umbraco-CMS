using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Used to store the external login info, this can be replaced with your own implementation
    /// </summary>
    public interface IExternalLoginService : IService
    {
        /// <summary>
        /// Returns all user logins assigned
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<IIdentityUserLogin> GetAll(int userId);

        /// <summary>
        /// Returns all logins matching the login info - generally there should only be one but in some cases 
        /// there might be more than one depending on if an adminstrator has been editing/removing members
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        IEnumerable<IIdentityUserLogin> Find(UserLoginInfo login);

        /// <summary>
        /// Save user logins
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="logins"></param>
        void SaveUserLogins(int userId, IEnumerable<UserLoginInfo> logins);

        /// <summary>
        /// Deletes all user logins - normally used when a member is deleted
        /// </summary>
        /// <param name="userId"></param>
        void DeleteUserLogins(int userId);
    }
}