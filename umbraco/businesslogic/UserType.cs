using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Caching;

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
            Cache();
            this.LoadByPrimaryKey(id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserType"/> class.
        /// </summary>
        /// <param name="id">The user type id.</param>
        /// <param name="name">The name.</param>
        public UserType(int id, string name)
        {
            m_id = id;
            m_name = name;
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
            m_name = name;
            m_id = id;
            m_defaultPermissions = defaultPermissions;
            m_alias = alias;
        }

        /// <summary>
        /// A static constructor that will Cache the current UserTypes
        /// </summary>
        static UserType()
        {
            Cache();
        }

        private const string CACHE_KEY = "UserTypeCache";
        private static string _connstring = GlobalSettings.DbDSN;

        private int m_id;
        private string m_name;
        private string m_defaultPermissions;
        private string m_alias;

        /// <summary>
        /// The cache storage for all user types
        /// </summary>
        private static List<UserType> UserTypes
        {
            get
            {
                //ensure cache exists
                if (HttpRuntime.Cache[CACHE_KEY] == null)
                    ReCache();
                return HttpRuntime.Cache[CACHE_KEY] as List<UserType>;
            }
            set
            {
                HttpRuntime.Cache[CACHE_KEY] = value;
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
            get { return m_alias; }
            set { m_alias = value; }
        }

        /// <summary>
        /// Gets the name of the user type.
        /// </summary>
        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// Gets the id the user type
        /// </summary>
        public int Id
        {
            get { return m_id; }
        }

        /// <summary>
        /// Gets the default permissions of the user type
        /// </summary>
        public string DefaultPermissions
        {
            get { return m_defaultPermissions; }
            set { m_defaultPermissions = value; }
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
            if (m_id == null || m_id <= 0)
                throw new Exception("The current UserType object does not exist in the database. New UserTypes should be created with the MakeNew method");

            SqlHelper.ExecuteNonQuery(@"
				update umbracoUserType set
				userTypeAlias=@alias,userTypeName=@name,userTypeDefaultPermissions=@permissions
                where id=@id",
              SqlHelper.CreateParameter("@alias", m_alias),
              SqlHelper.CreateParameter("@name", m_name),
              SqlHelper.CreateParameter("@permissions", m_defaultPermissions),
                SqlHelper.CreateParameter("@id", m_id)
            );

            ReCache();
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            //ensure that this object has an ID specified (it exists in the database)
            if (m_id == null || m_id <= 0)
                throw new Exception("The current UserType object does not exist in the database. New UserTypes should be created with the MakeNew method");

            SqlHelper.ExecuteNonQuery(@"
				delete from umbracoUserType where id=@id",
                SqlHelper.CreateParameter("@id", m_id));
            
            ReCache();
        }

        /// <summary>
        /// Load the data for the current UserType by it's id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns true if the UserType id was found
        /// and the data was loaded, false if it wasn't</returns>
        public bool LoadByPrimaryKey(int id)
        {
            UserType userType = GetUserType(id);
            if (userType == null)
                return false;

            this.m_id = userType.Id;
            this.m_alias = userType.Alias;
            this.m_defaultPermissions = userType.DefaultPermissions;
            this.m_name = userType.Name;

            return true;
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
            UserType existing = UserTypes.Find(
                delegate(UserType ut)
                {
                    return (ut.Alias == alias);
                }
            );
            if (existing != null)
                throw new Exception("The UserType alias specified already exists");

            SqlHelper.ExecuteNonQuery(@"
				insert into umbracoUserType 
				(userTypeAlias,userTypeName,userTypeDefaultPermissions) 
				values (@alias,@name,@permissions)",
                SqlHelper.CreateParameter("@alias", alias),
                SqlHelper.CreateParameter("@name", name),
                SqlHelper.CreateParameter("@permissions", defaultPermissions));

            ReCache();

            //find the new user type
            existing = UserTypes.Find(
                delegate(UserType ut)
                {
                    return (ut.Alias == alias);
                }
            );

            return existing;
        }

        /// <summary>
        /// Gets the user type with the specied ID
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static UserType GetUserType(int id)
        {
            return UserTypes.Find(
                delegate(UserType ut)
                {
                    return (ut.Id == id);
                }
            );
        }

        /// <summary>
        /// Returns all UserType's
        /// </summary>
        /// <returns></returns>
        public static List<UserType> GetAllUserTypes()
        {
            return UserTypes;
        }

        /// <summary>
        /// Removes the UserType cache and re-reads the data from the db.
        /// </summary>
        private static void ReCache()
        {
            HttpRuntime.Cache.Remove(CACHE_KEY);
            Cache();
        }

        /// <summary>
        /// Read all UserType data and store it in cache.
        /// </summary>
        private static void Cache()
        {
            //don't query the database is the cache is not null
            if (HttpRuntime.Cache[CACHE_KEY] != null)
                return;

            List<UserType> tmp = new List<UserType>();
            using (IRecordsReader dr =
                SqlHelper.ExecuteReader("select id, userTypeName, userTypeAlias, userTypeDefaultPermissions from umbracoUserType"))
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

            UserTypes = tmp;

        }


    }
}