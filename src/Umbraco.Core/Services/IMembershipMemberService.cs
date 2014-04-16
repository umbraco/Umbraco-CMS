using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines part of the MemberService, which is specific to methods used by the membership provider.
    /// </summary>
    /// <remarks>
    /// Idea is to have this is an isolated interface so that it can be easily 'replaced' in the membership provider impl.
    /// </remarks>
    public interface IMembershipMemberService : IMembershipMemberService<IMember>, IMembershipRoleService<IMember>
    {        
        IMember CreateMemberWithIdentity(string username, string email, IMemberType memberType);
    }

    /// <summary>
    /// Defines part of the UserService/MemberService, which is specific to methods used by the membership provider.
    /// </summary>
    /// <remarks>
    /// Idea is to have this is an isolated interface so that it can be easily 'replaced' in the membership provider impl.
    /// </remarks>
    public interface IMembershipMemberService<T> : IService
        where T : class, IMembershipUser
    {
        /// <summary>
        /// Returns the default member type alias
        /// </summary>
        /// <returns></returns>
        string GetDefaultMemberType();

        /// <summary>
        /// Checks if a member with the username exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        bool Exists(string username);

        /// <summary>
        /// Creates and persists a new member
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <param name="rawPasswordValue">
        /// This value should be the encoded/encrypted/hashed value for the password that will be stored in the database
        /// </param>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        T CreateWithIdentity(string username, string email, string rawPasswordValue, string memberTypeAlias);

        /// <summary>
        /// Gets the member by the provider key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetByProviderKey(object id);

        /// <summary>
        /// Get a member by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        T GetByEmail(string email);

        T GetByUsername(string login);

        void Delete(T membershipUser);
        
        void Save(T entity, bool raiseEvents = true);

        void Save(IEnumerable<T> entities, bool raiseEvents = true);

        IEnumerable<T> FindByEmail(string emailStringToMatch, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

        IEnumerable<T> FindByUsername(string login, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

        /// <summary>
        /// Gets the total number of members based on the count type
        /// </summary>
        /// <returns></returns>
        int GetCount(MemberCountType countType);

        /// <summary>
        /// Gets a list of paged member data
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        IEnumerable<T> GetAll(int pageIndex, int pageSize, out int totalRecords);
    }
}