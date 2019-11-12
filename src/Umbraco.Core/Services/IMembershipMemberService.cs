using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines part of the MemberService, which is specific to methods used by the membership provider.
    /// </summary>
    /// <remarks>
    /// Idea is to have this as an isolated interface so that it can be easily 'replaced' in the membership provider implementation.
    /// </remarks>
    public interface IMembershipMemberService : IMembershipMemberService<IMember>, IMembershipRoleService<IMember>
    {
        /// <summary>
        /// Creates and persists a new Member
        /// </summary>
        /// <param name="username">Username of the Member to create</param>
        /// <param name="email">Email of the Member to create</param>
        /// <param name="memberType"><see cref="IMemberType"/> which the Member should be based on</param>
        /// <returns><see cref="IMember"/></returns>
        IMember CreateMemberWithIdentity(string username, string email, IMemberType memberType);
    }

    
}
