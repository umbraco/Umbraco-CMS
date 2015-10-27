using System.Collections.Generic;
using System.Linq;
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
        /// <remarks>
        /// This method exists so that Umbraco developers can use one entry point to create/update users if they choose to.
        /// </remarks>
        /// <param name="user">The user to save the password for</param>
        /// <param name="password">The password to save</param>
        void SavePassword(IUser user, string password);

        /// <summary>
        /// Deletes or disables a User
        /// </summary>
        /// <param name="user"><see cref="IUser"/> to delete</param>
        /// <param name="deletePermanently"><c>True</c> to permanently delete the user, <c>False</c> to disable the user</param>
        void Delete(IUser user, bool deletePermanently);

        /// <summary>
        /// Gets an IProfile by User Id.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="IProfile"/></returns>
        IProfile GetProfileById(int id);

        /// <summary>
        /// Gets a profile by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns><see cref="IProfile"/></returns>
        IProfile GetProfileByUserName(string username);
        
        /// <summary>
        /// Gets a user by Id
        /// </summary>
        /// <param name="id">Id of the user to retrieve</param>
        /// <returns><see cref="IUser"/></returns>
        IUser GetUserById(int id);
        
        /// <summary>
        /// Removes a specific section from all users
        /// </summary>
        /// <remarks>This is useful when an entire section is removed from config</remarks>
        /// <param name="sectionAlias">Alias of the section to remove</param>
        void DeleteSectionFromAllUsers(string sectionAlias);
        
        /// <summary>
        /// Add a specific section to all users or those specified as parameters
        /// </summary>
        /// <remarks>This is useful when a new section is created to allow specific users accessing it</remarks>
        /// <param name="sectionAlias">Alias of the section to add</param>
        /// <param name="userIds">Specifiying nothing will add the section to all user</param>
        void AddSectionToAllUsers(string sectionAlias, params int[] userIds);
        
        /// <summary>
        /// Get permissions set for a user and optional node ids
        /// </summary>
        /// <remarks>If no permissions are found for a particular entity then the user's default permissions will be applied</remarks>
        /// <param name="user">User to retrieve permissions for</param>
        /// <param name="nodeIds">Specifiying nothing will return all user permissions for all nodes</param>
        /// <returns>An enumerable list of <see cref="EntityPermission"/></returns>
        IEnumerable<EntityPermission> GetPermissions(IUser user, params int[] nodeIds);

        /// <summary>
        /// Replaces the same permission set for a single user to any number of entities
        /// </summary>        
        /// <param name="userId">Id of the user</param>
        /// <param name="permissions">
        /// Permissions as enumerable list of <see cref="char"/>, 
        /// if no permissions are specified then all permissions for this node are removed for this user
        /// </param>
        /// <param name="entityIds">Specify the nodes to replace permissions for. If nothing is specified all permissions are removed.</param>
        /// <remarks>If no 'entityIds' are specified all permissions will be removed for the specified user.</remarks>
        void ReplaceUserPermissions(int userId, IEnumerable<char> permissions, params int[] entityIds);

        /// <summary>
        /// Assigns the same permission set for a single user to any number of entities
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <param name="permission"></param>
        /// <param name="entityIds">Specify the nodes to replace permissions for</param>
        void AssignUserPermission(int userId, char permission, params int[] entityIds);

        #region User types

        /// <summary>
        /// Gets all UserTypes or thosed specified as parameters
        /// </summary>
        /// <param name="ids">Optional Ids of UserTypes to retrieve</param>
        /// <returns>An enumerable list of <see cref="IUserType"/></returns>
        IEnumerable<IUserType> GetAllUserTypes(params int[] ids);
        
        /// <summary>
        /// Gets a UserType by its Alias
        /// </summary>
        /// <param name="alias">Alias of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        IUserType GetUserTypeByAlias(string alias);

        /// <summary>
        /// Gets a UserType by its Id
        /// </summary>
        /// <param name="id">Id of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        IUserType GetUserTypeById(int id);

        /// <summary>
        /// Gets a UserType by its Name
        /// </summary>
        /// <param name="name">Name of the UserType to retrieve</param>
        /// <returns><see cref="IUserType"/></returns>
        IUserType GetUserTypeByName(string name);

        /// <summary>
        /// Saves a UserType
        /// </summary>
        /// <param name="userType">UserType to save</param>
        /// <param name="raiseEvents">Optional parameter to raise events. 
        /// Default is <c>True</c> otherwise set to <c>False</c> to not raise events</param>
        void SaveUserType(IUserType userType, bool raiseEvents = true);

        /// <summary>
        /// Deletes a UserType
        /// </summary>
        /// <param name="userType">UserType to delete</param>
        void DeleteUserType(IUserType userType);

        #endregion
    }
}
