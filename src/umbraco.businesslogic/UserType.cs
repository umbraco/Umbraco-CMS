using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Caching;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using umbraco.DataLayer;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Represents a umbraco Usertype
    /// </summary>
    [Obsolete("Use the UserService instead")]
    public class UserType
    {

        internal Umbraco.Core.Models.Membership.IUserType UserTypeItem;

        /// <summary>
        /// Creates a new empty instance of a UserType
        /// </summary>
        public UserType()
        {
            UserTypeItem = new Umbraco.Core.Models.Membership.UserType();
        }

        internal UserType(Umbraco.Core.Models.Membership.IUserType userType)
        {
            UserTypeItem = userType;
        }

        /// <summary>
        /// Creates a new instance of a UserType and attempts to 
        /// load it's values from the database cache.
        /// </summary>
        /// <remarks>
        /// If the UserType is not found in the existing ID list, then this object 
        /// will remain an empty object
        /// </remarks>
        /// <param name="id">The UserType id to find</param>
        public UserType(int id)
        {
            this.LoadByPrimaryKey(id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="id">The user type id.</param>
        /// <param name="name">The name.</param>
        public UserType(int id, string name)
        {
            UserTypeItem = new Umbraco.Core.Models.Membership.UserType();
            UserTypeItem.Id = id;
            UserTypeItem.Name = name;
        }

        /// <summary>
        /// Creates a new instance of UserType with all parameters
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="defaultPermissions"></param>
        /// <param name="alias"></param>
        public UserType(int id, string name, string defaultPermissions, string alias)
        {
            UserTypeItem = new Umbraco.Core.Models.Membership.UserType();
            UserTypeItem.Id = id;
            UserTypeItem.Name = name;
            UserTypeItem.Alias = alias;
            UserTypeItem.Permissions = defaultPermissions.ToCharArray().Select(x => x.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The cache storage for all user types
        /// </summary>
        private static List<UserType> UserTypes
        {
            get
            {
                return ApplicationContext.Current.Services.UserService.GetAllUserTypes()
                    .Select(x => new UserType(x))
                    .ToList();
            }
        }
        
        #region Public Properties
        /// <summary>
        /// Gets or sets the user type alias.
        /// </summary>
        public string Alias
        {
            get { return UserTypeItem.Alias; }
            set { UserTypeItem.Alias = value; }
        }

        /// <summary>
        /// Gets the name of the user type.
        /// </summary>
        public string Name
        {
            get { return UserTypeItem.Name; }
            set { UserTypeItem.Name = value; }
        }

        /// <summary>
        /// Gets the id the user type
        /// </summary>
        public int Id
        {
            get { return UserTypeItem.Id; }
        }

        /// <summary>
        /// Gets the default permissions of the user type
        /// </summary>
        public string DefaultPermissions
        {
            get { return string.Join("", UserTypeItem.Permissions); }
            set { UserTypeItem.Permissions = value.ToCharArray().Select(x => x.ToString(CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// Returns an array of UserTypes
        /// </summary>
        [Obsolete("Use the GetAll method instead")]
        public static UserType[] getAll
        {
            get { return GetAllUserTypes().ToArray(); }
        }
        #endregion

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            //ensure that this object has an ID specified (it exists in the database)
            if (UserTypeItem.HasIdentity == false)
                throw new Exception("The current UserType object does not exist in the database. New UserTypes should be created with the MakeNew method");

            ApplicationContext.Current.Services.UserService.SaveUserType(UserTypeItem);
            
            //raise event
            OnUpdated(this, new EventArgs());
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            //ensure that this object has an ID specified (it exists in the database)
            if (UserTypeItem.HasIdentity == false)
                throw new Exception("The current UserType object does not exist in the database. New UserTypes should be created with the MakeNew method");

            ApplicationContext.Current.Services.UserService.DeleteUserType(UserTypeItem);

            //raise event
            OnDeleted(this, new EventArgs());
        }

        /// <summary>
        /// Load the data for the current UserType by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns true if the UserType id was found
        /// and the data was loaded, false if it wasn't</returns>
        public bool LoadByPrimaryKey(int id)
        {
            UserTypeItem = ApplicationContext.Current.Services.UserService.GetUserTypeById(id);
            return UserTypeItem != null;
        }

        /// <summary>
        /// Creates a new user type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultPermissions"></param>
        /// <param name="alias"></param>
        public static UserType MakeNew(string name, string defaultPermissions, string alias)
        {
            //ensure that the current alias does not exist
            //get the id for the new user type
            var existing = UserTypes.Find(ut => (ut.Alias == alias));

            if (existing != null)
                throw new Exception("The UserType alias specified already exists");

            var userType = new Umbraco.Core.Models.Membership.UserType
            {
                Alias = alias,
                Name = name,
                Permissions = defaultPermissions.ToCharArray().Select(x => x.ToString(CultureInfo.InvariantCulture))
            };
            ApplicationContext.Current.Services.UserService.SaveUserType(userType);

            var legacy = new UserType(userType);
            
            //raise event
            OnNew(legacy, new EventArgs());

            return legacy;
        }

        /// <summary>
        /// Gets the user type with the specied ID
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static UserType GetUserType(int id)
        {
            return UserTypes.Find(ut => (ut.Id == id));
        }

        /// <summary>
        /// Returns all UserType's
        /// </summary>
        /// <returns></returns>
        public static List<UserType> GetAllUserTypes()
        {
            return UserTypes;
        }

        internal static event TypedEventHandler<UserType, EventArgs> New;
        private static void OnNew(UserType userType, EventArgs args)
        {
            if (New != null)
            {
                New(userType, args);
            }
        }

        internal static event TypedEventHandler<UserType, EventArgs> Deleted;
        private static void OnDeleted(UserType userType, EventArgs args)
        {
            if (Deleted != null)
            {
                Deleted(userType, args);
            }
        }

        internal static event TypedEventHandler<UserType, EventArgs> Updated;
        private static void OnUpdated(UserType userType, EventArgs args)
        {
            if (Updated != null)
            {
                Updated(userType, args);
            }
        }

    }
}