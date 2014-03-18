using System.Collections.Generic;
using System.Web;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the UserService, which is an easy access to operations involving <see cref="IProfile"/> and eventually Users.
    /// </summary>
    public interface IUserService : IMembershipUserService
    {
        /// <summary>
        /// This is simply a helper method which essentially just wraps the MembershipProvider's ChangePassword method
        /// </summary>
        /// <param name="user">The user to save the password for</param>
        /// <param name="password"></param>
        /// <remarks>
        /// This method exists so that Umbraco developers can use one entry point to create/update users if they choose to.
        /// </remarks>
        void SavePassword(IUser user, string password);

        /// <summary>
        /// To permanently delete the user pass in true, otherwise they will just be disabled
        /// </summary>
        /// <param name="user"></param>
        /// <param name="deletePermanently"></param>
        void Delete(IUser user, bool deletePermanently);

        /// <summary>
        /// Gets an IProfile by User Id.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="IProfile"/></returns>
        IProfile GetProfileById(int id);

        /// <summary>
        /// Get profile by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        IProfile GetProfileByUserName(string username);
        
        /// <summary>
        /// Get user by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IUser GetUserById(int id);
        
        /// <summary>
        /// This is useful when an entire section is removed from config
        /// </summary>
        /// <param name="sectionAlias"></param>
        void DeleteSectionFromAllUsers(string sectionAlias);
        
        /// <summary>
        /// Get permissions set for user and specified node ids
        /// </summary>
        /// <param name="user"></param>
        /// <param name="nodeIds">
        /// Specifiying nothing will return all user permissions for all nodes
        /// </param>
        /// <returns></returns>
        IEnumerable<EntityPermission> GetPermissions(IUser user, params int[] nodeIds);

        /// <summary>
        /// Replaces the same permission set for a single user to any number of entities
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissions"></param>
        /// <param name="entityIds"></param>
        void ReplaceUserPermissions(int userId, IEnumerable<char> permissions, params int[] entityIds);

        #region User types

        IEnumerable<IUserType> GetAllUserTypes(params int[] ids);
        
        /// <summary>
        /// Gets an IUserType by its Alias
        /// </summary>
        /// <param name="alias">Alias of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        IUserType GetUserTypeByAlias(string alias);

        /// <summary>
        /// Gets an IUserType by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IUserType GetUserTypeById(int id);

        /// <summary>
        /// Gets an IUserType by its Name
        /// </summary>
        /// <param name="name">Name of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        IUserType GetUserTypeByName(string name);

        /// <summary>
        /// Saves an IUserType
        /// </summary>
        /// <param name="userType"></param>
        /// <param name="raiseEvents"></param>        
        void SaveUserType(IUserType userType, bool raiseEvents = true);

        /// <summary>
        /// Deletes an IUserType
        /// </summary>
        /// <param name="userType"></param>        
        void DeleteUserType(IUserType userType);

        #endregion
    }
}