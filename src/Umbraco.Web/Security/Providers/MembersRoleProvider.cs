using System;
using System.Configuration.Provider;
using System.Linq;
using System.Web.Security;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Security.Providers
{
    //TODO: Delete: should not be used
    [Obsolete("We are now using ASP.NET Core Identity instead of membership providers")]
    public class MembersRoleProvider : RoleProvider
    {
        private readonly IMembershipRoleService<IMember> _roleService;
        private string _applicationName;

        public MembersRoleProvider(IMembershipRoleService<IMember> roleService)
        {
            _roleService = roleService;
        }

        public MembersRoleProvider()
            : this(Current.Services.MemberService)
        {
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return GetRolesForUser(username).Any(x => x == roleName);
        }

        public override string[] GetRolesForUser(string username)
        {
            return _roleService.GetAllRoles(username).ToArray();
        }

        public override void CreateRole(string roleName)
        {
            _roleService.AddRole(roleName);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            return _roleService.DeleteRole(roleName, throwOnPopulatedRole);
        }

        /// <summary>
        /// Returns true if the specified member role name exists
        /// </summary>
        /// <param name="roleName">Member role name</param>
        /// <returns>True if member role exists, otherwise false</returns>
        public override bool RoleExists(string roleName) => _roleService.GetAllRoles().Any(x => x.Name == roleName);

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            _roleService.AssignRoles(usernames, roleNames);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            _roleService.DissociateRoles(usernames, roleNames);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return _roleService.GetMembersInRole(roleName).Select(x => x.Username).ToArray();
        }

        /// <summary>
        /// Gets all the member roles
        /// </summary>
        /// <returns>A list of member roles</returns>
        public override string[] GetAllRoles() => _roleService.GetAllRoles().Select(x => x.Name).ToArray();

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return _roleService.FindMembersInRole(roleName, usernameToMatch, StringPropertyMatchType.Wildcard).Select(x => x.Username).ToArray();
        }

        /// <summary>
        /// The name of the application using the custom role provider.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName
        {
            get
            {
                return _applicationName;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ProviderException("ApplicationName cannot be empty.");

                if (value.Length > 0x100)
                    throw new ProviderException("Provider application name too long.");

                _applicationName = value;
            }
        }
    }
}
