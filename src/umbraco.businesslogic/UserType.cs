using System;
using System.Collections.Generic;
using System.Data;
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
    public class UserType
    {

        /// <summary>
        /// Creates a new empty instance of a UserType
        /// </summary>
        public UserType() { }

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
            _id = id;
            _name = name;
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
            _name = name;
            _id = id;
            _defaultPermissions = defaultPermissions;
            _alias = alias;
        }

        
        private int _id;
        private string _name;
        private string _defaultPermissions;
        private string _alias;

        /// <summary>
        /// The cache storage for all user types
        /// </summary>
        private static List<UserType> UserTypes
        {
            get
            {
                return ApplicationContext.Current.ApplicationCache.GetCacheItem(
                    CacheKeys.UserTypeCacheKey,
                    () =>
                        {
                            var tmp = new List<UserType>();
                            using (var dr = SqlHelper.ExecuteReader("select id, userTypeName, userTypeAlias, userTypeDefaultPermissions from umbracoUserType"))
                            {
                                while (dr.Read())
                                {
                                    tmp.Add(new UserType(
                                                dr.GetShort("id"),
                                                dr.GetString("userTypeName"),
                                                dr.GetString("userTypeDefaultPermissions"),
                                                dr.GetString("userTypeAlias")));
                                }
                            }
                            return tmp;
                        });
            }
        }

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        #region Public Properties
        /// <summary>
        /// Gets or sets the user type alias.
        /// </summary>
        public string Alias
        {
            get { return _alias; }
            set { _alias = value; }
        }

        /// <summary>
        /// Gets the name of the user type.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Gets the id the user type
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the default permissions of the user type
        /// </summary>
        public string DefaultPermissions
        {
            get { return _defaultPermissions; }
            set { _defaultPermissions = value; }
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
            if (_id <= 0)
                throw new Exception("The current UserType object does not exist in the database. New UserTypes should be created with the MakeNew method");

            SqlHelper.ExecuteNonQuery(@"
				update umbracoUserType set
				userTypeAlias=@alias,userTypeName=@name,userTypeDefaultPermissions=@permissions
                where id=@id",
                                      SqlHelper.CreateParameter("@alias", _alias),
                                      SqlHelper.CreateParameter("@name", _name),
                                      SqlHelper.CreateParameter("@permissions", _defaultPermissions),
                                      SqlHelper.CreateParameter("@id", _id)
                );

            //raise event
            OnUpdated(this, new EventArgs());
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            //ensure that this object has an ID specified (it exists in the database)
            if (_id <= 0)
                throw new Exception("The current UserType object does not exist in the database. New UserTypes should be created with the MakeNew method");

            SqlHelper.ExecuteNonQuery(@"delete from umbracoUserType where id=@id", SqlHelper.CreateParameter("@id", _id));

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
            var userType = GetUserType(id);
            if (userType == null)
                return false;

            _id = userType.Id;
            _alias = userType.Alias;
            _defaultPermissions = userType.DefaultPermissions;
            _name = userType.Name;

            return true;
        }

        /// <summary>
        /// Creates a new user type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultPermissions"></param>
        /// <param name="alias"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static UserType MakeNew(string name, string defaultPermissions, string alias)
        {
            //ensure that the current alias does not exist
            //get the id for the new user type
            var existing = UserTypes.Find(ut => (ut.Alias == alias));

            if (existing != null)
                throw new Exception("The UserType alias specified already exists");

            SqlHelper.ExecuteNonQuery(@"
				insert into umbracoUserType 
				(userTypeAlias,userTypeName,userTypeDefaultPermissions) 
				values (@alias,@name,@permissions)",
                SqlHelper.CreateParameter("@alias", alias),
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@permissions", defaultPermissions));
            
            //get it's id
            var newId = SqlHelper.ExecuteScalar<int>("SELECT MAX(id) FROM umbracoUserType WHERE userTypeAlias=@alias", SqlHelper.CreateParameter("@alias", alias));

            //load the instance and return it
            using (var dr = SqlHelper.ExecuteReader(
                "select id, userTypeName, userTypeAlias, userTypeDefaultPermissions from umbracoUserType where id=@id", 
                SqlHelper.CreateParameter("@id", newId)))
            {
                if (dr.Read())
                {
                    var ut = new UserType(
                        dr.GetShort("id"),
                        dr.GetString("userTypeName"),
                        dr.GetString("userTypeDefaultPermissions"),
                        dr.GetString("userTypeAlias"));

                    //raise event
                    OnNew(ut, new EventArgs());

                    return ut;
                }
                throw new InvalidOperationException("Could not read the new User Type with id of " + newId);
            }
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