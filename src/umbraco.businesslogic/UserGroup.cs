using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Events;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Represents a umbraco Usergroup
    /// </summary>
    [Obsolete("Use the UserService instead")]
    public class UserGroup
    {
        internal Umbraco.Core.Models.Membership.IUserGroup UserGroupItem;

        /// <summary>
        /// Creates a new empty instance of a UserGroup
        /// </summary>
        public UserGroup()
        {
            UserGroupItem = new Umbraco.Core.Models.Membership.UserGroup();
        }

        internal UserGroup(Umbraco.Core.Models.Membership.IUserGroup userGroup)
        {
            UserGroupItem = userGroup;
        }

        /// <summary>
        /// Creates a new instance of a UserGroup and attempts to 
        /// load it's values from the database cache.
        /// </summary>
        /// <remarks>
        /// If the UserGroup is not found in the existing ID list, then this object 
        /// will remain an empty object
        /// </remarks>
        /// <param name="id">The UserGroup id to find</param>
        public UserGroup(int id)
        {
            this.LoadByPrimaryKey(id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserGroup"/> class.
        /// </summary>
        /// <param name="id">The user type id.</param>
        /// <param name="name">The name.</param>
        public UserGroup(int id, string name)
        {
            UserGroupItem = new Umbraco.Core.Models.Membership.UserGroup();
            UserGroupItem.Id = id;
            UserGroupItem.Name = name;
        }

        /// <summary>
        /// Creates a new instance of UserGroup with all parameters
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="defaultPermissions"></param>
        /// <param name="alias"></param>
        public UserGroup(int id, string name, string defaultPermissions, string alias)
        {
            UserGroupItem = new Umbraco.Core.Models.Membership.UserGroup();
            UserGroupItem.Id = id;
            UserGroupItem.Name = name;
            UserGroupItem.Alias = alias;
            UserGroupItem.Permissions = defaultPermissions.ToCharArray().Select(x => x.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The cache storage for all user groups
        /// </summary>
        private static List<UserGroup> UserGroups
        {
            get
            {
                return ApplicationContext.Current.Services.UserService.GetAllUserGroups()
                    .Select(x => new UserGroup(x))
                    .ToList();
            }
        }

        #region Public Properties
        /// <summary>
        /// Gets or sets the user type alias.
        /// </summary>
        public string Alias
        {
            get { return UserGroupItem.Alias; }
            set { UserGroupItem.Alias = value; }
        }

        /// <summary>
        /// Gets the name of the user type.
        /// </summary>
        public string Name
        {
            get { return UserGroupItem.Name; }
            set { UserGroupItem.Name = value; }
        }

        /// <summary>
        /// Gets the id the user group
        /// </summary>
        public int Id
        {
            get { return UserGroupItem.Id; }
        }

        /// <summary>
        /// Gets the default permissions of the user group
        /// </summary>
        public string DefaultPermissions
        {
            get { return UserGroupItem.Permissions == null ? string.Empty : string.Join("", UserGroupItem.Permissions); }
            set { UserGroupItem.Permissions = value.ToCharArray().Select(x => x.ToString(CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// Gets the applications which the group has access to.
        /// </summary>
        /// <value>The users applications.</value>
        public Application[] Applications
        {
            get
            {
                return GetApplications().ToArray();
            }
        }

        /// <summary>
        /// Get the application which the group has access to as a List
        /// </summary>
        /// <returns></returns>
        public List<Application> GetApplications()
        {
            var allApps = Application.getAll();
            var apps = new List<Application>();

            var sections = UserGroupItem.AllowedSections;

            foreach (var s in sections)
            {
                var app = allApps.SingleOrDefault(x => x.alias == s);
                if (app != null)
                    apps.Add(app);
            }

            return apps;
        }

        /// <summary>
        /// Clears the list of applications the user has access to, ensure to call Save afterwords
        /// </summary>
        public void ClearApplications()
        {
            foreach (var s in UserGroupItem.AllowedSections.ToArray())
            {
                UserGroupItem.RemoveAllowedSection(s);
            }
        }

        /// <summary>
        /// Adds a application to the list of allowed applications, ensure to call Save() afterwords
        /// </summary>
        /// <param name="appAlias"></param>
        public void AddApplication(string appAlias)
        {
            UserGroupItem.AddAllowedSection(appAlias);
        }

        #endregion

        /// <summary>
        /// Saves this instance
        /// </summary>
        public void Save()
        {
            PerformSave();
        }

        /// <summary>
        /// Saves this instance (along with the list of users in the group if provided)
        /// </summary>
        public void SaveWithUsers(int[] userIds)
        {
            PerformSave(true, userIds);
        }

        private void PerformSave(bool updateUsers = false, int[] userIds = null)
        {
            //ensure that this object has an ID specified (it exists in the database)
            if (UserGroupItem.HasIdentity == false)
                throw new Exception("The current UserGroup object does not exist in the database. New UserGroups should be created with the MakeNew method");

            ApplicationContext.Current.Services.UserService.SaveUserGroup(UserGroupItem, updateUsers, userIds);

            //raise event
            OnUpdated(this, new EventArgs());
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            //ensure that this object has an ID specified (it exists in the database)
            if (UserGroupItem.HasIdentity == false)
                throw new Exception("The current UserGroup object does not exist in the database. New UserGroups should be created with the MakeNew method");

            ApplicationContext.Current.Services.UserService.DeleteUserGroup(UserGroupItem);

            //raise event
            OnDeleted(this, new EventArgs());
        }

        /// <summary>
        /// Load the data for the current UserGroup by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns true if the UserGroup id was found
        /// and the data was loaded, false if it wasn't</returns>
        public bool LoadByPrimaryKey(int id)
        {
            UserGroupItem = ApplicationContext.Current.Services.UserService.GetUserGroupById(id);
            return UserGroupItem != null;
        }

        /// <summary>
        /// Creates a new user group
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultPermissions"></param>
        /// <param name="alias"></param>
        public static UserGroup MakeNew(string name, string defaultPermissions, string alias)
        {
            //ensure that the current alias does not exist
            //get the id for the new user group
            var existing = UserGroups.Find(ut => (ut.Alias == alias));

            if (existing != null)
                throw new Exception("The UserGroup alias specified already exists");

            var userType = new Umbraco.Core.Models.Membership.UserGroup
            {
                Alias = alias,
                Name = name,
                Permissions = defaultPermissions.ToCharArray().Select(x => x.ToString(CultureInfo.InvariantCulture))
            };
            ApplicationContext.Current.Services.UserService.SaveUserGroup(userType);

            var legacy = new UserGroup(userType);

            //raise event
            OnNew(legacy, new EventArgs());

            return legacy;
        }

        /// <summary>
        /// Gets the user group with the specfied ID
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static UserGroup GetUserGroup(int id)
        {
            return UserGroups.Find(ut => (ut.Id == id));
        }

        /// <summary>
        /// Returns all user groups
        /// </summary>
        /// <returns></returns>
        public static List<UserGroup> GetAllUserGroups()
        {
            return UserGroups;
        }

        internal static event TypedEventHandler<UserGroup, EventArgs> New;
        private static void OnNew(UserGroup userType, EventArgs args)
        {
            if (New != null)
            {
                New(userType, args);
            }
        }

        internal static event TypedEventHandler<UserGroup, EventArgs> Deleted;
        private static void OnDeleted(UserGroup userType, EventArgs args)
        {
            if (Deleted != null)
            {
                Deleted(userType, args);
            }
        }

        internal static event TypedEventHandler<UserGroup, EventArgs> Updated;
        private static void OnUpdated(UserGroup userType, EventArgs args)
        {
            if (Updated != null)
            {
                Updated(userType, args);
            }
        }
    }
}