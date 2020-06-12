using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Security.Providers;

namespace Umbraco.Web.UI.Poc
{
    
    public class MarkusRoleProvider : RoleProvider, IRoleProviderWithDisplayNames
    {
        public RoleDisplay[] GetAllRolesWithDisplayName()
        {
            return new[] {new RoleDisplay()
                {
                    Icon = "icon-users-alt",
                    Id = "CustomRole1",
                    Name = "Custom Role 1"
                },
                new RoleDisplay()
                {
                    Icon = "icon-users-alt",
                    Id = "CustomRole2",
                    Name = "Custom Role 2"
                },
                new RoleDisplay()
                {
                    Icon = "icon-users-alt",
                    Id = "CustomRole3",
                    Name = "Custom Role 3"
                }
            };
        }

        public override string[] GetAllRoles()
        {
            return this.GetAllRolesWithDisplayName().Select(x => x.Id).ToArray();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName { get; set; }
        
    }
}
