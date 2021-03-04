using System.Collections.Generic;
using Umbraco.Cms.Core.Models.Identity;

namespace Umbraco.Cms.Core.Services
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
        /// there might be more than one depending on if an administrator has been editing/removing members
        /// </summary>
        /// <param name="loginProvider"></param>
        /// <param name="providerKey"></param>
        /// <returns></returns>
        IEnumerable<IIdentityUserLogin> Find(string loginProvider, string providerKey);

        /// <summary>
        /// Saves the external logins associated with the user
        /// </summary>
        /// <param name="userId">
        /// The user associated with the logins
        /// </param>
        /// <param name="logins"></param>
        /// <remarks>
        /// This will replace all external login provider information for the user
        /// </remarks>
        void Save(int userId, IEnumerable<IExternalLogin> logins);

        /// <summary>
        /// Save a single external login record
        /// </summary>
        /// <param name="login"></param>
        void Save(IIdentityUserLogin login);

        /// <summary>
        /// Deletes all user logins - normally used when a member is deleted
        /// </summary>
        /// <param name="userId"></param>
        void DeleteUserLogins(int userId);
    }
}
