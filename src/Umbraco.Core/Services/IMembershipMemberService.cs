using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines part of the MemberService, which is specific to methods used by the membership provider.
    /// </summary>
    /// <remarks>
    /// Idea is to have this is an isolated interface so that it can be easily 'replaced' in the membership provider impl.
    /// </remarks>
    public interface IMembershipMemberService : IService
    {
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
        /// <param name="password"></param>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        IMember CreateMember(string username, string email, string password, string memberTypeAlias);

        IMember GetById(object id);

        /// <summary>
        /// Get a member by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        IMember GetByEmail(string email);

        IMember GetByUsername(string login);

        void Delete(IMember membershipUser);

        void Save(IMember membershipUser, bool raiseEvents = true);

        void Save(IEnumerable<IMember> members, bool raiseEvents = true);

        IEnumerable<IMember> FindMembersByEmail(string emailStringToMatch, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

        IEnumerable<IMember> FindMembersByUsername(string login, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

        /// <summary>
        /// Gets the total number of members based on the count type
        /// </summary>
        /// <returns></returns>
        int GetMemberCount(MemberCountType countType);

        /// <summary>
        /// Gets a list of paged member data
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetAllMembers(int pageIndex, int pageSize, out int totalRecords);
    }
}