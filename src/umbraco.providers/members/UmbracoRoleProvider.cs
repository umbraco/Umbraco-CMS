#region namespace
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Security;
using System.Configuration;
using umbraco.BusinessLogic;
using System.Security.Cryptography;
using System.Web.Util;
using System.Collections.Specialized;
using System.Configuration.Provider;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using System.Collections;
#endregion

namespace umbraco.providers.members
{
    /// <summary>
    /// A role provider for members
    /// </summary>
    [Obsolete("This has been superceded by Umbraco.Web.Security.Providers.MembersRoleProvider")]
    public class UmbracoRoleProvider : RoleProvider
    {

        #region
        private string _applicationName = Member.UmbracoRoleProviderName;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application to store and retrieve role information for.</returns>
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
        #endregion

        #region Initialization Method
        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call 
        /// <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider 
        /// after the provider has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {
            // Initialize values from web.config
            if (config == null) throw new ArgumentNullException("config");

            if (name == null || name.Length == 0) name = "UmbracoMemberRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Umbraco Member Role provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            this._applicationName = config["applicationName"];
            if (string.IsNullOrEmpty(this._applicationName))
                this._applicationName = SecUtility.GetDefaultAppName();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            ArrayList roles = new ArrayList();
            foreach (string role in roleNames)
                try
                {
                    roles.Add(MemberGroup.GetByName(role).Id);
                }
                catch
                {
                    throw new ProviderException(String.Format("No role with name '{0}' exists", role));
                }
            foreach (string username in usernames)
            {
                Member m = Member.GetMemberFromLoginName(username);
                foreach (int roleId in roles)
                    m.AddGroup(roleId);
            }
        }

        /// <summary>
        /// Adds a new role to the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            MemberGroup.MakeNew(roleName, User.GetUser(0));
        }

        /// <summary>
        /// Removes a role from the data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <param name="throwOnPopulatedRole">If true, throw an exception if roleName has one or more members and do not delete roleName.</param>
        /// <returns>
        /// true if the role was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            MemberGroup group = MemberGroup.GetByName(roleName);
            if (group == null)
                throw new ProviderException(String.Format("No role with name '{0}' exists", roleName));
            else if (throwOnPopulatedRole && group.GetMembersAsIds().Length > 0)
                throw new ProviderException(String.Format("Can't delete role '{0}', there are members assigned to the role", roleName));
            else
            {
                foreach (Member m in group.GetMembers())
                    m.RemoveGroup(group.Id);
                group.delete();
                return true;
            }
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>
        /// A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role.
        /// </returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            ArrayList members = new ArrayList();
            MemberGroup group = MemberGroup.GetByName(roleName);
            if (group == null)
                throw new ProviderException(String.Format("No role with name '{0}' exists", roleName));
            else
            {
                foreach (Member m in group.GetMembers(usernameToMatch))
                    members.Add(m.LoginName);
                return (string[])members.ToArray(typeof(string));
            }
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {
            ArrayList roles = new ArrayList();
            foreach (MemberGroup mg in MemberGroup.GetAll)
                roles.Add(mg.Text);
            return (string[])roles.ToArray(typeof(string));
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            ArrayList roles = new ArrayList();
            Member m = Member.GetMemberFromLoginName(username);
            if (m != null)
            {
                IDictionaryEnumerator ide = m.Groups.GetEnumerator();
                while (ide.MoveNext())
                    roles.Add(((MemberGroup)ide.Value).Text);
                return (string[])roles.ToArray(typeof(string));
            }
            else
                throw new ProviderException(String.Format("No member with username '{0}' exists", username));
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            ArrayList members = new ArrayList();
            MemberGroup group = MemberGroup.GetByName(roleName);
            if (group == null)
                throw new ProviderException(String.Format("No role with name '{0}' exists", roleName));
            else
            {
                foreach (Member m in group.GetMembers())
                    members.Add(m.LoginName);
                return (string[])members.ToArray(typeof(string));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            Member m = Member.GetMemberFromLoginName(username);
            if (m == null)
                throw new ProviderException(String.Format("No user with name '{0}' exists", username));
            else
            {
                MemberGroup mg = MemberGroup.GetByName(roleName);
                if (mg == null)
                    throw new ProviderException(String.Format("No Membergroup with name '{0}' exists", roleName));
                else
                    return mg.HasMember(m.Id);
            }
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            ArrayList roles = new ArrayList();
            foreach (string role in roleNames)
                try
                {
                    roles.Add(MemberGroup.GetByName(role).Id);
                }
                catch
                {
                    throw new ProviderException(String.Format("No role with name '{0}' exists", role));
                }
            foreach (string username in usernames)
            {
                Member m = Member.GetMemberFromLoginName(username);
                foreach (int roleId in roles)
                    m.RemoveGroup(roleId);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            MemberGroup mg = MemberGroup.GetByName(roleName);
            return mg != null;
        }
        #endregion

    }
}
