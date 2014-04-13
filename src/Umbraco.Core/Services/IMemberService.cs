using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{

    /// <summary>
    /// Defines the MemberService, which is an easy access to operations involving (umbraco) members.
    /// </summary>
    public interface IMemberService : IMembershipMemberService
    {
        IMember CreateMember(string username, string email, string name, string memberTypeAlias);
        IMember CreateMember(string username, string email, string name, IMemberType memberType);
        IMember CreateMemberWithIdentity(string username, string email, string name, string memberTypeAlias);
        IMember CreateMemberWithIdentity(string username, string email, string name, IMemberType memberType);
        
        /// <summary>
        /// This is simply a helper method which essentially just wraps the MembershipProvider's ChangePassword method
        /// </summary>
        /// <param name="member">The member to save the password for</param>
        /// <param name="password"></param>
        /// <remarks>
        /// This method exists so that Umbraco developers can use one entry point to create/update members if they choose to.
        /// </remarks>
        void SavePassword(IMember member, string password);

        /// <summary>
        /// Checks if a member with the id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(int id);
        
        /// <summary>
        /// Get a member by the unique key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IMember GetByKey(Guid id);

        /// <summary>
        /// Gets a member by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IMember GetById(int id);

        /// <summary>
        /// Get all members for the member type alias
        /// </summary>
        /// <param name="memberTypeAlias"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetMembersByMemberType(string memberTypeAlias);

        /// <summary>
        /// Get all members for the member type id
        /// </summary>
        /// <param name="memberTypeId"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetMembersByMemberType(int memberTypeId);

        /// <summary>
        /// Get all members in the member group name specified
        /// </summary>
        /// <param name="memberGroupName"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetMembersByGroup(string memberGroupName);

        /// <summary>
        /// Get all members with the ids specified
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetAllMembers(params int[] ids);
        
        /// <summary>
        /// Delete members of the specified member type id
        /// </summary>
        /// <param name="memberTypeId"></param>
        void DeleteMembersOfType(int memberTypeId);

        /// <summary>
        /// Find members based on their display name
        /// </summary>
        /// <param name="displayNameToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        IEnumerable<IMember> FindMembersByDisplayName(string displayNameToMatch, int pageIndex, int pageSize, out int totalRecords, StringPropertyMatchType matchType = StringPropertyMatchType.StartsWith);

        /// <summary>
        /// Get members based on a property search
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, string value, StringPropertyMatchType matchType = StringPropertyMatchType.Exact);

        /// <summary>
        /// Get members based on a property search
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, int value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact);

        /// <summary>
        /// Get members based on a property search
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, bool value);

        /// <summary>
        /// Get members based on a property search
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        /// <param name="matchType"></param>
        /// <returns></returns>
        IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, DateTime value, ValuePropertyMatchType matchType = ValuePropertyMatchType.Exact);
    }
}