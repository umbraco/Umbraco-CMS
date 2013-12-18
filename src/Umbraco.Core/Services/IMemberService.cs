using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the MemberService, which is an easy access to operations involving (umbraco) members.
    /// </summary>
    internal interface IMemberService : IMembershipMemberService
    {
        /// <summary>
        /// Checks if a member with the id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(int id);

        IMember GetById(int id);
        IMember GetByKey(Guid id);
        IEnumerable<IMember> GetMembersByMemberType(string memberTypeAlias);
        IEnumerable<IMember> GetMembersByMemberType(int memberTypeId);
        IEnumerable<IMember> GetMembersByGroup(string memberGroupName);
        IEnumerable<IMember> GetAllMembers(params int[] ids);
        
        void DeleteMembersOfType(int memberTypeId);

        IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, string value, StringPropertyMatchType matchType = StringPropertyMatchType.Exact);
        IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, int value);
        IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, bool value);
        IEnumerable<IMember> GetMembersByPropertyValue(string propertyTypeAlias, DateTime value);
    }
}