using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

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
        IEnumerable<IMember> GetMembersByGroup(string memberGroupName);
        IEnumerable<IMember> GetAllMembers(params int[] ids);
        
        //TODO: Need to get all members that start with a certain letter
    }

    /// <summary>
    /// Defines part of the MemberService, which is specific to methods used by the membership provider.
    /// </summary>
    /// <remarks>
    /// Idea is to have this is an isolated interface so that it can be easily 'replaced' in the membership provider impl.
    /// </remarks>
    internal interface IMembershipMemberService : IService
    {
        /// <summary>
        /// Checks if a member with the username exists
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        bool Exists(string username);

        IMember CreateMember(string username, string email, string password, string memberTypeAlias, int userId = 0);

        IMember GetById(object id);

        IMember GetByEmail(string email);

        IMember GetByUsername(string login);

        void Delete(IMember membershipUser);

        void Save(IMember membershipUser, bool raiseEvents = true);

        IEnumerable<IMember> FindMembersByEmail(string emailStringToMatch);
    }
}