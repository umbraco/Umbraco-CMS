using System.Collections.Generic;
using System.Web;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the UserService, which is an easy access to operations involving <see cref="IProfile"/> and eventually Users.
    /// </summary>
    internal interface IUserService : IMembershipUserService
    {
        /// <summary>
        /// Gets an IProfile by User Id.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="IProfile"/></returns>
        IProfile GetProfileById(int id);

        IProfile GetProfileByUserName(string username);
        IUser GetUserByUserName(string username);
        IUser GetUserById(int id);

        /// <summary>
        /// Gets an IUserType by its Alias
        /// </summary>
        /// <param name="alias">Alias of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        IUserType GetUserTypeByAlias(string alias);

        /// <summary>
        /// Gets an IUserType by its Name
        /// </summary>
        /// <param name="name">Name of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        IUserType GetUserTypeByName(string name);

        /// <summary>
        /// Saves changes to the user object
        /// </summary>
        /// <param name="user"></param>
        void SaveUser(IUser user);

        /// <summary>
        /// This is useful when an entire section is removed from config
        /// </summary>
        /// <param name="sectionAlias"></param>
        void DeleteSectionFromAllUsers(string sectionAlias);

        /// <summary>
        /// Returns a list of the sections that the user is allowed access to
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetUserSections(IUser user);

        IEnumerable<EntityPermission> GetPermissions(IUser user, params int[] nodeIds);
    }
}